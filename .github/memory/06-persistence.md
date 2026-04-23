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

## Repository Naming Conventions [2026-04-16]

- `GetByContainedResourceIdAsync` — finds a parent entity (e.g. ResourceGroup) by a child resource's ID. Renamed from the ambiguous `GetByResourceIdAsync`.
- Convention: use `ByContainedXxx` prefix when the lookup navigates from child to parent.

## Migrations
17+ migration files in `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`. Always add a new migration when changing domain model.
