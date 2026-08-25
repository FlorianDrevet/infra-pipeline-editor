# Domain Model
## Aggregates

| Aggregate | Root | Key Entities | Notes |
|---|---|---|---|
| `Project` | `Project` | `ProjectMember`, `ProjectEnvironmentDefinition`, `ProjectResourceNamingTemplate`, `ProjectResourceAbbreviation`, `ProjectRepository`, `ProjectPipelineVariableGroup` | Groups InfrastructureConfigs; owns membership/RBAC, default environments, naming conventions, shared pipeline variable groups, `AgentPoolName`, project-level resource abbreviation overrides, project-level `ProjectRepository` declarations, and `LayoutPreset` (`AllInOne` / `SplitInfraCode` / `MultiRepo`). |
| `InfrastructureConfig` | `InfrastructureConfig` | `ParameterDefinition`, `ResourceParameterUsage`, `ResourceNamingTemplate`, `ResourceAbbreviationOverride`, `CrossConfigResourceReference`, `InfraConfigRepository` | Has `ProjectId` FK to Project. Environments inherited from parent Project. Config-level resource abbreviation overrides. In `MultiRepo` projects, it can also carry nullable `LayoutMode` (`AllInOne` / `SplitInfraCode`) plus config-level `InfraConfigRepository` declarations. |
| `ResourceGroup` | `ResourceGroup` | `AzureResource` (base) | Hosts Azure resources. No child entities — `ResourceEnvironmentConfig` was removed. |
| `KeyVault` | extends `AzureResource` | `KeyVaultEnvironmentSettings` | TPT in EF Core |
| `RedisCache` | extends `AzureResource` | `RedisCacheEnvironmentSettings` | TPT in EF Core |
| `StorageAccount` | extends `AzureResource` | `StorageAccountEnvironmentSettings`, `BlobContainer`, `StorageQueue`, `StorageTable`, `CorsRule`, `BlobLifecycleRule` | TPT; sub-resources managed via dedicated CQRS commands |
| `AppServicePlan` | extends `AzureResource` | `AppServicePlanEnvironmentSettings` | TPT; OsType at resource level |
| `WebApp` | extends `AzureResource` | `WebAppEnvironmentSettings` | TPT; FK to AppServicePlan |
| `FunctionApp` | extends `AzureResource` | `FunctionAppEnvironmentSettings` | TPT; FK to AppServicePlan |
| `UserAssignedIdentity` | extends `AzureResource` | — | TPT; simplest resource type |
| `AppConfiguration` | extends `AzureResource` | `AppConfigurationEnvironmentSettings`, `AppConfigurationKey`, `AppConfigurationKeyEnvironmentValue` | TPT; configuration keys with 5 modes |
| `ContainerAppEnvironment` | extends `AzureResource` | `ContainerAppEnvironmentEnvironmentSettings` | TPT; abbreviation `cae`; `LogAnalyticsWorkspaceId` (AzureResourceId?) on aggregate root (moved from per-env [2026-04-04]) |
| `ContainerApp` | extends `AzureResource` | `ContainerAppEnvironmentSettings` | TPT; FK to ContainerAppEnvironment; per-env health probes (readiness/liveness/startup, HTTP path+port) |
| `LogAnalyticsWorkspace` | extends `AzureResource` | `LogAnalyticsWorkspaceEnvironmentSettings` | TPT; abbreviation `law` |
| `ApplicationInsights` | extends `AzureResource` | `ApplicationInsightsEnvironmentSettings` | TPT; FK to LogAnalyticsWorkspace |
| `CosmosDb` | extends `AzureResource` | `CosmosDbEnvironmentSettings` | TPT; abbreviation `cosmos` |
| `SqlServer` | extends `AzureResource` | `SqlServerEnvironmentSettings` | TPT; abbreviation `sql` |
| `SqlDatabase` | extends `AzureResource` | `SqlDatabaseEnvironmentSettings` | TPT; FK to SqlServer |
| `ServiceBusNamespace` | extends `AzureResource` | `ServiceBusNamespaceEnvironmentSettings` | TPT; sub-resources: Queue, TopicSubscription |
| `EventHubNamespace` | extends `AzureResource` | `EventHubNamespaceEnvironmentSettings` | TPT; sub-resources: EventHub, ConsumerGroup |
| `ContainerRegistry` | extends `AzureResource` | `ContainerRegistryEnvironmentSettings` | TPT; abbreviation `acr` |
| `DocumentIntelligence` | extends `AzureResource` | `DocumentIntelligenceEnvironmentSettings` | TPT; abbreviation `docint`; custom subdomain at resource level; per-env SKU, public network access, and local-auth toggle |
| `VirtualNetwork` | extends `AzureResource` | `Subnet`, `VirtualNetworkEnvironmentSettings` | TPT; abbreviation `vnet`; DDoS protection flag; Subnets own delegation, service endpoints, PE network policies, optional NSG FK; V2 [2026-05-28]: removed legacy PrivateEndpoint FK |
| `NetworkingProfile` | `NetworkingProfile` | `NetworkingProfileEnvironmentOverride` | V2 networking aggregate [2026-05-28]. Replaces V1 NSG/PrivateDnsZone/FrontDoor/PrivateEndpointConfig. Owns: `NetworkingMode` (Simplified/Standard/Advanced), `VnetReference` (source+CIDRs), `DnsConfig` (mode+hub IDs). Unique per `InfrastructureConfigId`. Methods: `Create()`, `ChangeMode()`, `UpdateVnetReference()`, `UpdateDnsConfig()`, `SetEnvironmentOverride()`, `RemoveEnvironmentOverride()`. V2 pipeline: 3 new stages (NetworkingResolution@520, PrivateEndpointCompanion@540, PublicNetworkAccess@560). **CORRECTION 2026-08-25 — cet agrégat est ORPHELIN, ne pas s'appuyer dessus.** The V3 migration is *done*, not "starting": the 3 stages (`NetworkingResolutionStage.cs:28`, `PrivateEndpointCompanionStage.cs:32`, `PublicNetworkAccessStage.cs:23`) read only `AzureResource.IsPrivatized` / `PrivateEndpointConfig` and never touch `NetworkingProfile`. No repository, no handler, no controller reads or writes this aggregate — only the domain classes, the EF configuration and the table survive. Cleanup of V2 was never done. See `docs/stabilization/feature-map.md`, D04 (and D11 for the V3 privatization gap). |
| `PersonalAccessToken` | `PersonalAccessToken` | `TokenHash` (VO), `PersonalAccessTokenId` (VO), `PatScope` (VO) | PAT for MCP auth. `ifs_` prefix + SHA-256 hash stored, plaintext returned once. `UserId` FK. Owns `PatScope` values (`Read` default, `Write`, `Generate`). Methods: `Revoke()`, `RecordUsage()`, `IsValid()`, `HasScope()`. |
| `User` | `User` | — | Azure AD user info |

