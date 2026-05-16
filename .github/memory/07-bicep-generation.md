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
- **Cancellation propagation [2026-05-13]:** `BicepGenerationEngine.Generate(...)` and `GenerateMonoRepo(...)` now accept an optional `CancellationToken`, store it on `BicepGenerationContext`, and rely on `BicepGenerationPipeline` plus `ModuleBuildStage` to check cancellation between stages and per-resource generator invocations. Keep `IBicepGenerationStage` and generator interfaces unchanged unless a future stage truly needs finer-grained cancellation.
- `TextManipulation/` = 6 pure static helpers (`Helpers`, `IdentityInjector`, `OutputInjector`, `AppSettingsInjector`, `TagsInjector`, `OutputPruner`) — byte-for-byte parity with legacy.
- DI: 18 generator singletons + 10 stage singletons + pipeline + engine in `Application/DependencyInjection.cs`. Stage order by `Order`, not DI order.
- `AzureResourceTypes.ComputeArmTypes` replaces magic ARM strings.

### Tests
`tests/InfraFlowSculptor.BicepGeneration.Tests/` is the active xUnit project for TextManipulation, pipeline-stage, IR emitter/builder/transformer, and generator coverage across the Bicep slice. Convention: `Given_When_Then`, AAA, `_sut`.
- **Networking generator coverage [2026-05-15]:** the active generator set now includes `VirtualNetworkTypeBicepGenerator`, `NetworkSecurityGroupTypeBicepGenerator`, `PrivateDnsZoneTypeBicepGenerator`, `FrontDoorTypeBicepGenerator`, and `PrivateEndpointTypeBicepGenerator` for the privatization/networking slice. `PrivateEndpointTypeBicepGenerator` is the companion-module path for Azure-resource private endpoint emission.
- **Sonar generator cleanup [2026-04-28]:** `ContainerAppTypeBicepGenerator`, `RedisCacheTypeBicepGenerator`, and `WebAppTypeBicepGenerator` now use targeted semantic constants for repeated module/type/parameter identifiers to reduce S1192 noise without turning the Bicep DSL into generic constant wrappers. Focused generator tests lock the touched identifiers/variant names.
- **Latest generator cleanup [2026-05-11]:** the typed legacy parameter-model migration was revalidated together with a full constant sweep across all 18 `*TypeBicepGenerator` implementations. The strict rule is now: in generator builder code, inline semantic Bicep literals are forbidden for module names, ARM types, import/type names, parameter names, resource symbols, property/output names, union literals, and repeated raw expressions. Keep generator-specific literals local, and use `Generators/Constants/BicepGeneratorSharedConstants.cs` only for literals with the same stable meaning across multiple generators.
- **Messaging namespace shared helper [2026-05-12]:** `EventHubNamespaceTypeBicepGenerator` and `ServiceBusNamespaceTypeBicepGenerator` now share `Generators/Helpers/MessagingNamespaceBicepGeneratorHelper` for the common `SkuName` / `TlsVersion` import names, default values, union literals, exported-type registration, and `types.bicep` template emission. Reuse that helper for cross-generator messaging namespace type metadata instead of cloning the same unions/templates into each generator.
- **StorageAccount CORS parsing helper [2026-05-12]:** `StorageAccountTypeBicepGenerator` now shares blob/table CORS JSON parsing through one private helper. Keep the distinction between `corsRules` and `tableCorsRules` at the property-name boundary only, and lock the mapping with a focused generator test that inspects the `Blobs` and `Tables` companions.
- **Catalog/output parity for backend connection strings [2026-05-12]:** `StorageAccountTypeBicepGenerator` and `SqlServerTypeBicepGenerator` must publish the `connectionString` output because `ResourceOutputCatalog` exposes that output to app-setting consumers and `ContainerApp` generation will emit `main.bicep` references like `*.outputs.connectionString`. Keep the catalog entries, generator outputs, and focused generator tests in sync or project-level generation can fail with `BCP052` during `main.bicep` linting.
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
- **ASCII-only generated section headers [2026-05-16]:** keep decorative section comments emitted into `main.bicep` ASCII-only. The `MainBicepDeploymentSectionAssembler` headers for cross-configuration existing resources and Key Vault secret batches must avoid Unicode box-drawing characters, otherwise generated files can show mojibake such as `â”€` in downstream editors/snapshots.

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
- `AzureResourceTypes.ArmTypeToFriendlyName` now stays behind a private `FrozenDictionary<string, string>` backing field exposed as `IReadOnlyDictionary<string, string>`. Keep that shape when extending the ARM-type catalog instead of reintroducing public mutable-looking fields plus `SuppressMessage` annotations [2026-05-13].

