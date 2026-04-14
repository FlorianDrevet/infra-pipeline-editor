# Génération Bicep — Moteur, stratégies et assembleur

## Vue d'ensemble

Le projet **InfraFlowSculptor** génère automatiquement des fichiers [Azure Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/) à partir de la configuration d'infrastructure modélisée par l'utilisateur. Le moteur de génération vit dans un projet dédié :

```
src/Api/InfraFlowSculptor.BicepGeneration/
```

Ce projet est **indépendant du domaine** (`InfraFlowSculptor.Domain`) — il ne manipule pas d'agrégats ou d'entités EF Core. Il travaille avec des **modèles de génération** (`Models/`) alimentés par les handlers de l'Application layer.

---

## Architecture du moteur

Le moteur suit le pattern **Strategy** : chaque type de ressource Azure a son propre générateur, et un engine central orchestre l'ensemble.

```
                GenerationRequest (modèle de données)
                        │
                        ▼
        ┌───────────────────────────────┐
        │   BicepGenerationEngine       │   ← Orchestrateur
        │   Pour chaque ressource :     │
        │     → trouve le bon generator │
        │     → appelle Generate()      │
        │     → collecte les modules    │
        └──────────────┬────────────────┘
                       │
          ┌────────────┼────────────────┐
          ▼            ▼                ▼
    KeyVaultType   StorageType   ... (16 generators)
    BicepGenerator BicepGenerator
          │            │
          ▼            ▼
    GeneratedTypeModule (par ressource)
                       │
                       ▼
        ┌───────────────────────────────┐
        │   BicepAssembler              │   ← Assembleur de sortie
        │   Produit :                   │
        │   • main.bicep                │
        │   • types.bicep               │
        │   • functions.bicep           │
        │   • constants.bicep           │
        │   • modules/{Type}/*.bicep    │
        │   • parameters/*.bicepparam   │
        └───────────────────────────────┘
```

---

## Les 3 composants principaux

### 1. `BicepGenerationEngine` — L'orchestrateur

```
Fichier : src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs
```

Le moteur reçoit un `GenerationRequest` (toutes les ressources, environnements, naming conventions) et :

1. Parcourt chaque ressource du request
2. Trouve le `IResourceTypeBicepGenerator` correspondant au `ResourceType` ARM
3. Appelle `generator.Generate(resource)` → obtient un `GeneratedTypeModule`
4. Gère les cas spéciaux : identités managées (system-assigned), role assignments, app settings
5. Transmet les modules collectés au `BicepAssembler`

```csharp
public sealed class BicepGenerationEngine
{
    private readonly IEnumerable<IResourceTypeBicepGenerator> _generators;

    public BicepGenerationEngine(IEnumerable<IResourceTypeBicepGenerator> generators)
    {
        _generators = generators;
    }

    public GenerationResult Generate(GenerationRequest request)
    {
        var modules = new List<GeneratedTypeModule>();
        // ... itère sur les ressources, appelle les generators, assemble
    }
}
```

### 2. `IResourceTypeBicepGenerator` — Les stratégies par type

```
Fichier : src/Api/InfraFlowSculptor.BicepGeneration/Generators/IResourceTypeBicepGenerator.cs
```

```csharp
public interface IResourceTypeBicepGenerator
{
    /// <summary>Type ARM (ex: "Microsoft.KeyVault/vaults")</summary>
    string ResourceType { get; }

    /// <summary>Nom simple pour les templates de nommage (ex: "KeyVault")</summary>
    string ResourceTypeName { get; }

    /// <summary>Génère le module Bicep pour une ressource donnée</summary>
    GeneratedTypeModule Generate(ResourceDefinition resource);
}
```

Chaque implémentation :
- Lit les propriétés spécifiques de la `ResourceDefinition` (SKU, runtime, options de sécurité…)
- Génère le contenu Bicep sous forme de string (le module `.bicep`)
- Retourne un `GeneratedTypeModule` avec le nom, le chemin et le contenu

### Générateurs existants

