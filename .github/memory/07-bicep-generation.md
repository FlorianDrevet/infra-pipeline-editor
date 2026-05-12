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
`tests/InfraFlowSculptor.BicepGeneration.Tests/` is the active xUnit project for TextManipulation, pipeline-stage, IR emitter/builder/transformer, and generator coverage across the Bicep slice. Convention: `Given_When_Then`, AAA, `_sut`.
- **Sonar generator cleanup [2026-04-28]:** `ContainerAppTypeBicepGenerator`, `RedisCacheTypeBicepGenerator`, and `WebAppTypeBicepGenerator` now use targeted semantic constants for repeated module/type/parameter identifiers to reduce S1192 noise without turning the Bicep DSL into generic constant wrappers. Focused generator tests lock the touched identifiers/variant names.
- **Latest generator cleanup [2026-05-11]:** the typed legacy parameter-model migration was revalidated together with a full constant sweep across all 18 `*TypeBicepGenerator` implementations. The strict rule is now: in generator builder code, inline semantic Bicep literals are forbidden for module names, ARM types, import/type names, parameter names, resource symbols, property/output names, union literals, and repeated raw expressions. Keep generator-specific literals local, and use `Generators/Constants/BicepGeneratorSharedConstants.cs` only for literals with the same stable meaning across multiple generators.
- **Role-assignment placement regression [2026-05-11]:** `BicepAssemblerTests` now locks the Container Registry RBAC module path to `modules/ContainerRegistry/containerregistry.roleassignments.module.bicep` so ACR role-assignment modules cannot silently drift into another resource folder during project-level generation.

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
- **Cross-platform text output [2026-04-30]:** generated artifacts consumed by tests must canonicalize line endings to LF before returning strings. `BicepEmitter` and `ConfigVarsStage` now normalize `AppendLine()` output with `ReplaceLineEndings("\n")` to avoid Windows-only CRLF regressions in exact/golden assertions.

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
- `AzureResourceTypes.ArmTypes` constants now use an explicit `*Type` suffix (`WebAppType`, `ContainerRegistryType`, etc.) so the ARM-type catalog no longer shadows the outer friendly-name constants (`WebApp`, `ContainerRegistry`, etc.). New code must use the suffixed members.

## BicepAssembler [2026-04-04]
Thin orchestrator (~180 LOC) + 14 specialized classes: 7 assemblers (`Types`, `Functions`, `Constants`, `MainBicep`, `ParameterFile`, `KvSecrets`, `RoleAssignment`), 4 helpers (`Formatting`, `ResourceTypeMetadata`, `ModuleHeader`, `Naming`), `StorageAccountCompanionHelper`, 2 model types. `MainBicepAssembler.Generate` returns `MainBicepEmissionResult` with `OutputUsageTracker`.
- **Key Vault secret-name guard [2026-05-11]:** app-setting / App Configuration Key Vault secret names must follow Azure's `1..127` alphanumeric-or-hyphen rule. Names like `JWT_SECRET` are invalid because `_` is forbidden. `AddAppSettingCommandValidator` and `AddAppConfigurationKeyCommandValidator` now reject them on write, and `MainBicepAssembler.Generate` throws before emitting a broken `kvSecrets.module.bicep` for legacy snapshots that still contain an invalid secret name.
- **Key Vault output property access [2026-05-11]:** Azure-valid secret names can still be invalid as Bicep dot-property identifiers. When `MainBicepAssembler` references `kvSecrets` module `secretUris`, it must use `BicepFormattingHelper.FormatBicepPropertyAccess(...)` so names like `jwt-secret` become `outputs.secretUris['jwt-secret']` instead of the invalid `outputs.secretUris.jwt-secret`.
- **RBAC role key formatting + used type imports [2026-05-11]:** `ConstantsBicepAssembler` must emit role names through `FormatBicepObjectKey(...)`, and `MainBicepAssembler` must reference them through `FormatBicepPropertyAccess(...)` so valid identifiers such as `AcrPull` stay unquoted while display names like `Key Vault Secrets User` keep bracket access. `MainBicepAssembler` must also import module custom types only for parameters that are actually declared in `main.bicep`; copying every `ParameterTypeOverride` from IR causes `no-unused-imports` warnings for spec-only defaults such as `WorkloadProfileType` and `IngestionMode`.
- **`kvSecrets.module.bicep` secret URI output [2026-05-11]:** the `secretUris` output must not iterate the deployed `kvSecrets` resource collection and read `kv.name` / `kv.properties.secretUri`. ARM evaluates that lambda over symbolic deployment metadata, which triggers `DeploymentOutputEvaluationFailed` because `name` is unavailable there. The safe pattern is to derive the object from the input `secrets` parameter and `keyVault.properties.vaultUri`, e.g. `toObject(secrets, secret => secret.name, secret => '${keyVault.properties.vaultUri}secrets/${secret.name}')`.
- **Container Registry RBAC IDs + role module symbols [2026-05-11]:** `AzureRoleDefinitionCatalog.AcrPull` must use the official built-in role ID `7f951dda-4ed3-4680-a7ca-43fe172d538d`, and the container registry catalog entry for `AcrPush` must use `8311e382-0749-4cb8-b61a-304f252e45ec`. `ResourceTypeMetadata.GetBaseModuleName(...)` must also cover every RBAC target already supported by `RoleAssignmentModuleTemplates` including `ContainerRegistry`, `ServiceBusNamespace`, and `EventHubNamespace`, otherwise generated role-assignment module symbols fall back to `unknown...Roles` in `main.bicep`.

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

- **Typed legacy parameter models [2026-05-11]:** fixed-schema `Generate(...)` parameter payloads should no longer be authored inline as `Dictionary<string, object>` in generator code. The legacy path now uses typed records under `Generators/ParameterModels/` plus `BicepParameterModelConverter` (System.Text.Json with `JsonPropertyName` + null omission) to adapt back to `GeneratedTypeModule.Parameters`. `BicepFormattingHelper` and `ParameterFileAssembler` honor `JsonPropertyName` when serializing or merging typed objects so environment overrides preserve the external Bicep field names.
- **Serialized Bicep object keys [2026-05-11]:** when `BicepFormattingHelper` emits object members from `JsonPropertyName` metadata or `Dictionary<string, object>` keys, it must route the key through `FormatBicepObjectKey(...)`. Annotated names are part of the public Bicep shape, but they are not guaranteed to be valid bare identifiers.

- **Generator identifier constants [2026-05-11]:** when a generator repeats Bicep parameter names, variable names, resource symbols, property/output names, union literals, or raw expressions across `GenerateSpec(...)` and `Generate(...)`, extract them as constants. This applies strictly even to common builder keys such as `name`, `location`, `kind`, `properties`, and `linuxFxVersion` when they appear in generator code. Default to file-local `private const`; promote only the high-frequency literals with an identical cross-generator contract to `Generators/Constants/BicepGeneratorSharedConstants.cs`.

- **Shared generator constants [2026-05-11]:** `Generators/Constants/BicepGeneratorSharedConstants.cs` is the only shared constants point for concrete generators today. It currently centralizes `TypesImportPath`, `NameParameterName`, `LocationParameterName`, `NamePropertyName`, `LocationPropertyName`, `PropertiesPropertyName`, `KindPropertyName`, `IdOutputName`, `BooleanTrueString`, and `BooleanFalseString`. Do not expand it with generator-specific values such as module names, ARM types, output expressions, or exported unions.

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
