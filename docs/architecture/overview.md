# Architecture du projet — Vue d'ensemble

## Objectif du projet

**Infra Flow Sculptor** est une application web permettant de **modéliser une infrastructure Azure** (Key Vault, Storage Account, Redis, App Service, Container Apps, SQL, Cosmos DB, Service Bus…) puis de **générer automatiquement les fichiers Bicep** correspondants. L'application gère également les conventions de nommage, les configurations par environnement et le push des fichiers générés vers un dépôt Git.

---

## Stack technique

| Composant | Technologie |
|-----------|-------------|
| Framework backend | .NET 10, ASP.NET Core Minimal APIs |
| CQRS | MediatR |
| Validation | FluentValidation |
| Object mapping | Mapster |
| Base de données | PostgreSQL via EF Core |
| Authentification | Microsoft Entra ID (Azure AD), JWT Bearer |
| Pattern de résultat | ErrorOr |
| Orchestration locale | .NET Aspire |
| Frontend | Angular 19, Angular Material, Tailwind CSS |
| Client HTTP frontend | Axios |
| Gestion de packages | Central Package Management (`Directory.Packages.props`) |

---

## Structure de la solution

```
src/
├── Api/                                     API unifiée
│   ├── InfraFlowSculptor.Api               Endpoints Minimal API, Mapster, DI, erreurs, rate limiting, OpenAPI
│   ├── InfraFlowSculptor.Application       Commandes/Queries (CQRS), handlers, validators, interfaces
│   ├── InfraFlowSculptor.BicepGeneration   Moteur de génération Bicep (auto-contenu, pas de dépendance au Domain)
│   ├── InfraFlowSculptor.Domain            Agrégats, entités, value objects, classes DDD de base, erreurs
│   ├── InfraFlowSculptor.Infrastructure    EF Core, repositories, services Azure, blob, auth
│   └── InfraFlowSculptor.Contracts         DTOs requête/réponse, attributs de validation
│
├── Front/                                   Application Angular
│   ├── src/app/core                         Composants de layout (navigation, footer)
│   ├── src/app/features                     Pages fonctionnelles (login, home, config-detail…)
│   ├── src/app/shared                       Services, guards, interfaces, composants partagés
│   └── src/environments                     Configuration des URLs API par environnement
│
└── Aspire/
    ├── InfraFlowSculptor.AppHost           Orchestration des services (PostgreSQL, API, frontend)
    └── InfraFlowSculptor.ServiceDefaults   Defaults Aspire partagés
```

---

## Couches et responsabilités

L'architecture suit le pattern **Clean Architecture** avec séparation stricte des couches :

```
┌──────────────────────────────────────────────────────┐
│                      API Layer                        │
│   Endpoints, Mapster mapping, error handling          │
│   (InfraFlowSculptor.Api)                            │
├──────────────────────────────────────────────────────┤
│                  Application Layer                    │
│   Commands, Queries, Handlers, Validators             │
│   Service interfaces, result DTOs                     │
│   (InfraFlowSculptor.Application)                    │
├──────────────────────────────────────────────────────┤
│                    Domain Layer                       │
│   Aggregates, Entities, Value Objects, Errors         │
│   Règles métier pures, aucune dépendance externe      │
│   (InfraFlowSculptor.Domain)                         │
├──────────────────────────────────────────────────────┤
│                Infrastructure Layer                   │
│   EF Core, Repositories, Azure services               │
│   Implémentation des interfaces Application           │
│   (InfraFlowSculptor.Infrastructure)                 │
├──────────────────────────────────────────────────────┤
│                    Contracts                          │
│   DTOs HTTP (Request/Response), partagés API↔Front    │
│   (InfraFlowSculptor.Contracts)                      │
└──────────────────────────────────────────────────────┘
```

### Règle de dépendance

Les dépendances vont **toujours vers l'intérieur** :
- **Domain** → aucune dépendance (couche la plus interne)
- **Application** → dépend de Domain
- **Infrastructure** → dépend de Application et Domain
- **API** → dépend de Application, Domain, Infrastructure et Contracts
- **Contracts** → aucune dépendance (DTOs purs)

Le Domain ne connaît ni EF Core, ni MediatR, ni ASP.NET. Il contient uniquement des classes C# pures.

---

## Flux d'une requête HTTP

Voici le cheminement complet d'une requête, de l'appel HTTP à la réponse :

```
1. HTTP Request (ex: POST /keyvault)
       │
2. Endpoint Minimal API  (Api/Controllers/KeyVaultController.cs)
       │  Mapster : Request DTO → Command
       │
3. MediatR Pipeline
       │  ValidationBehavior → FluentValidation
       │  si erreur → ErrorOr<T> avec erreurs de validation
       │
4. MediatR Pipeline (suite)
       │  UnitOfWorkBehavior (commands uniquement)
       │  Wraps le handler et appelle SaveChangesAsync si succès
       │
5. Command Handler  (Application/KeyVaults/Commands/CreateKeyVault/)
       │  Vérification d'accès (IInfraConfigAccessService)
       │  Création du domaine (KeyVault.Create(...))
       │  Repository : track les changements (pas de SaveChanges)
       │  Mapster : Domain → Result DTO
       │
6. UnitOfWorkBehavior : SaveChangesAsync() si succès
       │
7. Retour ErrorOr<KeyVaultResult>
       │
8. Endpoint → result.Match(ok => Results.Ok(...), errors => errors.ToErrorResult())
       │
9. HTTP Response (200 OK ou 400/403/404/500)
```

---

## Enregistrement des services (Dependency Injection)

Chaque couche expose une méthode d'extension pour enregistrer ses services :

| Méthode | Projet | Ce qu'elle enregistre |
|---------|--------|----------------------|
| `AddPresentation()` | Api | Mapster, Authorization policies, OpenAPI |
| `AddApplication()` | Application | MediatR, ValidationBehavior, UnitOfWorkBehavior, FluentValidation, services métier, générateurs Bicep |
| `AddInfrastructure()` | Infrastructure | Repositories, UnitOfWork, Auth JWT, Azure clients, Blob, Git providers, CurrentUser |

Dans `Program.cs` :
```csharp
builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);
```

---

## Pages suivantes

- [Domain-Driven Design (DDD)](ddd-concepts.md) — Agrégats, entités, value objects
- [CQRS et MediatR](cqrs-patterns.md) — Commandes, queries, handlers, validation
- [Couche API](api-layer.md) — Endpoints, mapping, gestion d'erreurs
- [Persistance EF Core](persistence.md) — Repositories, configurations, conventions
- [Lecture des ressources projet et views SQL](project-resource-queries.md) — Flux des queries de lecture projet, projections légères et views keyless
- [Guide de navigation](getting-started.md) — Comment trouver et ajouter du code
