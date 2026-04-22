# Bicep Generation

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

## Module Folder Structure [2026-03-24]
Each module folder contains: `{moduleType}.module.bicep` + `types.bicep` (with `@export()` union types).

## Companion Modules
`GeneratedTypeModule.CompanionModules` supports multiple companions (e.g., StorageAccount: blobs, queues, tables).

## Role Assignment Generation [2026-03-25]
- `RbacRoleType` in `types.bicep`, `constants.bicep` with used roles, per-target RBAC modules
- Identity injection: uniform vs mixed mode (parameterized `ManagedIdentityType`)

## ContainerApp Bicep Params [2026-04-03]
All typed per-env parameters (cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, etc.) **must** be added to the generator's `Parameters` dictionary — not just ACR params. Missing entries cause env-specific values to be silently ignored in `.bicepparam` files.

## ContainerApp Health Probes [2026-04-22]
- 6 new per-env fields: `readinessProbePath`, `readinessProbePort`, `livenessProbePath`, `livenessProbePort`, `startupProbePath`, `startupProbePort`
- Bicep templates use `union()` to conditionally build the `probes` array — each probe type only emitted when both path is non-empty and port > 0
- V1: HTTP probes only (no TCP/gRPC). Path must start with `/`, port 1-65535.

## ContainerApp User-Defined Types (Parameter Grouping) [2026-05-14]
- 16 flat Container App params refactored into 4 Bicep user-defined types:
  - `ContainerRuntimeConfig` (image, cpuCores, memoryGi)
  - `ScalingConfig` (minReplicas, maxReplicas)
  - `IngressConfig` (enabled, targetPort, external, transportMethod)
  - `HealthProbeConfig` (readinessPath/Port, livenessPath/Port, startupPath/Port)
- All types are `@export()` from ContainerApp `types.bicep`
- `GeneratedTypeModule` extended with:
  - `ParameterTypeOverrides`: key → Bicep type name (used by `MainBicepAssembler` for `param` declarations and module-level type imports)
  - `ParameterGroupMappings`: flat domain key → (groupKey, propertyName) (used by `ParameterFileAssembler.ApplyEnvironmentOverrides` to merge flat env overrides into structured objects)
- `BicepFormattingHelper.SerializeToBicep` now handles `IDictionary<string, object>` for merged parameter groups
- Domain `ToDictionary()` remains unchanged — mapping from flat keys to structured groups is handled by `ParameterGroupMappings`
- ACR params (`acrLoginServer`, `acrManagedIdentityClientId`) remain flat (conditional, not grouped)

## Identifier & Value Normalization [2026-04-22]
- All Bicep identifiers use **camelCase** via `BicepIdentifierHelper` and `BicepFormattingHelper`.
- `BicepGenerationEngine` gracefully skips resource types without a registered generator (logs warning, does not throw).
- **Domain enum → ARM pitfall:** `EnumValueObject` member names (e.g. `V12`) may not match ARM values (e.g. `12.0`). Generators must normalize via mapping switch.

## SecureParameters Pattern [2026-04-21]
- `GeneratedTypeModule.SecureParameters` (`IReadOnlyList<string>`) holds param names requiring `@secure()` with no default value (e.g. `administratorLoginPassword` for SqlServer).
- `MainBicepAssembler`: emits `@secure()` + `param {moduleName}{Capitalize(secureParam)} string` in declarations, and passes them in module call blocks.
- `ParameterFileAssembler`: emits placeholder `param {moduleName}{Capitalize(secureParam)} = ''` per env file.
- Generators opt-in by adding to `SecureParameters` list in their `GeneratedTypeModule` return.
- **Pipeline override [2026-04-21]**: `GenerationRequest.SecureParameterOverrides` carries the full Bicep param names (e.g. `sqlServerAdministratorLoginPassword`). `PipelineGenerationEngine.BuildOverrideParameters()` appends `-{paramName} $({paramName})` to the release pipeline's `overrideParameters`. Both `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler` derive them from registered `IResourceTypeBicepGenerator` instances. User must define matching secret pipeline variables in Azure DevOps.

## Output Injection Symbol Validation [2026-04-21]
- `InjectOutputDeclarations` in `BicepGenerationEngine` now validates that the root resource symbol in a Bicep output expression actually exists in the target module before injection.
- `ExtractRootSymbol()` handles direct identifiers (`kv.properties.vaultUri` → `kv`) and interpolated strings (`'${sqlServer.properties.fqdn}'` → `sqlServer`).
- Prevents BCP057 errors when `ResourceOutputCatalog` expressions reference symbols from a different module type (e.g. `applicationInsights` expression injected into sqlDatabase module due to data integrity issues in SourceResourceName).

