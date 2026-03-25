# Guide de navigation — Comprendre et étendre le projet

## Comment lire le code ?

### Point d'entrée : une requête HTTP

La meilleure façon de comprendre le projet est de **suivre une requête du début à la fin**. Prenons l'exemple « créer un Key Vault » :

| Étape | Fichier | Ce qui se passe |
|-------|---------|-----------------|
| 1 | `Api/Controllers/KeyVaultController.cs` | Endpoint `POST /keyvault` → reçoit un `CreateKeyVaultRequest` |
| 2 | `Api/Common/Mapping/KeyVaultMappingConfig.cs` | Mapster convertit `CreateKeyVaultRequest` → `CreateKeyVaultCommand` |
| 3 | `Application/KeyVaults/Commands/CreateKeyVault/CreateKeyVaultCommandValidator.cs` | FluentValidation vérifie les règles (si le fichier existe) |
| 4 | `Application/KeyVaults/Commands/CreateKeyVault/CreateKeyVaultCommandHandler.cs` | Handler : vérifie l'accès, appelle `KeyVault.Create()`, persiste |
| 5 | `Domain/KeyVaultAggregate/KeyVault.cs` | Factory method `Create()` — la logique métier |
| 6 | `Infrastructure/Persistence/Repositories/KeyVaultRepository.cs` | `AddAsync()` → `SaveChangesAsync()` via EF Core |
| 7 | `Api/Controllers/KeyVaultController.cs` | `result.Match()` → `Results.Ok(mapper.Map<KeyVaultResponse>(...))` |

### Les dossiers importants

```
src/Api/
├── InfraFlowSculptor.Domain/              🟢 Commencer ici pour comprendre le métier
│   ├── Common/Models/                     Classes de base DDD (AggregateRoot, Entity, ValueObject)
│   ├── Common/BaseModels/AzureResource.cs Classe de base partagée par toutes les ressources
│   ├── Common/Errors/                     Définitions d'erreurs métier
│   └── KeyVaultAggregate/                 Un dossier par agrégat
│
├── InfraFlowSculptor.Application/          🟡 Ensuite : la logique applicative
│   ├── Common/Behaviors/                  Pipeline MediatR (validation)
│   ├── Common/Interfaces/                 Interfaces de repository
│   └── KeyVaults/                         Un dossier par feature
│       ├── Commands/CreateKeyVault/       Commandes (écriture)
│       ├── Queries/GetKeyVault/           Queries (lecture)
│       └── Common/                        DTOs résultat
│
├── InfraFlowSculptor.Infrastructure/       🔵 Puis : l'implémentation technique
│   ├── Persistence/Configurations/        Mapping EF Core
│   ├── Persistence/Repositories/          Implémentations des repositories
│   └── Services/                          Services externes (Azure, blob, git)
│
├── InfraFlowSculptor.Api/                  🟠 Enfin : la couche HTTP
│   ├── Controllers/                       Endpoints Minimal API
│   ├── Common/Mapping/                    Configurations Mapster
│   └── Errors/                            Conversion ErrorOr → HTTP
│
└── InfraFlowSculptor.Contracts/            📦 DTOs HTTP (Request/Response)
    ├── KeyVaults/Requests/
    └── KeyVaults/Responses/
```

---

## Comment ajouter un nouveau type de ressource Azure ?

Voici la checklist complète pour ajouter un nouveau type (exemple : `CosmosDb`) :

### Étape 1 — Domain

Créer le dossier `Domain/CosmosDbAggregate/` avec :

| Fichier | Contenu |
|---------|---------|
| `CosmosDb.cs` | Aggregate root, hérite de `AzureResource`. Factory `Create()`, méthodes `Update()`, `SetAllEnvironmentSettings()` |
| `Entities/CosmosDbEnvironmentSettings.cs` | Entity enfant avec les paramètres par environnement |
| `ValueObjects/CosmosDbEnvironmentSettingsId.cs` | ID typé héritant de `Id<CosmosDbEnvironmentSettingsId>` |
| `ValueObjects/DatabaseApiType.cs` (etc.) | Enum value objects si nécessaire |

Ajouter le fichier d'erreurs : `Domain/Common/Errors/Errors.CosmosDb.cs`

### Étape 2 — Application

Créer le dossier `Application/CosmosDbs/` avec :

