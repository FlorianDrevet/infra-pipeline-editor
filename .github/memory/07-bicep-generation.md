# Bicep Generation

## Mono-Repo Shared Layout (Common/ vs flat) [2026-04-23]
- `MonoRepoBicepAssembler.Assemble(..., bool flattenShared = false)` and `MonoRepoGenerationRequest.FlattenShared` control whether shared files (`types.bicep`, `functions.bicep`, `constants.bicep`, `modules/...`) live under a `Common/` namespace (default, AllInOne) or directly at the repo root (SplitInfraCode).
- `GenerateProjectBicepCommandHandler` sets `FlattenShared = true` iff `project.LayoutPreset?.Value == LayoutPresetEnum.SplitInfraCode`. The same flag drives both the in-file `main.bicep` rewrites (`'../Common/modules/...'` vs `'../modules/...'`, `from '../Common/types.bicep'` vs `from '../types.bicep'`) and the upload prefix (`{prefix}/Common/{path}` vs `{prefix}/{path}`) plus the `CommonFileUris` keys returned to the API.
- Per-config flow (`InfrastructureConfig.GenerateBicepCommand` + `PushBicepToGitCommand`) used in `LayoutPresetEnum.MultiRepo` was already flat at the root (no `Common/`), so MultiRepo per-config repos receive `types.bicep`, `functions.bicep`, `main.bicep`, `parameters/...`, `modules/...` directly. No change there.
- Frontend: `SplitGenerationSwitcherComponent.buildBicepNodes` renders the new flat layout (no `Common/` folder node, shared files at depth 0, `modules/` folder at depth 0). The legacy `ProjectDetailComponent.buildBicepNodes` still wraps in a `Common/` folder for AllInOne (where the backend keeps the `Common/` prefix).

## V2 Multi-repo Git routing [2026-04-23]

**Decision:** engines (`BicepGenerationEngine`, `MonoRepoBicepAssembler`, `PipelineGenerationEngine`) remain **repo-agnostic**. They produce an abstract tree (`IReadOnlyDictionary<string,string>` path→content). Repo routing lives **only in Application-layer handlers** via `IRepositoryTargetResolver`.

- `IRepositoryTargetResolver.Resolve(project, config, kind)` returns `ResolvedRepositoryTarget` with `Alias`, `RepositoryUrl`, `Owner`, `RepositoryName`, `Branch`, `BasePath?`, `PipelineBasePath?`, `PatSecretName?`.
- Resolution priority: (1) `config.RepositoryBinding.Alias` → lookup in `project.Repositories`; (2) else alias `"default"` → lookup. Legacy fallback `project.GitRepositoryConfiguration` removed in V3 (table dropped).
- `ArtifactKind.{Infrastructure|Pipeline|Bootstrap|ApplicationPipeline}` selects which path fields are populated. `ApplicationPipeline` routes to `RepositoryContentKindsEnum.ApplicationCode` (SplitInfraCode dual push); the others route to `Infrastructure`.
- File-level routing in mixed pipeline output via `AppPipelineFileClassifier` (Application/Common/Generation): `path.StartsWith("apps/")` OR membership in the frozen set seeded from `AppPipelineGenerationEngine.GenerateSharedTemplates().Keys` (20 known shared app template paths under `.azuredevops/{steps,jobs,pipelines}/app-*.yml`) ⇒ Application Code repo; everything else ⇒ Infrastructure repo.
- `GenerateProjectPipelineCommandHandler` uploads under `pipeline/project/{id}/{ts}/{infra|app}/...` blob layout and returns 6 result fields: legacy `CommonFileUris`/`ConfigFileUris` (union, backward compat) + new `InfraCommonFileUris`, `AppCommonFileUris`, `InfraConfigFileUris`, `AppConfigFileUris`. `GeneratedPipelinePathNormalizer` strips the leading `infra/`/`app/` segment so legacy AllInOne push handler keeps reading the new layout transparently.
- 9 handlers bascules in V2 A3 (PushBicepToGit, PushPipelineToGit, GeneratePipeline, PushProjectBicepToGit, PushProjectPipelineToGit, PushProjectBootstrapPipelineToGit, PushProjectGeneratedArtifactsToGit V2-lite, GenerateProjectPipeline, GenerateProjectBootstrapPipeline).
- `GenerateProjectBicep` + `GenerateProjectPipeline` gate via `Project.CanGenerateAllFromProjectLevel(configs)` → `Errors.GitRouting.AmbiguousProjectLevelGeneration` if heterogeneous.
- **Parity harness** `tests/InfraFlowSculptor.GenerationParity.Tests/` (xUnit, 35 goldens byte-à-byte). Regenerate: `dotnet test -p:DefineConstants=REGENERATE_GOLDENS`.

