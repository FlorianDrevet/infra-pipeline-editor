# Unit of Work — Coordination atomique de la persistance

## Qu'est-ce que le pattern Unit of Work ?

Le **Unit of Work** est un pattern de conception décrit par Martin Fowler dans *Patterns of Enterprise Application Architecture*. Il répond à un problème courant : quand un cas d'utilisation modifie plusieurs entités, comment garantir que **toutes les modifications sont persistées ensemble** (ou qu'aucune ne l'est en cas d'erreur) ?

> « A Unit of Work keeps track of everything you do during a business transaction that can affect the database. When you're done, it figures out everything that needs to be done to alter the database as a result of your work. »
> — Martin Fowler

### Le problème sans Unit of Work

Imaginons un handler qui doit :
1. Créer un `KeyVault` dans un `ResourceGroup`
2. Ajouter un `RoleAssignment` sur ce KeyVault
3. Mettre à jour le compteur de ressources sur l'`InfrastructureConfig`

Sans Unit of Work, chaque repository appelle `SaveChangesAsync()` indépendamment :

```
Handler
  ├── keyVaultRepository.AddAsync(keyVault)      → SaveChanges ✅ (commit 1)
  ├── roleAssignmentService.Add(...)             → SaveChanges ✅ (commit 2)
  └── configRepository.UpdateAsync(config)       → SaveChanges ❌ (erreur !)
```

**Résultat :** Le KeyVault et le RoleAssignment sont créés, mais la config n'est pas mise à jour. La base de données est dans un **état incohérent**.

### La solution : Unit of Work

Avec le Unit of Work, les repositories **ne sauvegardent plus rien**. Ils se contentent de **tracker les changements** dans le DbContext. Une seule invocation de `SaveChangesAsync()` à la fin du pipeline persiste tout ou rien :

```
Handler
  ├── keyVaultRepository.AddAsync(keyVault)      → Track (pas de commit)
  ├── roleAssignmentService.Add(...)             → Track (pas de commit)
  └── configRepository.UpdateAsync(config)       → Track (pas de commit)
  │
  └── UnitOfWorkBehavior → SaveChangesAsync()    → Commit atomique de tout
```

Si le handler retourne une erreur ou lève une exception, `SaveChangesAsync()` n'est **jamais appelé** → aucune modification n'est persistée.

---

## Implémentation dans le projet

### Vue d'ensemble

Le Unit of Work est implémenté comme un **MediatR pipeline behavior** qui s'insère automatiquement dans la chaîne de traitement des commandes :

```
HTTP Request
    │
    ▼
Endpoint Minimal API
    │  Mapster: Request DTO → Command
    │
    ▼
ISender.Send(command)
    │
    ▼
┌─────────────────────────────────┐
│   ValidationBehavior            │   ← Étape 1 : validation
│   FluentValidation              │
│   → erreurs 400 si invalide    │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   UnitOfWorkBehavior            │   ← Étape 2 : wrapper de persistance
│   Appelle le handler            │
│   Si succès → SaveChangesAsync  │
│   Si erreur → rien (rollback)   │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   Command Handler               │   ← Étape 3 : logique métier
│   Mutation d'entités            │
│   Repositories: Add/Update/Del  │
│   → ErrorOr<T>                  │
└─────────────────────────────────┘
```

### Les 3 composants

#### 1. Interface `IUnitOfWork`

```
Fichier : src/Api/InfraFlowSculptor.Application/Common/Interfaces/IUnitOfWork.cs
```

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

L'interface vit dans la couche **Application** (elle ne connaît pas EF Core). Elle expose une seule méthode : persister tous les changements en attente.

#### 2. Implémentation `UnitOfWork`

```
Fichier : src/Api/InfraFlowSculptor.Infrastructure/Persistence/UnitOfWork.cs
```

```csharp
public sealed class UnitOfWork(ProjectDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
```

