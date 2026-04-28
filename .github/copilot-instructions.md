# Copilot Instructions

## Environnement de développement

> L'utilisateur travaille sur **Windows**. Toutes les commandes terminal doivent utiliser la syntaxe **PowerShell** (`pwsh`). Utiliser `.\ ` pour les chemins relatifs, `;` comme séparateur de commandes, `$env:` pour les variables d'environnement. Ne jamais suggérer de commandes bash/sh.

## Build, run, and template commands

- Use `.NET SDK 10.0.100` from `global.json`.
- Build the full solution with `dotnet build .\InfraFlowSculptor.slnx`.
- Run the full .NET test suite with `dotnet test .\InfraFlowSculptor.slnx`.
- Run a single .NET test project with `dotnet test .\tests\<TargetAssembly>.Tests\<TargetAssembly>.Tests.csproj`.
- Run the full local stack with Aspire via `dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj`.
- Build the infrastructure configuration API only with `dotnet build .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj`.
- Build the Bicep generator API only with `dotnet build .\src\BicepGenerators\BicepGenerator.Api\BicepGenerator.Api.csproj`.
- Frontend Angular commands run from `src\Front`:
  - `npm install`
  - `npm run start`
  - `npm run build`
  - `npm run typecheck`
- The repository is also used as a `dotnet new` template source: `dotnet new install .` then `dotnet new templatewebcqrs -o ProjectName`.
- No active .NET test project is currently checked in; recreate projects under `tests\<TargetAssembly>.Tests\` as needed.
- Unit test projects belong under `tests\` and follow the `<TargetAssembly>.Tests` naming convention, with one project per target assembly.
- No repository-specific lint or formatting command is defined in the checked-in files.

## High-level architecture

- `src\Aspire` contains the distributed application host and shared service defaults. `InfraFlowSculptor.AppHost` wires up PostgreSQL, DbGate, the main API, and the Bicep generator API for local orchestration.
- `src\Api` is the main infrastructure configuration stack. It follows a layered CQRS structure:
  - `InfraFlowSculptor.Api`: Minimal API endpoint definitions, OpenAPI setup, auth policy setup, and Mapster registration.
  - `InfraFlowSculptor.Application`: MediatR commands/queries, handlers, validators, pipeline behaviors, and application-layer interfaces.
  - `InfraFlowSculptor.Domain`: aggregates, entities, value objects, and domain behavior.
  - `InfraFlowSculptor.Infrastructure`: EF Core persistence, repository implementations, auth services, Azure integrations, and the Refit client used to call the Bicep generator API.
  - `InfraFlowSculptor.Contracts`: request/response DTOs grouped by feature.
- `src\BicepGenerators` mirrors the same layered split for the API that generates Bicep output and stores artifacts.
- `src\Front` contains the Angular frontend (standalone components, Angular Material, Tailwind, Axios services, auth facade/guards).
- `src\Shared` holds reusable cross-cutting pieces used by both APIs: base DDD model types, shared application abstractions, shared API middleware/options, and persistence converters/repository helpers.
- The main request flow is: Minimal API endpoint in `Api\Controllers` -> Mapster/request mapping -> MediatR command/query in `Application` -> handler/repository/service calls -> domain model changes or reads -> EF Core persistence -> Mapster/typed response DTO back to HTTP.

## Specialized agents

- **Main entry point** — Use the `dev` agent (`.github/agents/dev.agent.md`) as the primary entry point for any task. It reads `MEMORY.md` + thematic memory files in `.github/memory/`, routes to the right specialist, loads relevant Skills, and updates memory at the end.
- **Architecture review & planning** — Use the `architect` agent (`.github/agents/architect.agent.md`) for any architecture analysis, feasibility check, implementation planning, or challenge of a feature request against the existing codebase. The architect never codes — it produces structured implementation plans for expert agents to follow.
- **Expert code audits** — Use the `audit-expert` agent (`.github/agents/audit-expert.agent.md`) for repository audits that must produce a report in `audits/` and reconcile GitHub audit issues and labels on `FlorianDrevet/infra-pipeline-editor`.
- **Pre-merge code review** — Use the `review-expert` agent (`.github/agents/review-expert.agent.md`) for strict review of the code that will be merged from the current branch to `main`, with severity-ranked findings and a corrective backlog.
- **Anti-vibe coding review** — Use the `vibe-coding-refractaire` agent (`.github/agents/vibe-coding-refractaire.agent.md`) as a second-pass reviewer for generated or suspicious code, with a senior hater lens focused on unnecessary abstractions, copy-paste, weak tests, architecture drift, and other vibe-coding smells.
- **Review remediation** — Use the `review-remediator` agent (`.github/agents/review-remediator.agent.md`) to consume the corrective backlog produced by `review-expert`, implement only the accepted fixes, and validate them before a new review pass.
- **Backend C#/.NET** — Any C# code generation or modification MUST follow `.github/agents/dotnet-dev.agent.md` conventions (XML docs, no magic strings, SOLID, async/await, EF Core, FluentValidation, sealed, guard clauses, no code smells).
- **.NET unit testing** — Load the `xunit-unit-testing` skill (`.github/skills/xunit-unit-testing/SKILL.md`) for any xUnit unit-test creation, bug reproduction, snapshot test, coverage, or mutation-testing task.
- **Frontend Angular** — Any work in `src\Front` MUST use the `angular-front` agent (`.github/agents/angular-front.agent.md`).
- **Aspire runtime debugging** — Any runtime/AppHost investigation (resource failures, logs/traces, startup issues) MUST use the `aspire-debug` agent (`.github/agents/aspire-debug.agent.md`).
- **Memory consolidation (Dream)** — The `dream` agent (`.github/agents/dream.agent.md`) performs periodic memory consolidation (4 phases: Orient → Gather → Consolidate → Prune). Triggered automatically by `@dev` when time gate (≥24h) AND session gate (≥5 sessions) are both satisfied and an exclusive Dream lock at `$env:TEMP\infra-pipeline-editor-dream-lock` can be acquired. Dream state tracked in `.github/memory/dream-state.md`.
- **CQRS feature generation** — Load the `cqrs-feature` skill (`.github/skills/cqrs-feature/SKILL.md`) for any new aggregate or CQRS feature.
- **MCP / Model Context Protocol** — Load the `mcp-dotnet-server` skill (`.github/skills/mcp-dotnet-server/SKILL.md`) for any MCP server design or implementation in C#/.NET, VS Code `.vscode/mcp.json` exposure, transport/auth choices, conversational project creation from prompts, mandatory clarification workflows, IaC import tooling, or long-term MCP integration planning.
- **UI/UX frontend design quality** — Load the `ui-ux-front-saas` skill (`.github/skills/ui-ux-front-saas/SKILL.md`) for any UI-facing frontend task (pages, components, layouts, styles, UX states).
- **Pull Requests** — Use the `pr-manager` agent (`.github/agents/pr-manager.agent.md`) for PR title/description conventions.

## Skills

Skills are **lazy-loaded knowledge files** (`SKILL.md`) that agents load on demand via `read_file` when the task matches the skill's domain.
They differ from agents: no tools, pure structured knowledge, reusable across multiple agents.

> **BLOCKING REQUIREMENT:** When a skill applies to the user's request, you MUST load and read the SKILL.md file IMMEDIATELY as your first action, BEFORE generating any code or taking action on the task.

### Available project skills

| Skill | When to load | File |
|-------|-------------|------|
| `cqrs-feature` | Generating a new aggregate, new CQRS commands/queries/handlers, full feature scaffolding | `.github/skills/cqrs-feature/SKILL.md` |
| `mcp-dotnet-server` | Designing, planning, or implementing a Model Context Protocol server in C#/.NET for InfraFlowSculptor, including VS Code exposure, transport selection, security, conversational project creation from prompts, clarification/elicitation flows, IaC import/migration flows, and long-term integration strategy | `.github/skills/mcp-dotnet-server/SKILL.md` |
| `ui-ux-front-saas` | Any frontend UI/UX work: page design, component visuals, layout, styling, UX states, handoff specs | `.github/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | Adding a new Azure resource type end-to-end (Domain→App→Infra→Contracts→API→Bicep→Frontend→i18n) | `.github/skills/new-azure-resource/SKILL.md` |
| `gitnexus-workflow` | Code exploration via knowledge graph, impact analysis before modifications, post-change validation, safe refactoring | `.github/skills/gitnexus-workflow/SKILL.md` |
| `draw-io-diagram-generator` | Creating or updating `.drawio` architecture, flow, sequence, ER, or UML diagrams for the project | `.github/skills/draw-io-diagram-generator/SKILL.md` |
| `audit-workflow` | Running expert code audits, writing the report under `audits/`, and synchronizing audit findings with GitHub labels/issues | `.github/skills/audit-workflow/SKILL.md` |
| `dotnet-patterns` | Any C#/.NET code generation: naming, XML docs, SOLID, async/await, EF Core, pattern matching, security | `.github/skills/dotnet-patterns/SKILL.md` |
| `xunit-unit-testing` | Any .NET xUnit unit-test work: project placement, naming, AAA, FluentAssertions, NSubstitute, Verify, Bogus, MockQueryable, coverage, mutation | `.github/skills/xunit-unit-testing/SKILL.md` |
| `tdd-workflow` | **Any code modification**: enforces TDD Red→Green→Refactor→Verify cycle, test project init, test debt tracking in `.github/test-debt.md` | `.github/skills/tdd-workflow/SKILL.md` |
| `angular-patterns` | Any Angular 19 code: Signals, standalone components, forms, Axios, routing, Material+Tailwind, i18n | `.github/skills/angular-patterns/SKILL.md` |
| `bicep-v2-migration` | Migrating an IResourceTypeBicepGenerator from legacy string template to Builder + IR (Vague 2), including TDD tests, emitter parity, review cycle, and skill feedback loop | `.github/skills/bicep-v2-migration/SKILL.md` |

