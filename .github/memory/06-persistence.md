# Persistence (EF Core)

## DbContext
- `ProjectDbContext` at `src/Api/InfraFlowSculptor.Infrastructure/Persistence/ProjectDbContext.cs`
- PostgreSQL target, `ApplyConfigurationsFromAssembly()`

## Entity Configuration Pattern
```csharp
public sealed class SomethingConfiguration : IEntityTypeConfiguration<Something>
{
    public void Configure(EntityTypeBuilder<Something> builder)
    {
        builder.ToTable("Somethings");
        builder.HasKey(x => x.Id);
        builder.ConfigureAggregateRootId<Something, SomethingId>();
        builder.Property(x => x.Name).HasConversion(new SingleValueConverter<Name, string>());
    }
}
```

## Canonical String Lengths [2026-05-12]

- Core persisted string/value-object columns must declare `HasMaxLength(...)` explicitly instead of relying on provider default `text` columns.
- Current canonical matrix for the DB-001 sweep:
    - `Project.Name` = `80`
    - `Project.DefaultNamingTemplate` = `500`
    - `ProjectEnvironmentDefinition.Name` = `100`
    - `ProjectEnvironmentDefinition.ShortName` = `20`
    - `ProjectEnvironmentDefinition.Prefix` / `Suffix` = `50`
    - `InfrastructureConfig.Name` = `100`
    - `InfrastructureConfig.DefaultNamingTemplate` = `500`
    - `ResourceGroup.Name` = `90`
    - `AzureResource.Name` / `CustomNameOverride` = `260`
    - `ParameterDefinition.Name` = `100`
    - `ParameterDefinition.Type` = `20`
    - `ParameterDefinition.DefaultValue` = `500`
    - `ProjectResourceNamingTemplate.Template` / `ResourceNamingTemplate.Template` = `500`
- When a persistence cap is introduced on a create flow, align the application validator and request contract in the same change set to fail fast before SQL (done for `CreateProject` and `CreateInfrastructureConfig`).
- `ResourceGroup.Name` is capped from the official Azure `Microsoft.Resources/resourcegroups` rule (`1-90`); the repo now aligns `CreateResourceGroup` request + validator with that bound instead of letting SQL reject it late.
- `ParameterDefinition` now follows the same explicit-persistence rule even without a dedicated create/update API surface yet; if such a surface is added later, align its validator/contract in the same change set.

## Key Conventions

### Index coverage verification must use a relational provider [2026-05-12]

- For EF Core index metadata, do not rely on the InMemory provider as the sole observation point when checking convention-generated FK indexes.
- `IndexCoverageConfigurationTests` uses a Npgsql-configured `ProjectDbContext` without opening a connection so the relational model exposes the effective index coverage seen by migrations/snapshot.
- Verified DB-002-obsolete coverage in the current model:
    - explicit indexes: `InfrastructureConfig.ProjectId`, `AzureResource.ResourceType`
    - convention/FK indexes: `AzureResource.ResourceGroupId`, `ResourceGroup.InfraConfigId`, `AppSetting.SourceResourceId`, `AppSetting.KeyVaultResourceId`, `AppConfigurationKey.SourceResourceId`, `AppConfigurationKey.KeyVaultResourceId`
    - composite coverage: unique `RoleAssignment(SourceResourceId, TargetResourceId, UserAssignedIdentityId, RoleDefinitionId)` already covers source-target lookups

### Ignore computed navigations over shared backing field
When an aggregate exposes one persisted collection plus filtered/computed projections over the same backing field, map only the persisted navigation and add `builder.Ignore(...)` for every computed projection.

### OwnsMany + IReadOnlyCollection backing field must be explicit
Always add a `Navigation` hint after every `OwnsMany` targeting an `IReadOnlyCollection` property:
```csharp
builder.OwnsMany(p => p.Tags, tag => { ... });
builder.Navigation(p => p.Tags).HasField("_tags").UsePropertyAccessMode(PropertyAccessMode.Field);
```