L'implémentation vit dans la couche **Infrastructure**. Elle délègue simplement au `ProjectDbContext`. Comme tous les repositories partagent le même `DbContext` scopé (un par requête HTTP), un seul `SaveChangesAsync()` persiste **toutes** les modifications de **tous** les repositories utilisés pendant la requête.

#### 3. Pipeline Behavior `UnitOfWorkBehavior`

```
Fichier : src/Api/InfraFlowSculptor.Application/Common/Behaviors/UnitOfWorkBehavior.cs
```

```csharp
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase, IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Exécuter le handler
        var response = await next(cancellationToken);

        // 2. Si le handler retourne des erreurs → NE PAS persister
        if (response.IsError)
            return response;

        // 3. Si succès → persister tous les changements en une seule transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
```

**Points clés :**

| Aspect | Détail |
|--------|--------|
| **Contrainte `ICommandBase`** | Le behavior ne s'applique qu'aux **commandes** (écriture). Les queries ne passent pas par le UoW. |
| **Contrainte `IErrorOr`** | Permet d'inspecter `response.IsError` pour décider si on persiste ou non. |
| **Appel de `next()`** | Délègue au handler (ou au behavior suivant dans le pipeline). |
| **Commit conditionnel** | `SaveChangesAsync` n'est appelé que si `!response.IsError`. |

---

## Interfaces de marquage CQRS

Le Unit of Work repose sur des **interfaces de marquage** qui distinguent les commandes des queries à la compilation.

### Pourquoi des interfaces dédiées ?

Sans interfaces dédiées, tous les messages MediatR utilisent `IRequest<ErrorOr<T>>`. Le `UnitOfWorkBehavior` ne peut pas distinguer une commande (qui doit persister) d'une query (qui ne doit pas persister).

Les interfaces dédiées ajoutent un **marqueur** (`ICommandBase`) que le behavior utilise comme contrainte générique.

### Les interfaces

| Interface | Hérite de | Rôle |
|-----------|-----------|------|
| `ICommandBase` | – | Marqueur non-générique pour la contrainte du UoW |
| `ICommand<TResult>` | `IRequest<ErrorOr<TResult>>`, `ICommandBase` | Commande (écriture) avec résultat typé |
| `IQuery<TResult>` | `IRequest<ErrorOr<TResult>>` | Query (lecture) — pas de marqueur UoW |
| `ICommandHandler<TCmd, TResult>` | `IRequestHandler<TCmd, ErrorOr<TResult>>` | Handler de commande |
| `IQueryHandler<TQuery, TResult>` | `IRequestHandler<TQuery, ErrorOr<TResult>>` | Handler de query |

### Pourquoi `ICommandBase` non-générique ?

Le `UnitOfWorkBehavior` utilise `where TRequest : ICommandBase` comme contrainte générique. Si on avait seulement `ICommand<TResult>`, on aurait besoin de connaître `TResult` dans la contrainte du behavior, ce qui créerait un conflit de types avec `TResponse = ErrorOr<TResult>` de MediatR. Le marqueur non-générique `ICommandBase` contourne ce problème.

### Utilisation dans le code

```csharp
// ✅ Commande — sera traitée par le UnitOfWorkBehavior
public record CreateKeyVaultCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location
) : ICommand<KeyVaultResult>;                      // ← ICommand, pas IRequest

// ✅ Query — ne passe PAS par le UnitOfWorkBehavior
public record GetKeyVaultQuery(
    AzureResourceId Id
) : IQuery<KeyVaultResult>;                        // ← IQuery, pas IRequest

// ✅ Handler de commande
public sealed class CreateKeyVaultCommandHandler(...)
    : ICommandHandler<CreateKeyVaultCommand, KeyVaultResult>  // ← ICommandHandler
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(...) { ... }
}

// ✅ Handler de query
public sealed class GetKeyVaultQueryHandler(...)
    : IQueryHandler<GetKeyVaultQuery, KeyVaultResult>          // ← IQueryHandler
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(...) { ... }
}
```