## ContainerAppEnvironment — Secure LAW Integration [2026-04-22]
- **BREAKING security fix:** `ContainerAppEnvironmentTypeBicepGenerator` no longer calls `listKeys()` inside the module.
- Old pattern: `destination: 'log-analytics'` + `existing` LAW resource + `listKeys().primarySharedKey` → key exposed in ARM deployment history.
- **New pattern:** `destination: 'azure-monitor'` (no key) + `Microsoft.Insights/diagnosticSettings@2021-05-01-preview` scoped to the CAE resource, with `workspaceId` = LAW resource ID passed as a plain `string` param.
- The `logAnalyticsWorkspaceId` param (full resource ID injected by `BicepGenerationEngine`) is passed directly as `diagnosticSettings.properties.workspaceId` — no `listKeys()` anywhere.
- Diagnostic category: `categoryGroup: 'allLogs'`.

## LAW Auto-Detection Fallback [2026-04-21]
- For `Microsoft.Insights/components` and `Microsoft.App/managedEnvironments`, if the property-based `logAnalyticsWorkspaceId` GUID lookup fails, the engine auto-detects the first `Microsoft.OperationalInsights/workspaces` resource in the same config as fallback.
- **Second fallback**: if no local LAW exists either, checks `ExistingResourceReferences` for a cross-config LAW and stores it in `ExistingResourceIdReferences` (resolved as `existing_{symbol}.id` in main.bicep).
- Resolves BCP035 errors where `logAnalyticsWorkspaceId` (required, no default) was not being passed from `main.bicep`.

## Cross-Config Existing Resource ID References [2026-04-21]
- `GeneratedTypeModule.ExistingResourceIdReferences` maps param names to cross-config existing resource logical names.
- `MainBicepAssembler` resolves these as `existing_{BicepIdentifier}.id` (the `.id` property of the existing resource declaration).
- Used when a parent resource (e.g. LAW) is not in the same config but referenced as an `existing` resource.

## Bicep Syntax Pitfalls [2026-04-21]
- **BCP124**: `@secure()` only works on `object` or `string` params, NOT `array`.
- **BCP247**: Lambda variables cannot index resource collections. Use `toObject(resourceCollection, kv => ...)`.
- **BCP138**: Nested for-expressions not allowed in variable declarations. Use `map()` function.

## Naming Integration [2026-03-23]

### Azure Naming Constraints Catalog [2026-04-20]
- `AzureNamingConstraints` static class in `InfrastructureConfig/Common/`: maps each `AzureResourceTypes.*` to an `AzureNamingConstraint` record (InvalidStaticCharsRegex, AllowedCharsDescription, MinLength, MaxLength, optional RecommendedTemplate).
- 19 resource types covered: ContainerRegistry (alphanumeric only, 5-50), StorageAccount (lowercase alphanumeric, 3-24), KeyVault (alphanumeric+hyphens, 3-24), SqlServer (lowercase+hyphens, 1-63), etc.
- `NamingTemplateValidator.HasValidStaticCharsForResourceType(template, resourceType)` validates template static chars against the resource type's specific constraints, falling back to the generic check for unknown types.
- Both `SetResourceNamingTemplateCommandValidator` (InfraConfig) and `SetProjectResourceNamingTemplateCommandValidator` (Project) use the resource-type-aware validation. Error messages include the specific allowed chars description.
- ContainerRegistry and StorageAccount have `RecommendedTemplate: "{name}{resourceAbbr}{envShort}"` (no separators).

### Live Name Availability Check [2026-04-22]
- Endpoint: `POST /naming/check-availability/{resourceType}` → per-env status: `available | unavailable | unknown | invalid | current`.
- **DNS-based** (`DnsNameAvailabilityChecker`): resolves `{name}.{azureSuffix}` — no Azure auth required. 10 resource types supported (CR, SA, KV, Redis, AppConfig, SBus, EventHub, WebApp, FunctionApp, SqlServer).
- `IResourceNameResolver`: template precedence (config-resource-override → project-override → config-default → project-default → literal). Validates against `AzureNamingConstraints` before DNS call; `"invalid"` short-circuits.
- `"current"` status: compares generated name with `CurrentPersistedName` — avoids false-positive "unavailable" on user's own deployed resources.
- Frontend: debounced 500 ms `switchMap` on name changes, save blocking when unavailable/invalid, bypass override button. i18n: `RESOURCE_EDIT.NAME_AVAILABILITY.*`.

## Application Pipeline Generation [2026-04-04]

