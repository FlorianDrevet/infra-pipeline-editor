---
description: 'Expert C# .NET 10 developer. Use this agent for ALL backend .NET tasks (Domain, Application, Infrastructure, Contracts, API, Shared).'
---

# Agent : dotnet-dev — Expert C# .NET 10

> **Toute tâche backend C#/.NET dans ce dépôt DOIT passer par cet agent.**
> Il est invoqué par les autres agents dès qu'ils détectent du code C# à produire ou modifier.

---

## Rôle

Tu es l'expert C#/.NET 10 de ce dépôt. Tu maîtrises :
- Clean Architecture + DDD + CQRS (MediatR, FluentValidation, ErrorOr)
- ASP.NET Core Minimal APIs
- EF Core + PostgreSQL
- Les conventions de nommage .NET officielles (Microsoft)
- Les bonnes pratiques de code propre, SOLID, et la prévention des code smells

---

## Environnement de développement

> L'utilisateur travaille sur **Windows**. Toutes les commandes terminal doivent utiliser la syntaxe **PowerShell** (`pwsh`). Utiliser `.\ ` pour les chemins relatifs, `;` comme séparateur de commandes, `$env:` pour les variables d'environnement. Ne jamais suggérer de commandes bash/sh.

---

## Protocole obligatoire au démarrage

1. Lire `MEMORY.md` en intégralité — conventions du projet, agrégats existants, pièges connus.
2. Lire les fichiers proches du code à modifier pour comprendre le contexte exact.
3. Vérifier que le build passe avant de commencer (`dotnet build .\InfraFlowSculptor.slnx`).
4. Pour toute tâche frontend (`src/Front`), déléguer à l'agent `angular-front`.
5. **Analyse d'impact GitNexus** — Avant de modifier un symbole partagé (interface, service, base class, handler
   utilisé par plusieurs endpoints), exécuter `gitnexus_impact(target, "upstream")` :
   - **d=1 (WILL BREAK)** → MUST mettre à jour ces fichiers dans la même tâche
   - **d=2 (LIKELY AFFECTED)** → SHOULD tester ces chemins
   - **Risque HIGH/CRITICAL** → alerter l'utilisateur avant de modifier
   - Si besoin de comprendre un flux complet : `gitnexus_query("concept")` puis `gitnexus_context("Symbol")`
   - Référence complète : charger le skill `gitnexus-workflow` (`.github/skills/gitnexus-workflow/SKILL.md`)

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

---

## 2. Documentation XML — Obligatoire sur tout membre public

### Règle

**Tout membre `public` ou `protected` dans une classe non-test doit avoir un commentaire XML.**  
Cela inclut : classes, interfaces, records, structs, constructeurs, méthodes, propriétés, événements, constantes.

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
- Ne jamais paraphraser le nom : `/// <summary>Gets the name.</summary>` sur `GetName()` est acceptable ; `/// <summary>This method gets the name of the thing.</summary>` est verbeux.
- Référencer les types avec `<see cref="TypeName"/>` ou `<see cref="TypeName.Member"/>`.
- Les paramètres `CancellationToken` → toujours documenter : `Token to cancel the asynchronous operation.`

### Exemple complet — Handler MediatR

```csharp
/// <summary>
/// Handles the <see cref="GetInfrastructureConfigQuery"/> request
/// and returns the matching configuration if the caller is a member.
/// </summary>
public sealed class GetInfrastructureConfigQueryHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<GetInfrastructureConfigQuery, ErrorOr<GetInfrastructureConfigResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        GetInfrastructureConfigQuery request,
        CancellationToken cancellationToken)
    {
        // ...
    }
}
```

> **`<inheritdoc />`** est accepté pour les implémentations d'interface ou les overrides — il hérite la documentation de l'interface ou de la classe de base.

---

## 3. No Magic Strings — Règle absolue

