# Architecture

> Comprehensive documentation of InfraFlowSculptor's software architecture,
> from high-level system design to low-level implementation patterns.

## Table of Contents

1. [Overview](overview.md) — System architecture, C4 diagrams, principles
2. [Domain-Driven Design](ddd.md) — Aggregates, entities, value objects, invariants
3. [CQRS Pattern](cqrs.md) — Commands, queries, handlers, pipeline
4. [Data Flow](data-flow.md) — Request lifecycle, generation flow, push flow
5. [Persistence](persistence.md) — EF Core, PostgreSQL, repositories
6. [Generation Engines](generation-engines.md) — Bicep & Pipeline generation architecture
7. [Scalability & Performance](scalability.md) — Caching, read models, optimization
8. [Security Architecture](security-architecture.md) — Auth, authorization, hardening
9. [Observability](observability.md) — Logging, tracing, metrics

---

## Architecture Principles

| Principle | Application |
|-----------|-------------|
| **Clean Architecture** | Strict layer separation, dependencies point inward |
| **Domain-Driven Design** | Rich domain model, ubiquitous language |
| **CQRS** | Separate read/write paths for clarity and optimization |
| **Result over Exceptions** | Business errors via `ErrorOr<T>`, not exceptions |
| **Convention over Configuration** | Structural tests enforce architecture rules |
| **Generation as Code** | Infrastructure artifacts are generated, not handwritten |
| **API-first** | All functionality exposed via HTTP API |
| **AI-ready** | MCP server enables AI agent interactions |
