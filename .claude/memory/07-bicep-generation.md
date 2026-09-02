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
| 520 | `NetworkingResolutionStage` | V2 [2026-05-28]: Resolves VNet/subnet for privatized resources |
| 540 | `PrivateEndpointCompanionStage` | V2 [2026-05-28]: Emits PE modules for privatized resources |
| 560 | `PublicNetworkAccessStage` | V2 [2026-05-28]: Sets publicNetworkAccess property on privatized resources |
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
- **D01 corrigé (2026-08-28).** Les dépôts `InfrastructureConfig` portent désormais leur PAT,
  nommé par `ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(...)`; le resolver retourne
  ce secret au lieu de `null`. La validation réelle Git reste à faire avec D09/D10.
- **D10 corrigé (2026-09-01).** `PushProjectMultiRepoArtifactsCommand` orchestre des pushes
  indépendants par configuration et dépôt config-level. `AllInOne` pousse Bicep, pipelines et bootstrap
  dans un commit par dépôt; `SplitInfraCode` sépare les scopes Infrastructure/ApplicationCode et les
  bootstraps `FullOwner`/`ApplicationOnly`. Les plans sont prévalidés avant le premier push et les
  résultats signalent les échecs partiels sans prétendre à une atomicité inter-dépôts.
- **Durcissement D09/D10 (2026-09-01).** Les handlers config-level exigent désormais le nom du
  secret du dépôt résolu et ne recréent plus de fallback `git-pat-{projectId}`. Le push bootstrap
  direct sélectionne le bucket `infra/` en `SplitInfraCode`; le service bulk utilise les buckets
  infra/app selon le rôle. Les `CancellationToken` sont propagés par `BlobDownloadHelper`,
  `GeneratedArtifactService`, Key Vault et les providers Git; une annulation n'est pas
  transformée en erreur Git et n'autorise pas la cible suivante dans les orchestrateurs.

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
- **Container App image placeholder [2026-05-17]:** `DefaultContainerImage` is now `mcr.microsoft.com/azuredocs/containerapps-helloworld:latest` (public, no auth). `containerImage` stays a separate top-level `param containerImage string` (not inside `containerRuntime`), making it independently overridable via pipeline `overrideParameters`. `ContainerRuntimeConfig` still only contains `cpuCores` and `memoryGi`. Until `dockerImageValidated` is `true`, ACA generation keeps this Azure Docs hello-world placeholder for both the IR/spec path and the legacy module template path. Validated image names still flow through the normal pipeline override path.
- **Container App custom-domain module pruning [2026-05-17]:** `ContainerAppTypeBicepGenerator` must only emit the `customDomains` parameter, the `customDomainBindings` variable, and the ingress `customDomains` property when the resource has at least one `CustomDomain` whose `DnsValidationStatus == "Validated"`. Keep the IR/spec path and the legacy `Generate()` template path aligned on the same validated-domain predicate so modules with pending-only domains do not carry dead custom-domain plumbing.
- **Custom domain DNS validation guard [2026-05-16]:** Bicep generators (ContainerApp, WebApp, FunctionApp) now filter `resource.CustomDomains` to only emit bindings where `DnsValidationStatus == "Validated"`. Domains with `Pending` status are silently excluded from generated `.bicepparam` files. This prevents Azure deployment failures caused by missing `certificateId` on unvalidated domains.
- **`main.bicep` parameter metadata guard [2026-05-16]:** `MainBicepAssembler` must emit `@description()` for all generated top-level `main.bicep` parameters coming from module parameters and secure parameters. The only exception is environment-value app-setting inputs destined for application environment variables: those params stay undecorated and must be grouped under a simple `//` comment announcing the target application. Regression is locked by `MainBicepAssemblerParameterMetadataTests`.
- **Readable `main.bicep` parameter descriptions [2026-05-17]:** top-level parameter descriptions emitted by `MainBicepAssembler` must no longer wrap field names, resource names, or Key Vault secret names in quoted/escaped Bicep fragments. Resource, secure, and Storage Account companion params now use plain readable wording such as `Value for sku of WebApp resource api-service.` and `Blob lifecycle management rules for storage account ifs.`; the regression is covered in `MainBicepAssemblerParameterMetadataTests`.

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

## Per-env override plumbing — three required links [2026-06-02]
A per-environment value only reaches `main.{env}.bicepparam` if ALL of these hold (VNet `addressPrefixes` + per-env `enableDdosProtection` failed on all three before this fix):
1. **Module match** — `ResourceTypeMetadata.GetBaseModuleName(armType)` must return the generator's `ModuleName` (it returned `"unknown"` for `VirtualNetworkType`, so `ParameterFileAssembler.FindMatchingResource` never matched and ALL VNet overrides were dropped). Every generated resource type needs a case here. **`DocumentIntelligenceType` is still missing — likely the same latent gap.**
2. **Base key exists** — the override key must already be in the generator's `Generate()` `Parameters` dict (see rule above); `ApplyParameterOverrides` only merges keys present in `module.Parameters`.
3. **Type coercion** — `ParameterFileAssembler.CoerceToOriginalType` coerces the string override to the base value's type. Arrays are handled by `CoerceToBicepArray` (JSON string like `["10.0.0.0/16"]` → `List<object>` → real Bicep array). Booleans coerce only if the base value is a real `bool` (not the string `"false"`). The read repo serializes per-env arrays as JSON into `EnvironmentConfigs`.

