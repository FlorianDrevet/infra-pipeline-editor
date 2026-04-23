# Lecture Des Ressources D'un Projet Et Views SQL

## Pourquoi Cette Doc Existe

La lecture des ressources dans Infra Flow Sculptor s'appuie maintenant sur deux mecanismes complementaires :

1. une query projet simple qui retourne la liste plate des ressources visibles dans un projet ;
2. des views SQL de lecture qui centralisent les requetes polymorphes difficiles a exprimer proprement avec le modele TPT.

Cette page explique la frontiere entre ces deux mecanismes, le flux complet cote API et frontend, et ce qu'un nouveau developpeur doit modifier lorsqu'il ajoute un nouveau type de ressource ou un nouveau cas de lecture.

---

## TL;DR

- L'endpoint `GET /projects/{id}/resources` retourne toutes les ressources Azure de toutes les configurations d'un projet.
- Cette query passe par `ListProjectResourcesQueryHandler` et ne lit pas directement les views SQL.
- La refacto recente a introduit deux views PostgreSQL, mappees en entites EF Core keyless, pour sortir la logique de lecture polymorphe des handlers :
  - `vw_ResourceEnvironmentEntries`
  - `vw_ChildToParentLinks`
- Le resultat est une separation nette :
  - les handlers Application orchestrent le cas d'usage ;
  - la couche Persistence porte les unions SQL et les details lies au TPT.

---

## Le Probleme A Resoudre

Le modele de domaine des ressources Azure repose sur un heritage TPT :

- les champs communs vivent dans `AzureResource` ;
- les proprietes specifiques vivent dans les tables derivees (`WebApps`, `SqlDatabases`, `ContainerApps`, etc.) ;
- les configurations d'environnement sont eclatees dans de nombreuses tables `*EnvironmentSettings`.

Cela rend certaines lectures transverses couteuses ou fragiles si on les laisse remonter jusqu'aux handlers :

- retrouver le parent d'une ressource enfant sans connaitre son type concret ;
- retrouver les environnements configures pour n'importe quel type de ressource ;
- recuperer des listes legeres sans forcer EF Core a materialiser toute la hierarchie TPT.

La refacto repond a ce besoin avec des read models SQL specialises, exposes via `ProjectDbContext` et encapsules dans `ResourceGroupRepository`.

---

## Surfaces De Lecture A Connaitre

| Besoin | Surface utilisee | Portee |
|--------|------------------|--------|
| Lister toutes les ressources d'un projet | `ListProjectResourcesQueryHandler` + `GetByInfraConfigIdAsync()` | Query projet |
| Lister les ressources d'une resource group avec parent + environnements | `GetResourceSummariesByGroupIdAsync()` + views SQL | Query resource group |
| Resoudre les relations parent-enfant cross-config | `GetChildToParentMappingAsync()` + `GetResourceMetadataBatchAsync()` | Query cross-config |
| Recuperer les environnements configures pour une ressource | `GetConfiguredEnvironmentsByResourceGroupAsync()` + `vw_ResourceEnvironmentEntries` | Enrichissement lecture |

Le point important est le suivant : la query projet reste volontairement simple, tandis que les views prennent en charge les lectures polymorphes reutilisables.

---

## Flux Complet De `GET /projects/{id}/resources`

### Cas d'usage fonctionnel

Cet endpoint sert principalement a alimenter les pickers de ressources cross-config dans le frontend.

- Le frontend appelle `ProjectService.getProjectResources(projectId)`.
- Le backend retourne une liste plate de `ProjectResourceResponse`.
- Le dialogue d'ajout de reference cross-config filtre ensuite :
  - la configuration courante ;
  - les ressources deja referencees.

### Sequence technique

```mermaid
flowchart LR
    A[Angular<br/>ProjectService.getProjectResources] --> B[GET /projects/{id}/resources]
    B --> C[ProjectController]
    C --> D[ListProjectResourcesQuery]
    D --> E[ListProjectResourcesQueryHandler]
    E --> F[VerifyReadAccessAsync]
    F --> G[GetByProjectIdAsync]
    G --> H[GetByInfraConfigIdAsync<br/>Include(Resources)]
    H --> I[ProjectResourceResult]
    I --> J[Mapster -> ProjectResourceResponse]
```

### Etape par etape

1. `ProjectController` expose `GET /projects/{id}/resources`.
2. L'endpoint cree `ListProjectResourcesQuery` et l'envoie a MediatR.
3. `ListProjectResourcesQueryHandler` verifie l'acces en lecture au projet via `IProjectAccessService`.
4. Le handler charge toutes les configurations du projet via `IInfrastructureConfigRepository.GetByProjectIdAsync()`.
5. Pour chaque configuration, il appelle `IResourceGroupRepository.GetByInfraConfigIdAsync()`.
6. Cette methode charge les resource groups de la configuration avec `Include(r => r.Resources)`.
7. Le handler aplatit le resultat dans `ProjectResourceResult`.
8. Mapster convertit ce resultat en `ProjectResourceResponse` avec des IDs `string`.

### Ce que retourne exactement la query