## BicepAssembler [2026-04-04]
Thin orchestrator (~180 LOC) + 14 specialized classes: 7 assemblers (`Types`, `Functions`, `Constants`, `MainBicep`, `ParameterFile`, `KvSecrets`, `RoleAssignment`), 4 helpers (`Formatting`, `ResourceTypeMetadata`, `ModuleHeader`, `Naming`), `StorageAccountCompanionHelper`, 2 model types. `MainBicepAssembler.Generate` returns `MainBicepEmissionResult` with `OutputUsageTracker`.
- **P2 size guard [2026-05-13]:** `MainBicepAssembler` is back to an orchestrator over `Assemblers/MainBicep/*` section collaborators, and the current large generation files are locked below the audit threshold by `tests/InfraFlowSculptor.BicepGeneration.Tests/Architecture/GenerationSourceFileSizeGuardTests.cs`.
- **Representative Bicep CI validation [2026-05-13]:** `.github/workflows/bicep-generation-validation.yml` runs `scripts/Invoke-GeneratedBicepValidation.ps1`, which uses `RepresentativeBicepHarness` to emit deterministic sample artifacts, then executes explicit `bicep build` on each generated `.bicep` file.
- **Key Vault secret-name guard [2026-05-11]:** app-setting / App Configuration Key Vault secret names must follow Azure's `1..127` alphanumeric-or-hyphen rule. Names like `JWT_SECRET` are invalid because `_` is forbidden. `AddAppSettingCommandValidator` and `AddAppConfigurationKeyCommandValidator` now reject them on write, and `MainBicepAssembler.Generate` throws before emitting a broken `kvSecrets.module.bicep` for legacy snapshots that still contain an invalid secret name.
- **Key Vault output property access [2026-05-11]:** Azure-valid secret names can still be invalid as Bicep dot-property identifiers. When `MainBicepAssembler` references `kvSecrets` module `secretUris`, it must use `BicepFormattingHelper.FormatBicepPropertyAccess(...)` so names like `jwt-secret` become `outputs.secretUris['jwt-secret']` instead of the invalid `outputs.secretUris.jwt-secret`.
- **RBAC role key formatting + used type imports [2026-05-11]:** `ConstantsBicepAssembler` must emit role names through `FormatBicepObjectKey(...)`, and `MainBicepAssembler` must reference them through `FormatBicepPropertyAccess(...)` so valid identifiers such as `AcrPull` stay unquoted while display names like `Key Vault Secrets User` keep bracket access. `MainBicepAssembler` must also import module custom types only for parameters that are actually declared in `main.bicep`; copying every `ParameterTypeOverride` from IR causes `no-unused-imports` warnings for spec-only defaults such as `WorkloadProfileType` and `IngestionMode`.
- **`kvSecrets.module.bicep` secret URI output [2026-05-11]:** the `secretUris` output must not iterate the deployed `kvSecrets` resource collection and read `kv.name` / `kv.properties.secretUri`. ARM evaluates that lambda over symbolic deployment metadata, which triggers `DeploymentOutputEvaluationFailed` because `name` is unavailable there. The safe pattern is to derive the object from the input `secrets` parameter and `keyVault.properties.vaultUri`, e.g. `toObject(secrets, secret => secret.name, secret => '${keyVault.properties.vaultUri}secrets/${secret.name}')`.
- **Container Registry RBAC IDs + role module symbols [2026-05-11]:** `AzureRoleDefinitionCatalog.AcrPull` must use the official built-in role ID `7f951dda-4ed3-4680-a7ca-43fe172d538d`, and the container registry catalog entry for `AcrPush` must use `8311e382-0749-4cb8-b61a-304f252e45ec`. `ResourceTypeMetadata.GetBaseModuleName(...)` must also cover every RBAC target already supported by `RoleAssignmentModuleTemplates` including `ContainerRegistry`, `ServiceBusNamespace`, and `EventHubNamespace`, otherwise generated role-assignment module symbols fall back to `unknown...Roles` in `main.bicep`.
- **Container App image placeholder [2026-05-16]:** `DefaultContainerImage` is now `mcr.microsoft.com/k8s/core/pause:3.6` (public, no auth). `containerImage` is a separate top-level `param containerImage string` with that default (not inside `containerRuntime` object), making it independently overridable via pipeline `overrideParameters`. `ContainerRuntimeConfig` type now only contains `cpuCores` and `memoryGi`. First infra deploy uses the pause image, and `ContainerApp` / `WebApp` generation keeps that placeholder whenever `dockerImageValidated` is false. Validated image names still flow through the normal pipeline override path.
- **Custom domain DNS validation guard [2026-05-16]:** Bicep generators (ContainerApp, WebApp, FunctionApp) now filter `resource.CustomDomains` to only emit bindings where `DnsValidationStatus == "Validated"`. Domains with `Pending` status are silently excluded from generated `.bicepparam` files. This prevents Azure deployment failures caused by missing `certificateId` on unvalidated domains.
- **`main.bicep` parameter metadata guard [2026-05-16]:** `MainBicepAssembler` must emit `@description()` for all generated top-level `main.bicep` parameters coming from module parameters and secure parameters. The only exception is environment-value app-setting inputs destined for application environment variables: those params stay undecorated and must be grouped under a simple `//` comment announcing the target application. Regression is locked by `MainBicepAssemblerParameterMetadataTests`.

