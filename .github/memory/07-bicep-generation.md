# Bicep Generation

## Onboarding [2026-04-27]
`docs/architecture/bicep-generation.md` is the full onboarding guide: end-to-end flow, staged pipeline, Builder + IR, assemblers, mono-repo, reading path.

## Pipeline Architecture (Vague 1) [2026-04-27]
Legacy 920-line `BicepGenerationEngine` → thin facade (~85 LOC) + `BicepGenerationPipeline` over 9 ordered `IBicepGenerationStage` instances. Public surface preserved (handlers untouched).

### Stages (under `Pipeline/Stages/`)
| Order | Stage | Role |
|-------|-------|------|
| 100 | `IdentityAnalysisStage` | System/user identity sets + mixed-ARM-types |
| 200 | `AppSettingsAnalysisStage` | Outputs to inject + compute ARM types needing app-settings |
| 300 | `ModuleBuildStage` | Calls generator, builds `ModuleWorkItem` (IR or legacy) |
| 400 | `IdentityInjectionStage` | System/user/parameterized identity blocks |
| 500 | `OutputInjectionStage` | Output declarations for app settings |
| 600 | `AppSettingsInjectionStage` | `appSettings`/`envVars` param injection |
| 700 | `TagsInjectionStage` | `param tags object = {}` + `tags: tags` |
| 800 | `ParentReferenceResolutionStage` | Resolves parent FKs (ASP, CAE, LAW, SQL) with cross-config fallback |
| 850 | `SpecEmissionStage` | IR→text via `LegacyTextModuleAdapter` (Vague 2) |
| 900 | `AssemblyStage` | Delegates to `BicepAssembler.Assemble` |

- `BicepGenerationContext` = mutable per-generation state. `ModuleWorkItem` holds resource + `GeneratedTypeModule` + optional `BicepModuleSpec`.
- `TextManipulation/` = 6 pure static helpers (`Helpers`, `IdentityInjector`, `OutputInjector`, `AppSettingsInjector`, `TagsInjector`, `OutputPruner`) — byte-for-byte parity with legacy.
- DI: 18 generator singletons + 10 stage singletons + pipeline + engine in `Application/DependencyInjection.cs`. Stage order by `Order`, not DI order.
- `AzureResourceTypes.ComputeArmTypes` replaces magic ARM strings.

### Tests
`tests/InfraFlowSculptor.BicepGeneration.Tests/`: 842+ tests (xUnit + FluentAssertions + NSubstitute). Covers TextManipulation, Pipeline stages, IR emitter/builder/transformers, and all 18 migrated generators. Convention: `Given_When_Then`, AAA, `_sut`.

### Constraints
- **Never** call `BicepOutputPruner` from a stage — pruning is engine-owned (mono-repo cross-config).
- Mutation stages must use `item.Module = item.Module with { ... }` to avoid losing earlier stage data.
- `TextManipulation/` helpers must be `internal`/`public static` and pure (no DI).
- `BicepTagsInjector` regex requires `\n` before first `param` in test modules.

## Vague 2 — Builder + IR [2026-04-27]
**All 18 generators migrated** from legacy `const string` → typed `BicepModuleSpec` via `BicepModuleBuilder`. Pipeline is dual-mode (backward-compatible). Phase 6 removed legacy dual-mode branches; `ModuleWorkItem.Spec` is now required.

### IR layer (`Ir/`)
- **Model:** 9 record files (`BicepType`, `BicepExpression`, `BicepParam`, `BicepOutput`, `BicepImport`, `BicepVar`, `BicepTypeDefinition`, `BicepResourceDeclaration`, `BicepModuleSpec`)
- **Builder:** `BicepModuleBuilder` (fluent: `.Module().Resource().Param().Property().Build()`), `BicepObjectBuilder`
- **Emitter:** `BicepEmitter.EmitModule/EmitTypes` — handles decorators, nested indentation, LF endings
- **Transformers:** `IdentityTransformer`, `OutputTransformer`, `TagsTransformer`, `AppSettingsTransformer` (extension methods, immutable)
- **Adapter:** `LegacyTextModuleAdapter` bridges IR to legacy `GeneratedTypeModule`
- **Interface:** `IResourceTypeBicepSpecGenerator` extends `IResourceTypeBicepGenerator`, adds `GenerateSpec()`
- **Output pruning [2026-04-27]:** `IrOutputPruningStage` (Order 950) + `IrMonoRepoOutputPruner` replace regex-based pruner. `OutputUsageTracker` (emit-time tracking in `MainBicepAssembler`) feeds consumed outputs — 0 regex.
- **Key decisions:** identity in `Body` as property, `IReadOnlyList<T>`, collection expressions `[..existing, new]`, LF line endings

## Bootstrap Split (SplitInfraCode) [2026-04-25]
- `BootstrapMode` enum: `FullOwner` (infra: 3 jobs) vs `ApplicationOnly` (code: validate + provision pipelines).
- `ArtifactKind.BootstrapApplication` routes to `ApplicationCode` repo.
- Handler produces `infra/` + `app/` blob buckets; response has `InfraFileUris` + `AppFileUris` + backward-compat `FileUris`.
- Code push scope = app pipeline + app bootstrap.

## Mono-Repo Layout [2026-04-24]
- Project-level generation keeps `Common/` for `AllInOne` and `SplitInfraCode` (`main.bicep` imports `../Common/...`).
- Per-config `MultiRepo` generation stays flat (no `Common/`).