| Champ | Origine | Remarque |
|-------|---------|----------|
| `ResourceId` | `resource.Id.Value` | Converti en `string` a la frontiere HTTP |
| `ResourceName` | `resource.Name.Value` | Nom metier |
| `ResourceType` | `resource.GetType().Name` | Type CLR materialise par EF, pas la colonne SQL `ResourceType` |
| `ResourceGroupName` | `rg.Name.Value` | Contexte d'affichage |
| `ConfigId` | `config.Id.Value` | Converti en `string` via Mapster |
| `ConfigName` | `config.Name.Value` | Utilise notamment pour suggerer un alias cote frontend |

### Ce que cette query ne fait pas

Cette query ne calcule pas :

- les relations parent-enfant generiques ;
- les environnements configures par ressource ;
- les projections legeres basees sur la seule table `AzureResource`.

Ces besoins sont traites plus bas dans la couche de persistance via les views et des methodes repository dediees.

---

## Les Views SQL Introduites Par La Refacto

### `vw_ResourceEnvironmentEntries`

#### But

Unifier tous les `*EnvironmentSettings` dans une seule surface de lecture.

#### Forme logique

Chaque ligne represente :

- un `ResourceGroupId` ;
- un `ResourceId` ;
- un `EnvironmentName`.

#### Ce que la view agrege

La migration `AddResourceTypeAndEnvSettingsView` construit la view en `UNION ALL` sur toutes les tables d'environnements typees :

- `KeyVaultEnvironmentSettings`
- `RedisCacheEnvironmentSettings`
- `StorageAccountEnvironmentSettings`
- `AppServicePlanEnvironmentSettings`
- `WebAppEnvironmentSettings`
- `FunctionAppEnvironmentSettings`
- `AppConfigurationEnvironmentSettings`
- `ContainerAppEnvironmentEnvironmentSettings`
- `ContainerAppEnvironmentSettings`
- `LogAnalyticsWorkspaceEnvironmentSettings`
- `ApplicationInsightsEnvironmentSettings`
- `CosmosDbEnvironmentSettings`
- `SqlServerEnvironmentSettings`
- `SqlDatabaseEnvironmentSettings`
- `ServiceBusNamespaceEnvironmentSettings`
- `ContainerRegistryEnvironmentSettings`
- `EventHubNamespaceEnvironmentSettings`

#### Mapping EF Core

- Entite keyless : `ResourceEnvironmentEntryView`
- Enregistrement : `ProjectDbContext.ResourceEnvironmentEntryViews`
- Mapping : `HasNoKey()` + `ToView("vw_ResourceEnvironmentEntries")`

#### Consommateur principal

`ResourceGroupRepository.GetConfiguredEnvironmentsByResourceGroupAsync()` lit cette view, groupe par `ResourceId`, puis retourne `Dictionary<Guid, List<string>>`.

Le handler `ListResourceGroupResourcesQueryHandler` s'en sert ensuite pour alimenter `AzureResourceResult.ConfiguredEnvironments`.

---

### `vw_ChildToParentLinks`

#### But

Unifier les relations parent-enfant entre ressources concretes sans demander au handler de connaitre chaque table derivee.

#### Forme logique

Chaque ligne represente :

- un `ChildResourceId` ;
- un `ParentResourceId` ;
- un `ResourceGroupId`.

#### Ce que la view agrege

La migration construit cette view en `UNION ALL` sur 5 relations parent-enfant :

- `WebApp -> AppServicePlan`
- `FunctionApp -> AppServicePlan`
- `ContainerApp -> ContainerAppEnvironment`
- `SqlDatabase -> SqlServer`
- `ApplicationInsights -> LogAnalyticsWorkspace`

#### Mapping EF Core

- Entite keyless : `ChildToParentLinkView`
- Enregistrement : `ProjectDbContext.ChildToParentLinkViews`
- Mapping : `HasNoKey()` + `ToView("vw_ChildToParentLinks")`

#### Consommateurs principaux

`ResourceGroupRepository.GetChildToParentMappingAsync()` lit cette view et retourne `Dictionary<Guid, Guid>`.

Ce mapping est reutilise dans :

- `ListResourceGroupResourcesQueryHandler` pour reconstruire une hierarchie parent/enfant stable ;
- `ListIncomingCrossConfigReferencesQueryHandler` pour retrouver les ressources enfants dependantes d'une ressource cible dans une autre configuration.

---

## Le Role De La Colonne `AzureResource.ResourceType`

La meme migration ajoute une colonne `ResourceType` a la table de base `AzureResource`.

Cette colonne remplit deux objectifs :

1. backfill des donnees existantes a partir des tables TPT concretes ;
2. lecture legere depuis la table de base, sans devoir rematerialiser les types derives.

Le `ProjectDbContext.SaveChangesAsync()` renseigne automatiquement `AzureResource.ResourceType` pour toute nouvelle ressource ajoutee.

Cette colonne est particulierement utile dans les methodes repository suivantes :

- `GetResourceSummariesByGroupIdAsync()`
- `GetResourceMetadataBatchAsync()`
- `GetDistinctResourceTypesByProjectIdAsync()`

Autrement dit :

