# Bicep Generation

## Pipeline Architecture (Vague 1) [2026-04-27]

The legacy 920-line `BicepGenerationEngine` God Object was decomposed into a thin facade (~85 LOC) over a staged pipeline. **Public surface preserved** — `BicepGenerationEngine.Generate(GenerationRequest)` and `GenerateMonoRepo(MonoRepoGenerationRequest)` keep the same signatures, so all handlers (`GenerateBicepCommandHandler`, etc.) are untouched.

### Layout under `src/Api/InfraFlowSculptor.BicepGeneration/`

- `BicepGenerationEngine.cs` — facade. Builds a `BicepGenerationContext`, runs the pipeline, applies post-assembly output pruning (single-config or mono-repo).
- `Pipeline/`
  - `IBicepGenerationStage.cs` — `int Order` + `void Execute(BicepGenerationContext)`. Stateless, idempotent, singleton-safe.
  - `BicepGenerationContext.cs` — mutable per-generation state (`Request`, `WorkItems`, `Identity`, `AppSettings`, `ResourceIdToInfo`, `Result`). `ModuleWorkItem` holds the resource + the in-flight `GeneratedTypeModule` (record `with`-mutated by stages).
  - `BicepGenerationPipeline.cs` — orchestrator; sorts injected stages by `Order` at construction.
  - `Stages/` — 9 stages, ordered with spacing of 100 to allow future insertions:
    1. `IdentityAnalysisStage` (100) — system/user identity sets + mixed-ARM-types set.
    2. `AppSettingsAnalysisStage` (200) — outputs to inject per source resource + compute ARM types needing app-settings param.
    3. `ModuleBuildStage` (300) — calls each `IResourceTypeBicepGenerator`, builds initial `WorkItem`. Throws `NotSupportedException` for unregistered ARM types.
    4. `IdentityInjectionStage` (400) — applies system/user/parameterized identity blocks.
    5. `OutputInjectionStage` (500) — appends `output` declarations referenced by app settings; secure outputs marked `@secure()`.
    6. `AppSettingsInjectionStage` (600) — injects `appSettings` (Web/Function App) or `envVars` (Container App) param.
    7. `TagsInjectionStage` (700) — injects `param tags object = {}` + `tags: tags`.
    8. `ParentReferenceResolutionStage` (800) — resolves `appServicePlanId`, `containerAppEnvironmentId`, `logAnalyticsWorkspaceId` (with same-config / cross-config existing fallback for `Microsoft.Insights/components` and `Microsoft.App/managedEnvironments`), and `sqlServerId`.
    9. `AssemblyStage` (900) — delegates to `BicepAssembler.Assemble`. Output pruning is **not** here — owned by the engine to support cross-config pruning in mono-repo.
- `TextManipulation/` — pure static helpers extracted from the engine, **byte-for-byte parity** with the legacy implementation:
  - `BicepTextManipulationHelpers` — 13 compiled regex (`ResourceSymbolPattern`, `IdentityBlockPattern`, etc.), `InsertAt`, `FindClosingBrace`, `FindFirstLineIndex`, `HasParam`, `HasOutput`, `Capitalize`, `AddTypeImport`.
  - `BicepIdentityInjector` — `InjectSystemAssigned`, `InjectUserAssigned`, `InjectParameterized` + `ManagedIdentityTypeBicepType` const.
  - `BicepOutputInjector` — `Inject(string, IReadOnlyCollection<ModuleOutputInjection>)`, `ExtractRootSymbol`.
  - `BicepAppSettingsInjector` — `Inject(string, string armType)` routing to `InjectWebFunctionAppSettings` / `InjectContainerAppEnvVars`.
  - `BicepTagsInjector` — `Inject(string)`.
  - `BicepOutputPruner` — `PruneSingleConfig`, `PruneMonoRepo`, `CollectUsedOutputsByPath`.

### DI registration

In `src/Api/InfraFlowSculptor.Application/DependencyInjection.cs`, the 18 `IResourceTypeBicepGenerator` singletons are followed by 9 `IBicepGenerationStage` singletons + `BicepGenerationPipeline` + `BicepGenerationEngine`. Stage order is determined by `IBicepGenerationStage.Order`, **not** by DI registration order.

### Magic strings removed

Compute ARM identifiers were extracted into `AzureResourceTypes.ComputeArmTypes` (`IReadOnlySet<string>` containing `WebApp`, `FunctionApp`, `ContainerApp` ARM strings).

### Testability

Each stage and each `TextManipulation/*` helper is independently unit-testable. `InternalsVisibleTo` on the BicepGeneration csproj exposes `internal` helpers to the test assembly. Test project at `tests/InfraFlowSculptor.BicepGeneration.Tests/` with xUnit 2.9.3 + FluentAssertions 6.12.2 + NSubstitute 5.3.0:

- **124 unit tests** covering:
  - `TextManipulation/`: `BicepTextManipulationHelpers` (18), `BicepIdentityInjector` (12), `BicepOutputInjector` (11), `BicepAppSettingsInjector` (7), `BicepTagsInjector` (5), `BicepOutputPruner` (4)
  - `Pipeline/`: `BicepGenerationPipeline` orchestration (4)
  - `Pipeline/Stages/`: `IdentityAnalysis` (8), `AppSettingsAnalysis` (8), `ModuleBuild` (6), `IdentityInjection` (7), `OutputInjection` (4), `AppSettingsInjection` (4), `TagsInjection` (4), `ParentReferenceResolution` (9), `Assembly` (3)
- Convention: `Given_When_Then` naming, AAA pattern, `_sut` for SUT, FluentAssertions, NSubstitute for `IBicepGenerationStage` / `IResourceTypeBicepGenerator` mocks
- Pitfall: `BicepTagsInjector.Inject` regex requires `\n` before first `param`; test modules must include a `@description` or blank line above the first param

### Constraints for future modifications

- **Never** call `BicepOutputPruner.PruneSingleConfig` from a stage — pruning is engine-owned to keep the mono-repo cross-config pruning correct.
- A new mutation stage that touches `ModuleBicepContent` must use `item.Module = item.Module with { ModuleBicepContent = ... }` to avoid losing fields set by earlier stages (identity kind, parameterized flag, etc.).
- Adding a new helper to `TextManipulation/` requires keeping it `internal` or `public static` and **pure** (no DI, no shared state).

## Bootstrap split for SplitInfraCode [2026-04-25]
- `BootstrapPipelineGenerationEngine` is mode-driven via `BootstrapMode` enum (`FullOwner` default, `ApplicationOnly` for code-side in SplitInfraCode). `FullOwner` keeps the historical 3-job structure (provision pipelines + environments + variable groups). `ApplicationOnly` emits a `ValidateSharedResources` job (env via REST, variable groups via `az pipelines variable-group list`) followed by a `ProvisionPipelineDefinitions` job with `dependsOn: ValidateSharedResources`. The validation step throws an actionable error listing missing items if the infra bootstrap was not run first.
- `ArtifactKind.BootstrapApplication` was added next to `Bootstrap`. `RepositoryTargetResolver` routes `BootstrapApplication` (and `ApplicationPipeline`) to `RepositoryContentKindsEnum.ApplicationCode`.
- `GenerateProjectBootstrapPipelineCommandHandler` detects `LayoutPresetEnum.SplitInfraCode` and produces TWO requests in the same execution: an infra `FullOwner` request keyed at `bootstrap/project/{id}/{ts}/infra/...` and a code `ApplicationOnly` request keyed at `.../app/...`. For `AllInOne`/`MultiRepo` the historical single bootstrap is preserved at the root `bootstrap/project/{id}/{ts}/...`. Pipeline definitions are split via `BuildPipelineDefinitionsSplitAsync` so infra YAML paths point at `target.PipelineBasePath` and app YAML paths at `appTarget.PipelineBasePath`.
- Response contracts (`GenerateProjectBootstrapPipelineResult`, `GenerateProjectBootstrapPipelineResponse`) carry three dictionaries: backward-compatible `FileUris` (union, prefixed with `infra/`/`app/` in split mode) plus new `InfraFileUris` and `AppFileUris` (un-prefixed, repo-relative).
- `PushProjectBootstrapPipelineToGitCommandHandler` filters by bucket prefix in `GetLatestBootstrapFilesAsync(projectId, bucketPrefix, ct)`. In SplitInfraCode it pushes only the `infra/` bucket to the infra repo. `PushProjectArtifactsToMultiRepoCommandHandler` now also adds the `app/` bootstrap bucket to code-repo pushes, so the code-side push scope is `app pipeline + app bootstrap`. [2026-04-26]
- Frontend `SplitGenerationSwitcherComponent` exposes separate `infraBootstrapNodes`/`appBootstrapNodes` from `InfraFileUris` and `AppFileUris`, while preserving `infra/` / `app/` prefixes in the file-loader URI passed to `/projects/{id}/generate-bootstrap-pipeline/files/{*filePath}`. The split view now renders a dedicated code bootstrap tab (`PROJECT_DETAIL.GENERATION.TAB_BOOTSTRAP_APP`) with code-side setup/usage guidance. New i18n keys: `SWITCHER.BOOTSTRAP_APP_INFO`, `SWITCHER.BOOTSTRAP_APP_GUIDE_TITLE`, `SWITCHER.BOOTSTRAP_APP_STEP_{1..3}` (EN/FR), plus updated `CODE_PUSH_DESC` to reflect that the code push now includes both app pipeline and app bootstrap artifacts.
- File-content endpoint `/projects/{id}/generate-bootstrap-pipeline/files/{*filePath}` works as-is because `SafeRelativePath.TryNormalize` accepts segments with `/`. The download endpoint zips the entire latest blob prefix and naturally preserves the `infra/`/`app/` sub-folders.