| Générateur | Type ARM | Abréviation |
|-----------|----------|-------------|
| `KeyVaultTypeBicepGenerator` | `Microsoft.KeyVault/vaults` | `kv` |
| `StorageAccountTypeBicepGenerator` | `Microsoft.Storage/storageAccounts` | `stg` |
| `RedisCacheTypeBicepGenerator` | `Microsoft.Cache/redis` | `redis` |
| `AppServicePlanTypeBicepGenerator` | `Microsoft.Web/serverfarms` | `asp` |
| `WebAppTypeBicepGenerator` | `Microsoft.Web/sites` | `app` |
| `FunctionAppTypeBicepGenerator` | `Microsoft.Web/sites` (Functions) | `func` |
| `UserAssignedIdentityTypeBicepGenerator` | `Microsoft.ManagedIdentity/userAssignedIdentities` | `id` |
| `AppConfigurationTypeBicepGenerator` | `Microsoft.AppConfiguration/configurationStores` | `appcs` |
| `ContainerAppEnvironmentTypeBicepGenerator` | `Microsoft.App/managedEnvironments` | `cae` |
| `ContainerAppTypeBicepGenerator` | `Microsoft.App/containerApps` | `ca` |
| `LogAnalyticsWorkspaceTypeBicepGenerator` | `Microsoft.OperationalInsights/workspaces` | `law` |
| `ApplicationInsightsTypeBicepGenerator` | `Microsoft.Insights/components` | `appi` |
| `CosmosDbTypeBicepGenerator` | `Microsoft.DocumentDB/databaseAccounts` | `cosmos` |
| `SqlServerTypeBicepGenerator` | `Microsoft.Sql/servers` | `sql` |
| `SqlDatabaseTypeBicepGenerator` | `Microsoft.Sql/servers/databases` | `sqldb` |
| `ServiceBusNamespaceTypeBicepGenerator` | `Microsoft.ServiceBus/namespaces` | `sb` |

Tous les générateurs sont enregistrés comme **singletons** dans `Application/DependencyInjection.cs`.

### 3. `BicepAssembler` — L'assembleur de sortie

```
Fichier : src/Api/InfraFlowSculptor.BicepGeneration/BicepAssembler.cs
```

L'assembleur prend les modules générés et l'ensemble du contexte (environments, naming, role assignments) pour produire la structure de fichiers finale :

```csharp
public static class BicepAssembler
{
    public static GenerationResult Assemble(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        IReadOnlyList<EnvironmentDefinition> environments,
        IReadOnlyList<string> environmentNames,
        IEnumerable<ResourceDefinition> resources,
        NamingContext namingContext,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        IReadOnlyList<AppSettingDefinition> appSettings,
        IReadOnlyList<ExistingResourceReference>? existingResourceReferences = null)
    { ... }
}
```

---

## Fichiers générés

### Mode single-config

```
output/
├── main.bicep                    ← Orchestration : déclare les modules, paramètres, dépendances
├── types.bicep                   ← Types exportés (environnements, variables partagées)
├── functions.bicep               ← Fonctions de nommage (conventions de nommage en Bicep)
├── constants.bicep               ← Constantes RBAC (roleDefinitionIds par service)
├── modules/
│   ├── KeyVault/
│   │   └── keyvault.module.bicep ← Module Bicep pour le Key Vault
│   ├── StorageAccount/
│   │   └── storageaccount.module.bicep
│   └── .../                      ← Un dossier par type de ressource
└── parameters/
    ├── main.dev.bicepparam       ← Paramètres pour l'environnement Dev
    ├── main.staging.bicepparam   ← Paramètres pour Staging
    └── main.prod.bicepparam      ← Paramètres pour Prod
```

### Mode mono-repo

En mode mono-repo (projet avec plusieurs `InfrastructureConfig`), le `MonoRepoBicepAssembler` organise la sortie différemment :

```
output/
├── Common/
│   ├── modules/
│   │   ├── KeyVault/
│   │   │   └── keyvault.module.bicep     ← Modules partagés entre configs
│   │   └── .../
│   ├── types.bicep
│   ├── functions.bicep
│   └── constants.bicep
├── ConfigA/
│   ├── main.bicep                        ← Réf. relative vers ../../Common/modules/...
│   └── parameters/
│       ├── main.dev.bicepparam
│       └── main.prod.bicepparam
└── ConfigB/
    ├── main.bicep
    └── parameters/
        └── ...
```

