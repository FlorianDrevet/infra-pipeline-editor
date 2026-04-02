# Copilot Instructions

## Environnement de développement

> L'utilisateur travaille sur **Windows**. Toutes les commandes terminal doivent utiliser la syntaxe **PowerShell** (`pwsh`). Utiliser `.\ ` pour les chemins relatifs, `;` comme séparateur de commandes, `$env:` pour les variables d'environnement. Ne jamais suggérer de commandes bash/sh.

## Build, run, and template commands

- Use `.NET SDK 10.0.100` from `global.json`.
- Build the full solution with `dotnet build .\InfraFlowSculptor.slnx`.
- Run the full local stack with Aspire via `dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj`.
- Build the infrastructure configuration API only with `dotnet build .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj`.
- Build the Bicep generator API only with `dotnet build .\src\BicepGenerators\BicepGenerator.Api\BicepGenerator.Api.csproj`.
- Frontend Angular commands run from `src\Front`:
  - `npm install`
  - `npm run start`
  - `npm run build`
  - `npm run typecheck`
- The repository is also used as a `dotnet new` template source: `dotnet new install .` then `dotnet new templatewebcqrs -o ProjectName`.
- No test projects are currently present in the repository, so there is no supported full-suite or single-test command yet.
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
- **Backend C#/.NET** — Any C# code generation or modification MUST follow `.github/agents/dotnet-dev.agent.md` conventions (XML docs, no magic strings, SOLID, async/await, EF Core, FluentValidation, sealed, guard clauses, no code smells).
- **Frontend Angular** — Any work in `src\Front` MUST use the `angular-front` agent (`.github/agents/angular-front.agent.md`).
- **Aspire runtime debugging** — Any runtime/AppHost investigation (resource failures, logs/traces, startup issues) MUST use the `aspire-debug` agent (`.github/agents/aspire-debug.agent.md`).
- **Memory consolidation (Dream)** — The `dream` agent (`.github/agents/dream.agent.md`) performs periodic memory consolidation (4 phases: Orient → Gather → Consolidate → Prune). Triggered automatically by `@dev` when time gate (≥24h) AND session gate (≥5 sessions) are both satisfied. Dream state tracked in `.github/memory/dream-state.md`.
- **CQRS feature generation** — Load the `cqrs-feature` skill (`.github/skills/cqrs-feature/SKILL.md`) for any new aggregate or CQRS feature.
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
| `ui-ux-front-saas` | Any frontend UI/UX work: page design, component visuals, layout, styling, UX states, handoff specs | `.github/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | Adding a new Azure resource type end-to-end (Domain→App→Infra→Contracts→API→Bicep→Frontend→i18n) | `.github/skills/new-azure-resource/SKILL.md` |

---

## Repository-specific conventions

- Treat the domain as strict DDD. When adding domain code, decide whether it belongs as an aggregate root, entity, value object, repository, or domain service before creating files. Shared base classes live in `src\Shared\Shared.Domain\Models`.
- In the main API, domain types are organized by aggregate folders such as `src\Api\InfraFlowSculptor.Domain\InfrastructureConfigAggregate`, with nested `Entities` and `ValueObjects` folders.
- Keep CQRS artifacts together by feature in the application layer. Commands, queries, handlers, validators, and result models live under feature folders like `src\Api\InfraFlowSculptor.Application\InfrastructureConfig\...` and `src\Api\InfraFlowSculptor.Application\ResourceGroups\...`.
- API endpoints are defined as static endpoint-mapping extensions in `src\Api\InfraFlowSculptor.Api\Controllers`, not MVC controller classes. Follow the existing `MapGroup(...)`, `MapGet(...)`, and `MapPost(...)` style.
- Contracts belong in `src\Api\InfraFlowSculptor.Contracts`, typically grouped by feature with `Requests` and `Responses` subfolders. The existing GitHub agent guidance assumes request/response models stay in the contracts project.
- Use Mapster for request/command/response mapping. Mapping configuration lives in `src\Api\InfraFlowSculptor.Api\Common\Mapping` and is registered by scanning the API assembly in `AddPresentation()`.
- Register application services, repositories, auth, and external clients through the layer-specific `DependencyInjection.cs` files instead of wiring dependencies ad hoc in `Program.cs`.
- EF Core configuration belongs in `src\Api\InfraFlowSculptor.Infrastructure\Persistence\Configurations`. Existing mappings rely heavily on owned types and shared converters from `src\Shared\Shared.Infrastructure\Persistence\Configurations\Converters`.
- Repositories are defined by interfaces in the application layer and implemented in infrastructure persistence/repository folders. Reuse that split instead of referencing EF Core directly from handlers.
- Error handling uses `ErrorOr<T>` plus the shared API error extensions in `Shared.Api.Errors`; handlers return `ErrorOr` results rather than throwing for expected validation/domain failures.
- Validation is implemented with FluentValidation and enforced through the MediatR `ValidationBehavior`.
- Authentication uses Microsoft Entra ID / JWT bearer auth. The API projects set a fallback authenticated policy, expose an `IsAdmin` policy, and use `ICurrentUser`/`CurrentUser` for user context access.
- The GitHub prompt and agent files describe the product goal as storing infrastructure configuration in one API and generating Azure Bicep and Azure DevOps pipeline output in the second API. Keep that split in mind when deciding which project should own new behavior.
- Frontend conventions:
  - **Any frontend work in `src\Front` MUST use the `angular-front` agent** (`.github/agents/angular-front.agent.md`). This agent owns all Angular 19 conventions, signals, standalone components, and project-specific patterns.
  - **Any frontend UI task MUST load `ui-ux-front-saas` first** (`.github/skills/ui-ux-front-saas/SKILL.md`) to enforce SaaS B2B cloud UX rules and alignment with the existing login page visual baseline.
  - Keep feature code under `src\Front\src\app` with clear split between `core`, `features`, and `shared`.
  - Keep API endpoint URLs centralized through `src\Front\src\environments\environment*.ts` and consumed via `AxiosService` (never hardcode base URLs in services or components).
  - Prefer standalone components and route-level lazy loading (`loadComponent`) for all new screens.
  - Keep backend contracts aligned: if API request/response contracts change, update frontend interfaces and services in the same change.
  - All Angular components use 3 separate files (`.ts`, `.html`, `.scss`) — never inline templates or styles.
  - Use Signals exclusively for state management (`signal`, `computed`, `toSignal`). The project is zoneless (`provideExperimentalZonelessChangeDetection`).
  - Use `inject()` for dependency injection — never constructor injection.
  - Use the new Angular control flow syntax (`@if`, `@for`, `@switch`) — never `*ngIf`, `*ngFor`.

## Pull Request conventions

Every PR created by a Copilot agent **must** follow these rules. Full details are in `.github/agents/pr-manager.agent.md`.

### PR title format

```
type(scope): short description of the main goal
```

- **type** (required): `feat` | `fix` | `refactor` | `perf` | `docs` | `test` | `chore` | `ci` | `style` | `revert`
- **scope** (recommended): aggregate or component in kebab-case (e.g. `key-vault`, `role-assignment`, `bicep`)
- **description**: short sentence in present tense, no leading capital, no trailing period

The title must describe the **overall goal** of the PR, never the last task performed.

**Correct examples:**
- `feat(storage-account): add StorageAccount aggregate with full CRUD`
- `fix(key-vault): correct EF Core LINQ translation for KeyVaultId comparison`
- `refactor(member): extract MemberCommandHelper to reduce duplication`

### PR description

Use the template at `.github/PULL_REQUEST_TEMPLATE.md`. It must include:
1. A one-sentence summary of the main goal.
2. Type of change (checkboxes).
3. Changes listed **per layer** (Domain, Application, Infrastructure, Contracts, API, BicepGenerators, Shared, Aspire/CI).
4. EF Core migration name if applicable.
5. Completed checklist before merge.

Every created or modified file must appear in at least one section of the description.

