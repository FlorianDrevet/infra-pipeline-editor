# Domain-Driven Design (DDD) — Concepts et implémentation

## Qu'est-ce que le DDD ?

Le **Domain-Driven Design** est une méthode de conception logicielle qui place le **modèle métier** au centre de l'architecture. Au lieu de concevoir autour de la base de données ou de l'interface, on modélise d'abord les concepts du domaine (ici : les ressources Azure, les projets, les configurations d'infrastructure).

Le code métier vit dans le projet **`InfraFlowSculptor.Domain`** et ne dépend d'aucun framework externe (ni EF Core, ni ASP.NET, ni MediatR). C'est du C# pur.

---

## Les building blocks DDD du projet

### 1. Value Object

> **Définition :** Un objet défini par ses **valeurs**, pas par une identité. Deux value objects avec les mêmes propriétés sont considérés égaux.

**Exemples concrets :** un nom de ressource (`Name`), une localisation (`Location`), un SKU (`Sku`).

#### Classe de base : `ValueObject`

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/ValueObject.cs
```

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetEqualityComponents();

    public bool Equals(ValueObject? other) { /* compare via GetEqualityComponents */ }
    public override bool Equals(object? obj) { /* ... */ }
    public override int GetHashCode() { /* hash de GetEqualityComponents */ }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
```

**Comment ça marche :** Chaque value object hérite de `ValueObject` et surcharge `GetEqualityComponents()` pour retourner ses propriétés. L'égalité est **structurelle** : deux `Name("monApp")` sont égaux même s'ils sont des instances différentes.

#### Variantes spécialisées

Le projet propose des classes de base facilitant la création de value objects courants :

| Classe de base | Usage | Exemple |
|----------------|-------|---------|
| `Id<TId>` | Identifiants (wraps `Guid`) | `AzureResourceId`, `ProjectId`, `KeyVaultEnvironmentSettingsId` |
| `SingleValueObject<T>` | Enveloppe un seul primitif | `Name` (wraps `string`), `Location` (wraps `string`) |
| `EnumValueObject<TEnum>` | Enveloppe un enum | `Sku`, `AppServicePlanOsType` |

##### `Id<TId>` — Identifiants typés

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/Id.cs
```

```csharp
public abstract class Id<TId> : ValueObject where TId : Id<TId>, new()
{
    public Guid Value { get; }

    // Factory methods
    public static TId CreateUnique() => new() { Value = Guid.NewGuid() };
    public static TId Create(Guid value) => new() { Value = value };

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Pourquoi des ID typés ?** Au lieu d'utiliser `Guid` partout (ce qui permet de passer accidentellement un `ProjectId` là où on attend un `AzureResourceId`), chaque agrégat a son propre type d'ID. Le compilateur empêche les erreurs de mélange.

Utilisation :
```csharp
// Créer un nouvel ID unique
var id = AzureResourceId.CreateUnique();

// Recréer un ID existant à partir d'un Guid
var id = AzureResourceId.Create(someGuid);
```

##### `SingleValueObject<T>` — Wrapper de primitif

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/SingleValueObject.cs
```

```csharp
public abstract class SingleValueObject<T> : ValueObject
{
    public T Value { get; }

