# Domain Model

## Aggregates

| Aggregate | Root | Key Entities | Notes |
|---|---|---|---|
| `Project` | `Project` | `ProjectMember`, `ProjectEnvironmentDefinition`, `ProjectResourceNamingTemplate`, `GitRepositoryConfiguration`, `ProjectPipelineVariableGroup` | Groups InfrastructureConfigs; owns membership/RBAC, default environments, naming conventions, optional Git push config, shared pipeline variable groups |
| `InfrastructureConfig` | `InfrastructureConfig` | `ParameterDefinition`, `ResourceParameterUsage`, `ResourceNamingTemplate`, `CrossConfigResourceReference` | Has `ProjectId` FK to Project. Environments inherited from parent Project. |
| `ResourceGroup` | `ResourceGroup` | `AzureResource` (base) | Hosts Azure resources. No child entities — `ResourceEnvironmentConfig` was removed. |
| `KeyVault` | extends `AzureResource` | `KeyVaultEnvironmentSettings` | TPT in EF Core |
| `RedisCache` | extends `AzureResource` | `RedisCacheEnvironmentSettings` | TPT in EF Core |
| `StorageAccount` | extends `AzureResource` | `StorageAccountEnvironmentSettings`, `BlobContainer`, `StorageQueue`, `StorageTable`, `CorsRule`, `BlobLifecycleRule` | TPT; sub-resources managed via dedicated CQRS commands |
| `AppServicePlan` | extends `AzureResource` | `AppServicePlanEnvironmentSettings` | TPT; OsType at resource level |
| `WebApp` | extends `AzureResource` | `WebAppEnvironmentSettings` | TPT; FK to AppServicePlan |
| `FunctionApp` | extends `AzureResource` | `FunctionAppEnvironmentSettings` | TPT; FK to AppServicePlan |
| `UserAssignedIdentity` | extends `AzureResource` | — | TPT; simplest resource type |
| `AppConfiguration` | extends `AzureResource` | `AppConfigurationEnvironmentSettings`, `AppConfigurationKey`, `AppConfigurationKeyEnvironmentValue` | TPT; configuration keys with 5 modes |
| `ContainerAppEnvironment` | extends `AzureResource` | `ContainerAppEnvironmentEnvironmentSettings` | TPT; abbreviation `cae` |
| `ContainerApp` | extends `AzureResource` | `ContainerAppEnvironmentSettings` | TPT; FK to ContainerAppEnvironment |
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
- These are persisted in TPT tables (EF Core), exposed in Create/Update commands and contracts

## Cross-Config References

`InfrastructureConfig` owns `_crossConfigReferences` collection of `CrossConfigResourceReference` entities. Each reference points to a `TargetResourceId` in another config of the same project, with an `Alias` and optional `Purpose`. The Bicep generator emits `existing` resource group + `existing` resource declarations for each referenced resource.

## Domain Invariants

- `Project.Members` is `IReadOnlyCollection<ProjectMember>` — mutated via `AddMember()`, `ChangeRole()`, `RemoveMember()`.
- `InfrastructureConfig` has a `ProjectId` FK. Access checks resolved via **project membership** — `IInfraConfigAccessService`.
- `AzureResource` inheritance uses EF Core **TPT**: `HasBaseType<AzureResource>().ToTable("...")`.

## Domain Code Quality Rules [2026-03-30]

- All domain classes must have XML `<summary>` docs.
- Value object properties must use `private set`.
- Error strings must be in English.
- Collection initializers: prefer `= []` over `= new()`.
- `EnumValueObject` types: use primary constructor pattern.

## Error Definitions

Errors live in `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.*.cs` as partial static classes. When adding a new aggregate, add `Errors.AggregateName.cs`. Convention: no inline `Error.*()` calls in handlers — always use `Errors.AggregateName.MethodName()`.
