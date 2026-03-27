# Skill : new-azure-resource — Checklist complète pour ajouter une nouvelle ressource Azure

> **Charger ce skill avec `read_file` AVANT de générer tout artefact pour une nouvelle ressource Azure.**
> Il contient la liste exhaustive de TOUS les fichiers à créer ou modifier.

---

## Pré-requis

1. `MEMORY.md` lu en entier.
2. Identifier : **Nom de la ressource** (ex: `ServiceBus`), **type ARM** (ex: `Microsoft.ServiceBus/namespaces`), **abréviation** (ex: `sb`), **propriétés per-env**.
3. Déterminer si c'est un **simple resource** (comme CosmosDb) ou un **parent-child** (comme SqlServer→SqlDatabase).

---

## Checklist complète — 38+ fichiers / 15 points de modification

### 1. DOMAIN (`src/Api/InfraFlowSculptor.Domain/{ResourceName}Aggregate/`)

| # | Fichier | Description |
|---|---------|-------------|
| 1 | `{ResourceName}.cs` | Aggregate root (extends `AzureResource`), `Create()`, `Update()`, `SetEnvironmentSettings()`, `SetAllEnvironmentSettings()` |
| 2 | `Entities/{ResourceName}EnvironmentSettings.cs` | Entity per-env (extends `Entity<{ResourceName}EnvironmentSettingsId>`), `Create()`, `Update()`, `ToDictionary()` |
| 3 | `ValueObjects/{ResourceName}EnvironmentSettingsId.cs` | Strongly-typed ID (`Id<T>`) |

### 2. DOMAIN — Errors et Catalogs

| # | Fichier | Action |
|---|---------|--------|
| 4 | `Common/Errors/Errors.{ResourceName}.cs` | **CRÉER** — `NotFoundError(AzureResourceId)` |
| 5 | `Common/AzureRoleDefinitions/AzureRoleDefinitionCatalog.cs` | **MODIFIER** — ajouter roles RBAC + entrée dans le dictionnaire `RolesByResourceType` |

### 3. APPLICATION (`src/Api/InfraFlowSculptor.Application/{ResourceName}s/`)

| # | Fichier | Description |
|---|---------|-------------|
| 6 | `Common/{ResourceName}Result.cs` | Record DTO résultat |
| 7 | `Common/{ResourceName}EnvironmentConfigData.cs` | Record DTO per-env |
| 8 | `Commands/Create{ResourceName}/Create{ResourceName}Command.cs` | Commande Create |
| 9 | `Commands/Create{ResourceName}/Create{ResourceName}CommandHandler.cs` | Handler Create |
| 10 | `Commands/Create{ResourceName}/Create{ResourceName}CommandValidator.cs` | Validator Create |
| 11 | `Commands/Update{ResourceName}/Update{ResourceName}Command.cs` | Commande Update |
| 12 | `Commands/Update{ResourceName}/Update{ResourceName}CommandHandler.cs` | Handler Update |
| 13 | `Commands/Update{ResourceName}/Update{ResourceName}CommandValidator.cs` | Validator Update |
| 14 | `Commands/Delete{ResourceName}/Delete{ResourceName}Command.cs` | Commande Delete |
| 15 | `Commands/Delete{ResourceName}/Delete{ResourceName}CommandHandler.cs` | Handler Delete |
| 16 | `Queries/Get{ResourceName}/Get{ResourceName}Query.cs` | Query Get |
| 17 | `Queries/Get{ResourceName}/Get{ResourceName}QueryHandler.cs` | Handler Get |

### 4. APPLICATION — Interfaces et DI

| # | Fichier | Action |
|---|---------|--------|
| 18 | `Common/Interfaces/Persistence/I{ResourceName}Repository.cs` | **CRÉER** — Interface repository |
| 19 | `DependencyInjection.cs` | **MODIFIER** — `services.AddSingleton<IResourceTypeBicepGenerator, {ResourceName}TypeBicepGenerator>()` |
| 20 | `InfrastructureConfig/Common/ResourceAbbreviationCatalog.cs` | **MODIFIER** — ajouter `["{ResourceName}"] = "{abbr}"` |

### 5. INFRASTRUCTURE (`src/Api/InfraFlowSculptor.Infrastructure/`)

| # | Fichier | Action |
|---|---------|--------|
| 21 | `Persistence/Configurations/{ResourceName}Configuration.cs` | **CRÉER** — TPT config `HasBaseType<AzureResource>().ToTable(...)` |
| 22 | `Persistence/Configurations/{ResourceName}EnvironmentSettingsConfiguration.cs` | **CRÉER** — Entity config avec unique index `(ResourceId, EnvironmentName)` |
| 23 | `Persistence/Repositories/{ResourceName}Repository.cs` | **CRÉER** — Repository avec `.Include(EnvironmentSettings)` |
| 24 | `Persistence/ProjectDbContext.cs` | **MODIFIER** — Ajouter 2 `DbSet<>` (resource + env settings) |
| 25 | `DependencyInjection.cs` | **MODIFIER** — `services.AddScoped<I{ResourceName}Repository, {ResourceName}Repository>()` |
| 26 | `Persistence/Repositories/InfrastructureConfigReadRepository.cs` | **MODIFIER 3 endroits** : (A) Chargement settings, (B) `MapResource()` switch case, (C) `GetResourceTypeString()` switch case |

