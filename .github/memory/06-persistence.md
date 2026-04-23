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

## Key Conventions

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
- `SingleValueConverter<TValueObject, TPrimitive>` — single-value objects
- `EnumValueConverter<TEnumValueObject, TEnum>` — enum value objects as strings

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

## Polymorphic TPT Queries [2026-04-16]

- `ProjectDbContext` must include `DbSet<AzureResource> AzureResources` for polymorphic TPT queries that need to resolve any resource type by ID without knowing the concrete type.
- Used by `AzureResourceBaseRepository` and `ResourceGroupRepository` for cross-type lookups.

## SQL Read Views [2026-04-23]

- `ProjectDbContext` maps `vw_ResourceEnvironmentEntries` and `vw_ChildToParentLinks` as keyless read models (`ResourceEnvironmentEntryView`, `ChildToParentLinkView`).
- `ResourceGroupRepository` uses these views through `GetConfiguredEnvironmentsByResourceGroupAsync()` and `GetChildToParentMappingAsync()` so Application handlers do not need to know all typed environment-setting tables or child-resource TPT tables.
- `ListProjectResourcesQueryHandler` still lists project resources via `GetByInfraConfigIdAsync()` with `Include(r => r.Resources)`; the views support adjacent resource-read scenarios like `ListResourceGroupResources` and incoming cross-config reference resolution.

## Resource Group Storage List Optimization [2026-04-23]

- `ListResourceGroupResourcesQueryHandler` enriches Storage Accounts with lightweight child collections through `IResourceGroupRepository.GetStorageSubResourcesByStorageAccountIdsAsync()`.
- `ResourceGroupRepository` intentionally uses 3 narrow batch queries over `BlobContainers`, `StorageQueues`, and `StorageTables` filtered by Storage Account IDs, instead of loading full StorageAccount aggregates or adding a new SQL view/migration.
- This keeps the first Resource Group list to a single HTTP payload while avoiding the previous frontend N+1 pattern (`GET /storage-accounts/{id}` per account).

## Repository Naming Conventions [2026-04-16]

- `GetByContainedResourceIdAsync` — finds a parent entity (e.g. ResourceGroup) by a child resource's ID. Renamed from the ambiguous `GetByResourceIdAsync`.
- Convention: use `ByContainedXxx` prefix when the lookup navigates from child to parent.

## Multi-Repo Topology V1 — EF [2026-04-23]

- Nouvelle table `ProjectRepositories` (Id, ProjectId FK Cascade, Alias, ProviderType, RepositoryUrl, Owner, RepositoryName, DefaultBranch, ContentKinds CSV string).
- Index unique `(ProjectId, Alias)` pour empêcher doublons d'alias dans un projet.
- `RepositoryAliasConverter` et `RepositoryContentKindsConverter` (custom car `RepositoryAlias` ctor privé et `RepositoryContentKinds` est un VO Flags non `SingleValueObject`).
- `InfrastructureConfigs` étendu avec 4 colonnes inline owned : `RepositoryBinding_Alias`, `_Branch`, `_InfraPath`, `_PipelinePath` (toutes nullable). Configuré via `OwnsOne(RepositoryBinding) + Navigation.IsRequired(false)`.
- Colonne `Projects.RepositoryMode` **renommée** `LayoutPreset` (1 migration, pas une suppression). Backfill `'MonoRepo' → 'AllInOne'`, autre → `'MultiRepo'`. Nouvelle colonne `CommonsStrategy` (default `'DuplicatePerRepo'`).
- Backfill SQL automatique : pour chaque `GitRepositoryConfigurations` existant, INSERT 1 row dans `ProjectRepositories` avec `Alias='default'`, `ContentKinds='Infrastructure,Pipelines'` ; UPDATE InfraConfigs pour binder leur `RepositoryBinding_Alias='default'`.
- 1 seule migration consolidée `AddMultiRepoTopologyV1` (timestamp 20260423151014) — décision pragmatique car EF auto-gen produit toujours 1 diff entre snapshot et état Domain.
- `GitRepositoryConfigurations` table **dropped en V3** (migration `RemoveLegacyGitRepositoryConfiguration` du 2026-04-23). Rollback possible via `Down()` (recrée la table avec FK cascade vers `Projects`).

## Legacy Repository Topology Repair [2026-04-23]

- After the V3 removal of `RepositoryContentKindsEnum.Pipelines`, old rows persisted as `Infrastructure,Pipelines` or `ApplicationCode,Pipelines` can no longer be materialized safely by `RepositoryContentKindsConverter`; they must be normalized in the database first.
- Reusable repair script: `scripts/fix-legacy-repository-topology.ps1`.
- The script is idempotent and currently performs three repairs on PostgreSQL:
    - removes the legacy `Pipelines` token from both `ProjectRepositories.ContentKinds` and `InfraConfigRepositories.ContentKinds`;
    - upgrades a single-repository `AllInOne` project to `Infrastructure,ApplicationCode` when its lone repo was still infra-only or app-only after normalization;
    - upgrades an `AllInOne` project to `SplitInfraCode` when the persisted topology is actually two repos (one infra, one app code).
- It auto-detects the local Aspire `postgres:17.6` container, reads `POSTGRES_PASSWORD`, and patches `infraDb` through `psql`.

## Migrations
17+ migration files in `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`. Always add a new migration when changing domain model.
