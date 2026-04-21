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

## Bicepparam File Naming Convention [2026-04-17]
- **File name** uses `EnvironmentDefinition.ShortName` (e.g. `main.dev.bicepparam`) to match the pipeline convention where `${{ environment }}` resolves to ShortName.
- **Internal `param environmentName`** uses the full `EnvironmentDefinition.Name` sanitized (e.g. `param environmentName = 'development'`) for Bicep `EnvironmentName` union type matching.
- Previous bug: file was named using full Name (`main.development.bicepparam`) causing pipeline `Could not find any file matching the template file pattern` error.

## Bicep Param Relative Path [2026-04-04]
- Generated `.bicepparam` files are stored under `parameters/`, so they must declare `using '../main.bicep'`.
- `using 'main.bicep'` is invalid from that folder and breaks Azure DevOps ARM deployments after the task compiles `main.bicep`, typically surfacing as `Could not find any file matching the template file pattern`.

## Module Folder Structure [2026-03-24]
Each module folder contains: `{moduleType}.module.bicep` + `types.bicep` (with `@export()` union types).

## Companion Modules
`GeneratedTypeModule.CompanionModules` supports multiple companions (e.g., StorageAccount: blobs, queues, tables).

## Role Assignment Generation [2026-03-25]
- `RbacRoleType` in `types.bicep`, `constants.bicep` with used roles, per-target RBAC modules
- Identity injection: uniform vs mixed mode (parameterized `ManagedIdentityType`)

## ContainerApp Bicep Params [2026-04-03]
All typed per-env parameters (cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, etc.) **must** be added to the generator's `Parameters` dictionary — not just ACR params. Missing entries cause env-specific values to be silently ignored in `.bicepparam` files.

## Identifier Normalization [2026-04-16]

- All Bicep identifiers use **camelCase** via `BicepIdentifierHelper` and `BicepFormattingHelper`.
- `BicepGenerationEngine` gracefully skips resource types without a registered `IResourceTypeBicepGenerator` (logs warning, does not throw).

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

## LAW Auto-Detection Fallback [2026-04-21]
- For `Microsoft.Insights/components` and `Microsoft.App/managedEnvironments`, if the property-based `logAnalyticsWorkspaceId` GUID lookup fails, the engine auto-detects the first `Microsoft.OperationalInsights/workspaces` resource in the same config as fallback.
- **Second fallback**: if no local LAW exists either, checks `ExistingResourceReferences` for a cross-config LAW and stores it in `ExistingResourceIdReferences` (resolved as `existing_{symbol}.id` in main.bicep).
- Resolves BCP035 errors where `logAnalyticsWorkspaceId` (required, no default) was not being passed from `main.bicep`.

## Cross-Config Existing Resource ID References [2026-04-21]
- `GeneratedTypeModule.ExistingResourceIdReferences` maps param names to cross-config existing resource logical names.
- `MainBicepAssembler` resolves these as `existing_{BicepIdentifier}.id` (the `.id` property of the existing resource declaration).
- Used when a parent resource (e.g. LAW) is not in the same config but referenced as an `existing` resource.

## Bicep Syntax Pitfalls [2026-04-21]
- **BCP124**: `@secure()` decorator only works on `object` or `string` params, NOT on `array`.
- **BCP247**: Lambda variables (e.g. `i` from `range()`) cannot index resource collections. Use `toObject(resourceCollection, kv => ...)` instead.
- **BCP138**: Nested for-expressions not allowed in variable declarations. Use `map()` function instead.
- **BCP057**: Output expressions with resource symbols injected into wrong modules. Validate symbol existence before injection.

## Naming Integration [2026-03-23]

### Azure Naming Constraints Catalog [2026-04-20]
- `AzureNamingConstraints` static class in `InfrastructureConfig/Common/`: maps each `AzureResourceTypes.*` to an `AzureNamingConstraint` record (InvalidStaticCharsRegex, AllowedCharsDescription, MinLength, MaxLength, optional RecommendedTemplate).
- 19 resource types covered: ContainerRegistry (alphanumeric only, 5-50), StorageAccount (lowercase alphanumeric, 3-24), KeyVault (alphanumeric+hyphens, 3-24), SqlServer (lowercase+hyphens, 1-63), etc.
- `NamingTemplateValidator.HasValidStaticCharsForResourceType(template, resourceType)` validates template static chars against the resource type's specific constraints, falling back to the generic check for unknown types.
- Both `SetResourceNamingTemplateCommandValidator` (InfraConfig) and `SetProjectResourceNamingTemplateCommandValidator` (Project) use the resource-type-aware validation. Error messages include the specific allowed chars description.
- ContainerRegistry and StorageAccount have `RecommendedTemplate: "{name}{resourceAbbr}{envShort}"` (no separators).

