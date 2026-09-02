---
name: dotnet-patterns
description: 'C#/.NET project conventions. Use when generating or reviewing backend code for naming, XML docs, one-type-per-file, no magic strings, strong typing, and design-pattern selection.'
---

# Skill : dotnet-patterns — Conventions C# .NET 10 du projet

> **Chargé automatiquement par l'agent `dotnet-dev`.**
> Contient les patterns techniques C#/.NET 10 spécifiques au projet InfraFlowSculptor.

---

## 1. Conventions de nommage .NET — Règles absolues

### Casse

| Élément | Convention | Exemple |
|---------|------------|---------|
| Classe, struct, record, interface | `PascalCase` | `InfrastructureConfig`, `IRepository<T>` |
| Méthode, propriété, événement | `PascalCase` | `GetByIdAsync`, `IsActive` |
| Paramètre, variable locale | `camelCase` | `configId`, `cancellationToken` |
| Champ privé | `_camelCase` (préfixe `_`) | `_repository`, `_logger` |
| Constante (`const`) | `PascalCase` | `MaxRetryCount`, `DefaultPageSize` |
| Enum et ses membres | `PascalCase` | `LocationEnum.WestEurope` |
| Namespace | `PascalCase`, hiérarchique | `InfraFlowSculptor.Application.Common` |
| Interface | Préfixe `I` + `PascalCase` | `ICurrentUser`, `IRepository<T>` |
| Type générique param | `T` seul ou `T` + nom | `T`, `TId`, `TResponse` |
| Fichier source | Même nom que le type public | `InfrastructureConfig.cs` |

### Règles supplémentaires

- **Pas d'abréviation** : `configuration` pas `cfg`, `cancellationToken` pas `ct` (sauf conventions ASP.NET très répandues).
- **Verbes pour les méthodes** : `Get`, `Create`, `Update`, `Delete`, `Add`, `Remove`, `Validate`, `Handle`, `Send`.
- **Suffixes sémantiques** : `Async` pour toute méthode `Task`-retournante, `Repository` pour les repos, `Handler` pour les handlers MediatR, `Validator` pour FluentValidation, `Configuration` pour les configs EF Core.
- **Pluriel pour les collections** : `Members` pas `MemberList`, `Items` pas `ItemCollection`.
- **Pas de préfixe hongrois** : jamais `strName`, `intCount`.

## 1bis. Granularité des fichiers — Une classe publique par fichier

Pour tout code de production non-test :

- Un seul type public top-level par fichier (`class`, `record`, `struct`, `interface`, `enum`).
- Le nom du fichier doit correspondre exactement au type public principal.
- Les fichiers poubelles `Dtos.cs`, `Models.cs`, `Requests.cs`, `Responses.cs`, et `Helpers.cs` sont interdits s'ils accumulent plusieurs types sans raison locale forte.
- Les types `private` ou `file` strictement locaux peuvent rester imbriqués ; dès qu'un type est réutilisé ailleurs, il doit être extrait dans son propre fichier.
- Les DTOs, records, résultats, commandes, réponses, options, et enums ne sont pas une exception : chacun mérite son propre fichier si son schéma est explicite.

---

## 2. Documentation XML — Obligatoire sur tout membre public

**Tout membre `public` ou `protected` dans une classe non-test doit avoir un commentaire XML.**

### Format

```csharp
/// <summary>
/// Retrieves an infrastructure configuration by its unique identifier.
/// Returns <c>null</c> if no configuration with the given identifier exists.
/// </summary>
/// <param name="id">The unique identifier of the configuration to retrieve.</param>
/// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
/// <returns>
/// The matching <see cref="InfrastructureConfig"/> aggregate root,
/// or <c>null</c> if not found.
/// </returns>
public async Task<InfrastructureConfig?> GetByIdAsync(
    InfrastructureConfigId id,
    CancellationToken cancellationToken = default)
```

### Balises obligatoires par contexte

| Contexte | Balises requises |
|----------|-----------------|
| Méthode avec paramètres | `<summary>`, `<param>` (un par paramètre), `<returns>` |
| Méthode sans retour (`void` / `Task`) | `<summary>`, `<param>` |
| Propriété | `<summary>` |
| Classe / Interface / Record | `<summary>` |
| Exception possible | `<exception cref="XyzException">` |

### Règles de rédaction

- Écrire en **anglais**.
- Commencer par un **verbe à l'infinitif** pour les méthodes : `Gets`, `Creates`, `Validates`, `Handles`.
- Ne jamais paraphraser le nom.
- Référencer les types avec `<see cref="TypeName"/>`.
- `CancellationToken` → toujours documenter : `Token to cancel the asynchronous operation.`
- **`<inheritdoc />`** est accepté pour les implémentations d'interface ou les overrides.

---

## 3. No Magic Strings — Règle absolue

