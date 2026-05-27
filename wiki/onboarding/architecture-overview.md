# Architecture Overview (Quick Mental Model)

> This page gives you a 5-minute understanding of how InfraFlowSculptor works.
> For deep dives, see the [Architecture](../architecture/README.md) section.

---

## The Big Picture

```mermaid
flowchart TB
    subgraph "Clients"
        UI[Angular Frontend]
        MCP[MCP Agents / Copilot]
    end

    subgraph "API Layer"
        API[ASP.NET Core Minimal API]
        MCPS[MCP HTTP Server]
    end

    subgraph "Application Layer"
        CMD[Commands / Handlers]
        QRY[Queries / Handlers]
        VAL[Validators]
    end

    subgraph "Domain Layer"
        AGG[Aggregates]
        VO[Value Objects]
        EVT[Domain Events]
    end

    subgraph "Infrastructure Layer"
        EF[EF Core / PostgreSQL]
        BLOB[Azure Blob Storage]
        GIT[Azure DevOps Git Client]
        KV[Key Vault Client]
    end

    subgraph "Generation Engines"
        BICEP[Bicep Generation Engine]
        PIPE[Pipeline Generation Engine]
    end

    UI --> API
    MCP --> MCPS
    MCPS --> CMD
    MCPS --> QRY
    API --> CMD
    API --> QRY
    CMD --> VAL
    CMD --> AGG
    QRY --> EF
    AGG --> EF
    CMD --> BICEP
    CMD --> PIPE
    EF --> DB[(PostgreSQL)]
    BICEP --> BLOB
    PIPE --> BLOB
    CMD --> GIT
```

## Layered Architecture

InfraFlowSculptor follows **Clean Architecture** with clear separation of concerns:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **API** | `InfraFlowSculptor.Api` | HTTP endpoints, request routing, OpenAPI |
| **Application** | `InfraFlowSculptor.Application` | CQRS handlers, business orchestration |
| **Domain** | `InfraFlowSculptor.Domain` | Aggregates, entities, business rules |
| **Infrastructure** | `InfraFlowSculptor.Infrastructure` | Persistence, external services |
| **Contracts** | `InfraFlowSculptor.Contracts` | Request/Response DTOs |
| **Generation** | `InfraFlowSculptor.BicepGeneration` | Bicep code generation engine |
| **Generation** | `InfraFlowSculptor.PipelineGeneration` | Pipeline YAML generation engine |
| **Generation** | `InfraFlowSculptor.GenerationCore` | Shared generation contracts |

## Key Design Patterns

| Pattern | Where | Why |
|---------|-------|-----|
| **CQRS** | Application layer | Separate read/write models, clear intent |
| **DDD** | Domain layer | Rich domain model, enforced invariants |
| **Repository** | Infrastructure | Abstract persistence, testable |
| **Unit of Work** | MediatR pipeline | Single SaveChanges per command |
| **Result Pattern** | All handlers | `ErrorOr<T>` instead of exceptions |
| **TPT Inheritance** | EF Core | 22 resource types share a base |
| **Value Objects** | Domain | Type safety for IDs, enums |
| **Pipeline (staged)** | Bicep generation | 10 ordered stages for module building |

## Request Flow

Every HTTP request follows this path:

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API
    participant Mapster
    participant MediatR
    participant Validator as FluentValidation
    participant Auth as PAT/JWT Auth
    participant UoW as Unit of Work
    participant Handler
    participant Domain
    participant EF as EF Core
    participant DB as PostgreSQL

    Client->>API: HTTP Request
    API->>Mapster: Map Request DTO → Command/Query
    Mapster->>MediatR: Send Command/Query
    MediatR->>Validator: Validate input
    Validator-->>MediatR: Pass/Fail
    MediatR->>Auth: Check PAT scopes
    MediatR->>UoW: Begin tracking
    MediatR->>Handler: Execute
    Handler->>Domain: Business logic
    Domain->>EF: Persist changes
    EF->>DB: SQL
    UoW->>EF: SaveChangesAsync
    Handler-->>MediatR: ErrorOr<Result>
    MediatR-->>API: Response
    API->>Mapster: Map Result → Response DTO
    API-->>Client: HTTP Response
```

## Generation Flow

Infrastructure artifact generation:

```mermaid
flowchart LR
    A[Domain Model] --> B[Read Repository]
    B --> C[Generation Engine]
    C --> D[9 Pipeline Stages]
    D --> E[IR / BicepModuleSpec]
    E --> F[BicepEmitter]
    F --> G[Generated Files]
    G --> H[Azure Blob Storage]
    G --> I[Push to Git]
```

## What You Don't Need to Know Yet

- MCP tool internals (see [MCP section](../infrastructure/mcp.md) when ready)
- Bicep IR details (see [Backend > Bicep Generation](../backend/bicep-generation.md))
- Frontend design system internals (see [Frontend](../frontend/README.md))
- Deployment procedures (see [DevOps](../devops/README.md))
