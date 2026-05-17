# Transactional Strategy

## Overview

InfraFlowSculptor uses an **implicit Unit of Work** pattern via EF Core's `DbContext` change tracking, coordinated by the MediatR `UnitOfWorkBehavior` pipeline behavior.

## How It Works

### Single-Aggregate Operations (Default)

Most operations modify a **single aggregate** within one MediatR command handler:

1. The API endpoint dispatches a MediatR command
2. `ValidationBehavior` validates the command
3. The command handler loads the aggregate, applies domain logic, and calls `repository.Update(aggregate)`
4. `UnitOfWorkBehavior` calls `DbContext.SaveChangesAsync()` **after** the handler returns successfully
5. EF Core flushes all tracked changes in a **single implicit transaction**

This guarantees atomicity: either all changes within a single `SaveChangesAsync()` call succeed, or none do.

### Multi-Aggregate Operations

Some operations span multiple aggregates (e.g., creating an infrastructure config also updates the project). These are handled in two ways:

1. **Same DbContext** — When both aggregates share the same `ProjectDbContext`, EF Core batches all changes into a single `SaveChangesAsync()` call (one implicit transaction). This is the common case.

2. **Cross-service calls** — Operations that span the main API and the Bicep Generator API (e.g., generate-and-store) are **not** transactional. Failures in the downstream service are reported to the user but do not roll back changes in the main API.

### Concurrency Control

PostgreSQL `xmin` system column is used as a concurrency token on the three main aggregates:
- `Project`
- `InfrastructureConfig`
- `AzureResource`

EF Core automatically checks `xmin` during `SaveChangesAsync()` and throws `DbUpdateConcurrencyException` if the row was modified by another transaction since it was loaded.

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| No explicit `IDbContextTransaction` | Single-`SaveChangesAsync` per command is sufficient for DDD aggregate boundaries |
| No distributed transactions | Cross-service operations use eventual consistency with error reporting |
| No `TransactionScope` | Not needed; each command operates within a single `DbContext` |
| `xmin` over `RowVersion` | PostgreSQL-native, zero-storage-cost optimistic concurrency |

### When to Add Explicit Transactions

Use `IDbContextTransaction` (via `DbContext.Database.BeginTransactionAsync()`) only when:
- A single command must perform **multiple** `SaveChangesAsync()` calls (e.g., insert then read-back for computed columns)
- A command must coordinate with an external system and needs rollback capability

Currently, no command handler requires explicit transactions.
