# Frequently Asked Questions

## General

### Q: What is InfraFlowSculptor in one sentence?
**A:** A platform that lets you model Azure infrastructure and automatically generates Bicep modules + Azure DevOps pipelines from that model.

### Q: Why not just write Bicep by hand?
**A:** You can, but InfraFlowSculptor ensures consistency across environments, handles cross-resource dependencies, generates pipelines, and lets AI agents create infrastructure from natural language.

### Q: What's the difference between a Project and an InfrastructureConfig?
**A:** A **Project** is the top-level container (environments, naming, repos). An **InfrastructureConfig** is a logical grouping of resources within a project (e.g., "shared-infra", "app-services").

---

## Development

### Q: Why does the build fail with locked assemblies?
**A:** Running instances (API, MCP, AppHost) lock DLLs on Windows. Stop all processes before rebuilding.

### Q: How do I add a new Azure resource type?
**A:** Follow the CQRS feature skill. It involves: Domain aggregate → Application commands/queries → Infrastructure persistence → Contracts → API endpoints → Bicep generator → Frontend components.

### Q: Why must I write tests before code?
**A:** TDD is mandatory in this project. It ensures design quality, regression safety, and documentation through tests.

### Q: What's the `ErrorOr<T>` pattern?
**A:** Instead of throwing exceptions for business errors, handlers return `ErrorOr<T>` which is either a success value or a list of errors. This makes error handling explicit and composable.

### Q: Why can't repositories call SaveChanges?
**A:** The Unit of Work pattern ensures a single `SaveChangesAsync()` per command, maintaining transactional consistency. It runs in the MediatR pipeline behavior.

---

## Frontend

### Q: Why don't we use `@angular/msal`?
**A:** We use `@azure/msal-browser` directly for maximum control over token acquisition and redirect handling, without the Angular wrapper's lifecycle assumptions.

### Q: What's the Design System (DS)?
**A:** A set of reusable Angular components (`app-ds-*`) that wrap Material components with our brand tokens. Always prefer DS components over raw Material.

### Q: Why signals instead of RxJS?
**A:** Angular 21 signals are simpler for UI state. RxJS is still used for async operations, but component state uses signals.

---

## Infrastructure

### Q: What's Aspire?
**A:** .NET Aspire is a local orchestrator that starts all services (API, DB, frontend, MCP) in the correct order with proper configuration. Think of it as docker-compose for .NET.

### Q: Can I run without Aspire?
**A:** Yes, but you need to manually start PostgreSQL, configure connection strings, and run each service individually.

### Q: What's the MCP server for?
**A:** It allows AI agents (GitHub Copilot) to interact with InfraFlowSculptor programmatically — creating projects, discovering resources, and generating artifacts through natural language.

---

## Generation

### Q: How does Bicep generation work?
**A:** The domain model is read from the database, passed through a 10-stage pipeline that builds an Intermediate Representation (IR), then emitted as Bicep text files.

### Q: What's the difference between per-config and project-level generation?
**A:** Per-config generates for one InfrastructureConfig. Project-level generates all configs with shared `Common/` modules and cross-config references.

### Q: What happens when I "Push to Git"?
**A:** Generated Bicep/pipeline files are pushed to the configured Azure DevOps repository using the project's repository PAT.
