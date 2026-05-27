# Architecture Overview

## C4 Model — System Context

```mermaid
C4Context
    title InfraFlowSculptor — System Context Diagram

    Person(platformEng, "Platform Engineer", "Models Azure infrastructure configurations")
    Person(devOps, "DevOps Engineer", "Manages generated pipelines and deployments")
    Person(aiAgent, "AI Agent", "Creates projects via MCP natural language")

    System(ifs, "InfraFlowSculptor", "Azure infrastructure modeling and code generation platform")

    System_Ext(entra, "Microsoft Entra ID", "Authentication & identity provider")
    System_Ext(azdo, "Azure DevOps", "Git repositories, pipelines, variable groups")
    System_Ext(azure, "Azure Cloud", "Target infrastructure environment")
    System_Ext(blob, "Azure Blob Storage", "Generated artifact storage")
    System_Ext(kv, "Azure Key Vault", "Secrets management (production)")

    Rel(platformEng, ifs, "Models infrastructure", "HTTPS")
    Rel(devOps, ifs, "Reviews & deploys", "HTTPS")
    Rel(aiAgent, ifs, "Creates via MCP", "HTTP/MCP")
    Rel(ifs, entra, "Authenticates users", "OAuth2/OIDC")
    Rel(ifs, azdo, "Pushes artifacts", "REST API")
    Rel(ifs, blob, "Stores generated files", "SDK")
    Rel(ifs, kv, "Reads secrets", "SDK")
    Rel(azdo, azure, "Deploys infrastructure", "Pipelines")
```

## C4 Model — Container Diagram

```mermaid
C4Container
    title InfraFlowSculptor — Container Diagram

    Person(user, "User", "Platform Engineer / DevOps")

    System_Boundary(ifs, "InfraFlowSculptor Platform") {
        Container(spa, "Angular SPA", "Angular 21, TypeScript", "Single-page application for infrastructure modeling")
        Container(api, "API Server", ".NET 10, ASP.NET Core", "REST API with CQRS, domain model, generation engines")
        Container(mcp, "MCP Server", ".NET 10, ASP.NET Core", "Model Context Protocol server for AI agents")
        Container(db, "PostgreSQL", "PostgreSQL 17.6", "Persistent storage for projects, configs, resources")
        Container(blobStore, "Blob Storage", "Azure/Emulator", "Generated Bicep & pipeline file storage")
    }

    Rel(user, spa, "Uses", "HTTPS")
    Rel(spa, api, "Calls", "REST/JSON")
    Rel(mcp, api, "Shares Application layer", "In-process")
    Rel(api, db, "Reads/Writes", "EF Core/SQL")
    Rel(api, blobStore, "Stores artifacts", "SDK")
    Rel(mcp, db, "Reads/Writes", "EF Core/SQL")
```

## C4 Model — Component Diagram (API Server)

```mermaid
C4Component
    title API Server — Internal Components

    Container_Boundary(api, "API Server") {
        Component(endpoints, "Minimal API Endpoints", "ASP.NET Core", "HTTP routing, request/response mapping")
        Component(mediatr, "MediatR Pipeline", "MediatR", "Command/query dispatch with behaviors")
        Component(validators, "Validators", "FluentValidation", "Input validation")
        Component(handlers, "Handlers", "C#", "Business logic orchestration")
        Component(domain, "Domain Model", "C# DDD", "Aggregates, entities, value objects")
        Component(repos, "Repositories", "EF Core", "Data access layer")
        Component(bicepEngine, "Bicep Engine", "C#", "10-stage Bicep generation pipeline")
        Component(pipeEngine, "Pipeline Engine", "C#", "Azure DevOps YAML generation")
        Component(gitClient, "Git Client", "Refit", "Azure DevOps REST API client")
    }

    Rel(endpoints, mediatr, "Sends commands/queries")
    Rel(mediatr, validators, "Validates")
    Rel(mediatr, handlers, "Dispatches")
    Rel(handlers, domain, "Uses")
    Rel(handlers, repos, "Reads/Writes")
    Rel(handlers, bicepEngine, "Generates Bicep")
    Rel(handlers, pipeEngine, "Generates Pipelines")
    Rel(handlers, gitClient, "Pushes to Git")
    Rel(repos, domain, "Persists")
```

---

## Design Principles

### Clean Architecture