## Shared Base Entities (Common/BaseModels/Entites)

These reusable entity types are owned by multiple aggregates:

| Entity | Usage |
|--------|-------|
| `AppSetting` | App settings with key/value, used by WebApp, FunctionApp, ContainerApp |
| `AppSettingEnvironmentValue` | Per-environment overrides for app settings |
| `InputOutputLink` | Links between resource outputs and other resource inputs |
| `RoleAssignment` | RBAC role assignment on any AzureResource |
| `CustomDomain` | Per-environment custom domain binding for ContainerApp, WebApp, FunctionApp |
| `SecureParameterMapping` | Maps secure Bicep params to project pipeline variable groups |
| `PrivateEndpointConfiguration` | V3 PE configuration owned by `AzureResource`: selected `VirtualNetworkId`, `SubnetName`, typed `DnsMode`, and optional DNS hub resource group/subscription for existing hub DNS. The older `PrivateEndpointConfig` entity was removed with V1 networking. |

## AzureResource.PrivateEndpointConfiguration [2026-05-29]

- `AzureResource` now owns nullable `PrivateEndpointConfiguration` for V3 resource-level privatization while retaining `IsPrivatized` as the compatibility flag during the migration away from config-scoped `NetworkingProfile`.
- Public methods: `ConfigurePrivateEndpoint(PrivateEndpointConfiguration configuration)` sets `IsPrivatized = true`; `DisablePrivateEndpoint()` clears the owned configuration and sets `IsPrivatized = false`; `Deprivatize()` delegates to the same cleanup path.
- `PrivateEndpointDnsMode` is a sealed enum value object with `AutoManaged`, `ExistingHub`, and `Disabled`. `ExistingHub` requires both DNS hub resource group id and DNS hub subscription id.
- V3 scope is Private Endpoint only. Container App Environment VNet integration is a separate Azure concept and must not be folded into this private endpoint configuration.

## AzureResource.AssignedUserAssignedIdentityId [2026-04-02]

