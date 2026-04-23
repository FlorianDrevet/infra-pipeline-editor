# Domain Model

## Aggregates

| Aggregate | Root | Key Entities | Notes |
|---|---|---|---|
| `Project` | `Project` | `ProjectMember`, `ProjectEnvironmentDefinition`, `ProjectResourceNamingTemplate`, `ProjectResourceAbbreviation`, `GitRepositoryConfiguration`, `ProjectPipelineVariableGroup` | Groups InfrastructureConfigs; owns membership/RBAC, default environments, naming conventions, optional Git push config, shared pipeline variable groups, `AgentPoolName` (self-hosted pool for pipelines), project-level resource abbreviation overrides |
| `InfrastructureConfig` | `InfrastructureConfig` | `ParameterDefinition`, `ResourceParameterUsage`, `ResourceNamingTemplate`, `ResourceAbbreviationOverride`, `CrossConfigResourceReference` | Has `ProjectId` FK to Project. Environments inherited from parent Project. Config-level resource abbreviation overrides. |
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

## AzureResource.AssignedUserAssignedIdentityId [2026-04-02]

`AzureResource` has an optional nullable FK `AssignedUserAssignedIdentityId` → `UserAssignedIdentity`. Methods: `AssignUserAssignedIdentity(id)`, `UnassignUserAssignedIdentity()`. Bicep engine honors this for identity block injection.

## ContainerApp.DockerImageName [2026-04-02]

`ContainerApp` owns `DockerImageName` at the resource level (not per-env). The `containerImage` property was removed from `ContainerAppEnvironmentSettings`. Bicep generator reads `resource.Properties["dockerImageName"]`.

## Application Pipeline Properties [2026-04-04]

3 compute aggregates now have CI/CD pipeline config properties:
- **ContainerApp**: `DockerfilePath` (string?), `ApplicationName` (string?)
- **WebApp/FunctionApp**: `DockerfilePath`, `SourceCodePath`, `BuildCommand`, `ApplicationName` (all string?)
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
- `CustomDomain` stores `EnvironmentName`, normalized `DomainName`, and `BindingType` (`SniEnabled` or `Disabled`). Duplicate `(EnvironmentName, DomainName)` pairs are rejected.
- Custom domains are supported for compute resources only (ContainerApp, WebApp, FunctionApp) and are blocked on `IsExisting` resources.
- `SecureParameterMapping` stores `SecureParameterName`, optional `VariableGroupId`, and `PipelineVariableName` so a secure Bicep param can be injected from an Azure DevOps variable group.
- `AzureResource.SetSecureParameterMapping(...)` acts as upsert/clear: `null` group clears an existing mapping, inconsistent half-filled mappings are rejected.

## Domain Invariants

- `Project.Members` is `IReadOnlyCollection<ProjectMember>` — mutated via `AddMember()`, `ChangeRole()`, `RemoveMember()`.
- `InfrastructureConfig` has a `ProjectId` FK. Access checks resolved via **project membership** — `IInfraConfigAccessService`.
- `AzureResource` inheritance uses EF Core **TPT**: `HasBaseType<AzureResource>().ToTable("...")`.

## Domain Code Quality Rules [2026-03-30]

- All domain classes must have XML `<summary>` docs.
- Concrete aggregates inheriting from `AzureResource` must be declared `sealed`.
- All `EnumValueObject<T>`-derived classes must be declared `sealed` [2026-04-16].
- Value object properties must use `private set`.
- Error strings must be in English.

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

## Error Definitions

Errors live in `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.*.cs` as partial static classes. When adding a new aggregate, add `Errors.AggregateName.cs`. Convention: no inline `Error.*()` calls in handlers — always use `Errors.AggregateName.MethodName()`.