The project strictly follows Clean Architecture with the **Dependency Rule**: source code dependencies must point only inward.

```mermaid
flowchart TB
    subgraph Outer["Infrastructure & Presentation"]
        API[API Endpoints]
        EF[EF Core]
        BLOB[Blob Storage]
        GIT[Git Client]
    end
    
    subgraph Middle["Application"]
        CMD[Commands]
        QRY[Queries]
        HND[Handlers]
    end
    
    subgraph Inner["Domain"]
        AGG[Aggregates]
        VO[Value Objects]
        ERR[Domain Errors]
    end
    
    API --> CMD
    API --> QRY
    CMD --> AGG
    QRY --> AGG
    HND --> AGG
    EF --> AGG
    
    style Inner fill:#1a237e,color:#fff
    style Middle fill:#0288d1,color:#fff
    style Outer fill:#00bcd4,color:#000
```

**Key rule:** The Domain layer has **zero** external dependencies. It doesn't know about EF Core, ASP.NET, or any infrastructure concern.

### SOLID Principles Applied

| Principle | Example in Codebase |
|-----------|-------------------|
| **S**ingle Responsibility | Each handler does one thing |
| **O**pen/Closed | New resource types via new classes, not modifying existing |
| **L**iskov Substitution | All `AzureResource` subtypes are interchangeable |
| **I**nterface Segregation | `IRepository<T>` vs `IReadOnlyRepository<T>` |
| **D**ependency Inversion | Handlers depend on `IRepository<T>`, not `DbContext` |

### Error Handling Philosophy

```mermaid
flowchart LR
    A[Input] --> B{Valid?}
    B -->|No| C[FluentValidation Error]
    B -->|Yes| D{Authorized?}
    D -->|No| E[Error.Unauthorized]
    D -->|Yes| F{Business Rule?}
    F -->|Violated| G[Domain Error]
    F -->|OK| H[Success Result]
    
    C --> I[400 Bad Request]
    E --> J[401/403]
    G --> K[409 Conflict / 404 Not Found]
    H --> L[200/201 OK]
```

Exceptions are reserved for truly unexpected situations (infrastructure failures). All expected errors flow through `ErrorOr<T>`.

---

## Deployment Architecture

```mermaid
flowchart TB
    subgraph "Azure Cloud"
        subgraph "Container Apps Environment"
            FEAPP[Frontend Container App]
            APIAPP[API Container App]
            MCPAPP[MCP Container App]
        end
        
        subgraph "Data"
            PG[(PostgreSQL Flexible Server)]
            BLOB[Azure Blob Storage]
            KV[Azure Key Vault]
        end
        
        subgraph "Identity & Security"
            ENTRA[Microsoft Entra ID]
            UAI[User Assigned Identity]
        end
        
        subgraph "Monitoring"
            AI[Application Insights]
            LAW[Log Analytics Workspace]
        end
    end
    
    FEAPP --> APIAPP
    APIAPP --> PG
    APIAPP --> BLOB
    APIAPP --> KV
    MCPAPP --> PG
    MCPAPP --> BLOB
    APIAPP --> ENTRA
    APIAPP --> AI
    MCPAPP --> AI
    AI --> LAW
```

---

## Module Dependency Graph

```mermaid
flowchart TB
    API[InfraFlowSculptor.Api] --> APP[InfraFlowSculptor.Application]
    API --> CONTRACTS[InfraFlowSculptor.Contracts]
    API --> INFRA[InfraFlowSculptor.Infrastructure]
    
    MCP[InfraFlowSculptor.Mcp] --> APP
    MCP --> INFRA
    MCP --> CONTRACTS
    
    APP --> DOMAIN[InfraFlowSculptor.Domain]
    APP --> BICEP[InfraFlowSculptor.BicepGeneration]
    APP --> PIPE[InfraFlowSculptor.PipelineGeneration]
    APP --> GENCORE[InfraFlowSculptor.GenerationCore]
    
    INFRA --> DOMAIN
    INFRA --> APP
    
    BICEP --> GENCORE
    PIPE --> GENCORE
    
    style DOMAIN fill:#1a237e,color:#fff
    style APP fill:#0288d1,color:#fff
    style API fill:#00bcd4,color:#000
    style MCP fill:#00bcd4,color:#000
```

**Important:** The MCP server does **not** reference the API project directly. It shares the Application and Infrastructure layers.