`AzureResource` has an optional nullable FK `AssignedUserAssignedIdentityId` → `UserAssignedIdentity`. Methods: `AssignUserAssignedIdentity(id)`, `UnassignUserAssignedIdentity()`. Bicep engine honors this for identity block injection.

## ContainerApp.DockerImageName [2026-04-02]

`ContainerApp` owns `DockerImageName` at the resource level (not per-env). The `containerImage` property was removed from `ContainerAppEnvironmentSettings`. Bicep generator reads `resource.Properties["dockerImageName"]`.

## Compute Docker Image Validation [2026-05-17]

- `ContainerApp`, `WebApp`, and `FunctionApp` now persist `DockerImageValidated` alongside `DockerImageName`.
- The flag defaults to `false` and is the canonical cross-layer signal for “image name entered” versus “image confirmed”, reused by frontend validation UX, diagnostics, and generation.
## Application Pipeline Properties [2026-04-04]

3 compute aggregates now have CI/CD pipeline config properties:
- **ContainerApp**: `DockerfilePath` (string?), `ApplicationName` (string?)
- **WebApp/FunctionApp**: `DockerfilePath`, `SourceCodePath`, `BuildCommand`, `ApplicationName` (all string?)
- `AppPipelineStepOptions` now lives under `Domain/Common/OwnedEntities/` and is shared by `WebApp`, `FunctionApp`, and `ContainerApp`. Update it through the single `AppPipelineStepOptionsData` payload instead of reintroducing long flat mutator signatures [2026-05-15].
- `ApplicationStack` is the pipeline-app stack selector and is intentionally distinct from Azure hosting runtime stacks (`WebAppRuntimeStack` / `FunctionAppRuntimeStack`). It lives on `AppPipelineStepOptions` with an optional typed `AppPipelineStackProfile` hierarchy (`DotNet`, `NodeJs`, `Angular`, `Java`, `Python`, `StaticSite`, `Custom`) to drive future stack-aware pipeline options [2026-05-21].
- `AppPipelineStackProfile` classes are plain owned profile classes, not `ValueObject` derivatives; this avoids treating nested stack payloads as generic structural value objects while keeping one typed profile per application stack [2026-05-21].
- `ApplicationName` is a user-friendly name displayed in Azure DevOps pipeline runs (fallback: resource name)
- `InfrastructureConfig` has `AppPipelineMode` enum (`Isolated`/`Combined`) — controls whether app pipelines are generated per-resource or as a single combined pipeline
- `Project` has `AgentPoolName` (string?) — when set, pipeline YAML uses `pool: name: '<value>'` (self-hosted); when null, `pool: vmImage: ubuntu-latest` (Microsoft-hosted). Endpoint: `PUT /projects/{id}/agent-pool`
- Resource-specific pipeline properties are persisted in compute-resource TPT tables and exposed in Create/Update commands and contracts. `Project.AgentPoolName` is persisted on the Project aggregate and consumed by both config-level and project-level pipeline generation.

## ACR Authentication Mode [2026-04-23]

- `ContainerApp`, `WebApp`, and `FunctionApp` now persist nullable `AcrAuthMode` alongside `ContainerRegistryId`.
- `AcrAuthMode` is a shared `EnumValueObject` with values `ManagedIdentity` and `AdminCredentials`.
- `AcrAuthMode` is cleared automatically when `ContainerRegistryId` is removed; `null` remains backward-compatible and is treated as managed identity by generators and diagnostics.
- The mode lives on the 3 compute aggregates only; `AzureResource` was intentionally left unchanged to avoid widening the blast radius of a compute-specific concern.

## Cross-Config References

`InfrastructureConfig` owns `_crossConfigReferences` collection of `CrossConfigResourceReference` entities. Each reference points to a `TargetResourceId` in another config of the same project, with an `Alias` and optional `Purpose`. The Bicep generator emits `existing` resource group + `existing` resource declarations for each referenced resource.

## Custom Domains & Secure Parameter Mappings [2026-04-23]