### Solutions par contexte

#### Codes d'erreur ErrorOr

```csharp
// ✅ Classe d'erreurs partielle centralisée
public static partial class Errors
{
    public static class KeyVault
    {
        private const string NotFoundCode = "KeyVault.NotFound";
        public static Error NotFoundError(KeyVaultId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No KeyVault with id {id} was found.");
    }
}
```

#### Noms de claims JWT

```csharp
// ✅ Constantes dans une classe dédiée
public static class ClaimTypes
{
    public const string Roles = "roles";
    public const string ObjectId = "oid";
    public const string TenantId = "tid";
}
```

#### Noms de policies d'autorisation

```csharp
public static class AuthorizationPolicies
{
    public const string IsAdmin = "IsAdmin";
    public const string IsAuthenticated = "IsAuthenticated";
}
```

#### Noms de tables EF Core

```csharp
public sealed class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault>
{
    private const string TableName = "KeyVaults";
    public void Configure(EntityTypeBuilder<KeyVault> builder) => builder.ToTable(TableName);
}
```

### Règles complémentaires

- Quand le domaine expose un ensemble fini de valeurs, préférer `enum`, constante centralisée, ou ValueObject plutôt qu'un littéral répété.
- Les codes d'erreur, noms de policy, claims, routes nommées, noms de table, noms de colonne, et clés de configuration ne doivent pas être dispersés en dur.
- Les constantes doivent vivre au niveau le plus proche de leur responsabilité, mais rester réutilisables.

---

## 3bis. Typage fort — Modéliser le schéma, pas le contour

Quand la forme des données est connue, elle doit être modélisée explicitement.

- Préférer `record`, `class`, `enum`, `ValueObject`, options typées, ou read models dédiés.
- Éviter `object`, `dynamic`, `Dictionary<string, object>`, `IDictionary<string, object>`, `JsonDocument`, `JsonNode`, `JObject`, `ExpandoObject`, et les blobs JSON génériques en base quand un schéma stable est connu.
- Les dictionnaires restent autorisés pour de vrais cas de lookup, d'indexation, ou de clés réellement dynamiques. Ils ne doivent pas servir à masquer un contrat métier ou applicatif.
- Si une frontière externe impose une forme faible (payload tiers, input libre, métadonnées dynamiques), l'isoler dans l'adapter, valider, puis mapper immédiatement vers un modèle canonique fortement typé. Le reste de l'application ne doit jamais dépendre de cette forme faible.
- En persistance, préférer des colonnes explicites, owned types, converters, tables dédiées, ou read models typés avant de stocker un JSON non structuré.

---

## 4. Code propre — Principes SOLID et Clean Code

### Responsabilité unique (SRP)

- Handlers MediatR : **une seule opération** par handler.
- Repositories : **persistance uniquement** — pas de logique métier.
- Services domaine : **logique métier** — pas de persistance.
- Validators : **validation uniquement** — pas de logique applicative.

### Open/Closed (OCP)

Préférer l'extension par héritage ou composition plutôt que la modification.

### Liskov Substitution (LSP)

Les sous-types héritant d'`AzureResource` : tout code traitant un `AzureResource` doit fonctionner sans connaître le sous-type concret.

### Interface Segregation (ISP)

Définir des interfaces étroites et spécialisées :

```csharp
// ✅ Interfaces ségrégées
public interface IKeyVaultRepository : IRepository<KeyVault> { }
public interface IKeyVaultBicepGenerator { string Generate(KeyVault keyVault); }
```

### Dependency Inversion (DIP)

```
Application → définit IRepository<T>, ICurrentUser, IInfraConfigAccessService
Infrastructure → implémente ces interfaces
API → orchestre via DI
```

## 4bis. Choix des design patterns — Abstraction avec levier uniquement

Avant d'introduire une abstraction structurelle, comparer explicitement les options plausibles :

- aucune abstraction supplémentaire / composition directe
- `Strategy`
- `Factory`
- `Builder`
- `Specification`
- `Policy`
- autre pattern déjà établi dans le repo

Critères de choix obligatoires :

- lisibilité immédiate pour un lecteur du dépôt
- maintenabilité à moyen terme
- scalabilité du comportement attendu
- cohérence avec les patterns déjà présents

Règles :

- Si une méthode, un service simple, ou un `switch` explicite est plus clair, il faut le préférer au pattern.
- Un pattern n'est justifié que s'il simplifie les call sites, réduit la duplication structurelle, ou rend l'évolution future crédible.
- Refuser les abstractions génériques sans responsabilité nette (`Helper`, `Manager`, `Processor`) si elles ne portent pas une intention métier ou technique stable.

---

## 5. Organisation du code — Découpage en services et helpers

### Quand extraire une méthode dans un helper statique

- La logique est **réutilisée dans 2+ handlers/services**.
- La logique n'a **pas d'état**.
- Le code est **purement fonctionnel**.

