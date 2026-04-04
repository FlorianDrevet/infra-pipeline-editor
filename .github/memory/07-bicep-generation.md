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
- `main.{env}.bicepparam` — per-environment parameter files
- `modules/{FolderName}/{moduleType}.module.bicep` + `types.bicep` per resource type
- `constants.bicep` — RBAC role constants (only when role assignments exist)

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

## Naming Integration [2026-03-23]

## Application Pipeline Generation [2026-04-04]

Engine `AppPipelineGenerationEngine` in `PipelineGeneration` project:
- Strategy pattern: `IAppPipelineGenerator` dispatched by (ResourceType × DeploymentMode)
- 5 generators: ContainerApp(Container), WebApp(Container/Code), FunctionApp(Container/Code)
- `GenerateAll()` method: accepts list of requests + `AppPipelineMode` (Isolated/Combined)
- **Isolated mode**: per-resource files under `apps/{applicationName}/ci.app-pipeline.yml` + `release.app-pipeline.yml`
- **Combined mode**: single `ci.app-pipeline.yml` + `release.app-pipeline.yml` with parallel jobs per resource
- Shared helper: `AppPipelineYamlHelper` with reusable YAML building blocks
- **Agent pool**: configurable via `InfrastructureConfig.AgentPoolName`. When set → `pool: name: '<value>'` (self-hosted). When null → `pool: vmImage: ubuntu-latest` (Microsoft-hosted). Central helper: `PipelineGenerationEngine.AppendPool()`. API: `PUT /infra-configs/{id}/agent-pool`.
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