---

## Critical pitfalls (quick reference)

1. **EF Core LINQ:** Never `x.Id.Value == id.Value` — always `x.Id == id`
2. **Mapster nulls:** Use `x != null`, never `(object?)x`, never `is not null` (CS8122 in expression trees)
3. **Repositories:** MUST NOT call `SaveChangesAsync()` — Unit of Work handles it
4. **i18n dialog keys:** Use full nested path `RESOURCE_EDIT.DIALOG_NAME.*`
5. **Magic strings:** Never hardcode Azure resource type identifiers — use `AzureResourceTypes.*`
6. **Domain quality:** XML docs, `private set`, English errors, `= []` initializers, `sealed` on aggregates
7. **FK cascade on delete:** Cross-resource FKs must be SetNull or Cascade, never Restrict
8. **Response DTO IDs:** Always `string` (not `Guid`) — Mapster maps `Id.Value.ToString()`
9. **OpenAPI 401:** All protected endpoints must include `.ProducesProblem(401)`
10. **GitNexus:** Before modifying a shared symbol, run `gitnexus_impact()` to assess blast radius
11. **TDD obligatoire:** Never write production code without tests first — load `tdd-workflow` skill, follow RED→GREEN→REFACTOR→VERIFY, track debt in `.github/test-debt.md`
12. **DS obligatoire (Frontend):** Tout composant UI doit utiliser les `app-ds-*` existants. Si un pattern n'a pas de DS component, en créer un réutilisable dans `shared/components/ds/` avant usage.

## Pull Request conventions

Full details in `.github/agents/pr-manager.agent.md`. Title format: `type(scope): description`. Use `.github/PULL_REQUEST_TEMPLATE.md` for description. Before creating or submitting a PR, run the mandatory technical review gate: `review-main` / `review-expert` plus `vibe-coding-refractaire`.