    // Conversions implicites dans les deux sens
    public static implicit operator T(SingleValueObject<T> obj) => obj.Value;
    public static implicit operator SingleValueObject<T>(T value) => /* ... */;
}
```

Exemple d'utilisation : `Name` est un `SingleValueObject<string>`. On peut écrire `string n = myResource.Name;` grâce à la conversion implicite.

##### `EnumValueObject<TEnum>` — Wrapper d'enum

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/EnumValueObject.cs
```

```csharp
public class EnumValueObject<TEnum>(TEnum value) : ValueObject
    where TEnum : struct, Enum
{
    public TEnum Value { get; } = value;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

Utilisé par `Sku`, `AppServicePlanOsType`, `FunctionAppRuntimeStack`, etc.

---

### 2. Entity

> **Définition :** Un objet avec une **identité unique** qui persiste dans le temps. Deux entités avec les mêmes propriétés mais des IDs différents sont **distinctes**.

**Exemples concrets :** un `KeyVaultEnvironmentSettings` (paramètres d'un Key Vault pour un environnement donné), un `ProjectMember`.

#### Classe de base : `Entity<TId>`

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/Entity.cs
```

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id) => Id = id;

    // Égalité basée sur l'ID (pas sur les propriétés)
    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }
}
```

**Différence avec un Value Object :** L'entité est identifiée par son `Id`, pas par ses propriétés. Si on change le nom d'un `ProjectMember`, c'est toujours le même membre. Par contre, deux `Name("dev")` créés séparément sont le même value object.

---

### 3. Aggregate Root

> **Définition :** Un **agrégat** est un groupe d'objets (entités + value objects) traité comme une unité de cohérence. L'**aggregate root** est le point d'entrée unique de l'agrégat : toute modification passe par lui.

**Exemples concrets :** `Project`, `KeyVault`, `StorageAccount`.

#### Classe de base : `AggregateRoot<TId>`

```
Fichier : src/Api/InfraFlowSculptor.Domain/Common/Models/AggregateRoot.cs
```

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }

    // Conversion implicite vers l'ID
    public static implicit operator TId(AggregateRoot<TId> aggregate) => aggregate.Id;
}
```

L'`AggregateRoot` hérite de `Entity<TId>` (il a donc une identité) et représente la racine d'un groupe d'objets.

#### Hiérarchie dans le projet

Dans ce projet, la plupart des agrégats sont des **ressources Azure** et héritent d'une classe intermédiaire :

```
AggregateRoot<AzureResourceId>
    └── AzureResource                    (classe de base commune à toutes les ressources Azure)
        ├── KeyVault                     (agrégat spécialisé)
        ├── StorageAccount
        ├── RedisCache
        ├── AppServicePlan
        ├── WebApp
        ├── FunctionApp
        ├── ContainerApp
        ├── CosmosDb
        ├── SqlServer
        ├── SqlDatabase
        ├── ServiceBusNamespace
        └── ...
```

Les agrégats non-Azure (`Project`, `InfrastructureConfig`, `User`) héritent directement de `AggregateRoot<T>` avec leur propre type d'ID.

---

## Anatomie d'un agrégat — Exemple : KeyVault

```
Fichier : src/Api/InfraFlowSculptor.Domain/KeyVaultAggregate/KeyVault.cs
```

Voici comment ces concepts s'assemblent concrètement :

```csharp
public sealed class KeyVault : AzureResource
{
    // ---- Collections d'entités enfants ----
    // Champ privé mutable (pour EF Core)
    private readonly List<KeyVaultEnvironmentSettings> _environmentSettings = [];
    // Propriété publique en lecture seule (pour le code appelant)
    public IReadOnlyCollection<KeyVaultEnvironmentSettings> EnvironmentSettings
        => _environmentSettings.AsReadOnly();

    // ---- Constructeur privé (empêche l'instanciation directe) ----
    private KeyVault() { }

    // ---- Factory method statique (seul point de création) ----
    public static KeyVault Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, Sku? Sku)>? environmentSettings)
    {
        var keyVault = new KeyVault();
        // Appel à la méthode de base (AzureResource)
        keyVault.Initialize(resourceGroupId, name, location);
        keyVault.SetAllEnvironmentSettings(environmentSettings);
        return keyVault;
    }

    // ---- Méthodes de domaine (modifient l'état interne) ----
    public void Update(Name name, Location location,
        IReadOnlyList<(string EnvironmentName, Sku? Sku)>? settings)
    {
        SetName(name);
        SetLocation(location);
        SetAllEnvironmentSettings(settings);
    }

    private void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, Sku? Sku)>? settings)
    {
        _environmentSettings.Clear();
        if (settings is null) return;

        foreach (var (envName, sku) in settings)
            _environmentSettings.Add(KeyVaultEnvironmentSettings.Create(Id, envName, sku));
    }
}
```

### Points clés de ce pattern