## Architecture
- Pure engine in `InfraFlowSculptor.BicepGeneration` (no domain dependency)
- Strategy pattern: `IResourceTypeBicepGenerator` per resource type
- Registered as singletons in `Application/DependencyInjection.cs`
- `AzureResourceTypes` static class in `GenerationCore`: centralized constants for all 18 resource type identifiers (+ ResourceGroup constant). **Never use magic strings.**

## BicepAssembler Structure [2026-04-04]
Refactored from monolith (~1680 lines) to thin orchestrator (~180 lines) + 14 specialized classes:
- `BicepAssembler.cs` — public thin orchestrator (`Assemble()`, `PruneUnusedOutputs()`)
- `Assemblers/` — per-file generators: `TypesBicepAssembler`, `FunctionsBicepAssembler`, `ConstantsBicepAssembler`, `MainBicepAssembler`, `ParameterFileAssembler`, `KvSecretsModuleAssembler`, `RoleAssignmentAssembler`
- `Helpers/` — pure functions: `BicepFormattingHelper` (string formatting), `ResourceTypeMetadata` (switch maps), `ModuleHeaderHelper` (headers), `BicepNamingHelper` (naming expressions + param names)
- `StorageAccount/` — `StorageAccountCompanionHelper` (CORS, lifecycle, companion modules)
- `Models/` — `GroupedRoleAssignment`, `RoleRef` (extracted internal types)
- Pattern: Helpers = pure functions (no model deps). Assemblers = produce Bicep strings from models. Orchestrator wires them.

## Output Files
- `types.bicep` — `EnvironmentName` union, `EnvironmentVariables` type, `environments` map
- `functions.bicep` — naming functions from project naming templates
- `main.bicep` — resource declarations, module imports
- `main.{shortName}.bicepparam` — per-environment parameter files (named by `EnvironmentDefinition.ShortName`, e.g. `main.dev.bicepparam`)
- `modules/{FolderName}/{moduleType}.module.bicep` + `types.bicep` per resource type
- `constants.bicep` — RBAC role constants (only when role assignments exist)

## Bicepparam Conventions [2026-04-17]
- **File name** uses `EnvironmentDefinition.ShortName` (e.g. `main.dev.bicepparam`); pipeline `${{ environment }}` resolves to ShortName.
- **Internal `param environmentName`** uses the full `EnvironmentDefinition.Name` sanitized for Bicep `EnvironmentName` union type matching.
- **Relative path**: `.bicepparam` files live under `parameters/` → must declare `using '../main.bicep'` (not `using 'main.bicep'`).

## Module Conventions
- Each module folder contains `{moduleType}.module.bicep` + `types.bicep` with exported types.
- `GeneratedTypeModule.CompanionModules` supports multiple companions (for example StorageAccount blobs, queues, tables).
- Role assignment generation uses `RbacRoleType`, `constants.bicep`, and uniform vs mixed managed identity injection.