- `AzureResource` now owns `_customDomains` and `_secureParameterMappings` backing collections on the base class.
- `CustomDomain` stores `EnvironmentName`, normalized `DomainName`, `CertificateMode` (`ManagedCertificate`, `KeyVaultCertificate`, `ManualCertificate`, or `Disabled`), optional `KeyVaultUrl` / `ManagedIdentityResourceId` / `CertificateName`, and `DnsValidationStatus` (`Pending` or `Validated`). Duplicate `(EnvironmentName, DomainName)` pairs are rejected.
- `DnsValidationStatus` is an `EnumValueObject<DnsValidationStatus>` (sealed) with values `Pending` and `Validated`. New domains start as `Pending`. Methods: `ValidateDns()` → sets `Validated`, `ResetDnsValidation()` → resets to `Pending`.
- Custom domains are supported for compute resources only (ContainerApp, WebApp, FunctionApp) and are blocked on `IsExisting` resources.
- `ManagedCertificate` is the default replacement for the legacy `BindingType` flow; downstream Bicep emission maps `CertificateMode` back to the resource-specific binding representation expected by Container App versus Web/Function App.
- Bicep generators only emit custom domain bindings for domains where `DnsValidationStatus == Validated`; `Pending` domains are excluded from generated artifacts.
- `SecureParameterMapping` stores `SecureParameterName`, optional `VariableGroupId`, and `PipelineVariableName` so a secure Bicep param can be injected from an Azure DevOps variable group.
- `AzureResource.SetSecureParameterMapping(...)` acts as upsert/clear: `null` group clears an existing mapping, inconsistent half-filled mappings are rejected.

## Domain Events [2026-05-13]

- `AggregateRoot<TId>` now implements `IHasDomainEvents` and owns an in-process `IReadOnlyCollection<IDomainEvent>` exposed through `DomainEvents`, plus `AddDomainEvent(...)` / `ClearDomainEvents()` helpers.
- `Project.Create(...)` is the first event producer on the current branch and raises `ProjectCreatedDomainEvent`; keep this seam intentionally narrow (in-process only, no outbox, no integration-event rollout, and no requirement that every aggregate emits events yet).

## Domain Invariants

- `Project.Members` is `IReadOnlyCollection<ProjectMember>` — mutated via `AddMember()`, `ChangeRole()`, `RemoveMember()`.
- `InfrastructureConfig` has a `ProjectId` FK. Access checks resolved via **project membership** — `IInfraConfigAccessService`.
- `AzureResource` inheritance uses EF Core **TPT**: `HasBaseType<AzureResource>().ToTable("...")`.
- `AzureResource` no longer exposes public setters for `ResourceGroupId`, `ResourceGroup`, `Name`, `Location`, or `CustomNameOverride`; the shared mutation surface is now `Rename(...)`, `MoveToResourceGroup(...)`, `OverrideName(...)`, `ClearNameOverride()`, plus the protected `SetNameAndLocation(...)` / `SetLocation(...)` helpers for derived aggregates [2026-05-13].
- EF navigations that may legitimately be absent outside an eager-loaded query should be nullable in the domain model. The current reference cases are `AzureResource.ResourceGroup`, `ProjectEnvironmentDefinition.Project`, and `ProjectMember.Project` [2026-05-13].
- `AzureResource.SetNameAndLocation(...)` is the shared helper for the common `Name` + `Location` mutation path; concrete Azure-resource `Update(...)` methods delegate this shared part to the base while keeping their resource-specific assignments local [2026-05-13].
- `AzureResource.AddDependency(...)` now enforces same-resource-group dependencies and rejects cyclic graphs; self-dependency still throws and duplicate dependencies remain a no-op [2026-05-12].
- `CorsRule` now keeps its string collections behind read-only views backed by private lists, and `StorageAccount.GetBlobCorsRules()` / `GetTableCorsRules()` reuse cached filtered views instead of recomputing `Where(...).ToList()` on every access [2026-05-12].
- `CorsRule` is now the reference for EF-only entity constructors that must initialize cached read-only views: keep the parameterless materialization constructor non-private but still non-public (`internal` here) so the domain model does not rely on `SuppressMessage` for EF Core access [2026-05-13].
- Concrete aggregate roots now follow the DOM-006 convention: expose a public static `Create(...)` factory and keep the EF parameterless constructor non-public. `AggregateFactoryConventionTests` guards this for every concrete `AggregateRoot<>` except the `AzureResource` base template; `PersonalAccessToken.Create(...)` remains allowed to return `(Token, PlainTextToken)` because the plaintext secret only exists at creation time [2026-05-13].

## Domain Code Quality Rules [2026-03-30]

