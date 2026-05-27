# ADR-001: Use CQRS with MediatR

## Status
Accepted

## Context

InfraFlowSculptor needs a clear separation between read and write operations. The domain model is complex (22 resource types, multi-tenant) and operations have different performance characteristics — reads are frequent and must be fast, writes are less frequent but require full validation and domain logic.

We need a pattern that:
- Separates read and write concerns
- Enables independent optimization of each path
- Supports cross-cutting behaviors (validation, auth, transactions)
- Keeps handlers focused and testable

## Decision

Adopt **CQRS (Command Query Responsibility Segregation)** with **MediatR** as the mediator library.

- Commands implement `ICommand<TResponse>` (writes)
- Queries implement `IQuery<TResponse>` (reads)
- Pipeline behaviors handle cross-cutting concerns
- One handler per operation, one file per handler

## Consequences

### Positive
- Clear separation of concerns
- Read path uses `AsNoTracking()`, projections
- Write path uses full domain model with change tracking
- Pipeline behaviors apply uniformly (validation, auth, UoW)
- Easy to test handlers in isolation
- Easy to add new operations without touching existing code

### Negative
- More files per feature (command, handler, validator)
- Indirection through MediatR (harder to navigate call chain)
- Pipeline behaviors are implicit (must know they exist)

### Risks
- Over-engineering for simple CRUD (mitigated: even simple ops benefit from validation pipeline)
