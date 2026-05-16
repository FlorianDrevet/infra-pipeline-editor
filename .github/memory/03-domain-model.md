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
| `VirtualNetwork` | extends `AzureResource` | `Subnet`, `VirtualNetworkEnvironmentSettings` | TPT; abbreviation `vnet`; DDoS protection flag; Subnets own delegation, service endpoints, PE network policies, optional NSG FK |
| `NetworkSecurityGroup` | extends `AzureResource` | `NsgRule` | TPT; abbreviation `nsg`; Rules have priority/direction/access/protocol/CIDR |
| `PrivateDnsZone` | extends `AzureResource` | `VirtualNetworkLink` | TPT; abbreviation `pdnsz`; VNet links with auto-registration flag |
| `FrontDoor` | extends `AzureResource` | `FrontDoorOrigin`, `FrontDoorEnvironmentSettings` | TPT; abbreviation `afd`; WAF policy flag; Origins with target resource, private link, weight/priority; per-env SKU (Standard/Premium) |
| `PersonalAccessToken` | `PersonalAccessToken` | `TokenHash` (VO), `PersonalAccessTokenId` (VO) | PAT for MCP auth. `ifs_` prefix + SHA-256 hash stored, plaintext returned once. `UserId` FK. `Revoke()`, `RecordUsage()`, `IsValid()` methods. |
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
| `PrivateEndpointConfig` | PE configuration on any AzureResource: subnet, group ID, auto-approval, DNS zone, custom NIC name |

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

- All domain classes must have XML `<summary>` docs.
- Concrete aggregates inheriting from `AzureResource` must be declared `sealed`.
- All `EnumValueObject<T>`-derived classes must be declared `sealed` [2026-04-16].
- Value object properties must use `private set`.
- `tests/InfraFlowSculptor.Domain.Tests/Common/Models/ValueObjectEqualityComponentsCoverageTests.cs` is the DOM-012 guardrail: every covered concrete `ValueObject` must change structural equality when one meaningful public instance property changes. Keep computed/read-only projections out of that guard by leaving them without a writable path or compiler-generated backing field [2026-05-13].
- `Name` rejects `null`, empty, and whitespace strings, and `EntraId` rejects `Guid.Empty`; keep these guards local to the owning value objects and do not generalize them to every `SingleValueObject<string>` / `SingleValueObject<Guid>` because some setup flows still rely on `Guid.Empty` sentinels such as `SubscriptionId` [2026-05-13].
- `SingleValueObject<T>.ToString()` now returns the wrapped value string (or `string.Empty` for `null`) instead of the CLR type name [2026-05-12].
- Regex-backed domain validation must declare an explicit timeout; `VirtualNetworkAggregate.Entities.Subnet.ServiceEndpointPattern` (100 ms) is the current reference fix for regex guards in the domain layer [2026-05-15].
- Error strings must be in English.
- `Location` is the canonical source for Azure wire-format region keys: use `Location.DefaultAzureRegionKey` for the default region and `Location.ToAzureRegionKey(...)` instead of hardcoding values like `westeurope` or `francecentral` [2026-04-29].

## IsExisting Resources [2026-04-23]

All 18 concrete `AzureResource` aggregates support `IsExisting` (bool, `protected set`, default `false`):
- Added as last param in `Create()` factory with default `bool isExisting = false`
- `Update()` and `SetEnvironmentSettings()`/`SetAllEnvironmentSettings()` guard: `if (IsExisting) return;` (no-op, no exception)
- EF Core: `HasDefaultValue(false)` in `AzureResourceConfiguration`, migration `AddIsExistingToAzureResource`
- Existing resources: excluded from Bicep deploy modules + pipeline stages, added as `ExistingResourceReference` (with `SourceConfigName = string.Empty`) for cross-config lookup
- Frontend guard: tabs "Environments" and "App Pipeline" hidden; amber info banner shown; add-resource-dialog skips environment step

## Resource Abbreviation Overrides [2026-04-22]

Two-level abbreviation override system matching NamingTemplate precedence:
- **`ProjectResourceAbbreviation`** (`Entity<ProjectResourceAbbreviationId>`): owned by `Project`, unique `(ProjectId, ResourceType)`, cascade delete. Aggregate methods: `SetResourceAbbreviation(type, abbr)`, `RemoveResourceAbbreviation(type)`.
- **`ResourceAbbreviationOverride`** (`Entity<ResourceAbbreviationOverrideId>`): owned by `InfrastructureConfig`, unique `(InfraConfigId, ResourceType)`, cascade delete. Aggregate methods: `SetResourceAbbreviationOverride(type, abbr)`, `RemoveResourceAbbreviationOverride(type)`.
- **Resolution precedence** in Bicep/Pipeline generation: Config override → Project override → `ResourceAbbreviationCatalog` default.
- Validation: regex `^[a-z0-9]+$`, max 10 chars.
- `NamingContextReadModel` includes `ResourceAbbreviations` dictionary (already merged at read time). All 4 generator handlers + `InfrastructureConfigReadRepository.BuildNamingContext` use `MergeAbbreviations()` helper.
- Collection initializers: prefer `= []` over `= new()`.
- `EnumValueObject` types: use primary constructor pattern.

## Layout-Driven Repository Topology [2026-04-23]

- `Project.LayoutPreset` is now the top-level switch: `AllInOne`, `SplitInfraCode`, or `MultiRepo`. Switching preset clears `Project.Repositories` so the repository slots can be reconfigured safely.
- `ProjectRepository.ContentKinds` only supports `Infrastructure` and `ApplicationCode`. `AllInOne` requires exactly one repo carrying both flags; `SplitInfraCode` requires exactly two repos, one infra-only and one app-only; `MultiRepo` forbids project-level repositories entirely.
- `InfrastructureConfig` now owns nullable `LayoutMode` (`AllInOne` or `SplitInfraCode`) plus a `Repositories` collection of `InfraConfigRepository` entities used only when the parent project layout is `MultiRepo`.
- `InfrastructureConfig.SetLayoutMode(...)` clears config-level repositories whenever the mode changes, mirroring the project-level reset behavior.
- `Project.CanGenerateAllFromProjectLevel(...)` now returns `false` for `MultiRepo`; project-level generate-all remains reserved for layouts where the project itself owns the effective repositories.
- Legacy `GitRepositoryConfiguration`, `RepositoryMode`, `RepositoryBinding`, and `CommonsStrategy` were removed during the V3/layout-driven cleanup. Only persisted data repair remains relevant (see `06-persistence.md`).

## Error Definitions

Errors live in `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.*.cs` as partial static classes. When adding a new aggregate, add `Errors.AggregateName.cs`. Convention: no inline `Error.*()` calls in handlers — always use `Errors.AggregateName.MethodName()`. New in V1: `Errors.ProjectRepository.cs` (`InvalidAlias`, `DuplicateAlias`, `NotFound(id|alias)`, `NoContentKind`, `UnsupportedCommonsStrategy`, `RepositoryInUse`); extensions `Errors.Project.InvalidLayoutPreset`, `Errors.Project.InvalidCommonsStrategy`.
