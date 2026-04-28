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
├── Mcp/
│   └── InfraFlowSculptor.Mcp              MCP server (HTTP transport under Aspire, ModelContextProtocol SDK v1.2.0)
│       ├── Tools/                         DiscoveryTools, ProjectDraftTools, ProjectCreationTools, BicepGenerationTools, IacImportTools
│       ├── Drafts/                        IProjectDraftService, ProjectDraftService, DraftOverrides, ProjectCreationDraft
│       ├── Imports/                       IImportPreviewService, ImportPreviewService, canonical models, ImportPreviewResources
│       ├── Prompts/                       ProjectCreationPrompts
│       └── Resources/                     ProjectResources
└── Aspire/
    ├── InfraFlowSculptor.AppHost           Service orchestration (PostgreSQL, DbGate, single API)
    └── InfraFlowSculptor.ServiceDefaults   Shared Aspire defaults
```

## Documentation Artifacts

- `docs/architecture/overview.md` provides the written architecture overview.
- `docs/architecture/mcp-integration.md` is the MCP onboarding course for this repository: concepts, .NET integration model, local vs remote exposure, recommended tool surface, and review checklist for future generated MCP code. The current workspace now runs MCP through HTTP under Aspire rather than workspace-local stdio.
- `docs/architecture/mcp-v1-implementation-plan.md` is the MCP V1 implementation plan: 5 phases (scaffolding → discovery → creation → generation → import), exhaustive JSON contracts for 8 tools, 2 resources, 1 prompt, canonical import model (`ImportedProjectDefinition`), test strategy, and validation checklist.
- `docs/architecture/infraflowsculptor-architecture.drawio` provides a visual Azure deployment diagram centered on deployed resources and interactions, with separate frontend/backend Azure Container Apps and surrounding Azure services.

## Automation Scripts

- `scripts/create-audit-labels.ps1` is the Windows-first entry point for creating the GitHub audit labels via `gh`.
- `scripts/create-audit-issues.ps1` is the Windows-first entry point for creating the GitHub audit issues via `gh`; it parses the existing shell definitions to avoid duplicating 62 issue bodies.
- `scripts/seed-project-snapshot.ps1` [2026-04-22] recreates a full project configuration from a snapshot doc by calling the API in order (Project → Envs → Naming → InfraConfigs → Resources → RoleAssignments → AppSettings). Accepts `-ApiBaseUrl`, `-BearerToken`, `-GitPat`.
- The audit PowerShell entry points are intentionally compatible with both Windows PowerShell 5.1 and PowerShell 7.x; do not add a `#Requires -Version 7.0` guard unless a real 7-only feature is introduced.
