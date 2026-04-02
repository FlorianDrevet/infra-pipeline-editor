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

## Migrations
14+ migration files in `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`. Always add a new migration when changing domain model.
