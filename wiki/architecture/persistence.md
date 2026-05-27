# Persistence

## Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| ORM | Entity Framework Core 10 | Object-relational mapping |
| Database | PostgreSQL 17.6 | Persistent storage |
| Migrations | EF Core Migrations | Schema evolution |
| Concurrency | PostgreSQL `xmin` | Optimistic concurrency control |

---

## Database Context

`ProjectDbContext` is the single database context for the entire application:

```csharp
public class ProjectDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<InfrastructureConfig> InfrastructureConfigs { get; set; }
    public DbSet<ResourceGroup> ResourceGroups { get; set; }
    public DbSet<AzureResource> AzureResources { get; set; } // Polymorphic TPT
    public DbSet<User> Users { get; set; }
    public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }
    // ... keyless views ...
}
```

**Key design:** One context, one connection string (`infraDb`), automatic migrations at startup.

---

## Entity Configuration Pattern

Each aggregate has a sealed `IEntityTypeConfiguration<T>`:

```csharp
public sealed class ContainerAppConfiguration 
    : IEntityTypeConfiguration<ContainerApp>
{
    public void Configure(EntityTypeBuilder<ContainerApp> builder)
    {
        // TPT inheritance
        builder.HasBaseType<AzureResource>().ToTable("ContainerApps");
        
        // Strongly-typed ID converter
        builder.Property(x => x.ContainerAppEnvironmentId)
            .HasConversion(new IdValueConverter<ContainerAppEnvironmentId>());
        
        // Nullable enum value object
        builder.Property(x => x.AcrAuthMode)
            .HasConversion(new NullableEnumValueConverter<AcrAuthMode, AcrAuthModeEnum>());
        
        // String length constraints
        builder.Property(x => x.DockerImageName)
            .HasMaxLength(512);
        
        // Owned entities
        builder.OwnsMany(x => x.EnvironmentSettings, env =>
        {
            env.ToTable("ContainerAppEnvironmentSettings");
            env.HasKey(e => e.Id);
        });
    }
}
```

---

## Type Converters

Custom converters bridge between domain value objects and database primitives:

| Converter | Domain Type → DB Type |
|-----------|----------------------|
| `IdValueConverter<TId>` | `ProjectId` → `Guid` |
| `NullableIdValueConverter<TId>` | `ProjectId?` → `Guid?` |
| `SingleValueConverter<TVO, T>` | `Location` → `string` |
| `EnumValueConverter<TVO, TEnum>` | `AcrAuthMode` → `string` |
| `NullableEnumValueConverter<TVO, TEnum>` | `AcrAuthMode?` → `string?` |

**Pitfall:** Converter expressions must be expression-tree-safe. No `(object?)` casts.

---

## Repository Pattern

### Interface (Application Layer)

```csharp
public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByIdWithMembersAsync(ProjectId id);
    Task<Project?> GetByIdWithRepositoriesAsync(ProjectId id);
    Task<Project?> GetByIdReadOnlyAsync(ProjectId id);
}
```

### Base Repository (Infrastructure Layer)

```csharp
public abstract class BaseRepository<T, TContext> : IRepository<T>
    where T : class
    where TContext : DbContext
{
    public virtual async Task<T?> GetByIdAsync(/* ... */) { }
    public virtual async Task<List<T>> GetAllAsync(/* ... */) { }
    public virtual async Task AddAsync(T entity) { }
    public virtual async Task UpdateAsync(T entity) { }
    public virtual async Task DeleteAsync(T entity) { }
}
```

### Conventions

- **Read-only queries** use `AsNoTracking()` via `GetByIdReadOnlyAsync()`
- **Write operations** use tracked entities
- **Eager loading** is explicit: `GetByIdWithMembersAsync()`, `GetByIdWithSettingsAsync()`
- **Shared include graphs** use private helpers (e.g., `WithSubResources()`)
- **Repositories NEVER call `SaveChangesAsync()`** — the Unit of Work handles it

---

## Concurrency Control

PostgreSQL `xmin` is used for optimistic concurrency on key aggregates:

```csharp
// In configuration
builder.Property<uint>("xmin")
    .HasColumnType("xid")
    .IsRowVersion();
```

**How it works:** EF Core includes `WHERE xmin = @previousValue` in UPDATE statements. If another transaction modified the row, the update returns 0 affected rows and EF throws `DbUpdateConcurrencyException`.

**Applied to:** `Project`, `InfrastructureConfig`, `AzureResource`.

---

## Database Views

Two SQL views provide optimized read paths:

| View | Purpose |
|------|---------|
| `vw_ResourceEnvironmentEntries` | Flat projection of resources × environments |
| `vw_ChildToParentLinks` | Parent-child relationships between resources |

These are mapped as keyless entities in the DbContext.

---

## FK Cascade Strategy

| Relationship | Delete Behavior | Reason |
|-------------|-----------------|--------|
| Project → InfraConfig | Cascade | Config belongs to project |
| InfraConfig → ResourceGroup | Cascade | Group belongs to config |
| ResourceGroup → AzureResource | Cascade | Resource belongs to group |
| AzureResource → AppSetting (source) | SetNull | Setting survives source deletion |
| AzureResource → RoleAssignment (target) | Cascade | Assignment dies with target |
| AzureResource → Dependency | Cascade | Dependency dies with resource |

**Critical rule:** Never use `Restrict` on cross-resource FKs. It causes cascade violations.

---

## String Length Standards

All string columns have explicit max lengths. Key examples:

| Column | Max Length |
|--------|-----------|
| `Project.Name` | 80 |
| `InfrastructureConfig.Name` | 100 |
| `ResourceGroup.Name` | 90 |
| `AzureResource.Name` | 260 |
| `NamingTemplate.Template` | 500 |
| `DockerImageName` | 512 |
| `EnvironmentName` | 100 |

The structural test `CoreStringLengthConfigurationTests` guards that every string column has a configured max length.

---

## Migration Strategy

- Migrations are **code-first** and generated with `dotnet ef migrations add`
- Both API and MCP services apply migrations at startup
- Views (`vw_*`) are dropped and recreated within migrations when underlying columns change
- **Pitfall:** Both API and MCP run migrations concurrently under Aspire — a bad migration drops both services

---

## Performance Optimizations

| Technique | Where | Impact |
|-----------|-------|--------|
| `AsNoTracking()` | Read queries | No change tracking overhead |
| Dedicated read repositories | Query handlers | Projection instead of full load |
| SQL views | Resource/environment reads | Pre-joined data |
| Include helpers | Resource repositories | Avoid N+1 |
| Targeted sub-queries | Storage Account children | 3 batch queries vs full graph |
| PostgreSQL UPSERT | User provisioning | Race-free provisioning |
