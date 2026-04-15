# Project Structure

```
src/
├── Api/                                    Single unified API
│   ├── InfraFlowSculptor.Api               Minimal API endpoints, Mapster, DI wiring, error handling, rate limiting, OpenAPI config
│   ├── InfraFlowSculptor.Application       CQRS commands/queries/handlers/validators, IRepository<T>, service interfaces
│   ├── InfraFlowSculptor.BicepGeneration   Pure Bicep generation engine (self-contained, no domain dependency)
│   │   ├── Models/                        DTOs: GenerationRequest, GenerationResult, EnvironmentDefinition, ResourceDefinition, etc.
│   │   ├── Generators/                    IResourceTypeBicepGenerator + per-resource-type implementations + factory
│   │   └── Helpers/                       BicepIdentifierHelper, NamingTemplateTranslator
│   ├── InfraFlowSculptor.GenerationCore    Shared generation models between Bicep and Pipeline engines
│   │   └── Models/                        IGenerationResult, GenerationRequest, EnvironmentDefinition, ResourceDefinition, etc.
│   ├── InfraFlowSculptor.PipelineGeneration  Azure DevOps pipeline YAML generation engine (mono-repo structure)
│   │   ├── Models/                        PipelineGenerationResult, MonoRepoPipelineResult, PipelineGenerationOptions
│   │   └── MonoRepoPipelineAssembler.cs   Assembles shared templates (.azuredevops/v0/) + per-config pipeline files
│   ├── InfraFlowSculptor.Domain            Aggregates, entities, value objects, DDD base classes, errors
│   ├── InfraFlowSculptor.Infrastructure    EF Core, repositories, base repository, converters, Azure services, blob storage
│   └── InfraFlowSculptor.Contracts         Request/response DTOs, validation attributes
├── Front/                                  Angular frontend (UI + API consumption)
│   ├── src/app/core                        Layout components (navigation/footer)
│   ├── src/app/shared                      Cross-cutting frontend services/guards/facades
│   ├── src/environments                    API base URL and runtime environment config
│   └── src/scss                            Global theming variables and style modules
└── Aspire/
    ├── InfraFlowSculptor.AppHost           Service orchestration (PostgreSQL, DbGate, single API)
    └── InfraFlowSculptor.ServiceDefaults   Shared Aspire defaults
```

## Documentation Artifacts

- `docs/architecture/overview.md` provides the written architecture overview.
- `docs/architecture/infraflowsculptor-architecture.drawio` provides a visual Azure deployment diagram centered on deployed resources and interactions, with separate frontend/backend Azure Container Apps and surrounding Azure services.

## Automation Scripts

- `scripts/create-audit-labels.ps1` is the Windows-first entry point for creating the GitHub audit labels via `gh`.
- `scripts/create-audit-issues.ps1` is the Windows-first entry point for creating the GitHub audit issues via `gh`; it parses the existing shell definitions to avoid duplicating 62 issue bodies.
- The audit PowerShell entry points are intentionally compatible with both Windows PowerShell 5.1 and PowerShell 7.x; do not add a `#Requires -Version 7.0` guard unless a real 7-only feature is introduced.