Engine `AppPipelineGenerationEngine` in `PipelineGeneration` project:
- Strategy pattern: `IAppPipelineGenerator` dispatched by (ResourceType × DeploymentMode)
- 5 generators: ContainerApp(Container), WebApp(Container/Code), FunctionApp(Container/Code)
- `GenerateAll()` method: accepts list of requests + `AppPipelineMode` (Isolated/Combined)
- **Isolated mode**: per-resource files under `apps/{applicationName}/ci.app-pipeline.yml` + `release.app-pipeline.yml`
- **Combined mode**: single `ci.app-pipeline.yml` + `release.app-pipeline.yml` with parallel jobs per resource
- Shared helper: `AppPipelineYamlHelper` with reusable YAML building blocks
- **Agent pool**: configurable via `Project.AgentPoolName`. When set → `pool: name: '<value>'` (self-hosted). When null → `pool: vmImage: ubuntu-latest` (Microsoft-hosted). Central helper: `PipelineGenerationEngine.AppendPool()`. API: `PUT /projects/{id}/agent-pool`.
- Container mode: Docker@2 build+push → per-env deploy (AzureCLI for ACA, AzureWebApp@1, AzureFunctionApp@2)
- Code mode: SDK setup → restore/build/test/publish → per-env deploy via app-specific tasks
- Supports 5 runtimes: DOTNETCORE, NODE, PYTHON, JAVA + fallback
- Sequential per-env stages: Deploy_dev → Deploy_staging → Deploy_prod
- **No separate endpoint**: app pipelines generated together with infra via `POST /generate-pipeline`
- `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler` both orchestrate infra + app generation
- Each compute resource has `ApplicationName` (string?) — user-friendly name for DevOps runs (fallback: resource name)
- `NamingTemplateTranslator` converts placeholders to Bicep interpolation
- Templates: `{name}`, `{prefix}`, `{suffix}`, `{env}`, `{envShort}`, `{resourceType}`, `{resourceAbbr}`, `{location}`
- Convention: `envSuffix = '-dev'` (with hyphen), `envShortSuffix = 'dev'` (raw)

## Infra Pipeline Generation — Artifact Flow [2026-04-20]

- **CI artifact name**: stable `drop` (not version-dependent). Contains Bicep files copied from BicepBasePath.
- **Release pipeline**: declares `resources.pipelines: - pipeline: ci, source: '<configName>-CI'` to reference the CI pipeline.
- **Deploy job**: `checkout: none` + `download: ci, artifact: drop`. No Git checkout in release — all files come from artifact.
- **Template paths in release**: use `$(Pipeline.Workspace)/ci/drop/<configName>/main.bicep` (artifact-relative), NOT `$(Build.SourcesDirectory)`.
- **Artifact structure**: `Common/` (shared modules) + `<configName>/` (main.bicep, parameters/) — BicepBasePath is resolved at CI copy time, NOT present in artifact paths.
- **Removed dead params**: `buildVersion`, `buildPipeline`, `artifactsPath` no longer generated in release/deploy job YAML.
- **Deploy preflight**: generated deploy step now resolves `templateFile` / `templateParametersFile` with PowerShell before ARM deployment, prints directory contents when files are missing, and can fall back to a single legacy `main.*.bicepparam` found in the parameters folder.
- **Compiled deploy inputs**: release preflight compiles `.bicep` to `.json` and `.bicepparam` to `.parameters.json` with Azure CLI before invoking `AzureResourceManagerTemplateDeployment@3`. ARM task receives concrete JSON files instead of source Bicep inputs.
- **CI artifact validation**: shared CI template validates `Common/`, `<project>/`, and `<project>/main.bicep` exist before publishing `drop`.

## Path Sanitization [2026-04-21]
- `PathSanitizer.Sanitize()` in `GenerationCore`: spaces/underscores → dashes, strips invalid chars, collapses dashes. Applied at all generation engine entry points.
- Effect: `"My Config"` → `My-Config/` in folders, pipeline sources, and artifact paths.

## Module Reference Disambiguation [2026-04-21]
- Multiple resources can share the same `LogicalResourceName` (e.g., "ifs" for KeyVault, SqlDatabase, ContainerAppEnvironment, ApplicationInsights).
- `ParentModuleIdReferences` and `ParentModuleNameReferences` in `GeneratedTypeModule` now store `(string Name, string ResourceTypeName)` tuples (not plain strings).
- `BicepGenerationEngine` builds `resourceIdToInfo` (GUID → (Name, ResourceTypeName)) using the registered generators.
- `MainBicepAssembler` matches modules by both `LogicalResourceName` AND `ResourceTypeName` in all 6 `FirstOrDefault` lookups: ParentModuleIdRefs, ParentModuleNameRefs, output reference source, KV module, KV secrets source, KV secrets batch.
- For app settings, `SourceResourceTypeName` on `AppSettingDefinition` is used for disambiguation (null-safe fallback to name-only match).
- **Pitfall:** never assume `LogicalResourceName` is unique within a config — always match by (Name, ResourceTypeName).

## Bicepparam Type Coercion [2026-04-22]
- `ResourceDefinition.EnvironmentConfigs` stores all values as `string` (from DB persistence).
- `ParameterFileAssembler.CoerceToOriginalType()` converts env-override strings to the C# type of the generator's default parameter value (`bool`, `int`, `long`, `double`) so `SerializeToBicep()` emits correct Bicep literals.

## Storage Companion dependsOn [2026-04-21]
- Companion modules (blobs, queues, tables) emit `dependsOn: [ {moduleName}Module ]` to prevent ARM race condition.

## ContainerApp / CAE Pitfalls [2026-04-22]
- `InjectContainerAppEnvVars`: insert `env: envVars` BEFORE `resources: {` (container level); `ContainerResources` only allows `cpu`/`memory` → BCP037.
- CAE module: removed unused `sku`/`SkuName` param (plan determined by workload profiles, not a resource property).

## Bicepparam Type Coercion [2026-04-22]