## Output Files
- `types.bicep`, `functions.bicep`, `main.bicep`, `constants.bicep` (RBAC only)
- `main.{shortName}.bicepparam` per environment (under `parameters/`, `using '../main.bicep'`)
- `modules/{Folder}/{type}.module.bicep` + `types.bicep` per resource
- **Parameter-file spacing guard [2026-05-16]:** `ParameterFileAssembler` must append the blank separator line only for modules that actually emitted at least one parameter (regular, secure, CORS, lifecycle). Empty/derived-only modules must not accumulate blank lines in generated `main.*.bicepparam` files; regression is locked by `ParameterFileAssemblerTests.Given_EmptyModulesBetweenEmittedModules_When_GeneratingParameterFiles_Then_DoesNotAccumulateBlankLines`.

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
- **Nested grouped-param indentation [2026-05-16]:** when `BicepFormattingHelper` serializes a multiline nested object or dictionary inside another object, every line after the first must be reindented relative to the parent property. Without that carry-over indentation, generated `.bicepparam` groups such as `containerApp*HealthProbes` flatten child lines under the parent key. Lock both the helper output and the final `ParameterFileAssembler` snippet with focused regressions.
- **Multiline module-property indentation [2026-05-16]:** when `BicepEmitter.EmitPropertyAssignment(...)` emits a multiline expression as a property value, every continuation line must be prefixed with the property's indentation while preserving the raw expression's internal relative spacing. This prevents generated module blocks such as `ContainerApp` `probes: union(...)` from collapsing left after IR emission.

- **Generator identifier constants [2026-05-11]:** when a generator repeats Bicep parameter names, variable names, resource symbols, property/output names, union literals, or raw expressions across `GenerateSpec(...)` and `Generate(...)`, extract them as constants. This applies strictly even to common builder keys such as `name`, `location`, `kind`, `properties`, and `linuxFxVersion` when they appear in generator code. Default to file-local `private const`; promote only the high-frequency literals with an identical cross-generator contract to `Generators/Constants/BicepGeneratorSharedConstants.cs`.

- **Typed networking collection params [2026-05-15]:** networking generators must expose repeated inputs through exported custom types rather than raw `array` parameters. The current reference shapes are `SubnetConfig[]` in `VirtualNetworkTypeBicepGenerator`, `VirtualNetworkLinkConfig[]` in `PrivateDnsZoneTypeBicepGenerator`, and `SecurityRuleConfig[]` in `NetworkSecurityGroupTypeBicepGenerator`.

- **Shared generator constants [2026-05-11]:** `Generators/Constants/BicepGeneratorSharedConstants.cs` is the only shared constants point for concrete generators today. It currently centralizes `TypesImportPath`, `NameParameterName`, `LocationParameterName`, `NamePropertyName`, `LocationPropertyName`, `PropertiesPropertyName`, `KindPropertyName`, `IdOutputName`, `BooleanTrueString`, and `BooleanFalseString`. Do not expand it with generator-specific values such as module names, ARM types, output expressions, or exported unions.