Les modules identiques sont **déduplicés** dans `Common/`. Chaque config référence les modules via des chemins relatifs (`../../Common/modules/...`).

---

## `NamingTemplateTranslator` — Conventions de nommage

```
Fichier : src/Api/InfraFlowSculptor.BicepGeneration/NamingTemplateTranslator.cs
```

Ce composant traduit les templates de nommage définis par l'utilisateur (ex: `{name}-{resourceAbbr}{suffix}`) en expressions Bicep interpolées utilisables dans les fonctions de nommage générées.

| Placeholder | Expression Bicep |
|------------|-----------------|
| `{name}` | `${name}` |
| `{resourceAbbr}` | `${resourceAbbr}` |
| `{resourceType}` | `${resourceType}` |
| `{suffix}` | `${env.envSuffix}` |
| `{prefix}` | `${env.envPrefix}` |
| `{env}` | `${env.envName}` |
| `{envShort}` | `${env.envShort}` |
| `{location}` | `${env.location}` |

---

## Modèles de données

### `GenerationRequest`

Le modèle d'entrée du moteur, construit par les handlers de l'Application layer à partir du domaine :

| Propriété | Type | Description |
|-----------|------|-------------|
| `Resources` | `IReadOnlyList<ResourceDefinition>` | Toutes les ressources Azure à générer |
| `ResourceGroups` | `IReadOnlyList<ResourceGroupDefinition>` | Les resource groups avec leurs locations |
| `Environments` | `IReadOnlyList<EnvironmentDefinition>` | Environnements (Dev, Staging, Prod) avec paramètres |
| `NamingContext` | `NamingContext` | Templates de nommage et informations projet |
| `RoleAssignments` | `IReadOnlyList<RoleAssignmentDefinition>` | Attributions de rôles RBAC |
| `AppSettings` | `IReadOnlyList<AppSettingDefinition>` | App settings des ressources compute |

### `GeneratedTypeModule`

La sortie de chaque `IResourceTypeBicepGenerator` :

| Propriété | Description |
|-----------|-------------|
| `ModuleName` | Nom du module (ex: `"keyVault"`) |
| `ModuleFileName` | Fichier (normalisé en `.module.bicep`) |
| `ModuleBicepContent` | Contenu du fichier `.module.bicep` |
| `ModuleTypesBicepContent` | Types spécifiques au module (si nécessaire) |
| `ModuleFolderName` | Dossier sous `modules/` (ex: `"KeyVault"`) |
| `ResourceGroupName` | Resource group d'appartenance |

### `GenerationResult`

La sortie finale après assemblage :

| Propriété | Description |
|-----------|-------------|
| `MainBicep` | Contenu de `main.bicep` |
| `TypesBicep` | Contenu de `types.bicep` |
| `FunctionsBicep` | Contenu de `functions.bicep` |
| `ConstantsBicep` | Contenu de `constants.bicep` |
| `EnvironmentParameterFiles` | `.bicepparam` par environnement |
| `ModuleFiles` | Modules individuels par type de ressource |
| `Files` | Propriété calculée combinant tout en `Dictionary<path, content>` |

---

## Comment ajouter un nouveau générateur Bicep ?

1. **Créer** `Generators/{TypeName}TypeBicepGenerator.cs` implémentant `IResourceTypeBicepGenerator`
2. **Définir** `ResourceType` (type ARM) et `ResourceTypeName` (nom simple)
3. **Implémenter** `Generate()` : lire les propriétés de la `ResourceDefinition`, générer le contenu Bicep, retourner un `GeneratedTypeModule`
4. **Enregistrer** le générateur comme singleton dans `Application/DependencyInjection.cs`

---

## Pages connexes

- [Architecture du projet](overview.md) — Vue d'ensemble des couches
- [Guide de navigation](getting-started.md) — Checklist pour ajouter un nouveau type de ressource
- [Génération Pipeline](pipeline-generation.md) — Le moteur compagnon pour les pipelines Azure DevOps