## ContainerApp Bicep Params [2026-04-03]
All typed per-env parameters (cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, etc.) **must** be added to the generator's `Parameters` dictionary — not just ACR params. Missing entries cause env-specific values to be silently ignored in `.bicepparam` files.

## ContainerApp Health Probes [2026-04-22]
- 6 new per-env fields: `readinessProbePath`, `readinessProbePort`, `livenessProbePath`, `livenessProbePort`, `startupProbePath`, `startupProbePort`
- Bicep templates use `union()` to conditionally build the `probes` array — each probe type only emitted when both path is non-empty and port > 0
- V1: HTTP probes only (no TCP/gRPC). Path must start with `/`, port 1-65535.

## ContainerApp User-Defined Types (Parameter Grouping) [2026-04-22]
- 16 flat Container App params were grouped into 4 exported types: `ContainerRuntimeConfig`, `ScalingConfig`, `IngressConfig`, `HealthProbeConfig`.
- `GeneratedTypeModule` now exposes `ParameterTypeOverrides` and `ParameterGroupMappings`; `MainBicepAssembler` imports the type names and `ParameterFileAssembler` merges flat env overrides into structured objects.
- `BicepFormattingHelper.SerializeToBicep` handles `IDictionary<string, object>` for grouped params.
- ACR params (`acrLoginServer`, `acrManagedIdentityClientId`) stay flat because they are conditional.

## Custom Domains & Secure Parameter Mapping Integration [2026-04-23]
- `ResourceDefinition.CustomDomains` carries per-environment domain bindings into generation; `ParameterFileAssembler` emits `customDomains` arrays with `domainName` and `bindingType` entries.
- Custom domains are rendered only for compute resources that support them: `ContainerAppTypeBicepGenerator`, `WebAppTypeBicepGenerator`, and `FunctionAppTypeBicepGenerator`.
- `SecureParameterOverrideHelper` merges user-configured `SecureParameterMappings` into `PipelineVariableGroupDefinition.Mappings` and only auto-derives `overrideParameters` entries for secure params that are not mapped to a variable group.

## Identifier & Value Normalization [2026-04-22]
- All Bicep identifiers use **camelCase** via `BicepIdentifierHelper` and `BicepFormattingHelper`.
- `BicepGenerationEngine` gracefully skips resource types without a registered generator (logs warning, does not throw).
- **Domain enum → ARM pitfall:** `EnumValueObject` member names (e.g. `V12`) may not match ARM values (e.g. `12.0`). Generators must normalize via mapping switch.

## SecureParameters Pattern [2026-04-21]
- `GeneratedTypeModule.SecureParameters` (`IReadOnlyList<string>`) holds param names requiring `@secure()` with no default value (e.g. `administratorLoginPassword` for SqlServer).
- `MainBicepAssembler` emits secure param declarations and module call arguments; `ParameterFileAssembler` emits empty placeholders per environment file.
- Generators opt in through `GeneratedTypeModule.SecureParameters`.
- `GenerationRequest.SecureParameterOverrides` still feeds `PipelineGenerationEngine.BuildOverrideParameters()`, but only for secure params not redirected through `SecureParameterMappings`.
- `MainBicepAssembler` must emit `dependsOn: [ {keyVaultModule}Module ]` on each generated `kvSecrets.module.bicep` declaration for locally created Key Vaults. The secrets module uses an `existing` Key Vault reference internally, so without the explicit dependency ARM can race secret creation ahead of vault creation and fail with `Microsoft.KeyVault/vaults/secrets NotFound` on paths like `ifs-kv-dev/JWT_SECRET`.

