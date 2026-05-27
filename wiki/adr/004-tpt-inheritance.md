# ADR-004: TPT Inheritance for Azure Resources

## Status
Accepted

## Context

We have 22 Azure resource types that share common properties (Id, Name, ResourceGroupId, Tags) but have vastly different specific properties (Container App has replicas, Key Vault has RBAC settings, etc.).

Options considered:
1. **TPH (Table-Per-Hierarchy)** — One table with all columns, discriminator column
2. **TPT (Table-Per-Type)** — Base table + one table per type
3. **TPC (Table-Per-Concrete)** — One table per type, no base table
4. **Separate DbSets** — No inheritance, duplicated base properties

## Decision

Use **TPT (Table-Per-Type)** inheritance:
- `AzureResources` base table with shared properties
- One additional table per resource type with specific properties
- Shared `AzureResourceId` primary key across the hierarchy

## Consequences

### Positive
- Clean schema: each table only has its specific columns
- Base table enables cross-type queries (list all resources)
- Foreign keys to base table work naturally
- Adding a new resource type = one new table (no ALTER on existing)
- No null columns (unlike TPH)

### Negative
- JOINs required for full entity materialization
- More complex queries for single resource reads
- EF Core generates multi-table UPDATEs

### Risks
- Performance degradation with many resource types (mitigated: 22 types is manageable)
- Deep hierarchies are slow (mitigated: only 2 levels: base + concrete)
