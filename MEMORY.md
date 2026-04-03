# Project Memory — InfraFlowSculptor

> **Index file.** Detailed knowledge lives in thematic files under `.github/memory/`.
> Agents read this index first, then load relevant topic files on demand.
> The `@dream` agent periodically consolidates and prunes memory (see `.github/memory/dream-state.md`).

---

## Memory Architecture

| File | Content |
|------|---------|
| `.github/memory/01-solution-overview.md` | Tech stack, product goal, solution file |
| `.github/memory/02-project-structure.md` | Folder tree (Api, Front, Aspire, Shared) |
| `.github/memory/03-domain-model.md` | All 22 aggregates, domain invariants, code quality rules, error conventions |
| `.github/memory/04-cqrs-pattern.md` | CQRS folder structure, marker interfaces, Unit of Work, authorization |
| `.github/memory/05-api-layer.md` | Endpoint registration, error conversion, contracts, Mapster conventions |
| `.github/memory/06-persistence.md` | EF Core config, converters, repository pattern, LINQ pitfalls, migrations |
| `.github/memory/07-bicep-generation.md` | Generation engine, module structure, naming, role assignments, companions |
| `.github/memory/08-frontend.md` | Angular 19, signals, i18n, MSAL auth, visual baseline, shared components |
| `.github/memory/09-aspire.md` | AppHost wiring, proxy config, OTel, PostgreSQL reset |
| `.github/memory/10-auth-and-build.md` | Auth config, build commands, Sonar rules |
| `.github/memory/11-agents-skills.md` | All agents and skills registry |
| `.github/memory/12-api-endpoints.md` | Full API endpoint reference table |
| `.github/memory/13-code-graph.md` | GitNexus knowledge cache: high-risk symbols, critical flows, clusters |
| `.github/memory/changelog.md` | Recent changes (pruned > 60 days by dream) |
| `.github/memory/dream-state.md` | Dream trigger state (lastDreamDate, sessionsSinceLastDream) |

---

## Quick Reference — Critical Pitfalls

1. **EF Core LINQ:** Never `x.Id.Value == id.Value` — always `x.Id == id` (see `06-persistence.md`)
2. **Mapster nulls:** Use `x != null`, never `(object?)x`, never `is not null` (see `05-api-layer.md`)
3. **Repositories:** MUST NOT call `SaveChangesAsync()` — Unit of Work handles it (see `04-cqrs-pattern.md`)
4. **i18n dialog keys:** Use full nested path `RESOURCE_EDIT.DIALOG_NAME.*` (see `08-frontend.md`)
5. **Magic strings:** Never hardcode Azure resource type identifiers — use `AzureResourceTypes.*` (see `07-bicep-generation.md`)
6. **Domain quality:** XML docs, `private set`, English errors, `= []` initializers (see `03-domain-model.md`)

---

## How Memory Works

- **`@dev`** reads this index at session start, then loads relevant topic files for the task
- **After each task**, `@dev` updates the appropriate topic file + `changelog.md`
- **`@dream`** runs when: >= 24h since last dream AND >= 5 sessions (gates in `dream-state.md`)
- **Dream phases:** Orient -> Gather -> Consolidate -> Prune (keeps files < 150 lines, changelog < 60 days)