## Mono-Repo Shared Layout (Common restored for project generation) [2026-04-24]
- `MonoRepoBicepAssembler.Assemble(..., bool flattenShared = false)` still supports a flat shared layout, but project-level generation no longer enables it for `LayoutPresetEnum.SplitInfraCode`.
- `GenerateProjectBicepCommandHandler` now keeps shared files under `Common/` for both `AllInOne` and `SplitInfraCode`. Generated `main.bicep` files continue to import `../Common/{types,functions,constants}.bicep` and shared modules via `../Common/modules/...`.
- Per-config flow (`InfrastructureConfig.GenerateBicepCommand` + `PushBicepToGitCommand`) used in `LayoutPresetEnum.MultiRepo` remains flat at the repository root (no `Common/`), so isolated infra repos still receive `types.bicep`, `functions.bicep`, `main.bicep`, `parameters/...`, `modules/...` directly.
- Frontend: `SplitGenerationSwitcherComponent.buildBicepNodes` now renders a `Common/` folder again for project-level split generation, matching the backend keys returned as `Common/...`.

## V2 Multi-repo Git routing [2026-04-23]

**Decision:** engines (`BicepGenerationEngine`, `MonoRepoBicepAssembler`, `PipelineGenerationEngine`) remain **repo-agnostic**. They produce an abstract tree (`IReadOnlyDictionary<string,string>` path→content). Repo routing lives **only in Application-layer handlers** via `IRepositoryTargetResolver`.

- `IRepositoryTargetResolver.Resolve(project, config, kind)` returns `ResolvedRepositoryTarget` with `Alias`, `RepositoryUrl`, `Owner`, `RepositoryName`, `Branch`, `BasePath?`, `PipelineBasePath?`, `PatSecretName?`.
- Resolution priority: (1) `config.RepositoryBinding.Alias` → lookup in `project.Repositories`; (2) else alias `"default"` → lookup. Legacy fallback `project.GitRepositoryConfiguration` removed in V3 (table dropped).
- `ArtifactKind.{Infrastructure|Pipeline|Bootstrap|BootstrapApplication|ApplicationPipeline}` selects which path fields are populated. `ApplicationPipeline` and `BootstrapApplication` route to `RepositoryContentKindsEnum.ApplicationCode` (SplitInfraCode); the others route to `Infrastructure`.
- File-level routing in mixed pipeline output via `AppPipelineFileClassifier` (Application/Common/Generation): `path.StartsWith("apps/")` OR membership in the frozen set seeded from `AppPipelineGenerationEngine.GenerateSharedTemplates().Keys` (20 known shared app template paths under `.azuredevops/{steps,jobs,pipelines}/app-*.yml`) ⇒ Application Code repo; everything else ⇒ Infrastructure repo.
- `GenerateProjectPipelineCommandHandler` uploads under `pipeline/project/{id}/{ts}/{infra|app}/...` blob layout and returns 6 result fields: legacy `CommonFileUris`/`ConfigFileUris` (union, backward compat) + new `InfraCommonFileUris`, `AppCommonFileUris`, `InfraConfigFileUris`, `AppConfigFileUris`. `GeneratedPipelinePathNormalizer` strips the leading `infra/`/`app/` segment so legacy AllInOne push handler keeps reading the new layout transparently.
- 9 handlers bascules in V2 A3 (PushBicepToGit, PushPipelineToGit, GeneratePipeline, PushProjectBicepToGit, PushProjectPipelineToGit, PushProjectBootstrapPipelineToGit, PushProjectGeneratedArtifactsToGit V2-lite, GenerateProjectPipeline, GenerateProjectBootstrapPipeline).
- `GenerateProjectBicep` + `GenerateProjectPipeline` gate via `Project.CanGenerateAllFromProjectLevel(configs)` → `Errors.GitRouting.AmbiguousProjectLevelGeneration` if heterogeneous.
- **Parity placeholder folder** `tests/InfraFlowSculptor.GenerationParity.Tests/` is still present, but it is now empty and the active xUnit `.csproj` was removed on 2026-04-27 to reset the .NET test strategy. Recreate a dedicated test project there before relying on golden-file commands again.

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

## Module Reuse Across Same-Type Resources [2026-04-26]
- **AVM-style generic params:** Modules use a single `param userAssignedIdentityId string` (or `string = ''` when parameterized) instead of per-UAI-name params like `userAssignedIdentityBackendId`. `main.bicep` passes the correct UAI module output for each resource instance.
- For compute ARM types, `envVars` (Container App) / `appSettings` (Web/Function App) is injected into **every** module instance as soon as **any** resource of that type has app settings. Instances without app settings simply omit the argument in `main.bicep` and rely on the default `[]`.
- Together these two rules let `BicepAssembler.NormalizePrimaryModuleFileNames` collapse all instances onto a single shared `*.module.bicep` file (no `.{hash}` suffix).
- The `differingUaiArmTypes` pre-computation was removed — no longer needed since all instances naturally produce identical module content with the generic param.
- When `InjectParameterizedIdentity` is used (mixed identity kinds), the module template includes: `userAssignedIdentities: contains(identityType, 'UserAssigned') && !empty(userAssignedIdentityId) ? { '${userAssignedIdentityId}': {} } : null`.

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
