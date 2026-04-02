# Solution Overview

**Product goal:** A single unified API for managing Azure infrastructure configuration and generating Azure Bicep files and Azure DevOps pipelines from it. All code lives in the standard layered projects (Domain, Application, Infrastructure, Contracts, Api) plus a dedicated `InfraFlowSculptor.BicepGeneration` project for the pure Bicep generation engine.

**Technology stack:**
- .NET 10 (global.json SDK 10.0.100)
- ASP.NET Core Minimal APIs
- MediatR (CQRS)
- FluentValidation
- Mapster (object mapping)
- EF Core + PostgreSQL
- Azure AD / Entra ID (JWT Bearer auth)
- ErrorOr (result pattern)
- .NET Aspire (local orchestration)
- Central Package Management (`Directory.Packages.props`)

**Solution file:** `InfraFlowSculptor.slnx`
