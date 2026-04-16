# Persistance EF Core — Repositories, Configurations et Conventions

## Vue d'ensemble

La couche Infrastructure (`InfraFlowSculptor.Infrastructure`) implémente la persistance via **Entity Framework Core** avec **PostgreSQL**. Elle fournit :

1. **Le DbContext** — Point d'accès à la base de données
2. **Les configurations d'entités** — Comment chaque type de domaine est mappé en table
3. **Les repositories** — Abstraction entre les handlers et EF Core
4. **Le Unit of Work** — Persistance atomique de tous les changements d'une commande
5. **Les converters** — Conversion des value objects en types SQL

---

## DbContext

```
Fichier : src/Api/InfraFlowSculptor.Infrastructure/Persistence/ProjectDbContext.cs
```

Le `ProjectDbContext` déclare les `DbSet<>` pour chaque agrégat et applique automatiquement toutes les configurations :

```csharp
public class ProjectDbContext : DbContext
{
    public DbSet<KeyVault> KeyVaults { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<StorageAccount> StorageAccounts { get; set; }
    // ... un DbSet par agrégat

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Découverte automatique de toutes les IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

---

## Configurations d'entités

Chaque agrégat a sa propre classe de configuration EF Core dans `Infrastructure/Persistence/Configurations/` :

```
Fichier : src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/KeyVaultConfiguration.cs
```

```csharp
public class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault>
{
    public void Configure(EntityTypeBuilder<KeyVault> builder)
    {
        // TPT : table dédiée, hérite de AzureResource
        builder.HasBaseType<AzureResource>()
            .ToTable("KeyVaults");

        // Collection d'entités enfants
        builder.HasMany(kv => kv.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.KeyVaultId)
            .OnDelete(DeleteBehavior.Cascade);

        // Accès au champ privé pour l'encapsulation DDD
        builder.Navigation(kv => kv.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

### TPT — Table Per Type

Le projet utilise l'héritage **Table Per Type (TPT)** pour les ressources Azure. Cela signifie :

- **Une table de base** `AzureResources` contient les colonnes communes (`Name`, `Location`, `ResourceGroupId`)
- **Une table dérivée** par type (`KeyVaults`, `StorageAccounts`, `RedisCaches`…) contient les colonnes spécifiques
- EF Core fait un `JOIN` automatiquement quand on charge un `KeyVault`

```
┌─────────────────────────┐
│     AzureResources      │  ← Table de base (Name, Location, ResourceGroupId)
├─────────────────────────┤
│ Id (PK) | Name | Loc... │
└──────┬──────────────────┘
       │ JOIN sur Id
┌──────┴──────────────────┐
│      KeyVaults          │  ← Table dérivée (colonnes spécifiques KeyVault)
├─────────────────────────┤
│ Id (FK/PK)              │
└─────────────────────────┘
```

Configuration :
```csharp
builder.HasBaseType<AzureResource>().ToTable("KeyVaults");
```

### Support de l'encapsulation DDD

EF Core doit pouvoir remplir les collections privées du domaine (ex: `_environmentSettings`). La configuration indique à EF Core d'utiliser le **champ privé** plutôt que la propriété publique :

```csharp
builder.Navigation(kv => kv.EnvironmentSettings)
    .HasField("_environmentSettings")           // champ privé dans le domaine
    .UsePropertyAccessMode(PropertyAccessMode.Field);  // écrit directement dedans
```

Cela permet au domain de garder sa collection encapsulée (`IReadOnlyCollection<T>` en public) tout en laissant EF Core la remplir au chargement.

---

## Converters — Value Objects ↔ SQL

Les value objects du domaine doivent être convertis en types SQL natifs. Le projet fournit des converters réutilisables :

```
Dossier : src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/Converters/
```

| Converter | Conversion | Usage |
|-----------|-----------|-------|
| `IdValueConverter<TId>` | `Id<T>` ↔ `Guid` | Tous les ID typés (`AzureResourceId`, `ProjectId`…) |
| `SingleValueConverter<TVO, T>` | `SingleValueObject<T>` ↔ `T` | `Name` ↔ `string`, `Location` ↔ `string` |
| `EnumValueConverter<TVO, TEnum>` | `EnumValueObject<TEnum>` ↔ `string` | `Sku` ↔ `"Standard"`, `OsType` ↔ `"Linux"` |

### Utilisation dans une configuration

```csharp
public void Configure(EntityTypeBuilder<Something> builder)
{
    // ID typé → Guid en base
    builder.ConfigureAggregateRootId<Something, SomethingId>();

    // Value object mono-valeur → string en base
    builder.Property(x => x.Name)
        .HasConversion(new SingleValueConverter<Name, string>());

    // Enum value object → string en base
    builder.Property(x => x.Status)
        .HasConversion(new EnumValueConverter<Status, StatusEnum>());
}
```

`ConfigureAggregateRootId<T, TId>()` est une extension helper qui configure la clé primaire avec le converter d'ID approprié.

---

## Repository Pattern

### Interface (couche Application)

Les interfaces de repository sont définies dans la couche Application, ce qui garantit que les handlers ne dépendent pas d'EF Core :

```
Fichier : src/Api/InfraFlowSculptor.Application/Common/Interfaces/IRepository.cs
```

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(ValueObject id, Func<IQueryable<T>, IQueryable<T>> queryBuilder, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(ValueObject id);
}
```

La surcharge `GetByIdAsync(id, queryBuilder)` permet de personnaliser la requête (eager loading via `.Include()` / `.ThenInclude()`) de manière standardisée, sans créer de méthodes spécifiques par repository :

```csharp
// Exemple d'utilisation dans un handler
var keyVault = await repository.GetByIdAsync(
    id,
    q => q.Include(kv => kv.EnvironmentSettings)
          .Include(kv => kv.RoleAssignments),
    cancellationToken);
```

Chaque agrégat étend cette interface avec des méthodes spécialisées :

```csharp
public interface IKeyVaultRepository : IRepository<KeyVault>
{
    Task<KeyVault?> GetByIdWithEnvironmentSettingsAsync(
        AzureResourceId id, CancellationToken ct = default);
}
```

### Convention de nommage des méthodes repository

| Patron | Signification | Exemples |
|--------|--------------|---------|
| `GetByIdAsync(id)` | Récupère l'agrégat par sa **propre clé primaire**, sans eager loading | `IRepository<T>.GetByIdAsync` |
| `GetByIdAsync(id, queryBuilder)` | Récupère par clé primaire avec eager loading **standardisé** via un `Func<IQueryable<T>, IQueryable<T>>` | `repository.GetByIdAsync(id, q => q.Include(e => e.Nav))` |
| `GetByIdWith{Navigation}Async(id)` | Récupère par clé primaire **avec** chargement eager des navigations spécifiées (méthodes spécialisées legacy) | `GetByIdWithRoleAssignmentsAsync`, `GetByIdWithMembersAsync`, `GetByIdWithAllAsync` |
| `GetBy{EntityType}IdAsync(foreignId)` | Filtre par **clé étrangère** d'une entité liée | `GetByResourceGroupIdAsync`, `GetByProjectIdAsync`, `GetBySqlServerIdAsync` |
| `GetByContained{EntityType}IdAsync(id)` | Trouve l'agrégat qui **contient** l'entité enfant indiquée | `GetByContainedResourceIdAsync` dans `IResourceGroupRepository` |
| `GetAllForXAsync(x)` | Retourne tous les agrégats accessibles pour un contexte donné | `GetAllForUserAsync` |

> **Règle d'or :** le nom du paramètre doit toujours refléter ce par quoi on filtre. Éviter les noms génériques comme `id` lorsque la méthode filtre sur une clé étrangère — préférer `resourceGroupId`, `projectId`, etc.

### Implémentation (couche Infrastructure)

```
Fichier : src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/BaseRepository.cs
```

```csharp
public abstract class BaseRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext Context;

    protected BaseRepository(TContext context) => Context = context;

    public virtual async Task<T?> GetByIdAsync(ValueObject id, CancellationToken ct)
        => await Context.Set<TEntity>().FindAsync(ct, id);

    public virtual async Task<TEntity?> GetByIdAsync(
        ValueObject id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder,
        CancellationToken ct = default)
    {
        var query = queryBuilder(Context.Set<TEntity>());
        // Builds a predicate from the EF Core model primary key metadata
        return await query.FirstOrDefaultAsync(/* e => e.Id == id */, ct);
    }

    public virtual Task<TEntity> AddAsync(TEntity entity)
    {
        var res = Context.Set<TEntity>().Add(entity);
        return Task.FromResult(res.Entity);  // Track uniquement, pas de SaveChanges
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();
        foreach (var include in includes)
            query = query.Include(include);
        return await query.ToListAsync();
    }

    // ... UpdateAsync, DeleteAsync (même pattern : track sans SaveChanges)
}
```

> **Important — Unit of Work :** Les repositories ne doivent **jamais** appeler `SaveChangesAsync()`. Ils se contentent de tracker les changements dans le DbContext. C'est le `UnitOfWorkBehavior` (pipeline MediatR) qui appelle `SaveChangesAsync()` une seule fois après l'exécution réussie du handler. Voir [Unit of Work](unit-of-work.md) pour les détails.

Les repositories spécialisés héritent de `BaseRepository` et ajoutent des queries avec `Include` :

```csharp
public class KeyVaultRepository(ProjectDbContext context)
    : BaseRepository<KeyVault, ProjectDbContext>(context), IKeyVaultRepository
{
    public async Task<KeyVault?> GetByIdWithEnvironmentSettingsAsync(
        AzureResourceId id, CancellationToken ct)
        => await Context.KeyVaults
            .Include(kv => kv.EnvironmentSettings)
            .FirstOrDefaultAsync(kv => kv.Id == id, ct);
}
```

### Enregistrement DI

Tous les repositories sont enregistrés dans `Infrastructure/DependencyInjection.cs` :

```csharp
private static IServiceCollection AddRepositories(this IServiceCollection services)
{
    services.AddScoped<IKeyVaultRepository, KeyVaultRepository>();
    services.AddScoped<IStorageAccountRepository, StorageAccountRepository>();
    services.AddScoped<IProjectRepository, ProjectRepository>();
    // ... un par agrégat
    return services;
}
```

---

## ⚠️ Piège EF Core : Comparaison de Value Objects

**Règle critique :** Dans les requêtes LINQ-to-EF, toujours comparer les value objects **entiers**, jamais `.Value` :

```csharp
// ✅ CORRECT — EF Core utilise le converter pour traduire en SQL
await Context.KeyVaults.FirstOrDefaultAsync(kv => kv.Id == id, ct);

// ❌ INCORRECT — EF Core ne peut pas traduire .Value dans LINQ
await Context.KeyVaults.FirstOrDefaultAsync(kv => kv.Id.Value == id.Value, ct);
// → InvalidOperationException: The LINQ expression could not be translated
```

EF Core utilise les `ValueConverter<>` enregistrés pour traduire automatiquement `kv.Id == id` en `WHERE "Id" = @p0` (comparaison Guid en SQL). L'accès à `.Value` n'est pas traduisible.

---

## Migrations

Les migrations EF Core sont dans `Infrastructure/Migrations/`. Quand le modèle de domaine change :

```bash
# Depuis la racine du projet
dotnet ef migrations add NomDeLaMigration \
    --project src/Api/InfraFlowSculptor.Infrastructure \
    --startup-project src/Api/InfraFlowSculptor.Api
```

Les migrations sont appliquées automatiquement au démarrage de l'application.

---

## Pages connexes

- [Unit of Work](unit-of-work.md) — Persistance atomique via le pipeline MediatR
- [Domain-Driven Design](ddd-concepts.md) — Les value objects et entités mappés par EF Core
- [CQRS et MediatR](cqrs-patterns.md) — Les handlers qui utilisent les repositories
- [Couche API](api-layer.md) — Le point d'entrée HTTP
- [Guide de navigation](getting-started.md) — Comment ajouter un nouveau type de ressource
