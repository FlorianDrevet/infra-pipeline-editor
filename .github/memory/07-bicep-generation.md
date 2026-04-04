# Bicep Generation

## Architecture
- Pure engine in `InfraFlowSculptor.BicepGeneration` (no domain dependency)
- Strategy pattern: `IResourceTypeBicepGenerator` per resource type
- Registered as singletons in `Application/DependencyInjection.cs`
- `AzureResourceTypes` static class in `GenerationCore`: centralized constants for all 18 resource type identifiers (+ ResourceGroup constant). **Never use magic strings.**

## Output Files
- `types.bicep` — `EnvironmentName` union, `EnvironmentVariables` type, `environments` map
- `functions.bicep` — naming functions from project naming templates
- `main.bicep` — resource declarations, module imports
- `main.{env}.bicepparam` — per-environment parameter files
- `modules/{FolderName}/{moduleType}.module.bicep` + `types.bicep` per resource type
- `constants.bicep` — RBAC role constants (only when role assignments exist)

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

## Application Pipeline Generation [2026-04-03]

Separate engine `AppPipelineGenerationEngine` in `PipelineGeneration` project:
- Strategy pattern: `IAppPipelineGenerator` dispatched by (ResourceType × DeploymentMode)
- 5 generators: ContainerApp(Container), WebApp(Container/Code), FunctionApp(Container/Code)
- Each produces 2 files: `{resourceName}/ci.app-pipeline.yml` + `{resourceName}/release.app-pipeline.yml`
- Shared helper: `AppPipelineYamlHelper` with reusable YAML building blocks
- Container mode: Docker@2 build+push → per-env deploy (AzureCLI for ACA, AzureWebApp@1, AzureFunctionApp@2)
- Code mode: SDK setup → restore/build/test/publish → per-env deploy via app-specific tasks
- Supports 5 runtimes: DOTNETCORE, NODE, PYTHON, JAVA + fallback
- Sequential per-env stages: Deploy_dev → Deploy_staging → Deploy_prod
- Endpoint: `POST /generate-pipeline/app` body `{ infrastructureConfigId, resourceId }`
- `NamingTemplateTranslator` converts placeholders to Bicep interpolation
- Templates: `{name}`, `{prefix}`, `{suffix}`, `{env}`, `{envShort}`, `{resourceType}`, `{resourceAbbr}`, `{location}`
- Convention: `envSuffix = '-dev'` (with hyphen), `envShortSuffix = 'dev'` (raw)
