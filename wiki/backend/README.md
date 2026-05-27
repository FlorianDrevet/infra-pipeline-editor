# Backend Documentation

> .NET 10 API server: Domain-Driven Design, CQRS, Bicep & Pipeline generation.

## Table of Contents

1. [Project Structure](project-structure.md) — Solution layout, assembly roles
2. [Domain Model](domain-model.md) — Aggregates, entities, value objects
3. [CQRS Handlers](cqrs-handlers.md) — Command/query implementation patterns
4. [Validation](validation.md) — FluentValidation strategy
5. [Bicep Generation](bicep-generation.md) — Generation engine deep dive
6. [Pipeline Generation](pipeline-generation.md) — Azure DevOps YAML generation
7. [Persistence Layer](persistence-layer.md) — EF Core, repositories, migrations
8. [Error Handling](error-handling.md) — ErrorOr pattern, domain errors
9. [Mapping](mapping.md) — Mapster configuration, conventions
10. [MCP Server](mcp-server.md) — Model Context Protocol implementation

---

## Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| API style | Minimal APIs | Less ceremony, faster startup, explicit routing |
| CQRS library | MediatR | Battle-tested, pipeline behaviors, community support |
| Validation | FluentValidation | Expressive rules, composition, testable |
| ORM | EF Core | .NET standard, migrations, LINQ |
| Error pattern | ErrorOr | No exceptions for business errors, composable |
| Mapping | Mapster | Faster than AutoMapper, less config |
| Auth | Entra ID + PAT | Human + automation auth in one API |
| Generation | Staged pipeline | Modular, testable, extensible |
