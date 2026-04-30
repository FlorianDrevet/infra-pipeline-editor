# CQRS et MediatR — Commandes, Queries et Pipeline

## Qu'est-ce que le CQRS ?

**CQRS** (Command Query Responsibility Segregation) sépare les opérations d'**écriture** (Commands) des opérations de **lecture** (Queries). Au lieu d'un seul service avec des méthodes `Create`, `Get`, `Update`, `Delete`, on a des objets dédiés pour chaque action :

- **Command** = « fais quelque chose » (crée, modifie, supprime) → retourne un résultat ou une erreur
- **Query** = « donne-moi des données » (lecture) → retourne des données ou une erreur

### Pourquoi ce pattern ?

1. **Séparation des responsabilités** — Le code de lecture et d'écriture peut évoluer indépendamment
2. **Testabilité** — Chaque handler a une seule responsabilité, facile à tester
3. **Pipeline transversal** — La validation, le logging, l'autorisation s'appliquent uniformément via des behaviors MediatR

---

## MediatR — Le médiateur

Le projet utilise **MediatR** comme implémentation du pattern médiateur. Au lieu d'injecter des services directement, on envoie un message (command/query) à MediatR, qui le route vers le bon handler :

```
Endpoint → ISender.Send(command) → MediatR → Pipeline Behaviors → Handler → Résultat
```

L'enregistrement se fait dans `Application/DependencyInjection.cs` :
```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly));
```

Tous les handlers sont découverts automatiquement par scan de l'assembly.

---

## Structure d'une feature CQRS

Chaque feature suit la même organisation de fichiers dans `InfraFlowSculptor.Application` :

```
src/Api/InfraFlowSculptor.Application/
└── KeyVaults/                              ← Dossier par feature/agrégat
    ├── Commands/
    │   ├── CreateKeyVault/
    │   │   ├── CreateKeyVaultCommand.cs         ← Le message
    │   │   ├── CreateKeyVaultCommandHandler.cs  ← Le traitement
    │   │   └── CreateKeyVaultCommandValidator.cs← La validation (optionnel)
    │   ├── UpdateKeyVault/
    │   │   └── ...
    │   └── DeleteKeyVault/
    │       └── ...
    ├── Queries/
    │   └── GetKeyVault/
    │       ├── GetKeyVaultQuery.cs
    │       └── GetKeyVaultQueryHandler.cs
    └── Common/
        ├── KeyVaultResult.cs                    ← DTO de résultat (Application layer)
        └── KeyVaultEnvironmentConfigData.cs     ← DTO intermédiaire
```

---

## Anatomie d'une Command

### 1. Le message (Command)

```
Fichier : src/Api/InfraFlowSculptor.Application/KeyVaults/Commands/CreateKeyVault/CreateKeyVaultCommand.cs
```

```csharp
public record CreateKeyVaultCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<KeyVaultEnvironmentConfigData>? EnvironmentSettings
) : ICommand<KeyVaultResult>;
```