```
CosmosDbs/
├── Commands/
│   ├── CreateCosmosDb/
│   │   ├── CreateCosmosDbCommand.cs
│   │   ├── CreateCosmosDbCommandHandler.cs
│   │   └── CreateCosmosDbCommandValidator.cs
│   ├── UpdateCosmosDb/
│   └── DeleteCosmosDb/
├── Queries/
│   └── GetCosmosDb/
│       ├── GetCosmosDbQuery.cs
│       └── GetCosmosDbQueryHandler.cs
└── Common/
    ├── CosmosDbResult.cs
    └── CosmosDbEnvironmentConfigData.cs
```

Ajouter l'interface repository : `Application/Common/Interfaces/Persistence/ICosmosDbRepository.cs`

### Étape 3 — Infrastructure

| Fichier | Contenu |
|---------|---------|
| `Persistence/Configurations/CosmosDbConfiguration.cs` | `IEntityTypeConfiguration` avec TPT, converters |
| `Persistence/Repositories/CosmosDbRepository.cs` | Hérite de `BaseRepository`, implémente `ICosmosDbRepository` |

Ajouter le `DbSet<CosmosDb>` dans `ProjectDbContext`.
Enregistrer le repository dans `Infrastructure/DependencyInjection.cs`.

### Étape 4 — Contracts

```
Contracts/CosmosDbs/
├── Requests/
│   ├── CreateCosmosDbRequest.cs
│   └── UpdateCosmosDbRequest.cs
└── Responses/
    ├── CosmosDbResponse.cs
    └── CosmosDbEnvironmentConfigResponse.cs
```

### Étape 5 — API

| Fichier | Contenu |
|---------|---------|
| `Controllers/CosmosDbController.cs` | Endpoints GET/POST/PUT/DELETE |
| `Common/Mapping/CosmosDbMappingConfig.cs` | Mapping Request ↔ Command ↔ Domain ↔ Response |

Enregistrer l'endpoint dans `Program.cs` : `app.UseCosmosDbController();`

### Étape 6 — Migration EF Core

```bash
dotnet ef migrations add AddCosmosDb \
    --project src/Api/InfraFlowSculptor.Infrastructure \
    --startup-project src/Api/InfraFlowSculptor.Api
```

### Étape 7 — Bicep Generator (optionnel)

Si le type doit générer du Bicep :
1. Créer `BicepGeneration/Generators/CosmosDbTypeBicepGenerator.cs` (implémente `IResourceTypeBicepGenerator`)
2. L'enregistrer dans `Application/DependencyInjection.cs` comme singleton

---

## Agrégats existants

| Agrégat | Type Azure | Abréviation |
|---------|-----------|-------------|
| KeyVault | `Microsoft.KeyVault/vaults` | `kv` |
| RedisCache | `Microsoft.Cache/redis` | `redis` |
| StorageAccount | `Microsoft.Storage/storageAccounts` | `stg` |
| AppServicePlan | `Microsoft.Web/serverfarms` | `asp` |
| WebApp | `Microsoft.Web/sites` | `app` |
| FunctionApp | `Microsoft.Web/sites` (Functions) | `func` |
| UserAssignedIdentity | `Microsoft.ManagedIdentity/userAssignedIdentities` | `id` |
| AppConfiguration | `Microsoft.AppConfiguration/configurationStores` | `appcs` |
| ContainerAppEnvironment | `Microsoft.App/managedEnvironments` | `cae` |
| ContainerApp | `Microsoft.App/containerApps` | `ca` |
| LogAnalyticsWorkspace | `Microsoft.OperationalInsights/workspaces` | `law` |
| ApplicationInsights | `Microsoft.Insights/components` | `appi` |
| CosmosDb | `Microsoft.DocumentDB/databaseAccounts` | `cosmos` |
| SqlServer | `Microsoft.Sql/servers` | `sql` |
| SqlDatabase | `Microsoft.Sql/servers/databases` | `sqldb` |
| ServiceBusNamespace | `Microsoft.ServiceBus/namespaces` | `sb` |

---

## Commandes utiles

```bash
# Build complet
dotnet build .\InfraFlowSculptor.slnx

# Lancer avec Aspire (PostgreSQL + API + Frontend)
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj

# Lancer l'API seule
dotnet run --project .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj

# Frontend (depuis src/Front)
npm install
npm run start        # Dev standalone (port 4200)
npm run build        # Build production
npm run typecheck    # Vérification TypeScript
```

---

## Pages connexes

- [Architecture du projet](overview.md) — Vue d'ensemble
- [Domain-Driven Design](ddd-concepts.md) — Concepts DDD
- [CQRS et MediatR](cqrs-patterns.md) — Pipeline de commandes/queries
- [Couche API](api-layer.md) — Endpoints et mapping
- [Persistance EF Core](persistence.md) — Repositories et configurations
