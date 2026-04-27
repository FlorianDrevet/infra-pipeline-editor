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

## Repository Positioning

- The root `README.md` was upgraded on 2026-04-16 into a repository landing page focused on product value, architecture credibility, engineering quality, and onboarding links for GitHub visitors.
- Since 2026-04-27, the repository is distributed under PolyForm Noncommercial 1.0.0 instead of MIT: the source stays public for evaluation and noncommercial use, while commercial use requires a separate agreement with the author.