## Converters
- `IdValueConverter<TId>` — ID value objects ↔ Guid
- `NullableIdValueConverter<TId>` — optional ID value objects ↔ nullable Guid (use this instead of `IdValueConverter<TId>` on `IsRequired(false)` properties)
- `SingleValueConverter<TValueObject, TPrimitive>` — single-value objects
- `EnumValueConverter<TEnumValueObject, TEnum>` — enum value objects as strings
- `NullableEnumValueConverter<TEnumValueObject, TEnum>` — optional enum value objects ↔ nullable string (use this instead of `EnumValueConverter<...>` on `IsRequired(false)` properties)

## Repository Pattern
- Interface in Application layer, implementation in Infrastructure
- `BaseRepository<T, TContext>` — `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- **⚠️ CRITICAL:** Never use `x.Id.Value == id.Value` in LINQ-to-EF. Always compare whole value objects: `x.Id == id`. EF uses `IdValueConverter<T>` to translate.
- **Namespace note:** `IInfrastructureConfigRepository` uses fully-qualified type name to avoid CS0118 ambiguity.

## FK Cascade / Delete Pitfalls [2026-04-04]

When adding cross-resource FKs (e.g. `SourceResourceId`, `KeyVaultResourceId`, `TargetResourceId`), think through the cascade path on parent deletion:
- **Restrict** causes FK violations when Cascade-delete on `AzureResources` runs before the referencing rows are removed.
- **SetNull** is safe for optional FKs (e.g. `AppSettings.SourceResourceId`, `AppConfigurationKeys.KeyVaultResourceId`).
- **Cascade** is safe for mandatory child relationships (e.g. `ResourceLinks.SourceResourceId`, `AzureResourceDependencies.DependsOnId`, `ResourceParameterUsages.ParameterId`, `RoleAssignment.TargetResourceId`).
- EF Core ordering conflict: if a Cascade-delete on parent already orphans rows, a parallel SetNull on the same rows emits SQL after the rows are gone → FK error. Solution: make both paths Cascade.
- Concrete rule [2026-04-23]: `AppSettingConfiguration.KeyVaultResourceId` must stay `SetNull`, not `Cascade`, so deleting a Key Vault detaches optional app-setting references instead of silently deleting the settings themselves.
- Concrete rule [2026-05-12]: `AppSettingConfiguration.SourceResourceId` must also stay `SetNull`; deleting a referenced source resource should invalidate the output-link mapping without deleting the `AppSetting` row. The fix is materialized by migration `20260512121558_SetNullOnAppSettingSourceResource`.

## Polymorphic TPT Queries [2026-04-16]

- `ProjectDbContext` must include `DbSet<AzureResource> AzureResources` for polymorphic TPT queries that need to resolve any resource type by ID without knowing the concrete type.
- Used by `AzureResourceBaseRepository` and `ResourceGroupRepository` for cross-type lookups.

## SQL Read Views [2026-04-23]

- `ProjectDbContext` maps `vw_ResourceEnvironmentEntries` and `vw_ChildToParentLinks` as keyless read models (`ResourceEnvironmentEntryView`, `ChildToParentLinkView`).
- `ResourceGroupRepository.GetConfiguredEnvironmentsByResourceGroupAsync(...)` is the canonical consumer of `vw_ResourceEnvironmentEntries`: the old audit finding about 16 sequential environment-settings queries is obsolete on the current codebase because the method now executes a single read-only query over that view.
- `ResourceGroupRepository` uses these views through `GetConfiguredEnvironmentsByResourceGroupAsync()` and `GetChildToParentMappingAsync()` so Application handlers do not need to know all typed environment-setting tables or child-resource TPT tables.
- `ListProjectResourcesQueryHandler` still lists project resources via `GetByInfraConfigIdAsync()` with `Include(r => r.Resources)`; the views support adjacent resource-read scenarios like `ListResourceGroupResources` and incoming cross-config reference resolution.

## Read-Only Authorization Lookups [2026-05-12]

- Do not add `.AsNoTracking()` blindly to a tracked repository method if that method is shared by read and write/owner flows.
- `ProjectAccessService` is the reference split for DB-003: `VerifyReadAccessAsync(...)` now uses `IProjectRepository.GetByIdWithMembersReadOnlyAsync(...)`, while `VerifyWriteAccessAsync(...)` and `VerifyOwnerAccessAsync(...)` keep using the tracked `GetByIdWithMembersAsync(...)` because several project commands mutate `accessResult.Value` afterward.
- `InfraConfigAccessService` now follows the same split: `VerifyReadAccessAsync(...)` uses `IInfrastructureConfigRepository.GetByIdReadOnlyAsync(...)`, while `VerifyWriteAccessAsync(...)` stays on the tracked `GetByIdAsync(...)` path because write flows may continue mutating or depending on the loaded aggregate state.
- `ResourceGroupRepository.GetByIdReadOnlyAsync(...)` is the reference split for pure `ResourceGroup` queries that only need group metadata and `InfraConfigId`: `GetResourceGroupQueryHandler` and `ListResourceGroupResourcesQueryHandler` now use this no-tracking lookup, while commands keep the tracked `GetByIdAsync(...)` path.
- The same `ResourceGroupRepository.GetByIdReadOnlyAsync(...)` split is now the reference for adjacent read-only handlers that only need `InfraConfigId` for authorization: `GetKeyVaultQueryHandler`, `ListKeyVaultsQueryHandler`, `GetRedisCacheQueryHandler`, `ListRedisCachesQueryHandler`, and `ListStorageAccountsQueryHandler` all use the read-only lookup instead of the tracked path.
- When the caller only needs membership/role checks, prefer a dedicated no-tracking lookup that loads only the navigation data actually needed for authorization.
- Counter-example: `PersonalAccessTokenRepository.GetByTokenHashAsync(...)` must stay tracked in the current auth flow because `PersonalAccessTokenAuthenticationHandler` records PAT usage and persists `LastUsedAt` immediately after loading the aggregate.

## Resource Group Storage List Optimization [2026-04-23]

- `ListResourceGroupResourcesQueryHandler` enriches Storage Accounts with lightweight child collections through `IResourceGroupRepository.GetStorageSubResourcesByStorageAccountIdsAsync()`.
- `ResourceGroupRepository` intentionally uses 3 narrow batch queries over `BlobContainers`, `StorageQueues`, and `StorageTables` filtered by Storage Account IDs, instead of loading full StorageAccount aggregates or adding a new SQL view/migration.
- This keeps the first Resource Group list to a single HTTP payload while avoiding the previous frontend N+1 pattern (`GET /storage-accounts/{id}` per account).

## Repository Naming Conventions [2026-04-16]

- `GetByContainedResourceIdAsync` — finds a parent entity (e.g. ResourceGroup) by a child resource's ID. Renamed from the ambiguous `GetByResourceIdAsync`.
- Convention: use `ByContainedXxx` prefix when the lookup navigates from child to parent.

## App Settings Eager-Loading Pitfall [2026-05-11]

- `AzureResourceBaseRepository.GetByIdWithRoleAssignmentsAndAppSettingsAsync(...)` must eager-load `AppSettings -> EnvironmentValues`, not just `AppSettings`.
- `ListAppSettingsQueryHandler` maps static app-setting values from `AppSetting.EnvironmentValues`; if the repository skips that `ThenInclude`, the UI still sees the setting names after reload but loses the per-environment values.
- The regression is covered by `tests/InfraFlowSculptor.Infrastructure.Tests/Persistence/Repositories/AzureResourceBaseRepositoryTests.cs`, which persists a static app setting, reloads it through the repository in a fresh context, and asserts the environment values are still present.

## Layout-Driven Repository Configuration [2026-04-23]

- `ProjectDbContext` now exposes both `ProjectRepositories` and `InfraConfigRepositories`.
- `ProjectRepositories` and `InfraConfigRepositories` both persist `RepositoryContentKinds` through `RepositoryContentKindsConverter`; valid flags are now only `Infrastructure` and `ApplicationCode`.
- `InfrastructureConfigs.LayoutMode` is a nullable enum-backed column (`ConfigLayoutMode`) and must use `NullableEnumValueConverter<ConfigLayoutMode, ConfigLayoutModeEnum>()`; same rule for other optional enum-backed columns such as `ProjectRepositories.ProviderType`.
- Optional strongly typed ID columns (for example `ContainerRegistryId` / `LogAnalyticsWorkspaceId` references on resource aggregates) must use `NullableIdValueConverter<TId>` rather than `IdValueConverter<TId>` to keep EF Core nullable mappings warning-free.
- `InfraConfigRepositories` is a dedicated child table with cascade delete and a unique `(InfrastructureConfigId, Alias)` index.
- `LayoutDrivenRepoConfiguration` removed `Projects.CommonsStrategy` and the inline `InfrastructureConfigs.RepositoryBinding_*` columns, and added `InfrastructureConfigs.LayoutMode` plus `InfraConfigRepositories`.
- `RemoveLegacyGitRepositoryConfiguration` dropped the old `GitRepositoryConfigurations` table. The presence of that table in historical migrations or designer snapshots is legacy history only, not the current model.

## Legacy Repository Topology Repair [2026-04-23]

- `scripts/fix-legacy-repository-topology.ps1` is the one-off database repair for rows persisted before `RepositoryContentKindsEnum.Pipelines` was removed.
- The script is idempotent and currently:
    - removes the legacy `Pipelines` token from both `ProjectRepositories.ContentKinds` and `InfraConfigRepositories.ContentKinds`;
    - upgrades one-repo `AllInOne` rows to `Infrastructure,ApplicationCode` when needed;
    - upgrades pseudo-two-repo `AllInOne` rows to `SplitInfraCode` when the persisted topology is really infra repo + app repo.
- It auto-detects the local Aspire `postgres:17.6` container and patches `infraDb` through `psql` executed as the container `postgres` user (no password extraction in the script).

## Duplicate Role Assignment Guard [2026-04-23]

- `RoleAssignmentConfiguration` now enforces a unique index on `(SourceResourceId, TargetResourceId, UserAssignedIdentityId, RoleDefinitionId)` so duplicate RBAC rows are rejected at the database level as well as in application logic.

## Demo Snapshot Invariants [2026-04-25]

- For Container Apps using Managed Identity ACR auth with a user-assigned `AcrPull` role, `AzureResource.AssignedUserAssignedIdentityId` must also point to that same UAI; otherwise Bicep generation falls back to the system identity for `acrManagedIdentityClientId`.
- The API JWT secret app setting must be stored as `JwtSettings__Secret`; `JWT_SECRET` is the Key Vault secret name, not the ASP.NET configuration key bound from `JwtSettings:Secret`.

## Migrations
17+ migration files in `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`. Always add a new migration when changing domain model.
- `20260512091902_AddCoreStringLengthConstraints` adds the first DB-001 migration slice for the core project / environment / infra-config / naming-template / AzureResource columns and keeps the snapshot in sync.
- `20260512095600_AddResourceGroupNameLengthConstraint` adds the follow-up DB-001 slice that constrains `ResourceGroup.Name` to `varchar(90)`.
- `20260512140453_AddParameterDefinitionLengthConstraints` closes DB-001 by constraining `ParameterDefinition.Name` / `Type` / `DefaultValue` to `varchar(100)` / `varchar(20)` / `varchar(500)`.