## ACR Auth Modes [2026-04-23]
- `InfrastructureConfigReadRepository` projects `acrAuthMode` into `ResourceDefinition.Properties` for `ContainerApp`, `WebApp`, and `FunctionApp`.
- `ContainerAppTypeBicepGenerator`, `WebAppTypeBicepGenerator`, and `FunctionAppTypeBicepGenerator` branch between `ManagedIdentity` and `AdminCredentials`.
- Missing / `null` `acrAuthMode` stays backward-compatible and is treated as managed identity.
- Admin-credentials mode uses `GeneratedTypeModule.SecureParameters = ["acrPassword"]` so the secret flows through the existing secure-parameter / pipeline-variable-group path.
- Container App admin mode emits `configuration.secrets` plus `registries.username/passwordSecretRef`; Web App and Function App admin mode emit `DOCKER_REGISTRY_SERVER_URL`, `DOCKER_REGISTRY_SERVER_USERNAME`, and secure `DOCKER_REGISTRY_SERVER_PASSWORD` app settings.
- App pipeline generation mirrors the same mode: managed identity keeps the `Docker@2` service-connection flow, admin credentials switch to explicit `docker login` and build/push script steps.

## Compute Module Variant Files [2026-04-23]
 - `ContainerApp`, `WebApp`, and `FunctionApp` variants with incompatible parameter surfaces must use distinct `GeneratedTypeModule.ModuleFileName` values (for example base vs ACR-managed-identity vs ACR-admin-credentials). `BicepAssembler` deduplicates emitted module files by file name, so sharing one file name across variants can make `main.bicep` pass params that the surviving module file does not declare, leading to BCP037.
 - `MainBicepAssembler` must import naming functions referenced only by cross-config existing resources or role-assignment targets, and must declare `existing_{resourceGroup}` scopes for cross-config role assignments even when no `ExistingResourceReference` directly targets that resource group. This prevents BCP057 on missing symbols such as `BuildContainerRegistryName` or undeclared RG identifiers.

## Module Content Disambiguation [2026-04-23]
- `BicepAssembler` must disambiguate module file names by final `ModuleBicepContent`, not just by logical variant file name. Resource-specific injections like managed identity, app settings, or exported outputs can change the final parameter surface even when `ModuleFileName` starts identical.
- `MonoRepoBicepAssembler` must perform the same content-based disambiguation across configurations when populating `Common/modules/...`, and rewrite each config `main.bicep` to the normalized Common path. Otherwise one config can reuse another config's incompatible module file and trigger BCP037 on params such as `identityType` or `userAssignedIdentityFrontendId`.
- Folder-level `modules/{ResourceType}/types.bicep` must be merged, not first-wins or last-wins, because identity parameterization can append exported types like `ManagedIdentityType` only for some configs.

## Mono-Repo Shared Constants [2026-04-23]
- `MonoRepoBicepAssembler` must rebuild `Common/constants.bicep` from the union of all per-config `RoleAssignments`, not by picking one config's `ConstantsBicep` payload by file size.
- Otherwise a config `main.bicep` can import `../Common/constants.bicep` and reference a role such as `Key Vault Secrets User` that is absent from the shared file, producing BCP053 while another config's file only contains roles like `AcrPull`.