**Points clés :**
- C'est un `record` immuable (les propriétés ne changent pas après création)
- Implémente `ICommand<T>` (pas `IRequest` directement) — le `T` est le type de résultat en cas de succès. Voir la section [Interfaces de marquage CQRS](#interfaces-de-marquage-cqrs) ci-dessous.
- Les paramètres utilisent des **value objects** du domaine (`Name`, `Location`, `ResourceGroupId`), pas des primitifs
- `ErrorOr<T>` permet de retourner soit un succès (`T`), soit une liste d'erreurs

### 2. Le handler

```
Fichier : src/Api/InfraFlowSculptor.Application/KeyVaults/Commands/CreateKeyVault/CreateKeyVaultCommandHandler.cs
```

```csharp
public sealed class CreateKeyVaultCommandHandler(
    IInfraConfigAccessService accessService,
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IMapper mapper)
    : ICommandHandler<CreateKeyVaultCommand, KeyVaultResult>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(
        CreateKeyVaultCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Vérification d'accès (l'utilisateur a-t-il le droit d'écrire ?)
        var resourceGroup = await resourceGroupRepository
            .GetByIdAsync(command.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFoundError(command.ResourceGroupId);

        var accessResult = await accessService
            .VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        // 2. Création du domaine via factory method
        var keyVault = KeyVault.Create(
            command.ResourceGroupId,
            command.Name,
            command.Location,
            command.EnvironmentSettings?
                .Select(es => (es.EnvironmentName, es.Sku != null
                    ? new Sku(Enum.Parse<Sku.SkuEnum>(es.Sku)) : (Sku?)null))
                .ToList());

        // 3. Persistance
        await keyVaultRepository.AddAsync(keyVault);

        // 4. Mapping vers le résultat
        return mapper.Map<KeyVaultResult>(keyVault);
    }
}
```

**Pattern récurrent dans chaque handler :**

| Étape | Description |
|-------|-------------|
| **Vérification d'accès** | `accessService.VerifyWriteAccessAsync(...)` ou `VerifyReadAccessAsync(...)` |
| **Chargement du domaine** | Via le repository (`GetByIdAsync`) |
| **Appel aux méthodes de domaine** | `KeyVault.Create(...)`, `keyVault.Update(...)` |
| **Persistance** | `repository.AddAsync(...)`, `repository.UpdateAsync(...)` |
| **Mapping du résultat** | `mapper.Map<ResultType>(domainEntity)` |

### 3. Le résultat (Result DTO)

```
Fichier : src/Api/InfraFlowSculptor.Application/KeyVaults/Common/KeyVaultResult.cs
```

```csharp
public record KeyVaultResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<KeyVaultEnvironmentConfigData> EnvironmentSettings);
```

Ce DTO vit dans la couche Application. Il peut contenir des value objects du domain. Le mapping vers les DTOs HTTP (responses) se fait ensuite dans la couche API (Mapster).

---

## Anatomie d'une Query

### 1. Le message (Query)

```
Fichier : src/Api/InfraFlowSculptor.Application/Projects/Queries/GetProject/GetProjectQuery.cs
```

```csharp
public record GetProjectQuery(ProjectId Id) : IQuery<ProjectResult>;
```

Même pattern que les commands : un `record`, mais avec `IQuery<T>` au lieu de `ICommand<T>`. Les queries ne passent pas par le `UnitOfWorkBehavior`.

### 2. Le handler

```
Fichier : src/Api/InfraFlowSculptor.Application/Projects/Queries/GetProject/GetProjectQueryHandler.cs
```

```csharp
public sealed class GetProjectQueryHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IMapper mapper)
    : IQueryHandler<GetProjectQuery, ProjectResult>
{
    public async Task<ErrorOr<ProjectResult>> Handle(
        GetProjectQuery query,
        CancellationToken cancellationToken)
    {
        // 1. Vérification d'accès en lecture
        var accessResult = await accessService.VerifyReadAccessAsync(query.Id, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        // 2. Chargement avec includes
        var project = await projectRepository.GetByIdWithAllAsync(query.Id, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(query.Id);

        // 3. Mapping vers le résultat
        return mapper.Map<ProjectResult>(project);
    }
}
```

**Différence avec un Command handler :** pas de mutation, pas de `SaveChanges`. On charge et on retourne. Comme la query implémente `IQuery<T>` (et non `ICommandBase`), le `UnitOfWorkBehavior` ne s'applique pas.

---

## FluentValidation — Pipeline de validation

La validation s'exécute **automatiquement** avant chaque handler grâce au `ValidationBehavior` :

```
ISender.Send(command) → ValidationBehavior → [validator trouvé ?] → Handler
                              │
                      si erreur → retourne ErrorOr avec erreurs de validation
```

### Le behavior (middleware MediatR)

```
Fichier : src/Api/InfraFlowSculptor.Application/Common/Behaviors/ValidationBehavior.cs
```

```csharp
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si aucun validator n'est enregistré pour ce request, on passe au handler
        if (validator is null)
            return await next();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
            return await next();

        // Conversion des erreurs FluentValidation en erreurs ErrorOr
        var errors = validationResult.Errors
            .ConvertAll(e => Error.Validation(e.PropertyName, e.ErrorMessage));

        return (dynamic)errors;
    }
}
```

Le behavior est enregistré dans `Application/DependencyInjection.cs` :
```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

> **Ordre important :** `ValidationBehavior` est enregistré **avant** `UnitOfWorkBehavior`. Ainsi, la validation s'exécute d'abord ; si elle échoue, le handler et le UoW ne sont jamais invoqués.

### Exemple de validator

```
Fichier : src/Api/InfraFlowSculptor.Application/Projects/Commands/CreateProject/CreateProjectCommandValidator.cs
```

```csharp
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Project description must not exceed 500 characters.");
    }
}
```

**Conventions :**
- Un validator par command (pas obligatoire — si absent, le behavior passe directement au handler)
- Classe `sealed`
- Toujours un `.WithMessage()` explicite
- Nommage : `{CommandName}Validator.cs`

---

## ErrorOr — Le pattern de résultat

Au lieu de lever des exceptions pour les erreurs attendues (not found, forbidden, validation), le projet utilise `ErrorOr<T>` :

```csharp
// Le handler retourne ErrorOr<KeyVaultResult>
// → soit un succès : le KeyVaultResult
// → soit des erreurs : une liste d'Error

// Dans le handler, retourner un succès :
return mapper.Map<KeyVaultResult>(keyVault);  // implicitement un ErrorOr en succès

// Retourner une erreur :
return Errors.KeyVault.NotFoundError(id);  // implicitement une liste d'erreurs

// Propager les erreurs d'un appel précédent :
var accessResult = await accessService.VerifyWriteAccessAsync(infraConfigId, ct);
if (accessResult.IsError)
    return accessResult.Errors;  // propage les erreurs sans modification
```

**Types d'erreurs disponibles :**
| Factory | HTTP status typique | Usage |
|---------|-------------------|-------|
| `Error.NotFound(...)` | 404 | Ressource introuvable |
| `Error.Forbidden(...)` | 403 | Accès refusé |
| `Error.Validation(...)` | 400 | Validation échouée |
| `Error.Conflict(...)` | 409 | Conflit métier (doublon, etc.) |
| `Error.Unexpected(...)` | 500 | Erreur inattendue |

---

## Contrôle d'accès dans les handlers

L'autorisation est vérifiée au niveau de chaque handler via des services dédiés :

```csharp
// Pour les ressources liées à un InfrastructureConfig
IInfraConfigAccessService accessService