---

## Enregistrement dans le DI

### Application (`DependencyInjection.cs`)

```csharp
// L'ordre est important : Validation d'abord, puis UoW wraps le handler
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
```

### Infrastructure (`DependencyInjection.cs`)

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

Le `UnitOfWork` est **scopé** (un par requête HTTP), comme le `ProjectDbContext`. Cela garantit que le même DbContext est partagé entre tous les repositories et le UoW dans une même requête.

---

## Règles critiques pour les repositories

### ❌ Les repositories ne doivent JAMAIS appeler `SaveChangesAsync()`

Avant l'ajout du Unit of Work, chaque repository appelait `SaveChangesAsync()` dans ses méthodes `AddAsync()`, `UpdateAsync()`, `DeleteAsync()`. Avec le Unit of Work, cette responsabilité est centralisée :

```csharp
// ❌ AVANT (chaque repository persistait individuellement)
public async Task<T> AddAsync(T entity)
{
    var result = _context.Set<T>().Add(entity);
    await _context.SaveChangesAsync();         // ← INTERDIT maintenant
    return result.Entity;
}

// ✅ APRÈS (le repository track, le UoW persiste)
public async Task<T> AddAsync(T entity)
{
    var result = _context.Set<T>().Add(entity);
    return result.Entity;                      // ← pas de SaveChanges
}
```

### Pourquoi ?

Si un repository appelle `SaveChangesAsync()`, il **brise l'atomicité** : il persiste immédiatement ses propres changements sans attendre que le handler ait terminé toutes ses opérations. En cas d'erreur ultérieure, une partie des changements est déjà commitée.

### Quels repositories sont impactés ?

- `BaseRepository<T, TContext>` — méthodes `AddAsync`, `UpdateAsync`, `DeleteAsync`
- `AzureResourceBaseRepository<T>` — hérite de `BaseRepository`
- Tous les repositories spécialisés (`KeyVaultRepository`, `StorageAccountRepository`, etc.)

---

## Flux de données détaillé

Voici ce qui se passe concrètement quand un `CreateKeyVaultCommand` est envoyé :

```
1. Endpoint reçoit POST /keyvault
   │ Mapster convertit CreateKeyVaultRequest → CreateKeyVaultCommand
   │
2. ISender.Send(command) → MediatR
   │
3. ValidationBehavior
   │ FluentValidation vérifie les règles
   │ ✅ Valide → passe au behavior suivant
   │
4. UnitOfWorkBehavior
   │ Appelle next() → le handler s'exécute
   │     │
   │     │  5. CreateKeyVaultCommandHandler.Handle()
   │     │     ├── Vérifie l'accès (IInfraConfigAccessService)
   │     │     ├── Crée le domaine : KeyVault.Create(...)
   │     │     ├── keyVaultRepository.AddAsync(keyVault)
   │     │     │   └── _context.Set<KeyVault>().Add(keyVault)  ← Track, pas de save
   │     │     └── return mapper.Map<KeyVaultResult>(keyVault) ← succès
   │     │
   │ Reçoit le résultat : ErrorOr<KeyVaultResult> (succès)
   │ response.IsError == false
   │ → await unitOfWork.SaveChangesAsync() ← COMMIT atomique
   │
6. Endpoint : result.Match(ok => Results.Ok(...))
   │
7. HTTP 200 OK avec KeyVaultResponse
```

Si le handler retourne une erreur (par exemple `Errors.ResourceGroup.NotFoundError`) :

```
4. UnitOfWorkBehavior
   │ Appelle next() → le handler s'exécute
   │     │
   │     │  5. CreateKeyVaultCommandHandler.Handle()
   │     │     ├── ResourceGroup not found
   │     │     └── return Errors.ResourceGroup.NotFoundError(id)  ← erreur
   │     │
   │ Reçoit le résultat : ErrorOr<KeyVaultResult> (erreur)
   │ response.IsError == true
   │ → SaveChangesAsync() N'EST PAS APPELÉ  ← aucune persistance
   │
6. Endpoint : result.Match(errors => errors.ToErrorResult())
   │
7. HTTP 404 Not Found
```