- Domain classes must keep XML `<summary>` docs, concrete aggregates inheriting from `AzureResource` must be `sealed`, and all `EnumValueObject<T>`-derived classes stay `sealed` [2026-04-16].
- Value object properties must use `private set`.
- `tests/InfraFlowSculptor.Domain.Tests/Common/Models/ValueObjectEqualityComponentsCoverageTests.cs` is the DOM-012 guardrail: every covered concrete `ValueObject` must change structural equality when one meaningful public instance property changes. Keep computed/read-only projections out of that guard by leaving them without a writable path or compiler-generated backing field [2026-05-13].
- `Name` rejects `null`, empty, and whitespace strings, `EntraId` rejects `Guid.Empty`, and `SingleValueObject<T>.ToString()` returns the wrapped value string (or `string.Empty` for `null`) instead of the CLR type name [2026-05-12/13].
- Regex-backed domain validation must declare an explicit timeout; `VirtualNetworkAggregate.Entities.Subnet.ServiceEndpointPattern` (100 ms) is the current reference fix for regex guards in the domain layer [2026-05-15].
- Error strings must stay in English.
- `Location` is the canonical source for Azure wire-format region keys: use `Location.DefaultAzureRegionKey` for the default region and `Location.ToAzureRegionKey(...)` instead of hardcoding values like `westeurope` or `francecentral` [2026-04-29].
## IsExisting Resources [2026-04-23]

All 18 concrete `AzureResource` aggregates support `IsExisting` (bool, `protected set`, default `false`):
- Added as last param in `Create()` factory with default `bool isExisting = false`
- `Update()` and `SetEnvironmentSettings()`/`SetAllEnvironmentSettings()` guard: `if (IsExisting) return;` (no-op, no exception)
- EF Core: `HasDefaultValue(false)` in `AzureResourceConfiguration`, migration `AddIsExistingToAzureResource`
- Existing resources: excluded from Bicep deploy modules + pipeline stages, added as `ExistingResourceReference` (with `SourceConfigName = string.Empty`) for cross-config lookup
- Frontend guard: tabs "Environments" and "App Pipeline" hidden; amber info banner shown; add-resource-dialog skips environment step

## V1 Privatization Demolition [2026-05-28]

- Deleted 18 folders + ~37 files: `NetworkSecurityGroup`, `PrivateDnsZone`, `FrontDoor` aggregates, `PrivateEndpoint` entities, all associated Application/Contracts/Infrastructure/Tests/Frontend/Bicep code.
- Cleaned: `AzureResource` base model, `AzureResourceBaseRepository`, `BicepArmTypeCatalog`, `ResourceCommandFactory`, `ResourceTypes`, MCP tools, frontend metadata/enums/i18n.
- Kept: `VirtualNetwork` aggregate (reduced), `PrivateEndpointTypeBicepGenerator` (recycled for V2 stage 540), `PrivateEndpointGroupIdCatalog` (V2 auto-derivation), `PrivateEndpointNetworkPolicy` enum (valid subnet config).
- V2 replacement: `NetworkingProfile` aggregate + `AzureResource.IsPrivatized` flag + 3 Bicep pipeline stages.

## Resource Abbreviation Overrides [2026-04-22]

Two-level override: `ProjectResourceAbbreviation` (project-level) and `ResourceAbbreviationOverride` (config-level). Resolution precedence: Config → Project → `ResourceAbbreviationCatalog` default. Validation: `^[a-z0-9]+$`, max 10 chars. `NamingContextReadModel` includes merged `ResourceAbbreviations` dictionary.

## Layout-Driven Repository Topology [2026-04-23]

- `Project.LayoutPreset`: `AllInOne`, `SplitInfraCode`, or `MultiRepo`. Switching preset clears `Project.Repositories`.
- `ProjectRepository.ContentKinds`: `Infrastructure` | `ApplicationCode`. `AllInOne` requires 1 repo with both; `SplitInfraCode` requires 2 repos (1 infra, 1 app); `MultiRepo` forbids project-level repos.
- `InfrastructureConfig`: nullable `LayoutMode` + `Repositories` collection (used only in `MultiRepo`). `SetLayoutMode()` clears config-level repositories.
- Repository aliases removed [2026-05-19]. Identity now typed (`ProjectRepositoryId` / `InfraConfigRepositoryId`). Routing is role-based via `RepositoryContentKinds`. Do not reintroduce alias-based lookup.
- Legacy `GitRepositoryConfiguration`, `RepositoryMode`, `RepositoryBinding`, `CommonsStrategy` removed.

## Error Definitions

Errors in `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.*.cs` as partial static classes. Convention: no inline `Error.*()` calls — always use `Errors.AggregateName.MethodName()`. Current repository errors: `NotFound(id)`, `RepositoryInUse(id)`, `PersonalAccessTokenRequired()`, `DefaultBranchNotFound(branch)`, `RepositorySlotNotConfigured(id)`, `RepositoryRoleMismatch(id, contentKind)`. Alias errors obsolete.