### Quand créer un service injectable

- La logique **dépend d'autres services**.
- La logique est **réutilisée entre plusieurs handlers**.
- La logique implique de l'**état ou du cycle de vie**.

### Quand garder dans le handler

- Logique **unique à cette opération**.
- **Aucune chance** d'être réutilisée.
- Son extraction rendrait le code **moins lisible**.

---

## 6. Async/Await — Bonnes pratiques

```csharp
// ✅ Toujours suffixer Async les méthodes qui retournent Task/ValueTask
public async Task<InfrastructureConfig?> GetByIdAsync(InfrastructureConfigId id, CancellationToken ct = default)

// ✅ Propager le CancellationToken partout
var entity = await _repository.GetByIdAsync(entity.Id, cancellationToken);

// ❌ Ne jamais bloquer sur une Task (deadlock possible)
var result = GetByIdAsync(id).Result;      // ❌
var result = GetByIdAsync(id).GetAwaiter().GetResult(); // ❌

// ❌ Async void interdit (sauf event handlers)
public async void LoadData() { }  // ❌

// ✅ Retourner Task directement si pas d'await dans le corps
public Task<int> GetCountAsync() => _repository.CountAsync();
```

### ConfigureAwait

Dans les couches Application/Infrastructure : `ConfigureAwait(false)`. Dans les couches API (ASP.NET Core) : optionnel.

### ValueTask vs Task

- `Task` : usage général. `ValueTask` : chemins chauds avec résultat souvent synchrone. Ne pas l'utiliser par défaut.

---

## 7. Gestion des nulls — Nullable Reference Types

Le projet est configuré avec `<Nullable>enable</Nullable>`.

```csharp
// ✅ Pattern matching null-safe
if (entity is null)
    return Errors.KeyVault.NotFoundError(id);

// ✅ Null-coalescing
var description = config.Description ?? string.Empty;

// ✅ ArgumentNullException.ThrowIfNull en début de constructeur/méthode publique
ArgumentNullException.ThrowIfNull(context);
```

### Null-check sur ValueObject dans Mapster (expression trees)

```csharp
// ✅ Null-check typé — fonctionne dans les expression trees
es.Sku != null ? es.Sku.Value.ToString() : null

// ❌ Cast (object?) — perd le typage
(object?)es.Sku != null ? es.Sku.Value.ToString() : null

// ❌ Pattern matching — interdit dans expression trees (CS8122)
es.Sku is not null ? es.Sku.Value.ToString() : null
```

---

## 8. Immutabilité — Records, init, readonly

```csharp
// ✅ Record pour les objets de transfert
public record GetInfrastructureConfigResult(
    InfrastructureConfigId Id,
    Name Name,
    IReadOnlyList<MemberResult> Members);

// ✅ Init-only pour les propriétés de requête
public class CreateKeyVaultRequest
{
    [Required] public required string Name { get; init; }
    [Required] public required string ResourceGroupId { get; init; }
}

// ✅ Collections exposées : IReadOnlyList / IReadOnlyCollection
public IReadOnlyCollection<Member> Members => _members.AsReadOnly();
```

---

## 9. Pattern matching et switch expressions

```csharp
// ✅ Switch expression
private static string MapTlsVersion(TlsVersion tlsVersion) => tlsVersion switch
{
    TlsVersion.Tls10 => "TLS1_0",
    TlsVersion.Tls11 => "TLS1_1",
    TlsVersion.Tls12 => "TLS1_2",
    _ => throw new ArgumentOutOfRangeException(nameof(tlsVersion), tlsVersion, null)
};

// ✅ Pattern matching pour les type checks
if (resource is KeyVault keyVault) { /* ... */ }

// ✅ Switch sur type pour polymorphisme
var generator = resource switch
{
    KeyVault kv       => _keyVaultGenerator.Generate(kv),
    RedisCache rc     => _redisCacheGenerator.Generate(rc),
    _ => throw new ArgumentOutOfRangeException(nameof(resource))
};
```

---

## 10. LINQ — Bonnes pratiques

```csharp
// ✅ Méthodes LINQ lisibles, une opération par ligne
var activeMembers = members
    .Where(m => m.Role != Role.Create(RoleEnum.Reader))
    .OrderBy(m => m.UserId.Value)
    .ToList();

// ✅ FirstOrDefaultAsync dans EF Core (pas ToListAsync puis First)
var entity = await context.KeyVaults
    .Include(kv => kv.RoleAssignments)
    .FirstOrDefaultAsync(kv => kv.Id == id, cancellationToken);

// ✅ AnyAsync plutôt que CountAsync > 0
bool exists = await context.KeyVaults.AnyAsync(kv => kv.Name == name, cancellationToken);

// ✅ Select pour les projections
var names = await context.KeyVaults.Select(kv => kv.Name).ToListAsync(cancellationToken);
```

