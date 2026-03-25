# Infra Flow Sculptor

Application web de **modélisation d'infrastructure Azure** et de **génération automatique de fichiers Bicep**.

Permet de configurer des ressources Azure (Key Vault, Storage Account, Redis, App Service, Container Apps, SQL, Cosmos DB, Service Bus…), de gérer les paramètres par environnement, les conventions de nommage, et de générer les fichiers Bicep prêts à être déployés.

---

## Stack technique

- **Backend :** .NET 10, ASP.NET Core Minimal APIs, MediatR (CQRS), FluentValidation, Mapster, EF Core + PostgreSQL, ErrorOr
- **Frontend :** Angular 19, Angular Material, Tailwind CSS, Axios
- **Auth :** Microsoft Entra ID (Azure AD), JWT Bearer
- **Orchestration locale :** .NET Aspire

## Démarrage rapide

```bash
# Build
dotnet build .\InfraFlowSculptor.slnx

# Lancer tout le stack en local (PostgreSQL + API + Frontend)
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj

# Ou lancer l'API seule
dotnet run --project .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj

# Frontend (depuis src/Front)
npm install
npm run start
```

## Documentation

La documentation complète (architecture, DDD, CQRS, couche API, persistance) se trouve dans [`docs/`](docs/README.md).