// Read (tout membre du projet) :
var result = await accessService.VerifyReadAccessAsync(infraConfigId, cancellationToken);

// Write (Owner ou Contributor uniquement) :
var result = await accessService.VerifyWriteAccessAsync(infraConfigId, cancellationToken);
```

Le service résout l'accès via l'appartenance au **projet** parent :
1. Charge l'`InfrastructureConfig` → récupère le `ProjectId`
2. Vérifie que l'utilisateur courant est membre du projet avec le rôle requis
3. Retourne `Error.NotFound` pour un non-membre (pas d'info leak) ou `Error.Forbidden` pour un Reader essayant d'écrire

### Pattern standardisé — Propagation de l'erreur d'accès

**Règle absolue :** tous les handlers (commandes et queries) propagent l'erreur retournée par le service d'accès via `return authResult.Errors`. Ne jamais remplacer cette erreur par un `NotFoundError` propre à la ressource.

```csharp
// ✅ CORRECT — toujours utiliser authResult.Errors
var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
if (authResult.IsError)
    return authResult.Errors;

// ❌ INCORRECT — masquage redondant et incohérent avec les commandes
var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
if (authResult.IsError)
    return Errors.ContainerApp.NotFoundError(query.Id);
```

**Pourquoi `return authResult.Errors` suffit :**
- `VerifyReadAccessAsync` retourne déjà `InfrastructureConfig.NotFoundError` pour les non-membres (masquage intégré au service, pas d'info leak).
- `VerifyWriteAccessAsync` retourne `InfrastructureConfig.NotFoundError` pour les non-membres **et** `Project.ForbiddenError` pour les lecteurs (rôle Reader) — cette distinction doit être préservée jusqu'au client.
- Remplacer l'erreur du service par un `NotFoundError` propre à la ressource masque silencieusement les erreurs `Forbidden`, rendant les commandes (écriture) indétectables pour les Readers.

---

## Interfaces de marquage CQRS

Le projet définit des interfaces dédiées au lieu d'utiliser directement `IRequest<ErrorOr<T>>` de MediatR. Cela permet au `UnitOfWorkBehavior` de distinguer les commandes (écriture) des queries (lecture) :

| Interface | Hérite de | Rôle |
|-----------|-----------|------|
| `ICommandBase` | – | Marqueur non-générique utilisé par le UoW |
| `ICommand<TResult>` | `IRequest<ErrorOr<TResult>>`, `ICommandBase` | Commande (écriture) → passe par le UoW |
| `IQuery<TResult>` | `IRequest<ErrorOr<TResult>>` | Query (lecture) → ne passe PAS par le UoW |
| `ICommandHandler<TCmd, TResult>` | `IRequestHandler<TCmd, ErrorOr<TResult>>` | Handler de commande |
| `IQueryHandler<TQuery, TResult>` | `IRequestHandler<TQuery, ErrorOr<TResult>>` | Handler de query |

> Pour une explication complète du fonctionnement et des raisons d'être de ces interfaces, voir la page dédiée [Unit of Work](unit-of-work.md).

---

## Résumé du pipeline complet

```
HTTP Request
    │
    ▼
Endpoint Minimal API
    │  Mapster: Request DTO → Command (avec value objects)
    │
    ▼
ISender.Send(command)
    │
    ▼
┌─────────────────────────────────┐
│   ValidationBehavior            │   ← Étape 1 : validation
│   FluentValidation si présent   │
│   → erreurs 400 si invalide    │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   UnitOfWorkBehavior            │   ← Étape 2 : wrapper persistance
│   (Commands uniquement)         │     (ICommandBase seulement)
│   Appelle le handler            │
│   Si succès → SaveChangesAsync  │
│   Si erreur → rien (rollback)   │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│   Command/Query Handler         │   ← Étape 3 : logique métier
│   1. Vérif accès               │
│   2. Logique de domaine        │
│   3. Repositories (track)      │
│   4. Mapping résultat          │
│   → ErrorOr<T>                 │
└──────────────┬──────────────────┘
               │
               ▼
Endpoint: result.Match(ok => ..., errors => ...)
    │
    ▼
HTTP Response (200/400/403/404/500)
```

> **Note :** Les repositories ne font que tracker les changements (pas de `SaveChangesAsync`). C'est le `UnitOfWorkBehavior` qui persiste tout de manière atomique. Voir [Unit of Work](unit-of-work.md) pour les détails.

---

## Pages connexes

- [Unit of Work](unit-of-work.md) — Persitance atomique via le pipeline MediatR
- [Domain-Driven Design (DDD)](ddd-concepts.md) — Les objets manipulés par les handlers
- [Couche API](api-layer.md) — Comme les endpoints appellent MediatR et mappent les résultats
- [Persistance EF Core](persistence.md) — Les repositories utilisés par les handlers