- la query projet derive le type depuis l'objet domaine materialise ;
- les lectures legeres derivent le type depuis la colonne `ResourceType` de la table de base.

Les deux approches coexistent volontairement selon le besoin de lecture.

---

## Pourquoi L'Architecture Est Meilleure Comme Ca

### Ce qui reste dans Application

- l'autorisation ;
- l'orchestration du cas d'usage ;
- la composition du resultat applicatif.

### Ce qui descend dans Persistence

- les unions SQL transverses ;
- les details de tables TPT ;
- les projections legeres reutilisables ;
- la resolution generique parent-enfant et environnements.

### Benefices concrets

- moins de duplication de logique SQL entre handlers ;
- moins de risque d'oublier un type de ressource dans une query metier ;
- des handlers plus faciles a lire et a faire evoluer ;
- un point unique a mettre a jour quand on ajoute une nouvelle relation ou une nouvelle table d'environnement.

---

## Impact Cote Frontend

Le frontend consomme cette lecture via `ProjectService.getProjectResources(projectId)`.

Le consumer le plus important aujourd'hui est `add-cross-config-reference-dialog` :

1. il charge toutes les ressources du projet ;
2. il exclut les ressources de la configuration courante ;
3. il exclut les ressources deja referencees ;
4. il propose un alias base sur `configName` + `resourceName`.

Le contrat HTTP associe est `ProjectResourceResponse` :

- `resourceId`
- `resourceName`
- `resourceType`
- `resourceGroupName`
- `configId`
- `configName`

Si vous changez ce contrat, il faut synchroniser backend, contracts, mapping Mapster et interfaces TypeScript.

---

## Comment Faire Evoluer Ce Systeme Sans Le Casser

### Si vous ajoutez un nouveau type de ressource avec `EnvironmentSettings`

Il faut mettre a jour :

1. la migration qui recree `vw_ResourceEnvironmentEntries` ;
2. le model snapshot EF Core ;
3. eventuellement la doc si le nouveau type modifie les cas d'usage de lecture.

### Si vous ajoutez une nouvelle relation parent-enfant inter-ressources

Il faut mettre a jour :

1. la migration qui recree `vw_ChildToParentLinks` ;
2. la logique de lecture si un nouveau consumer a besoin de cette relation ;
3. la documentation de la relation supportee.

### Si vous changez le payload de la liste projet

Il faut mettre a jour :

1. `ProjectResourceResult`
2. `ProjectResourceResponse`
3. `InfraConfigMappingConfig`
4. les interfaces TypeScript cote frontend
5. les composants qui filtrent ou affichent ces ressources

### Si vous voulez rendre la query projet plus legere

Ne deplacez pas de logique TPT dans le handler. Preferez introduire un read model dedie au niveau repository, sur le meme principe que les views actuelles.

---

## Fichiers A Connaitre Par Coeur

### Entree API et contrat

- `src/Api/InfraFlowSculptor.Api/Controllers/ProjectController.cs`
- `src/Api/InfraFlowSculptor.Application/Projects/Queries/ListProjectResources/ListProjectResourcesQuery.cs`
- `src/Api/InfraFlowSculptor.Application/Projects/Queries/ListProjectResources/ListProjectResourcesQueryHandler.cs`
- `src/Api/InfraFlowSculptor.Application/Projects/Queries/ListProjectResources/ProjectResourceResult.cs`
- `src/Api/InfraFlowSculptor.Contracts/Projects/Responses/ProjectResourceResponse.cs`
- `src/Api/InfraFlowSculptor.Api/Common/Mapping/InfraConfigMappingConfig.cs`

### Persistence et views

- `src/Api/InfraFlowSculptor.Infrastructure/Persistence/ProjectDbContext.cs`
- `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/ResourceGroupRepository.cs`
- `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Views/ResourceEnvironmentEntryView.cs`
- `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Views/ChildToParentLinkView.cs`
- `src/Api/InfraFlowSculptor.Infrastructure/Migrations/20260422141047_AddResourceTypeAndEnvSettingsView.cs`

### Consumers applicatifs adjacents

- `src/Api/InfraFlowSculptor.Application/ResourceGroups/Queries/ListResourceGroupResources/ListResourceGroupResourcesQueryHandler.cs`
- `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Queries/ListIncomingCrossConfigReferences/ListIncomingCrossConfigReferencesQueryHandler.cs`

### Frontend

- `src/Front/src/app/shared/services/project.service.ts`
- `src/Front/src/app/shared/interfaces/cross-config-reference.interface.ts`
- `src/Front/src/app/features/config-detail/add-cross-config-reference-dialog/add-cross-config-reference-dialog.component.ts`

---

## Resume Mental A Garder En Tete

Si vous devez retenir une seule chose :

> `ListProjectResources` sert a enumerer les ressources d'un projet, alors que les views servent a enrichir ou generaliser les lectures transverses que le modele TPT rendrait autrement trop dispersees.

Quand une nouvelle lecture devient polymorphe, repetitive ou dependante de nombreuses tables typees, la bonne direction est en general : repository + read model SQL dedie, pas plus de logique dans les handlers.