### 6. CONTRACTS (`src/Api/InfraFlowSculptor.Contracts/{ResourceName}s/`)

| # | Fichier | Description |
|---|---------|-------------|
| 27 | `Requests/{ResourceName}RequestBase.cs` | Base request + `{ResourceName}EnvironmentConfigEntry` + `{ResourceName}EnvironmentConfigResponse` |
| 28 | `Requests/Create{ResourceName}Request.cs` | Create request (`ResourceGroupId` requis) |
| 29 | `Requests/Update{ResourceName}Request.cs` | Update request (hérite de RequestBase) |
| 30 | `Responses/{ResourceName}Response.cs` | Response DTO |

### 7. API (`src/Api/InfraFlowSculptor.Api/`)

| # | Fichier | Action |
|---|---------|--------|
| 31 | `Controllers/{ResourceName}Controller.cs` | **CRÉER** — 4 endpoints (GET/POST/PUT/DELETE) |
| 32 | `Common/Mapping/{ResourceName}MappingConfig.cs` | **CRÉER** — Mapster IRegister. Null-check sur ValueObject nullable : `x != null` (typé, JAMAIS `(object?)x != null` ni `x is not null` — CS8122 expression trees) |
| 33 | `Program.cs` | **MODIFIER** — `app.Use{ResourceName}Controller()` |

### 8. BICEP GENERATION (`src/Api/InfraFlowSculptor.BicepGeneration/`)

| # | Fichier | Action |
|---|---------|--------|
| 34 | `Generators/{ResourceName}TypeBicepGenerator.cs` | **CRÉER** — `IResourceTypeBicepGenerator` impl |
| 35 | `Generators/RoleAssignmentModuleTemplates.cs` | **MODIFIER** — Ajouter entrée `Metadata` |

### 9. EF CORE MIGRATION

| # | Action |
|---|--------|
| 36 | `dotnet ef migrations add Add{ResourceName}` |

### 10. FRONTEND (`src/Front/src/app/`)

| # | Fichier | Action |
|---|---------|--------|
| 37 | `shared/interfaces/{resource-name}.interface.ts` | **CRÉER** — Interfaces TypeScript (Response, Create/Update requests, env config entry/response) |
| 38 | `shared/services/{resource-name}.service.ts` | **CRÉER** — Angular service (CRUD) |
| 39 | `features/config-detail/enums/resource-type.enum.ts` | **MODIFIER 5 endroits** : (A) `ResourceTypeEnum`, (B) `RESOURCE_TYPE_ICONS`, (C) `RESOURCE_TYPE_ABBREVIATIONS`, (D) `RESOURCE_TYPE_CATEGORIES`, (E) `PARENT_CHILD_RESOURCE_TYPES` (si parent) |
| 40 | `features/config-detail/add-resource-dialog/*` | **MODIFIER** — Ajouter type dans picker + formulaire per-env |
| 41 | `features/config-detail/config-detail.component.ts` | **MODIFIER** — Ajouter service injection + delete case |
| 42 | `features/resource-edit/resource-edit.component.ts` | **MODIFIER** — Ajouter load/save/delete/env-form cases |
| 43 | `features/resource-edit/resource-edit.component.html` | **MODIFIER** — Ajouter General + Environment tab `@case` blocks |

### 11. i18n

| # | Fichier | Action |
|---|---------|--------|
| 44 | `public/i18n/fr.json` | **MODIFIER** — Ajouter `ADD_DIALOG_TITLE_{ResourceName}` + clés per-env fields |
| 45 | `public/i18n/en.json` | **MODIFIER** — Idem en anglais |

---

## Résumé des compteurs

| Couche | Fichiers à créer | Fichiers à modifier |
|--------|-----------------|-------------------|
| Domain | 4 | 1 (AzureRoleDefinitionCatalog) |
| Application | 13 | 2 (DI + ResourceAbbreviationCatalog) |
| Infrastructure | 3 | 3 (DbContext + DI + ReadRepository) |
| Contracts | 4 | 0 |
| API | 2 | 1 (Program.cs) |
| BicepGeneration | 1 | 1 (RoleAssignmentModuleTemplates) |
| Migration | 1 | 0 |
| Frontend | 2 | 5+ (enum + dialog + detail + edit + edit.html) |
| i18n | 0 | 2 (fr.json + en.json) |
| **TOTAL** | **~30 créés** | **~15+ modifiés** |

---

## Pièges connus

1. **EF Core TPT** : toujours `HasBaseType<AzureResource>().ToTable(...)` — ne jamais `HasKey(x => x.Id)` sur la classe dérivée.
2. **Repository `.Include(EnvironmentSettings)`** : obligatoire dans `GetByIdAsync` ET `GetByResourceGroupIdAsync`, sinon `SetAllEnvironmentSettings().Clear()` orpheline les anciennes lignes.
3. **InfrastructureConfigReadRepository** : 3 modifications séparées — settings loading, `MapResource()` switch, `GetResourceTypeString()` switch.
4. **i18n JSON** : toujours valider la syntaxe JSON après modification — un JSON tronqué casse TOUTE la traduction.
5. **Build** : `dotnet build .\InfraFlowSculptor.slnx` + `npm run typecheck` + `npm run build` dans `src/Front`.