## Multi-Repo Git Routing [2026-04-23]
- Engines are **repo-agnostic** (produce `IReadOnlyDictionary<string,string>`). Routing is in Application handlers via `IRepositoryTargetResolver`.
- `ArtifactKind` enum selects path fields. `AppPipelineFileClassifier` routes `apps/` + frozen shared-template set to `ApplicationCode`.
- `GenerateProjectPipelineCommandHandler` returns 6 result fields (legacy union + split infra/app).

## Architecture
- Pure engine in `InfraFlowSculptor.BicepGeneration` (no domain dependency)
- `IResourceTypeBicepGenerator` per resource type, singletons in DI
- `AzureResourceTypes` in `GenerationCore`: **never use magic strings**

## BicepAssembler [2026-04-04]
Thin orchestrator (~180 LOC) + 14 specialized classes: 7 assemblers (`Types`, `Functions`, `Constants`, `MainBicep`, `ParameterFile`, `KvSecrets`, `RoleAssignment`), 4 helpers (`Formatting`, `ResourceTypeMetadata`, `ModuleHeader`, `Naming`), `StorageAccountCompanionHelper`, 2 model types. `MainBicepAssembler.Generate` returns `MainBicepEmissionResult` with `OutputUsageTracker`.

## Output Files
- `types.bicep`, `functions.bicep`, `main.bicep`, `constants.bicep` (RBAC only)
- `main.{shortName}.bicepparam` per environment (under `parameters/`, `using '../main.bicep'`)
- `modules/{Folder}/{type}.module.bicep` + `types.bicep` per resource

## Module Conventions
- Each module folder: `{type}.module.bicep` + `types.bicep` with exported types
- `CompanionModules` for sub-resources (StorageAccount blobs/queues/tables)
- Role assignments: `RbacRoleType`, `constants.bicep`, uniform/mixed identity injection
- Generic UAI param `userAssignedIdentityId` (AVM-style, project-agnostic)

## ContainerApp Bicep Params [2026-04-03]
All typed per-env parameters **must** be in the generator's `Parameters` dictionary — missing entries cause silent `.bicepparam` omissions.

## Generator-Specific Patterns

- **ContainerApp health probes [2026-04-22]:** 6 per-env fields (readiness/liveness/startup path+port). Bicep uses `union()` for conditional probe array. HTTP only, path must start `/`, port 1-65535.
- **ContainerApp param grouping [2026-04-22]:** 16 flat params → 4 exported types (`ContainerRuntimeConfig`, `ScalingConfig`, `IngressConfig`, `HealthProbeConfig`). `ParameterGroupMappings` + `ParameterTypeOverrides` on `GeneratedTypeModule`. ACR params stay flat (conditional).
- **Custom domains [2026-04-23]:** `ResourceDefinition.CustomDomains` → `ParameterFileAssembler` emits `customDomains` arrays. Compute resources only.
- **Secure parameters [2026-04-21]:** `GeneratedTypeModule.SecureParameters` → `@secure()` params with no default. `MainBicepAssembler` must emit `dependsOn` on `kvSecrets.module.bicep` for locally created Key Vaults (ARM race condition).
- **ACR auth modes [2026-04-23]:** `ManagedIdentity` (default/null) vs `AdminCredentials`. Admin mode uses `SecureParameters = ["acrPassword"]`. ContainerApp admin emits `configuration.secrets` + `registries`; WebApp/FunctionApp emit `DOCKER_REGISTRY_SERVER_*` app settings.
- **Identifiers [2026-04-22]:** camelCase via `BicepIdentifierHelper`. `EnumValueObject` names may not match ARM values — normalize in generator switch.
- **SecureParameterOverrideHelper [2026-04-23]:** merges `SecureParameterMappings` into `PipelineVariableGroupDefinition.Mappings`; only auto-derives overrides for unmapped secure params.
- **CAE secure LAW [2026-04-22]:** `destination: 'azure-monitor'` + `diagnosticSettings` resource (no `listKeys()`). `workspaceId` as plain string param.

## Module Reuse & Disambiguation

- **AVM-style generic params [2026-04-26]:** Single `param userAssignedIdentityId string` instead of per-UAI names. `envVars`/`appSettings` injected into all instances of a type when any instance has app settings. `NormalizePrimaryModuleFileNames` collapses to shared `*.module.bicep`.
- **Type import collisions [2026-04-27]:** `MainBicepAssembler` aliases colliding exported type names (`SkuName`, `TlsVersion`) as `<FolderName><TypeName>`. Same alias reused in param declarations.
- **Variant files:** Incompatible parameter surfaces → distinct `ModuleFileName`. Content-based disambiguation across configs in mono-repo. Merged `types.bicep` per folder (not first/last-wins).
- **Mono-repo constants:** Rebuild `Common/constants.bicep` from union of all configs' role assignments, not single config's payload.
- **Role grouping [2026-04-23]:** `RoleRef` carries own `ServiceCategory`. Grouping key includes target type, target RG, and cross-config flag.
- **Identity injection [2026-04-23]:** Must detect only resource-root `identity:` block, not nested properties (e.g. Container App ACR registry `identity:`).
- **Output validation [2026-04-21]:** `ExtractRootSymbol()` validates root symbol exists before injection (prevents BCP057).