### Live Name Availability Check [2026-04-20]
- Endpoint: `POST /naming/check-availability/{resourceType}` with body `{ projectId, configId?, name }` → response `{ resourceType, rawName, supported, environments[] }` where each env item has `{ environmentName, environmentShortName, subscriptionId, generatedName, appliedTemplate, status, reason?, message? }`. Status union: `available | unavailable | unknown | invalid`.
- `IResourceNameResolver` (`Application/Common/Services/ResourceNameResolver.cs`): applies template precedence (config-resource-override > project-resource-override > config-default > project-default > literal `{name}`); config templates only used when `configId` is provided AND `UseProjectNamingConventions == false`. Substitutes placeholders with Regex (IgnoreCase + Compiled, 250 ms timeout).
- `IAzureNameAvailabilityChecker` (`Infrastructure/Services/AzureNameAvailability/`): typed `HttpClient` + `DefaultAzureCredential` (scope `https://management.azure.com/.default`). Strategy dictionary keyed by resource type — currently only `ContainerRegistry` (api-version `2023-07-01`, ARM type `Microsoft.ContainerRegistry/registries`). Returns `Unknown` (does NOT throw) on auth/network/throttling failure.
- Handler `CheckResourceNameAvailabilityQueryHandler`: verifies project read access, resolves names per env, validates each generated name against `AzureNamingConstraints.GetConstraint(resourceType)` (length + InvalidStaticCharsRegex) BEFORE the ARM call. Status `"invalid"` short-circuits ARM (no HTTP call).
- DI: `services.AddScoped<IResourceNameResolver, ResourceNameResolver>()` (Application) + `services.AddHttpClient<IAzureNameAvailabilityChecker, AzureNameAvailabilityChecker>()` (Infrastructure).
- Frontend: `NameAvailabilityService.check$()` + `resource-edit.component` debounced (500 ms) `switchMap` on `name` valueChanges, gated by `isNameAvailabilityCheckEnabled` (currently `resourceType === 'ContainerRegistry'`). Per-env panel reuses `.acr-inline-status` visual baseline + new `.name-availability-status` / `.name-availability-list` SCSS classes.
- i18n keys: `RESOURCE_EDIT.NAME_AVAILABILITY.{CHECKING,ALL_AVAILABLE,SOME_UNAVAILABLE,SOME_INVALID,UNKNOWN}` (en + fr).
- To add a new resource type: extend the strategy dictionary in `AzureNameAvailabilityChecker` AND broaden the frontend `isNameAvailabilityCheckEnabled` computed.

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
- **CI artifact validation**: shared CI template validates that `Common/`, `<project>/`, and `<project>/main.bicep` exist in `$(Build.ArtifactStagingDirectory)` before publishing artifact `drop`.
- **Legacy params autofix**: if a legacy artifact still contains `main.development.bicepparam` with `using 'main.bicep'` inside the `parameters/` folder, the generated release preflight rewrites that line to `using '../main.bicep'` in a temporary `.autofix.bicepparam` before running `az bicep build-params`.

## Path Sanitization [2026-04-21]

- `PathSanitizer.Sanitize()` in `GenerationCore`: replaces spaces/underscores with dashes, strips invalid path chars (`[^\w\-.]`), collapses consecutive dashes, trims leading/trailing dashes.
- Applied at engine entry points: `PipelineGenerationEngine.Generate()`, `PipelineGenerationEngine.GenerateSharedTemplates()`, `AppPipelineGenerationEngine.Generate()`, `MonoRepoBicepAssembler.Assemble()`, `MonoRepoPipelineAssembler.Assemble()`, `AppPipelineYamlHelper.AppendCiHeader()`.
- Also applied in `GenerateProjectBicepCommandHandler` when building config request keys.
- Effect: a config named `"My Config"` → folder `My-Config/`, pipeline source `My-Config-CI`, artifact paths `My-Config/main.bicep`.

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
- `StorageAccountCompanionHelper.AppendStorageAccountCompanionModule()` now emits `dependsOn: [ {moduleName}Module ]` on every companion module (blobs, queues, tables).
- Prevents ARM deployment race condition where the companion module (e.g. blob containers) deploys before the parent Storage Account exists, causing `Microsoft.Storage/storageAccounts/{name} not found`.
- **Pitfall:** without coercion, `'false'` / `'0'` appear as quoted strings in `.bicepparam` → BCP033 type mismatch errors.

## ContainerApp Env Vars Injection [2026-04-22]
- `InjectContainerAppEnvVars` inserts `env: envVars` BEFORE `resources: {` (at container level), NOT after `memory:` (which is inside `ContainerResources`).
- `ContainerResources` type only allows `cpu` and `memory` — placing `env` there triggers BCP037.

## ContainerAppEnvironment Module [2026-04-22]
- Removed unused `sku` / `SkuName` parameter from module template (not part of ARM resource properties; plan determined by workload profiles).

## Domain Enum → ARM Value Normalization [2026-04-22]
- **Pitfall:** Domain `EnumValueObject` member names (e.g. `V12`) may not match ARM-accepted values (e.g. `12.0`). Generators must normalize via mapping switch.
- `SqlServerTypeBicepGenerator.NormalizeSqlServerVersion()`: `V12` → `12.0`.