## Role Reference ServiceCategory per Role [2026-04-23]
- `RoleRef` must carry its own `ServiceCategory` (not inherit from the group). `MainBicepAssembler` must emit `RbacRoles.{role.ServiceCategory}['{role.RoleDefinitionName}']`, not `RbacRoles.{group.ServiceCategory}[...]`.
- When a single (source, target) group contains roles from different Azure services (e.g. AcrPull → containerregistry + Key Vault Secrets User → keyvault), using `group.ServiceCategory` (which takes the first role's category) produces BCP053 because `constants.bicep` indexes roles by their actual service category.

## Role Assignment Grouping Key [2026-04-23]
- `RoleAssignmentAssembler.GroupRoleAssignments()` must include the target resource type, target resource group, and cross-config flag in its grouping key, not just source name + target name + identity.
- Otherwise distinct targets that share the same logical name (for example Key Vault `ifs` and Container Registry `ifs`) are merged into one role-assignment module, which can emit `Key Vault Secrets User` on the Container Registry module or collapse scopes incorrectly.

## Resource-Level Identity Injection [2026-04-23]
- `BicepGenerationEngine` identity injection must detect only the resource-root `identity:` block. Nested properties such as Container App ACR registry entries also use an `identity:` field; treating those as an existing resource identity prevents injection of the actual managed-identity block and causes mixed-identity modules to miss `identityType` / UAI params.

## Output Injection Symbol Validation [2026-04-21]
- `InjectOutputDeclarations` validates that the root symbol referenced by an output exists in the target module before injection.
- `ExtractRootSymbol()` handles both direct identifiers and interpolated strings, preventing BCP057 errors when the catalog points at the wrong module type.

## ContainerAppEnvironment — Secure LAW Integration [2026-04-22]
- **BREAKING security fix:** `ContainerAppEnvironmentTypeBicepGenerator` no longer calls `listKeys()` inside the module.
- Old pattern: `destination: 'log-analytics'` + `existing` LAW resource + `listKeys().primarySharedKey` → key exposed in ARM deployment history.
- **New pattern:** `destination: 'azure-monitor'` (no key) + `Microsoft.Insights/diagnosticSettings@2021-05-01-preview` scoped to the CAE resource, with `workspaceId` = LAW resource ID passed as a plain `string` param.
- The `logAnalyticsWorkspaceId` param (full resource ID injected by `BicepGenerationEngine`) is passed directly as `diagnosticSettings.properties.workspaceId` — no `listKeys()` anywhere.
- Diagnostic category: `categoryGroup: 'allLogs'`.
# Bicep Generation

## Architecture
- Pure engine in `InfraFlowSculptor.BicepGeneration` with no domain dependency.
- Strategy pattern: one `IResourceTypeBicepGenerator` per Azure resource type, registered in `Application/DependencyInjection.cs`.
- `AzureResourceTypes` in `GenerationCore` is the canonical source for all 18 resource identifiers plus `ResourceGroup`; never use magic strings.
- `BicepGenerationEngine` skips unregistered resource types with a warning instead of throwing.
## Assembler Layout [2026-04-04]
- `BicepAssembler` is a thin orchestrator (`Assemble()`, `PruneUnusedOutputs()`).
- `Assemblers/` own per-file output: `TypesBicepAssembler`, `FunctionsBicepAssembler`, `ConstantsBicepAssembler`, `MainBicepAssembler`, `ParameterFileAssembler`, `KvSecretsModuleAssembler`, `RoleAssignmentAssembler`.
- `Helpers/` stay pure: formatting, resource metadata, naming expressions, module headers.
- `StorageAccountCompanionHelper` owns blobs/queues/tables/CORS/lifecycle companion output.
## Output Files and Module Shape
- `types.bicep` defines environment unions/maps and exported module types.
- `functions.bicep` contains naming helpers from project naming templates.
- `main.bicep` contains module declarations, resource wiring, and output references.
- `main.{shortName}.bicepparam` is emitted per environment using `EnvironmentDefinition.ShortName`.
- Each module folder contains `{moduleType}.module.bicep` plus `types.bicep`; `constants.bicep` appears only when RBAC role assignments exist.
## Parameter, Path, and Identifier Conventions
- `.bicepparam` files must declare `using '../main.bicep'`; the internal `environmentName` param uses the full sanitized environment name.
- All generated identifiers are normalized to camelCase via `BicepIdentifierHelper` / `BicepFormattingHelper`.
- `PathSanitizer.Sanitize()` turns spaces and underscores into dashes for folders, artifact paths, and pipeline sources.
- `ParameterFileAssembler.CoerceToOriginalType()` converts persisted string overrides back to `bool`, `int`, `long`, or `double` before serialization.
- Domain enum member names can diverge from ARM literals (`V12` vs `12.0`); generators must map them explicitly.
## Container App Parameters and Health Probes
- All typed per-environment Container App values must be present in the generator `Parameters` dictionary or `.bicepparam` overrides are silently lost.
- Health probes add 6 nullable per-env HTTP fields: readiness/liveness/startup path + port.
- Templates use `union()` to emit probe entries only when path is non-empty and port is greater than 0.
- V1 supports HTTP only; path must start with `/`, port must be 1-65535.
- `InjectContainerAppEnvVars` must insert `env: envVars` before `resources: {` or Bicep emits BCP037.
## Container App User-Defined Types [2026-04-22]
- Container App generation now groups 16 flat parameters into 4 exported types: `ContainerRuntimeConfig`, `ScalingConfig`, `IngressConfig`, and `HealthProbeConfig`.
- `GeneratedTypeModule.ParameterTypeOverrides` controls module param type names in `MainBicepAssembler`.
- `GeneratedTypeModule.ParameterGroupMappings` maps flat persisted keys to grouped object properties in `ParameterFileAssembler.ApplyEnvironmentOverrides()`.
- `BicepFormattingHelper.SerializeToBicep()` supports merged `IDictionary<string, object>` payloads.
- Domain `ToDictionary()` stays flat; the grouping logic is a Bicep-layer concern.
## Secure Parameters and Cross-Config Resolution [2026-04-21]
- `GeneratedTypeModule.SecureParameters` marks params that need `@secure()` declarations with no default value.
- `MainBicepAssembler` emits secure param declarations and forwards them into module calls; `ParameterFileAssembler` emits blank placeholders per env file.
- Pipeline generation derives `overrideParameters` from registered generators and resource `SecureParameterMappings`, so secure Bicep params can be bound to Azure DevOps secret variables.
- `InjectOutputDeclarations` validates output root symbols before injection to avoid BCP057.
- `GeneratedTypeModule.ExistingResourceIdReferences` resolves parent IDs as `existing_{symbol}.id`; LAW lookup falls back from same-config to cross-config existing resources.
- Never match modules by logical name alone; `MainBicepAssembler` disambiguates by `(LogicalResourceName, ResourceTypeName)`.
## CAE Security Fix [2026-04-22]
- `ContainerAppEnvironmentTypeBicepGenerator` no longer calls `listKeys()` for Log Analytics.
- New pattern: `destination: 'azure-monitor'` plus `Microsoft.Insights/diagnosticSettings` scoped to the CAE resource, with the LAW resource ID passed as a plain string.
- Diagnostic category group is `allLogs`, and the obsolete `sku` / `SkuName` CAE param was removed.
## Custom Domains [2026-04-22]
- `ParameterFileAssembler` injects per-environment `customDomains` arrays from `AzureResource.CustomDomains`.
- `ContainerAppTypeBicepGenerator` wires custom domains into ingress.
- `WebAppTypeBicepGenerator` and `FunctionAppTypeBicepGenerator` emit `Microsoft.Web/sites/hostNameBindings` resources.
- Binding type is translated into the target SSL state.
## Naming Integration
- `NamingTemplateTranslator` converts `{name}`, `{prefix}`, `{suffix}`, `{env}`, `{envShort}`, `{resourceType}`, `{resourceAbbr}`, and `{location}` placeholders into Bicep expressions.
- `AzureNamingConstraints` provides resource-type-specific static-char rules, lengths, and recommended templates.
- `NamingTemplateValidator` is resource-type-aware; unknown resource types fall back to the generic check.
- Live availability checks are DNS-based and support `available`, `unavailable`, `unknown`, `invalid`, and `current`.
## Syntax and Deployment Pitfalls
- BCP124: `@secure()` only applies to `string` or `object`, never `array`.
- BCP247: lambda variables cannot index resource collections; use `toObject(...)`.
- BCP138: nested `for` expressions are not allowed in variable declarations; use `map()`.
- Storage companion modules emit `dependsOn: [ {moduleName}Module ]` to avoid ARM race conditions.