---

## 11. Gestion des exceptions

```csharp
// ❌ Exception avalée
catch (Exception) { /* rien */ }

// ✅ Catch ciblé + log + re-throw ou ErrorOr
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogError(ex, "Concurrency conflict while updating KeyVault {KeyVaultId}", id);
    return Error.Conflict(code: "KeyVault.ConcurrencyConflict", description: "...");
}

// ✅ Pour les erreurs métier prévisibles : ErrorOr, jamais d'exception
```

---

## 12. Logging — ILogger<T>

```csharp
// ✅ Structured logging : passer les valeurs en paramètre, pas interpolation
_logger.LogInformation("User {UserId} created InfraConfig {InfraConfigId}", userId, configId);

// ❌ Interpolation dans le message log
_logger.LogInformation($"User {userId} created config {configId}");

// Niveaux appropriés :
// Debug   → diagnostic haute fréquence
// Info    → événements métier importants
// Warning → situation anormale mais récupérée
// Error   → erreur non récupérée
// Critical → défaillance système
```

---

## 13. Primary Constructors (.NET 8+)

```csharp
// ✅ Pour les handlers et services simples
public class GetKeyVaultQueryHandler(
    IKeyVaultRepository repository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<GetKeyVaultQuery, ErrorOr<GetKeyVaultResult>>
{
    public async Task<ErrorOr<GetKeyVaultResult>> Handle(...) { /* ... */ }
}

// ⚠️ Ne pas utiliser quand : validation des paramètres, logique d'init complexe
```

---

## 14. Sealed — Classes finales

Marquer `sealed` toute classe qui n'est **pas conçue pour être héritée** :

```csharp
// ✅ Handlers, validators, configurations EF Core, repository implémentations
public sealed class CreateKeyVaultCommandHandler(...) : IRequestHandler<...> { }
public sealed class CreateKeyVaultCommandValidator : AbstractValidator<CreateKeyVaultCommand> { }
public sealed class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault> { }

// Exceptions : classes de base abstraites
public abstract class AzureResource : Entity<AzureResourceId> { }
```

---

## 15. Guard clauses — Early return

```csharp
// ✅ Guard clauses (early return)
public async Task<ErrorOr<Result>> Handle(Command cmd, CancellationToken ct)
{
    var entity = await _repo.GetByIdAsync(cmd.Id, ct);
    if (entity is null)
        return Errors.KeyVault.NotFoundError(cmd.Id);

    var access = await _access.VerifyWriteAccessAsync(entity.InfraConfigId, ct);
    if (access.IsError)
        return access.Errors;

    entity.UpdateName(cmd.Name);
    await _repo.UpdateAsync(entity, ct);
    return _mapper.Map<Result>(entity);
}
```

---

## 16. Éviter les code smells courants

- **Long Method** : < 30 lignes. Au-delà, extraire une méthode privée nommée.
- **Feature Envy** : ne pas manipuler les internals d'un agrégat — passer par les méthodes du domaine.
- **Primitive Obsession** : utiliser des **value objects** (`Name`, `Location`) pas des primitives.
- **Shotgun Surgery** : si un changement touche 10 fichiers, la logique est mal découpée.
- **Dead Code** : supprimer immédiatement (méthodes, `#if false`, paramètres, `using` inutilisés).

---

## 17. Performances

```csharp
// ✅ AsNoTracking pour les queries en lecture seule
var configs = await context.InfrastructureConfigs.AsNoTracking().ToListAsync(cancellationToken);

// ✅ Projection avec Select
var names = await context.KeyVaults.Select(kv => new { kv.Id, kv.Name }).ToListAsync(cancellationToken);

// ✅ Pagination
var page = await context.InfrastructureConfigs
    .OrderBy(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

// ✅ StringBuilder pour les concaténations en boucle
```

---

## 18. Sécurité — OWASP Top 10 appliqué au .NET

### Injection

```csharp
// ❌ SQL injection via concaténation
context.Database.ExecuteSqlRaw($"SELECT * FROM Users WHERE Id = '{userId}'");

// ✅ LINQ paramétré automatiquement
var user = context.Users.FirstOrDefault(u => u.Id == userId);
```

### Secrets — Ne jamais hardcoder

```csharp
// ✅ Via IConfiguration (Azure Key Vault / User Secrets / env vars)
private readonly string _apiKey = configuration["ExternalService:ApiKey"]
    ?? throw new InvalidOperationException("ExternalService:ApiKey is not configured.");
```

### Authorization

- Toujours retourner `NotFound` (pas `Forbidden`) si l'utilisateur n'est pas membre.
- Pattern : `VerifyReadAccessAsync` / `VerifyWriteAccessAsync` retournent `NotFound` pour les non-membres.
