# Data Flow

## Request Lifecycle

### HTTP Request → Response

```mermaid
sequenceDiagram
    participant Browser as Angular SPA
    participant Proxy as Proxy (dev)
    participant MW as Middleware Stack
    participant Endpoint as Minimal API
    participant Mapster as Mapster Mapping
    participant MediatR as MediatR
    participant Handler as Handler
    participant Domain as Domain Model
    participant EF as EF Core
    participant PG as PostgreSQL

    Browser->>Proxy: HTTP Request (4200 → 5000)
    Proxy->>MW: Forward
    MW->>MW: Security Headers
    MW->>MW: Rate Limiting
    MW->>MW: User Provisioning
    MW->>MW: Authentication (JWT/PAT)
    MW->>Endpoint: Route matched
    Endpoint->>Mapster: Map Request DTO → Command
    Mapster->>MediatR: ISender.Send(command)
    MediatR->>MediatR: ValidationBehavior
    MediatR->>MediatR: PATScopeBehavior
    MediatR->>Handler: Handle(command, ct)
    Handler->>Domain: Business logic
    Handler->>EF: Repository calls
    EF->>PG: SQL queries
    PG-->>EF: Results
    EF-->>Handler: Domain entities
    Handler-->>MediatR: ErrorOr<Result>
    MediatR->>MediatR: UnitOfWorkBehavior (SaveChanges)
    MediatR-->>Endpoint: Result
    Endpoint->>Mapster: Map Result → Response DTO
    Endpoint-->>Browser: HTTP Response (JSON)
```

---

## Bicep Generation Flow

The generation process transforms the domain model into deployable Bicep files:

```mermaid
flowchart TB
    subgraph "Input"
        CMD[GenerateProjectBicepCommand]
        REPO[Repository Reads]
    end
    
    subgraph "Engine Setup"
        REQ[GenerationRequest built from domain]
        CTX[BicepGenerationContext created]
    end
    
    subgraph "10-Stage Pipeline"
        S100[100: IdentityAnalysisStage]
        S200[200: AppSettingsAnalysisStage]
        S300[300: ModuleBuildStage]
        S400[400: IdentityInjectionStage]
        S500[500: OutputInjectionStage]
        S600[600: AppSettingsInjectionStage]
        S700[700: TagsInjectionStage]
        S800[800: ParentReferenceResolutionStage]
        S850[850: SpecEmissionStage]
        S900[900: AssemblyStage]
    end
    
    subgraph "Output"
        FILES[Generated File Dictionary]
        BLOB[Azure Blob Storage]
    end
    
    CMD --> REPO
    REPO --> REQ
    REQ --> CTX
    CTX --> S100 --> S200 --> S300 --> S400 --> S500 --> S600 --> S700 --> S800 --> S850 --> S900
    S900 --> FILES
    FILES --> BLOB
```

### Stage Details

| Stage | Order | Purpose | Input | Output |
|-------|-------|---------|-------|--------|
| IdentityAnalysis | 100 | Determines which resources need system/user-assigned identities | Resource definitions | Identity sets on context |
| AppSettingsAnalysis | 200 | Identifies which outputs feed into app settings | Resource connections | Output injection list |
| ModuleBuild | 300 | Calls per-type generators to produce IR specs | Resource definitions | `ModuleWorkItem` with `BicepModuleSpec` |
| IdentityInjection | 400 | Adds identity blocks to modules | Identity analysis | Modified specs |
| OutputInjection | 500 | Adds output declarations | AppSettings analysis | Modified specs |
| AppSettingsInjection | 600 | Injects app setting parameter references | Output analysis | Modified specs |
| TagsInjection | 700 | Adds `tags` parameter to all modules | — | Modified specs |
| ParentReferenceResolution | 800 | Resolves parent resource references (ASP→WebApp, etc.) | FK relationships | Modified specs |
| SpecEmission | 850 | Converts IR (`BicepModuleSpec`) to Bicep text | IR specs | Text modules |
| Assembly | 900 | Assembles `main.bicep`, parameter files, types | Text modules | Final file dictionary |

---

## Pipeline Generation Flow

```mermaid
flowchart TB
    subgraph "Input"
        CMD2[GenerateProjectPipelineCommand]
        MODEL[Project + Config + Resource data]
    end
    
    subgraph "Generation"
        OPTS[PipelineGenerationOptions]
        ASSEMBLER[MonoRepoPipelineAssembler]
        SHARED[Shared Templates .azuredevops/v0/]
        PERCONFIG[Per-config pipeline.yml]
    end
    
    subgraph "Output"
        RESULT[MonoRepoPipelineResult]
        INFRA[Infrastructure files]
        APP[Application files]
    end
    
    CMD2 --> MODEL
    MODEL --> OPTS
    OPTS --> ASSEMBLER
    ASSEMBLER --> SHARED
    ASSEMBLER --> PERCONFIG
    SHARED --> RESULT
    PERCONFIG --> RESULT
    RESULT --> INFRA
    RESULT --> APP
```

---

## Push-to-Git Flow

```mermaid
sequenceDiagram
    participant User as User/Agent
    participant API as API
    participant Handler as PushHandler
    participant Resolver as RepositoryTargetResolver
    participant Git as Azure DevOps Git API
    participant KV as Key Vault

    User->>API: POST /projects/{id}/push-to-git
    API->>Handler: PushProjectBicepToGitCommand
    Handler->>Resolver: Resolve target repository
    Resolver->>KV: Get repository PAT
    KV-->>Resolver: PAT secret
    Resolver-->>Handler: Repository connection info
    Handler->>Handler: Classify files by ArtifactKind
    Handler->>Git: Create/update files in branch
    Git-->>Handler: Commit result
    Handler-->>API: Success + commit info
    API-->>User: 200 OK
```

### Repository Routing (Layout-Driven)

| Layout Preset | Infra Files | App Files |
|--------------|-------------|-----------|
| `AllInOne` | Same repo | Same repo |
| `SplitInfraCode` | Infra repo | Code repo |
| `MultiRepo` | Per-config repos | Per-config repos |

---

## MCP Tool Flow

```mermaid
sequenceDiagram
    participant Copilot as GitHub Copilot
    participant MCP as MCP Server
    participant Draft as DraftService
    participant Tools as MCP Tools
    participant APP as Application Layer
    participant DB as PostgreSQL

    Copilot->>MCP: Tool call (create_project_from_draft)
    MCP->>MCP: PAT Authentication
    MCP->>Draft: Parse natural language intent
    Draft->>Draft: Extract resources, environments, topology
    Draft-->>MCP: DraftProjectIntent
    MCP->>Tools: Validate + clarify
    Tools->>APP: CreateProjectWithSetupCommand
    APP->>DB: Persist project + resources
    DB-->>APP: Success
    APP-->>MCP: ProjectResult
    MCP-->>Copilot: Structured response
```

---

## Data Persistence Flow

### Write Path (Command)

```mermaid
flowchart LR
    A[Command] --> B[Handler creates/modifies aggregate]
    B --> C[Repository adds to DbContext]
    C --> D[UoW calls SaveChangesAsync]
    D --> E[EF Core generates SQL]
    E --> F[PostgreSQL persists]
    F --> G[Domain events dispatched]
```

### Read Path (Query)

```mermaid
flowchart LR
    A[Query] --> B[Handler calls read repository]
    B --> C[AsNoTracking query]
    C --> D[EF Core generates SQL]
    D --> E[PostgreSQL returns rows]
    E --> F[EF materializes entities]
    F --> G[Handler maps to result]
```

**Key difference:** Write path uses tracked entities + Unit of Work. Read path uses `AsNoTracking()` for performance.
