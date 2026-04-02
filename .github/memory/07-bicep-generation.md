# Bicep Generation

## Architecture
- Pure engine in `InfraFlowSculptor.BicepGeneration` (no domain dependency)
- Strategy pattern: `IResourceTypeBicepGenerator` per resource type
- Registered as singletons in `Application/DependencyInjection.cs`
- `AzureResourceTypes` static class in `GenerationCore`: centralized constants for all 17 resource type identifiers. **Never use magic strings.**

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

## Naming Integration [2026-03-23]
- `NamingTemplateTranslator` converts placeholders to Bicep interpolation
- Templates: `{name}`, `{prefix}`, `{suffix}`, `{env}`, `{envShort}`, `{resourceType}`, `{resourceAbbr}`, `{location}`
- Convention: `envSuffix = '-dev'` (with hyphen), `envShortSuffix = 'dev'` (raw)