| Concept | Implémentation |
|---------|---------------|
| **Constructeur privé** | `private KeyVault() { }` — empêche `new KeyVault()` depuis l'extérieur |
| **Factory method** | `KeyVault.Create(...)` — seul moyen de créer une instance, garantit la cohérence |
| **Encapsulation des collections** | `_environmentSettings` (privé, mutable) + `EnvironmentSettings` (public, lecture seule) |
| **Méthodes de domaine** | `Update()`, `SetAllEnvironmentSettings()` — la logique métier vit dans l'agrégat |
| **Value Objects comme paramètres** | `Name name`, `Location location`, `Sku? sku` — pas de primitifs en paramètre |

---

## Organisation des fichiers dans le Domain

```
src/Api/InfraFlowSculptor.Domain/
├── Common/
│   ├── BaseModels/
│   │   └── AzureResource.cs             Classe de base pour toutes les ressources Azure
│   ├── Errors/
│   │   ├── Errors.KeyVault.cs           Erreurs métier par agrégat
│   │   ├── Errors.Project.cs
│   │   └── ...
│   ├── Models/
│   │   ├── AggregateRoot.cs             Classe de base : aggregate root
│   │   ├── Entity.cs                    Classe de base : entity
│   │   ├── ValueObject.cs               Classe de base : value object
│   │   ├── Id.cs                        Classe de base : identifiant typé
│   │   ├── SingleValueObject.cs         Wrapper mono-valeur
│   │   └── EnumValueObject.cs           Wrapper d'enum
│   └── ValueObjects/
│       ├── Name.cs                      Value objects partagés
│       ├── Location.cs
│       └── ...
│
├── KeyVaultAggregate/
│   ├── KeyVault.cs                      Aggregate root
│   ├── Entities/
│   │   └── KeyVaultEnvironmentSettings.cs
│   └── ValueObjects/
│       ├── KeyVaultEnvironmentSettingsId.cs
│       └── Sku.cs
│
├── ProjectAggregate/
│   ├── Project.cs
│   ├── Entities/
│   │   ├── ProjectMember.cs
│   │   ├── ProjectEnvironmentDefinition.cs
│   │   └── ProjectResourceNamingTemplate.cs
│   └── ValueObjects/
│       ├── ProjectId.cs
│       └── ...
│
└── ... (un dossier par agrégat)
```

---

## Erreurs de domaine

Les erreurs métier sont définies comme des classes statiques partielles dans `Domain/Common/Errors/` :

```csharp
// Fichier : Errors.KeyVault.cs
public static partial class Errors
{
    public static class KeyVault
    {
        public static Error NotFoundError(AzureResourceId keyVaultId) =>
            Error.NotFound(
                code: "KeyVault.NotFound",
                description: $"Key Vault with id {keyVaultId.Value} not found.",
                metadata: new() { { "KeyVaultId", keyVaultId.Value } });
    }
}
```

**Pourquoi ce pattern ?**
- Les erreurs sont **déclaratives** (pas des exceptions) — elles sont transportées via `ErrorOr<T>`
- Chaque agrégat a son fichier d'erreurs (`Errors.KeyVault.cs`, `Errors.Project.cs`…)
- Le `partial class Errors` permet d'ajouter des erreurs sans toucher aux fichiers existants

---

## Résumé — Quand utiliser quoi ?

| Concept | Quand l'utiliser | Exemples dans le projet |
|---------|-----------------|------------------------|
| **Value Object** | Objet sans identité propre, défini par sa valeur | `Name`, `Location`, `Sku`, `AzureResourceId` |
| **Entity** | Objet avec identité, fait partie d'un agrégat mais n'est pas la racine | `KeyVaultEnvironmentSettings`, `ProjectMember`, `BlobContainer` |
| **Aggregate Root** | Point d'entrée d'un groupe cohérent d'objets, unité de persistance | `KeyVault`, `Project`, `StorageAccount`, `InfrastructureConfig` |
| **Erreur de domaine** | Échec métier prévisible (not found, forbidden, conflit) | `Errors.KeyVault.NotFoundError(id)`, `Errors.Project.ForbiddenError()` |

---

## Pages connexes

- [Architecture du projet](overview.md) — Vue d'ensemble des couches
- [CQRS et MediatR](cqrs-patterns.md) — Comment le Domain est utilisé par les handlers
- [Persistance EF Core](persistence.md) — Comment le Domain est mappé en base de données