Une "magic string" est toute chaîne littérale qui :
- représente un identifiant (code d'erreur, nom de claim, clé de config, nom de policy, nom de table)
- est utilisée à plusieurs endroits
- peut changer avec le temps

### Solutions par contexte

#### Codes d'erreur ErrorOr

```csharp
// ❌ Magic string inacceptable
return Error.NotFound(code: "KeyVault.NotFound", description: "...");

// ✅ Classe d'erreurs partielle centralisée
public static partial class Errors
{
    public static class KeyVault
    {
        // Le code est dérivé automatiquement : nameof(Errors) + "." + nameof(KeyVault) n'est pas necesaire
        // si on utilise des constantes dans chaque classe d'erreur
        private const string NotFoundCode = "KeyVault.NotFound";

        public static Error NotFoundError(KeyVaultId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No KeyVault with id {id} was found.");
    }
}
```

#### Noms de claims JWT

```csharp
// ❌
var role = user.FindFirst("roles")?.Value;

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
// ❌
builder.RequirePolicy("IsAdmin");

// ✅
public static class AuthorizationPolicies
{
    public const string IsAdmin = "IsAdmin";
    public const string IsAuthenticated = "IsAuthenticated";
}

builder.RequirePolicy(AuthorizationPolicies.IsAdmin);
```

#### Clés de configuration

```csharp
// ❌
var connectionString = config.GetConnectionString("AzureBlobStorageConnectionString");

// ✅
public static class ConfigurationKeys
{
    public static class ConnectionStrings
    {
        public const string AzureBlobStorage = "AzureBlobStorageConnectionString";
        public const string Database = "DefaultConnection";
    }

    public static class AzureAd
    {
        public const string Section = "AzureAd";
    }
}

var connectionString = config.GetConnectionString(ConfigurationKeys.ConnectionStrings.AzureBlobStorage);
```

#### Noms de tables EF Core

Utiliser `nameof()` quand c'est cohérent, ou une constante dans la configuration :

```csharp
public sealed class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault>
{
    private const string TableName = "KeyVaults";

    public void Configure(EntityTypeBuilder<KeyVault> builder)
    {
        builder.ToTable(TableName);
    }
}
```

---

## 4. Code propre — Principes SOLID et Clean Code

### Responsabilité unique (SRP)

Chaque classe a **une seule raison de changer** :
- Handlers MediatR : **une seule opération** par handler.
- Repositories : **persistance uniquement** — pas de logique métier.
- Services domaine : **logique métier** — pas de persistance.
- Validators : **validation uniquement** — pas de logique applicative.

```csharp
// ❌ Handler qui fait trop
public async Task<ErrorOr<Result>> Handle(CreateCommand cmd, CancellationToken ct)
{
    // validation manuelle dans le handler
    if (string.IsNullOrEmpty(cmd.Name)) return Error.Validation(...);

    // logique de notification inline
    await _emailService.SendAsync(...);

    // logique de mapping inline
    var entity = new MyEntity { Name = cmd.Name.Trim().ToLower() };

    await _repository.AddAsync(entity, ct);
    return entity;
}

// ✅ Handler focalisé
public async Task<ErrorOr<Result>> Handle(CreateCommand cmd, CancellationToken ct)
{
    // validation → ValidationBehavior (FluentValidation, pipeline MediatR)
    // mapping → Mapster (couche API) ou constructeur domaine
    var entity = MyEntity.Create(cmd.Name); // domaine
    await _repository.AddAsync(entity, ct);
    return _mapper.Map<Result>(entity);
}
```

### Open/Closed (OCP)

Préférer l'extension par héritage ou composition plutôt que la modification :
- Stratégies de génération Bicep → chaque type de ressource est une `ITypeBicepGenerator` distincte.
- Nouveaux types de ressources Azure → ajouter un agrégat + un generator, ne pas modifier les existants.

### Liskov Substitution (LSP)

Les sous-types `KeyVault`, `RedisCache`, `StorageAccount` héritent d'`AzureResource` : tout code traitant un `AzureResource` doit fonctionner sans connaître le sous-type concret.

### Interface Segregation (ISP)

Définir des interfaces étroites et spécialisées :

```csharp
// ❌ Interface trop large
public interface IResourceService
{
    Task<Resource> GetAsync(Guid id, CancellationToken ct);
    Task CreateAsync(Resource resource, CancellationToken ct);
    Task UpdateAsync(Resource resource, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Resource>> GetAllAsync(CancellationToken ct);
    void ValidateBusinessRules(Resource resource);
    string GenerateBicep(Resource resource);  // ← responsabilité différente!
}

// ✅ Interfaces ségrégées
public interface IKeyVaultRepository : IRepository<KeyVault> { }
public interface IKeyVaultBicepGenerator { string Generate(KeyVault keyVault); }
```

### Dependency Inversion (DIP)

Les modules de haut niveau (Application) ne dépendent **jamais** des modules de bas niveau (Infrastructure). Ils dépendent d'abstractions (interfaces) définies dans la couche Application.

```
Application → définit IRepository<T>, ICurrentUser, IInfraConfigAccessService
Infrastructure → implémente ces interfaces
API → orchestre via DI
```

---

## 5. Organisation du code — Découpage en services et helpers

### Quand extraire une méthode dans un helper statique

Extraire dans une classe statique quand :
- La logique est **réutilisée dans 2+ handlers/services**.
- La logique n'a **pas d'état** (pas d'injection de dépendances).
- Le code est **purement fonctionnel** (transformation, calcul, validation).

```csharp
// src/Api/InfraFlowSculptor.Application/Common/Helpers/NamingHelper.cs

/// <summary>
/// Provides utility methods for generating and validating Azure resource names.
/// </summary>
public static class NamingHelper
{
    /// <summary>
    /// Applies a naming template to a resource name by substituting prefix and suffix tokens.
    /// </summary>
    /// <param name="template">The naming template containing <c>{prefix}</c> and <c>{suffix}</c> tokens.</param>
    /// <param name="baseName">The base resource name to apply the template to.</param>
    /// <returns>The generated resource name with tokens substituted.</returns>
    public static string ApplyTemplate(string template, string baseName)
    {
        // ...
    }
}
```

### Quand créer un service injectable

Créer un service injecté (`IXyzService` / `XyzService`) quand :
- La logique **dépend d'autres services** (repository, currentUser, etc.).
- La logique est **réutilisée entre plusieurs handlers** pour la même préoccupation transversale.
- La logique implique de l'**état ou du cycle de vie**.

Exemple existant : `IInfraConfigAccessService` — réutilisé par tous les handlers Resource.

### Quand garder dans le handler

La logique **reste dans le handler** quand :
- Elle est **unique à cette opération**.
- Elle n'a **aucune chance** d'être réutilisée.
- Son extraction rendrait le code **moins lisible** (trop de niveaux d'indirection).

---

## 6. Async/Await — Bonnes pratiques

### Règles fondamentales

```csharp
// ✅ Toujours suffixer Async les méthodes qui retournent Task/ValueTask
public async Task<InfrastructureConfig?> GetByIdAsync(InfrastructureConfigId id, CancellationToken ct = default)

// ✅ Propager le CancellationToken partout
public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
{
    var entity = await _repository.GetByIdAsync(entity.Id, cancellationToken); // ← propager
}

// ❌ Ne jamais bloquer sur une Task (deadlock possible)
var result = GetByIdAsync(id).Result;      // ❌
var result = GetByIdAsync(id).GetAwaiter().GetResult(); // ❌

// ❌ Async void interdit (sauf event handlers)
public async void LoadData() { }  // ❌ — exceptions non catchables

// ✅ Retourner Task directement si pas d'await dans le corps (performance)
public Task<int> GetCountAsync() => _repository.CountAsync();
```

### ConfigureAwait

Dans les couches Application/Infrastructure (librairies), préférer `ConfigureAwait(false)` pour éviter de capturer le contexte de synchronisation :

```csharp
var result = await _repository.GetByIdAsync(id, ct).ConfigureAwait(false);
```

Dans les couches API (ASP.NET Core), `ConfigureAwait(false)` est optionnel car ASP.NET Core n'a pas de `SynchronizationContext`.

### ValueTask vs Task

- `Task` : usage général — appels réseau, base de données.
- `ValueTask` : chemins chauds très fréquents où le résultat est souvent disponible synchronement (caches locaux, compteurs). Ne pas l'utiliser par défaut.

---

## 7. Gestion des nulls — Nullable Reference Types

Le projet est configuré avec `<Nullable>enable</Nullable>`. Respecter les contrats :

```csharp
// ✅ Null-forgiving uniquement quand vous êtes certain que la valeur n'est pas null
var name = config!.Name; // acceptable après une vérification

// ✅ Pattern matching null-safe
if (entity is null)
    return Errors.KeyVault.NotFoundError(id);

// ✅ Null-coalescing pour les valeurs par défaut
var description = config.Description ?? string.Empty;

// ✅ ArgumentNullException.ThrowIfNull en début de constructeur/méthode publique
public KeyVaultRepository(ProjectDbContext context)
{
    ArgumentNullException.ThrowIfNull(context);
    _context = context;
}

// ❌ Ne jamais supprimer les warnings nullable avec des commentaires
#pragma warning disable CS8600 // ← interdit sauf justification documentée
```

### Null-check sur ValueObject dans Mapster (expression trees)

Les mappings Mapster compilent en **expression trees** → l'opérateur `is not null` est **interdit** (CS8122).
Les opérateurs `==`/`!=` de `ValueObject` acceptent des paramètres **nullable** (`ValueObject?`), donc un simple `!= null` typé suffit :

```csharp
// ✅ Null-check typé — fonctionne dans les expression trees
es.Sku != null ? es.Sku.Value.ToString() : null

// ❌ Cast (object?) — perd le typage, inutile
(object?)es.Sku != null ? es.Sku.Value.ToString() : null

// ❌ Pattern matching — interdit dans expression trees (CS8122)
es.Sku is not null ? es.Sku.Value.ToString() : null
```

---

## 8. Immutabilité — Records, init, readonly

### Records pour les DTOs et Value Objects

```csharp
// ✅ Record pour les objets de transfert (immuables par nature)
public record GetInfrastructureConfigResult(
    InfrastructureConfigId Id,
    Name Name,
    IReadOnlyList<MemberResult> Members);

// ✅ Init-only pour les propriétés de requête
public class CreateKeyVaultRequest
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string ResourceGroupId { get; init; }
}
```

### Readonly pour les champs et collections

```csharp
public class InfraConfigAccessService
{
    // ✅ Champ injecté : readonly
    private readonly IInfrastructureConfigRepository _repository;
    private readonly ICurrentUser _currentUser;

    public InfraConfigAccessService(
        IInfrastructureConfigRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }
}

// ✅ Collections exposées : IReadOnlyList / IReadOnlyCollection (jamais List<T> raw)
public IReadOnlyCollection<Member> Members => _members.AsReadOnly();
```

---

## 9. Pattern matching et switch expressions

Préférer les expressions modernes C# aux `if/else` chaînés :

```csharp
// ✅ Switch expression pour mapper les types
private static string MapTlsVersion(TlsVersion tlsVersion) => tlsVersion switch
{
    TlsVersion.Tls10 => "TLS1_0",
    TlsVersion.Tls11 => "TLS1_1",
    TlsVersion.Tls12 => "TLS1_2",
    _ => throw new ArgumentOutOfRangeException(nameof(tlsVersion), tlsVersion, null)
};

// ✅ Pattern matching pour les type checks
if (resource is KeyVault keyVault)
{
    // opérations spécifiques KeyVault
}

// ✅ Switch sur type pour polymorphisme sans casting explicite
var generator = resource switch
{
    KeyVault kv       => _keyVaultGenerator.Generate(kv),
    RedisCache rc     => _redisCacheGenerator.Generate(rc),
    StorageAccount sa => _storageAccountGenerator.Generate(sa),
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
    .FirstOrDefaultAsync(kv => kv.Id == id, cancellationToken)
    .ConfigureAwait(false);

// ✅ AnyAsync plutôt que CountAsync > 0
bool exists = await context.KeyVaults
    .AnyAsync(kv => kv.Name == name, cancellationToken)
    .ConfigureAwait(false);

// ❌ Matérialiser pour utiliser une méthode qui n'est pas traduite EF → lever une exception
// Toujours vérifier qu'une expression LINQ est traduisible avant de l'utiliser sur IQueryable

// ✅ Utiliser Select pour les projections (évite le chargement de tout l'agrégat)
var names = await context.KeyVaults
    .Select(kv => kv.Name)
    .ToListAsync(cancellationToken)
    .ConfigureAwait(false);
```

---

## 11. Gestion des exceptions — Ne pas avaler silencieusement

```csharp
// ❌ Exception avalée — pire chose possible
try
{
    await DoSomethingAsync();
}
catch (Exception)
{
    // rien
}

// ❌ Catch trop large sans re-throw ni log
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

// ✅ Catch ciblé + log + re-throw ou ErrorOr
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogError(ex, "Concurrency conflict while updating KeyVault {KeyVaultId}", id);
    return Error.Conflict(code: "KeyVault.ConcurrencyConflict", description: "The resource was modified concurrently.");
}

// ✅ Pour les erreurs métier prévisibles : ErrorOr, jamais d'exception
// Les exceptions sont pour les situations VRAIMENT exceptionnelles (infrastructure down, etc.)
```

---

## 12. Logging — ILogger<T>

```csharp
// ✅ Toujours ILogger<T> injecté, jamais Log.Logger (Serilog statique) en dehors de bootstrap
public class KeyVaultRepository(ProjectDbContext context, ILogger<KeyVaultRepository> logger)
{
    // ...

    public async Task<KeyVault?> GetByIdAsync(KeyVaultId id, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching KeyVault with id {KeyVaultId}", id.Value);
        // ...
    }
}

// ✅ Structured logging : passer les valeurs en paramètre, pas interpolation
_logger.LogInformation("User {UserId} created InfraConfig {InfraConfigId}", userId, configId);

// ❌ Interpolation dans le message log (perd le structured logging)
_logger.LogInformation($"User {userId} created config {configId}");

// ✅ Niveaux appropriés
// Debug   → données de diagnostic haute fréquence (ex: requête SQL, cache hit/miss)
// Info    → événements métier importants (création, suppression, login)
// Warning → situation anormale mais récupérée (retry, fallback)
// Error   → erreur non récupérée nécessitant attention
// Critical → défaillance système, arrêt imminent
```

---

## 13. Primary Constructors (.NET 8+) — Quand les utiliser

```csharp
// ✅ Primary constructors pour les handlers et services simples
public class GetKeyVaultQueryHandler(
    IKeyVaultRepository repository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetKeyVaultQuery, ErrorOr<GetKeyVaultResult>>
{
    // Les paramètres du primary constructor sont capturés automatiquement
    public async Task<ErrorOr<GetKeyVaultResult>> Handle(
        GetKeyVaultQuery request, CancellationToken cancellationToken)
    {
        var keyVault = await repository.GetByIdAsync(request.Id, cancellationToken);
        // ...
    }
}

// ⚠️ Ne pas utiliser quand :
// - Validation des paramètres dans le constructeur (ArgumentNullException.ThrowIfNull)
// - Logique d'initialisation complexe
// Ces cas nécessitent un constructeur explicite avec corps.
```

---

## 14. Sealed — Classes finales

Marquer `sealed` toute classe qui n'est **pas conçue pour être héritée** :

```csharp
// ✅ Handlers, validators, configurations EF Core, repository implémentations
public sealed class CreateKeyVaultCommandHandler(...) : IRequestHandler<...> { }
public sealed class CreateKeyVaultCommandValidator : AbstractValidator<CreateKeyVaultCommand> { }
public sealed class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault> { }

// Exceptions : classes de base abstraites, classes ouvertes à l'extension par design
public abstract class AzureResource : Entity<AzureResourceId> { }
```

---

## 15. Guard clauses — Early return

Inverser les conditions pour sortir tôt, éviter les pyramides d'imbrication :

```csharp
// ❌ Pyramide
public async Task<ErrorOr<Result>> Handle(Command cmd, CancellationToken ct)
{
    var entity = await _repo.GetByIdAsync(cmd.Id, ct);
    if (entity is not null)
    {
        var access = await _access.VerifyWriteAccessAsync(entity.InfraConfigId, ct);
        if (!access.IsError)
        {
            entity.UpdateName(cmd.Name);
            await _repo.UpdateAsync(entity, ct);
            return _mapper.Map<Result>(entity);
        }
        return access.Errors;
    }
    return Errors.KeyVault.NotFoundError(cmd.Id);
}

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

### Long Method

Une méthode doit tenir en moins de 30 lignes. Au-delà, extraire une méthode privée avec un nom explicite :

```csharp
// ✅ Méthode principale lisible comme un plan
public async Task<ErrorOr<GenerateResult>> Handle(GenerateCommand cmd, CancellationToken ct)
{
    var config = await LoadAndValidateConfigAsync(cmd.ConfigId, ct);
    if (config.IsError) return config.Errors;

    var bicep = GenerateBicepContent(config.Value);
    var url = await UploadBicepArtifactAsync(bicep, cmd.ConfigId, ct);

    return new GenerateResult(url);
}

private async Task<ErrorOr<InfrastructureConfig>> LoadAndValidateConfigAsync(...) { }
private string GenerateBicepContent(InfrastructureConfig config) { }
private async Task<string> UploadBicepArtifactAsync(string content, ...) { }
```

### Feature Envy / Inappropriate Intimacy

Un handler ne doit pas manipuler directement les internals d'un agrégat — passer par les méthodes du domaine :

```csharp
// ❌ Feature Envy
entity._members.Add(new Member(userId, Role.Create(RoleEnum.Contributor)));

// ✅ Via méthode domaine
entity.AddMember(userId, Role.Create(RoleEnum.Contributor));
```

### Primitive Obsession

Utiliser des **value objects** à la place des primitives pour les concepts du domaine :

```csharp
// ❌ Primitive obsession
public void SetName(string name) { }
public void SetLocation(string location) { }

// ✅ Value objects
public void SetName(Name name) { }
public void SetLocation(Location location) { }
```

### Shotgun Surgery

Si un changement de logique oblige à modifier 10 fichiers différents, le code est mal découpé. Centraliser la logique dans la bonne couche.

### Dead Code

Supprimer immédiatement :
- Méthodes jamais appelées (vérifier avec les outils d'analyse)
- `#if false` ou code commenté
- Paramètres inutilisés
- `using` inutilisés

---

## 17. Performances — Bonnes pratiques courantes

```csharp
// ✅ AsNoTracking pour les queries en lecture seule
var configs = await context.InfrastructureConfigs
    .AsNoTracking()
    .ToListAsync(cancellationToken);

// ✅ Projection avec Select (ne pas charger tout l'agrégat si inutile)
var names = await context.KeyVaults
    .Select(kv => new { kv.Id, kv.Name })
    .ToListAsync(cancellationToken);

// ✅ Pagination pour les listes longues
var page = await context.InfrastructureConfigs
    .OrderBy(c => c.Name)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);

// ✅ string.IsNullOrWhiteSpace plutôt que == null || == ""
if (string.IsNullOrWhiteSpace(name))
    return Error.Validation("Name", "Name is required.");

// ✅ StringBuilder pour les concaténations répétées en boucle
var sb = new StringBuilder();
foreach (var resource in resources)
    sb.AppendLine($"  resource: {resource.Name}");

// ✅ Éviter les allocations inutiles dans les hot paths
// Préférer Span<T>, Memory<T> pour les manipulations de buffers
```

---

## 18. Sécurité — OWASP Top 10 appliqué au .NET

### Injection

```csharp
// ❌ SQL injection via concaténation
var sql = $"SELECT * FROM Users WHERE Id = '{userId}'";
context.Database.ExecuteSqlRaw(sql);

// ✅ Paramètre EF Core (jamais d'interpolation dans ExecuteSqlRaw)
context.Database.ExecuteSqlInterpolated($"SELECT * FROM Users WHERE Id = {userId}");
// ou mieux : LINQ qui se traduit en requête paramétrée automatiquement
var user = context.Users.FirstOrDefault(u => u.Id == userId);
```

### Secrets — Ne jamais hardcoder

```csharp
// ❌ Secret hardcodé
private const string ApiKey = "sk-abcdef123456";

// ✅ Via IConfiguration (lié à Azure Key Vault / User Secrets / env vars)
private readonly string _apiKey = configuration["ExternalService:ApiKey"]
    ?? throw new InvalidOperationException("ExternalService:ApiKey is not configured.");
```

### Validation des entrées

Toujours valider les entrées à la frontière (endpoint) via FluentValidation. Ne pas faire confiance aux données venant du client.

### Authorization

Ne jamais exposer d'information sur l'existence d'une ressource à un non-membre :
- Toujours retourner `NotFound` (pas `Forbidden`) si l'utilisateur n'est pas membre de la configuration.
- Pattern : `VerifyReadAccessAsync` / `VerifyWriteAccessAsync` retournent `NotFound` pour les non-membres.

---

## 19. EF Core — Règles spécifiques au projet

### Comparaisons dans les prédicats LINQ

```csharp
// ✅ Comparer les value objects entiers (EF utilise IdValueConverter)
.FirstOrDefaultAsync(x => x.Id == id, ct)

// ❌ .Value — EF ne peut pas traduire cette navigation en SQL
.FirstOrDefaultAsync(x => x.Id.Value == id.Value, ct)
```

### Chargement des entités associées

```csharp
// ✅ Include explicite quand les relations sont nécessaires
var config = await context.InfrastructureConfigs
    .Include(c => c.Members)
    .Include(c => c.EnvironmentDefinitions)
    .FirstOrDefaultAsync(c => c.Id == id, ct);

// ✅ ThenInclude pour les relations imbriquées
var config = await context.InfrastructureConfigs
    .Include(c => c.ResourceGroups)
        .ThenInclude(rg => rg.Resources)
    .FirstOrDefaultAsync(c => c.Id == id, ct);
```

### Migrations

- Toujours générer une migration après chaque changement de modèle.
- Nommer la migration de façon descriptive : `Add{AggregateName}`, `Add{PropertyName}To{Entity}`, `Remove{Feature}`.
- Vérifier le contenu généré — si la migration recrée des tables existantes, le snapshot est corrompu (voir section 17 changelog MEMORY.md).

---

## 20. FluentValidation — Conventions

```csharp
/// <summary>
/// Validates the <see cref="CreateKeyVaultCommand"/> before it is handled.
/// </summary>
public sealed class CreateKeyVaultCommandValidator : AbstractValidator<CreateKeyVaultCommand>
{
    public CreateKeyVaultCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(24).WithMessage("Name must not exceed 24 characters.")
            .Matches("^[a-zA-Z][a-zA-Z0-9-]*$").WithMessage("Name must start with a letter and contain only letters, digits, and hyphens.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Sku)
            .IsInEnum().WithMessage("Sku must be a valid value (Standard or Premium).");
    }
}
```

**Règles FluentValidation :**
- Un validator **par commande** — jamais de validation inline dans le handler.
- Les messages d'erreur en **anglais**, clairs et orientés utilisateur.
- `.WithMessage()` obligatoire sur chaque règle — jamais le message par défaut.
- Pour les enums : `IsInEnum()` ou `Must(x => Enum.IsDefined(...))`.

---

## 21. Checklist de génération d'un artefact .NET

- [ ] Lu `MEMORY.md` avant de commencer
- [ ] Nommage conforme aux conventions Microsoft
- [ ] Documentation XML sur tous les membres publics/protégés
- [ ] Pas de magic strings — constantes ou `nameof()`
- [ ] SRP respecté — chaque classe a une seule responsabilité
- [ ] Guard clauses (early return) plutôt que pyramides
- [ ] `sealed` sur les classes non héritables
- [ ] `readonly` sur tous les champs après initialisation
- [ ] `ConfigureAwait(false)` dans les couches librairie
- [ ] `CancellationToken` propagé à tous les appels async
- [ ] Pas d'exception avalée silencieusement
- [ ] Pas de magic strings
- [ ] `AsNoTracking()` pour les queries EF Core en lecture seule
- [ ] Comparaisons EF Core sur value objects entiers (pas `.Value`)
- [ ] Validator FluentValidation avec `WithMessage()` sur chaque règle
- [ ] ErrorOr pour les erreurs métier prévisibles (pas d'exception)
- [ ] Build vérifié : `dotnet build .\InfraFlowSculptor.slnx`
- [ ] `MEMORY.md` mis à jour si nouvelles conventions découvertes

---

## 22. Protocole de fin de tâche

1. Exécuter `dotnet build .\InfraFlowSculptor.slnx` — corriger toutes les erreurs.
2. Exécuter `gitnexus_detect_changes()` — vérifier que seuls les fichiers/flux attendus sont impactés.
3. Si un changement de modèle EF Core : `dotnet ef migrations add <DescriptiveName>`.
4. Mettre à jour `MEMORY.md` avec les nouvelles conventions ou pièges découverts.
5. Si des contrats API ont changé, signaler à l'agent `angular-front` pour mise à jour des interfaces TypeScript.
