# Architecture Decision Records (ADR)

> Documenting key architectural decisions for InfraFlowSculptor.

## Index

| # | Decision | Status | Date |
|---|----------|--------|------|
| 001 | [Use CQRS with MediatR](001-cqrs-mediatr.md) | Accepted | 2024-01 |
| 002 | [Domain-Driven Design with sealed aggregates](002-ddd-sealed-aggregates.md) | Accepted | 2024-01 |
| 003 | [ErrorOr over exceptions](003-erroror-pattern.md) | Accepted | 2024-02 |
| 004 | [TPT inheritance for resources](004-tpt-inheritance.md) | Accepted | 2024-02 |
| 005 | [.NET Aspire for local orchestration](005-aspire-orchestration.md) | Accepted | 2024-03 |
| 006 | [MCP for AI agent integration](006-mcp-integration.md) | Accepted | 2024-09 |
| 007 | [IR + Builder for Bicep generation](007-bicep-ir-builder.md) | Accepted | 2025-01 |
| 008 | [Staged pipeline for generation](008-staged-generation.md) | Accepted | 2025-01 |
| 009 | [Angular standalone + signals](009-angular-signals.md) | Accepted | 2024-06 |
| 010 | [Design System components](010-design-system.md) | Accepted | 2025-05 |
| 011 | [Axios over HttpClient](011-axios-http.md) | Accepted | 2024-04 |
| 012 | [PAT authentication for automation](012-pat-auth.md) | Accepted | 2024-08 |

---

## ADR Template

```markdown
# ADR-XXX: Title

## Status
Accepted | Deprecated | Superseded

## Context
What problem are we solving?

## Decision
What did we decide?

## Consequences
### Positive
### Negative
### Risks
```
