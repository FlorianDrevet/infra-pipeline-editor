# Glossary

> Terms, acronyms, and concepts used throughout InfraFlowSculptor.

---

## A

| Term | Definition |
|------|-----------|
| **ADR** | Architecture Decision Record — documented architectural choice with context, decision, and consequences |
| **Aggregate** | DDD concept: cluster of domain objects treated as a unit for data changes |
| **Aggregate Root** | The entry point entity of an aggregate; external access goes through it |
| **ARM** | Azure Resource Manager — Azure's deployment and management layer |
| **Aspire** | .NET Aspire — Microsoft's framework for building distributed applications with built-in observability |
| **AsNoTracking** | EF Core query mode that skips change tracking for read-only queries |

## B

| Term | Definition |
|------|-----------|
| **Bicep** | Azure's domain-specific language for deploying Azure resources (compiles to ARM JSON) |
| **BicepModuleBuilder** | Fluent API for constructing BicepModuleSpec objects in InfraFlowSculptor |
| **BicepModuleSpec** | Intermediate Representation (IR) of a Bicep module before text emission |
| **Blob Storage** | Azure service for storing unstructured data (files, images, backups) |

## C

| Term | Definition |
|------|-----------|
| **Container App** | Azure Container Apps — serverless container hosting service |
| **CPM** | Central Package Management — NuGet feature managing versions in Directory.Packages.props |
| **CQRS** | Command Query Responsibility Segregation — pattern separating reads from writes |
| **Cross-Config Reference** | Reference from a resource in one InfrastructureConfig to a resource in another |

## D

| Term | Definition |
|------|-----------|
| **DDD** | Domain-Driven Design — approach to software design focusing on the business domain |
| **DbGate** | Web-based database management tool (replacement for pgAdmin) |
| **Design System (DS)** | Set of reusable UI components (`app-ds-*`) ensuring visual consistency |
| **Draft** | MCP concept: uncommitted project state during conversational creation |

## E

| Term | Definition |
|------|-----------|
| **EF Core** | Entity Framework Core — .NET ORM for database access |
| **Entra ID** | Microsoft Entra ID (formerly Azure AD) — identity and access management service |
| **Environment** | Deployment target (dev, staging, production) with specific configuration values |
| **EnvironmentValue** | Per-environment override of a resource property (e.g., different SKU per env) |
| **ErrorOr** | .NET library providing discriminated union of success value or error list |

## F

| Term | Definition |
|------|-----------|
| **FluentValidation** | .NET library for building strongly-typed validation rules |

## G

| Term | Definition |
|------|-----------|
| **GenerationCore** | Shared assembly with DTOs and constants used by both generation engines |
| **GitNexus** | Code intelligence tool providing impact analysis and symbol graph for the codebase |
| **Graphify** | Knowledge graph tool for corpus-level analysis (docs + code + diagrams) |

## H

| Term | Definition |
|------|-----------|
| **Handler** | MediatR handler — executes the business logic for a command or query |

## I

| Term | Definition |
|------|-----------|
| **IFS** | InfraFlowSculptor — the platform name |
| **InfrastructureConfig** | A deployment configuration within a project (e.g., "shared-infra", "app-services") |
| **IR** | Intermediate Representation — typed model between logic and text output |

## J-K

| Term | Definition |
|------|-----------|
| **JWT** | JSON Web Token — compact token format for authentication |
| **Key Vault** | Azure Key Vault — cloud service for storing secrets, keys, and certificates |
| **KQL** | Kusto Query Language — query language for Azure Log Analytics |

## L

| Term | Definition |
|------|-----------|
| **Layout Preset** | Repository topology strategy: AllInOne, SplitInfraCode, or MultiRepo |
| **Lazy Loading** | Angular pattern: load feature code only when the route is accessed |

## M

| Term | Definition |
|------|-----------|
| **Mapster** | .NET object-to-object mapping library (alternative to AutoMapper) |
| **MCP** | Model Context Protocol — standard for AI agent tool integration |
| **MediatR** | .NET mediator library implementing the Mediator pattern for CQRS |
| **Minimal API** | ASP.NET Core pattern for lightweight HTTP endpoints without controllers |
| **Mono-Repo** | Repository layout where all infrastructure configs live in one repository |
| **MSAL** | Microsoft Authentication Library — handles OAuth 2.0 flows for Entra ID |

## N-O

| Term | Definition |
|------|-----------|
| **ngx-translate** | Angular internationalization library for runtime language switching |
| **OpenTelemetry (OTel)** | Observability framework for traces, metrics, and logs |
| **OTLP** | OpenTelemetry Protocol — standard export format for telemetry data |

## P

| Term | Definition |
|------|-----------|
| **PAT** | Personal Access Token — non-interactive auth token with scoped permissions |
| **Pipeline Behavior** | MediatR concept: middleware that wraps handler execution (validation, auth, UoW) |
| **ProjectToType** | Mapster method that generates SQL projection instead of loading full entities |

## Q-R

| Term | Definition |
|------|-----------|
| **Refit** | .NET library for generating typed HTTP clients from interfaces |
| **Resource Group** | Azure container for grouping related resources |

## S

| Term | Definition |
|------|-----------|
| **Scalar** | OpenAPI documentation UI (replacement for Swagger UI) |
| **Signals** | Angular reactive primitives for component state management |
| **SonarQube** | Code quality and security analysis platform |
| **Standalone Component** | Angular component that declares its own imports (no NgModule needed) |

## T

| Term | Definition |
|------|-----------|
| **TDD** | Test-Driven Development — write tests first, then implementation |
| **TPT** | Table-Per-Type — EF Core inheritance mapping strategy with one table per type |
| **Typed ID** | Value object wrapping a Guid to provide type safety (e.g., `ProjectId`) |

## U-V

| Term | Definition |
|------|-----------|
| **Unit of Work** | Pattern that groups all changes in a request into a single transaction |
| **UPSERT** | Database operation that inserts or updates depending on existence |
| **Value Object** | DDD concept: immutable object defined by its attributes, not identity |

## W-Z

| Term | Definition |
|------|-----------|
| **What-If** | Azure deployment preview showing planned changes without applying them |
| **xmin** | PostgreSQL system column used for optimistic concurrency control |
| **Zoneless** | Angular mode running without Zone.js (uses signals for change detection) |
