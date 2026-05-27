# Backend Project Structure

## Solution Layout

```
InfraFlowSculptor.slnx
│
├── src/Api/
│   ├── InfraFlowSculptor.Api/                    → HTTP layer (entry point)
│   ├── InfraFlowSculptor.Application/            → CQRS, handlers, services
│   ├── InfraFlowSculptor.Domain/                 → Aggregates, DDD model
│   ├── InfraFlowSculptor.Infrastructure/         → EF Core, external services
│   ├── InfraFlowSculptor.Contracts/              → Request/response DTOs
│   ├── InfraFlowSculptor.BicepGeneration/        → Bicep code generation
│   ├── InfraFlowSculptor.PipelineGeneration/     → Pipeline YAML generation
│   └── InfraFlowSculptor.GenerationCore/         → Shared generation contracts
│
├── src/Mcp/
│   └── InfraFlowSculptor.Mcp/                    → MCP HTTP server
│
├── src/Aspire/
│   ├── InfraFlowSculptor.AppHost/                → Aspire orchestration
│   └── InfraFlowSculptor.ServiceDefaults/        → Shared Aspire config
│
├── src/Front/                                     → Angular 21 SPA
│
└── tests/
    ├── InfraFlowSculptor.Domain.Tests/
    ├── InfraFlowSculptor.Application.Tests/
    ├── InfraFlowSculptor.Infrastructure.Tests/
    ├── InfraFlowSculptor.Api.Tests/
    ├── InfraFlowSculptor.BicepGeneration.Tests/
    ├── InfraFlowSculptor.PipelineGeneration.Tests/
    ├── InfraFlowSculptor.GenerationCore.Tests/
    ├── InfraFlowSculptor.Contracts.Tests/
    └── InfraFlowSculptor.Mcp.Tests/
```

---

## Assembly Responsibilities

### InfraFlowSculptor.Api

The HTTP entry point. Contains:

| Component | Location | Purpose |
|-----------|----------|---------|
| Minimal API endpoints | `Controllers/` | Route definitions, HTTP mapping |
| OpenAPI configuration | `Common/OpenApi/` | Scalar UI, schema generation |
| Mapster mapping | `Common/Mapping/` | Request/Response object mapping |
| DI registration | `Program.cs` | Service composition root |
| Middleware | `Common/Middleware/` | Security headers, error handling |
| Rate limiting | `Common/RateLimiting/` | Throttling configuration |

**Dependencies:** Application, Contracts, Infrastructure

### InfraFlowSculptor.Application

The orchestration layer. Contains:

| Component | Location | Purpose |
|-----------|----------|---------|
| Commands | `{Feature}/Commands/` | Write operations |
| Queries | `{Feature}/Queries/` | Read operations |
| Handlers | `{Feature}/Commands/*.Handler.cs` | Business logic |
| Validators | `{Feature}/Commands/*.Validator.cs` | Input validation |
| Interfaces | `Common/Interfaces/` | Repository contracts |
| Behaviors | `Common/Behaviors/` | MediatR pipeline |
| Services | `Common/Services/` | Application services |
| Imports | `Imports/` | ARM import analysis |

**Dependencies:** Domain, BicepGeneration, PipelineGeneration, GenerationCore

### InfraFlowSculptor.Domain

The business core. Contains:

| Component | Location | Purpose |
|-----------|----------|---------|
| Aggregates | `{Name}Aggregate/` | Root entities with behavior |
| Value Objects | `Common/Models/` | Immutable typed values |
| Errors | `Common/Errors/` | Domain error definitions |
| Base classes | `Common/BaseModels/` | `AggregateRoot`, `Entity`, `ValueObject` |
| Shared entities | `Common/BaseModels/Entities/` | `AppSetting`, `RoleAssignment`, etc. |

**Dependencies:** None (zero external packages)

### InfraFlowSculptor.Infrastructure

The external world adapter. Contains:

| Component | Location | Purpose |
|-----------|----------|---------|
| DbContext | `Persistence/ProjectDbContext.cs` | EF Core context |
| Configurations | `Persistence/Configurations/` | Entity type configs |
| Repositories | `Persistence/Repositories/` | Data access |
| Converters | `Persistence/Converters/` | Value type converters |
| Migrations | `Persistence/Migrations/` | Schema changes |
| Auth services | `Authentication/` | PAT handler, user provisioning |
| Git client | `Git/` | Azure DevOps API (Refit) |
| Blob storage | `BlobStorage/` | Generated file storage |

**Dependencies:** Application, Domain

### InfraFlowSculptor.Contracts

Public API surface (DTOs):

| Component | Location | Purpose |
|-----------|----------|---------|
| Requests | `{Feature}/Requests/` | Input DTOs |
| Responses | `{Feature}/Responses/` | Output DTOs |

**Dependencies:** None

### InfraFlowSculptor.BicepGeneration

The Bicep code generation engine:

| Component | Location | Purpose |
|-----------|----------|---------|
| Generators | `Generators/` | Per-resource-type generators |
| IR model | `Ir/` | Intermediate representation |
| Builder | `Ir/Builder/` | Fluent spec builder |
| Emitter | `Ir/Emitter/` | IR → text conversion |
| Pipeline | `Pipeline/` | 10-stage generation pipeline |
| Assembler | `Assembly/` | Final file assembly |
| Helpers | `Helpers/` | Naming, identifiers |

**Dependencies:** GenerationCore only

### InfraFlowSculptor.PipelineGeneration

The Azure DevOps pipeline generation engine:

| Component | Location | Purpose |
|-----------|----------|---------|
| Assembler | `MonoRepoPipelineAssembler.cs` | Main entry point |
| Models | `Models/` | Pipeline result types |
| Templates | (generated inline) | YAML template content |

**Dependencies:** GenerationCore only

### InfraFlowSculptor.GenerationCore

Shared contracts between generation engines:

| Component | Location | Purpose |
|-----------|----------|---------|
| Models | `Models/` | `GenerationRequest`, `ResourceDefinition`, etc. |
| Constants | `AzureResourceTypes.cs` | Resource type identifiers |

**Dependencies:** None (contracts-only assembly)

---

## Dependency Rules

```
Domain         → (nothing)
GenerationCore → (nothing)
Contracts      → (nothing)
Application    → Domain, GenerationCore, BicepGeneration, PipelineGeneration
Infrastructure → Domain, Application
Api            → Application, Contracts, Infrastructure
Mcp            → Application, Infrastructure, Contracts
BicepGeneration → GenerationCore
PipelineGeneration → GenerationCore
```

**Enforced by:** Build-time project references + structural tests (`McpProjectTopologyTests`, `GenerationCoreBoundaryTests`).