- **ContainerApp health probes [2026-04-22]:** 6 per-env fields (readiness/liveness/startup path+port). Bicep uses `union()` for conditional probe array. HTTP only, path must start `/`, port 1-65535.
- **ContainerApp param grouping [2026-04-22]:** 16 flat params → 4 exported types (`ContainerRuntimeConfig`, `ScalingConfig`, `IngressConfig`, `HealthProbeConfig`). `ParameterGroupMappings` + `ParameterTypeOverrides` on `GeneratedTypeModule`. ACR params stay flat (conditional).
- **Custom domains [2026-04-23]:** `ResourceDefinition.CustomDomains` → `ParameterFileAssembler` emits `customDomains` arrays, including the resource-specific `CertificateMode` to binding-type translation needed by Container App versus Web/Function App. Compute resources only.
- **Secure parameters [2026-04-21]:** `GeneratedTypeModule.SecureParameters` → `@secure()` params with no default. `MainBicepAssembler` must emit `dependsOn` on `kvSecrets.module.bicep` for locally created Key Vaults (ARM race condition).
- **ACR auth modes [2026-04-23]:** `ManagedIdentity` (default/null) vs `AdminCredentials`. Admin mode uses `SecureParameters = ["acrPassword"]`. ContainerApp admin emits `configuration.secrets` + `registries`; WebApp/FunctionApp emit `DOCKER_REGISTRY_SERVER_*` app settings.
- **ContainerApp managed-identity module naming [2026-05-16]:** the Container App managed-identity ACR variant now reuses the standard primary module filename `containerApp.module.bicep`; only the admin-credentials variant keeps a distinct filename (`containerAppAcrAdminCredentials.module.bicep`). If incompatible Container App variants share the same folder in one generation pass, `BicepAssembler.NormalizePrimaryModuleFileNames(...)` remains responsible for suffixing content-hash disambiguators.
- **Snapshot drift vs generated artifacts [2026-05-12]:** for active generation incidents, do not trust `docs/project-snapshots/*.md` blindly. On project `fb8699ea-f568-4afb-864b-e82d2efd0905`, the snapshot still showed `ifs-api` without `ContainerRegistryId`, while `infraDb` had both `ifs-api` and `ifs-frontend` linked to ACR `36ba74cb-c0a1-40a3-b224-e3be1c1b4b46`. The real deployment failure came from stale generated artifacts that still declared top-level `*AcrLoginServer` params with empty `.bicepparam` values instead of deriving `existing_*.properties.loginServer`.
- **Empty tag-merge guard [2026-05-16]:** `MainBicepAssembler.AppendTagsMergingBlock(...)` must treat empty `ProjectTags` / `ConfigTags` dictionaries exactly like missing tags. If both dictionaries are empty, emit `var tags = env.tags`; otherwise `main.bicep` can produce `var tags = union(configTags, env.tags)` without declaring `configTags`, which fails PR lint with `BCP057` on `configTags` and cascading `BCP062` errors on every `tags: tags` use. Regression is locked by `BicepAssemblerTests.Given_EmptyProjectAndConfigTags_When_Assemble_Then_MainBicepUsesEnvironmentTagsOnly`.
- **Project-level mono-repo ACR inference [2026-05-13]:** the stage-800/assembler fix for derived `acrLoginServer` was not sufficient on its own for project-level generation. `GenerateProjectBicepCommandHandler` now enriches each config request with inferred `ExistingResourceReferences` for cross-config Container Registries by scanning loaded configs for compute-resource `containerRegistryId` links. This covers cases where the `CrossConfigResourceReferences` table has no explicit ACR entry, so mono-repo generation still emits `existing_<acr>.properties.loginServer` instead of empty top-level `*AcrLoginServer` parameters.
- **Identity-preserving existing-resource ACR fallback [2026-05-13]:** `ExistingResourceReference` now carries nullable `TargetResourceId`, populated by both `GenerationRequestBuilder` and `GenerateProjectBicepCommandHandler` inference. `ParentReferenceResolutionStage` must resolve `acrLoginServer` fallbacks by `TargetResourceId` first and only fall back to a type-only match when there is a single existing ACR, otherwise multi-config requests with several external registries can silently bind compute resources to the wrong `existing_*.properties.loginServer` source.
- **Existing-resource API-version coverage [2026-05-13]:** `BicepArmTypeCatalog.GetExistingResourceApiVersion(...)` must explicitly cover `ContainerRegistryType` and `EventHubNamespaceType`. If either is omitted, `MainBicepAssembler` falls back to the generic `2023-01-01` existing-resource version even though the generator/role metadata already know the correct API version.
- **Identifiers [2026-05-12]:** canonical camelCase normalization now lives in `GenerationCore.BicepIdentifierNormalizer`. `BicepIdentifierHelper`, `BicepFormattingHelper.SanitizeBicepKey`, `AppSettingPipelineParameterNameHelper`, and `SecureParameterOverrideHelper` must delegate to it so generated module symbols, object keys, and secure-parameter names stay aligned across BicepGeneration and Application. `EnumValueObject` names may still differ from ARM values — normalize them in generator switches.
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