## Generator-Specific Patterns

- **Typed legacy parameter models [2026-05-11]:** `Generators/ParameterModels/` + `BicepParameterModelConverter` (System.Text.Json, `JsonPropertyName`, null omission) for fixed-schema payloads. `BicepFormattingHelper` + `ParameterFileAssembler` honor `JsonPropertyName`.
- **Serialized Bicep object keys [2026-05-11]:** route via `FormatBicepObjectKey(...)` (quoted if needed).
- **Nested grouped-param indentation [2026-05-16]:** carry-over indentation for multiline objects inside objects (locked by helper + assembler tests).
- **Multiline module-property indentation [2026-05-16]:** `BicepEmitter.EmitPropertyAssignment` preserves indentation for multiline expressions.
- **Generator identifier constants [2026-05-11]:** extract repeated Bicep names/symbols/literals as file-local `private const`. Promote high-frequency cross-generator literals to `BicepGeneratorSharedConstants.cs`.
- **Typed networking collection params [2026-05-15]:** export custom types (`SubnetConfig[]`, `VirtualNetworkLinkConfig[]`, `SecurityRuleConfig[]`), not raw `array`.
- **Shared generator constants [2026-05-11]:** `BicepGeneratorSharedConstants.cs` = `TypesImportPath`, `NameParameterName`, `LocationParameterName`, `IdOutputName`, `BooleanTrueString`, etc. No generator-specific values.
- **ContainerApp health probes [2026-04-22]:** 6 per-env fields (readiness/liveness/startup path+port), `union()` conditional array, HTTP only.
- **ContainerApp param grouping [2026-04-22]:** 16 flat → 4 exported types (`ContainerRuntimeConfig`, `ScalingConfig`, `IngressConfig`, `HealthProbeConfig`).
- **Custom domains [2026-04-23]:** `ResourceDefinition.CustomDomains` → `ParameterFileAssembler` emits `customDomains` arrays + `CertificateMode` translation.
- **Secure parameters [2026-04-21]:** `@secure()` params, no default, `dependsOn` KV secrets module for local KVs.
- **ACR auth modes [2026-04-23]:** `ManagedIdentity` vs `AdminCredentials`. Admin uses `SecureParameters = ["acrPassword"]`.
- **Dedicated ACR pull UAI [2026-05-19]:** `acrPullIdentityId` resolved in `ParentReferenceResolutionStage` to parent ACR managed-identity client id.
- **Project snapshot refreshed [2026-05-18]:** `docs/project-snapshots/fb8699ea-ifs-project.md` now aligned with `infraDb` (SplitInfraCode, dual repos, ACR binding, UAIs, ingress, probes, domain).
- **Identifiers [2026-05-12]:** `GenerationCore.BicepIdentifierNormalizer` for canonical camelCase. `BicepIdentifierHelper`, `BicepFormattingHelper.SanitizeBicepKey`, `AppSettingPipelineParameterNameHelper`, `SecureParameterOverrideHelper` delegate to it.
- **CAE secure LAW [2026-04-22]:** `destination: 'azure-monitor'` + `diagnosticSettings` (no `listKeys()`), `workspaceId` plain string param.


## Module Reuse & Disambiguation

- **AVM-style generic params [2026-04-26]:** Single `param userAssignedIdentityId string` instead of per-UAI names. `envVars`/`appSettings` injected into all instances of a type when any instance has app settings. `NormalizePrimaryModuleFileNames` collapses to shared `*.module.bicep`.
- **Type import collisions [2026-04-27]:** `MainBicepAssembler` aliases colliding exported type names (`SkuName`, `TlsVersion`) as `<FolderName><TypeName>`. Same alias reused in param declarations.
- **Variant files:** Incompatible parameter surfaces → distinct `ModuleFileName`. Content-based disambiguation across configs in mono-repo. Merged `types.bicep` per folder (not first/last-wins).
- **Mono-repo constants:** Rebuild `Common/constants.bicep` from union of all configs' role assignments, not single config's payload.
- **Role grouping [2026-04-23]:** `RoleRef` carries own `ServiceCategory`. Grouping key includes target type, target RG, and cross-config flag.
- **Identity injection [2026-04-23]:** Must detect only resource-root `identity:` block, not nested properties (e.g. Container App ACR registry `identity:`).
- **Output validation [2026-04-21]:** `ExtractRootSymbol()` validates root symbol exists before injection (prevents BCP057).
