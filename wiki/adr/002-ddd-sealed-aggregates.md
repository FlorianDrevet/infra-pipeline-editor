# ADR-002: Domain-Driven Design with Sealed Aggregates

## Status
Accepted

## Context

The domain has 22 Azure resource types with varying complexity, shared behaviors (tags, names, resource groups), and specific behaviors per type (Container App scaling, SQL Database collation, etc.).

We need a modeling approach that:
- Encapsulates business rules within entities
- Prevents invalid state transitions
- Supports the TPT persistence strategy
- Scales to more resource types without regression

## Decision

Adopt **Domain-Driven Design** with:
- **Sealed aggregate root classes** per resource type
- **Typed IDs** (value objects wrapping Guid)
- **Static factory methods** for creation
- **Explicit update methods** (not property setters)
- **Zero external dependencies** in the Domain assembly

## Consequences

### Positive
- Business rules are co-located with state
- Invalid states are unrepresentable
- Typed IDs prevent mixing up different entity references
- Sealed classes prevent uncontrolled inheritance
- Domain can be tested without any infrastructure

### Negative
- More boilerplate (factory methods, typed IDs)
- Learning curve for developers unfamiliar with DDD
- TPT generates more JOINs than TPH

### Risks
- Aggregate boundaries might need adjustment as requirements evolve