---

## Avantages du Unit of Work dans ce projet

### 1. Atomicité des transactions

Toutes les modifications d'une commande sont persistées en un seul `SaveChanges`. Pas d'état intermédiaire incohérent en base.

### 2. Séparation des préoccupations

Les repositories se concentrent sur le tracking des changements. La décision de persister est prise par l'infrastructure (le behavior), pas par la logique métier.

### 3. Rollback implicite

Si le handler retourne une erreur ErrorOr ou lève une exception, rien n'est persisté. Le DbContext scopé est simplement disposé à la fin de la requête HTTP, et les changements trackés sont perdus.

### 4. Consistance avec le pattern CQRS

Le UoW ne s'applique qu'aux **commandes** (via `ICommandBase`). Les queries n'ont pas besoin de persister et ne sont pas affectées par ce behavior.

### 5. Performance

Un seul appel réseau vers la base de données par commande, au lieu de N appels (un par mutation). EF Core regroupe automatiquement les insertions/montées à jour dans un batch SQL optimal.

---

## Comparaison avec l'approche sans Unit of Work

| Aspect | Sans UoW (ancien comportement) | Avec UoW (actuel) |
|--------|-------------------------------|-------------------|
| Qui appelle `SaveChanges` ? | Chaque repository individuellement | Le `UnitOfWorkBehavior` une seule fois |
| Atomicité | ❌ Partielle — un save peut réussir pendant qu'un autre échoue | ✅ Totale — tout ou rien |
| Performance | N round-trips vers la DB | 1 seul round-trip |
| Responsabilité du handler | Indirecte (via les repositories) | Aucune — juste tracker les changements |
| Rollback en cas d'erreur | ❌ Changements déjà persistés partiellement | ✅ Rien n'est persisté |
| Code des repositories | Complexe (gère la persistance) | Simple (juste du tracking) |

---

## Quand le Unit of Work ne s'applique PAS

- **Queries** (`IQuery<T>`) — Lecture seule, pas de mutation, pas de `SaveChanges`
- **Services externes** — Les appels à Azure Blob Storage, Key Vault, ou des API externes ne sont pas trackés par le DbContext et ne font pas partie du UoW
- **Opérations de masse** — `ExecuteUpdateAsync()` / `ExecuteDeleteAsync()` d'EF Core 7+ contournent le change tracker et s'exécutent immédiatement sans passer par le UoW

---

## Fichiers de référence

| Fichier | Rôle |
|---------|------|
| `Application/Common/Interfaces/IUnitOfWork.cs` | Interface abstraite |
| `Infrastructure/Persistence/UnitOfWork.cs` | Implémentation (wraps DbContext) |
| `Application/Common/Behaviors/UnitOfWorkBehavior.cs` | Pipeline behavior MediatR |
| `Application/Common/Interfaces/ICommand.cs` | Interfaces `ICommand<T>` et `ICommandBase` |
| `Application/Common/Interfaces/IQuery.cs` | Interface `IQuery<T>` |
| `Application/Common/Interfaces/ICommandHandler.cs` | Interface `ICommandHandler<TCmd, T>` |
| `Application/Common/Interfaces/IQueryHandler.cs` | Interface `IQueryHandler<TQuery, T>` |
| `Application/DependencyInjection.cs` | Enregistrement des behaviors |
| `Infrastructure/DependencyInjection.cs` | Enregistrement de `UnitOfWork` |

---

## Pages connexes

- [CQRS et MediatR](cqrs-patterns.md) — Le pipeline complet de commandes/queries
- [Persistance EF Core](persistence.md) — Les repositories qui trackent les changements
- [Architecture du projet](overview.md) — Vue d'ensemble des couches
