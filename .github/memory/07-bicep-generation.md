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

## Naming Integration [2026-03-23]

### Azure Naming Constraints Catalog [2026-04-20]
- `AzureNamingConstraints` static class in `InfrastructureConfig/Common/`: maps each `AzureResourceTypes.*` to an `AzureNamingConstraint` record (InvalidStaticCharsRegex, AllowedCharsDescription, MinLength, MaxLength, optional RecommendedTemplate).
- 19 resource types covered: ContainerRegistry (alphanumeric only, 5-50), StorageAccount (lowercase alphanumeric, 3-24), KeyVault (alphanumeric+hyphens, 3-24), SqlServer (lowercase+hyphens, 1-63), etc.
- `NamingTemplateValidator.HasValidStaticCharsForResourceType(template, resourceType)` validates template static chars against the resource type's specific constraints, falling back to the generic check for unknown types.
- Both `SetResourceNamingTemplateCommandValidator` (InfraConfig) and `SetProjectResourceNamingTemplateCommandValidator` (Project) use the resource-type-aware validation. Error messages include the specific allowed chars description.
- ContainerRegistry and StorageAccount have `RecommendedTemplate: "{name}{resourceAbbr}{envShort}"` (no separators).

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
