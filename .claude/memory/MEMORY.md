# Project Memory — InfraFlowSculptor

> **Index file.** Detailed knowledge lives in thematic files under `.claude/memory/`.
> Le thread principal (protocole `/dev`) lit cet index en premier, puis charge les fichiers thématiques pertinents à la demande.
> Le sous-agent `dream` (lancé par le thread principal quand les gates passent) consolide et prune périodiquement la mémoire (voir `.claude/memory/dream-state.md`).

---

## Memory Architecture

| File | Content |
|------|---------|
| `.claude/memory/01-solution-overview.md` | Tech stack, product goal, solution file |
| `.claude/memory/02-project-structure.md` | Folder tree (Api, Front, Aspire, Shared) |
| `.claude/memory/03-domain-model.md` | All 22 aggregates, domain invariants, code quality rules, error conventions |
| `.claude/memory/04-cqrs-pattern.md` | CQRS folder structure, marker interfaces, Unit of Work, authorization |
| `.claude/memory/05-api-layer.md` | Endpoint registration, error conversion, contracts, Mapster conventions |
| `.claude/memory/06-persistence.md` | EF Core config, converters, repository pattern, LINQ pitfalls, migrations |
| `.claude/memory/07-bicep-generation.md` | Generation engine, module structure, naming, role assignments, companions |
| `.claude/memory/08-frontend.md` | Angular 21 app architecture, i18n, auth, shared UX patterns |
| `.claude/memory/09-aspire.md` | AppHost wiring, proxy config, OTel, PostgreSQL reset |
| `.claude/memory/10-auth-and-build.md` | Auth config, build commands, Sonar rules |
| `.claude/memory/11-agents-skills.md` | All agents and skills registry |
| `.claude/memory/12-api-endpoints.md` | Full API endpoint reference table |
| `.claude/memory/13-code-graph.md` | Codegraph knowledge cache: high-risk symbols, critical flows, clusters |
| `.claude/memory/14-frontend-design-system.md` | DS tokens, component suite, migration status, UI caveats |
| `.claude/memory/changelog.md` | Recent changes (pruned > 60 days by dream) |
| `.claude/memory/dream-state.md` | Date de la dernière consolidation (`lastDreamDate`). Dream déclenché manuellement via `/dream`. |

---

## Quick Reference — Critical Pitfalls

1. **EF Core LINQ:** Never `x.Id.Value == id.Value` — always `x.Id == id` (see `06-persistence.md`)
2. **Mapster nulls:** Use `x != null`, never `(object?)x`, never `is not null` (see `05-api-layer.md`)
3. **Repositories:** MUST NOT call `SaveChangesAsync()` — Unit of Work handles it (see `04-cqrs-pattern.md`)
4. **i18n dialog keys:** Use full nested path `RESOURCE_EDIT.DIALOG_NAME.*` (see `08-frontend.md`)
5. **Magic strings:** Never hardcode Azure resource type identifiers — use `AzureResourceTypes.*` (see `07-bicep-generation.md`)
6. **Domain quality:** XML docs, `private set`, English errors, `= []` initializers, `sealed` on aggregates and `EnumValueObject` subclasses (see `03-domain-model.md`)
7. **FK cascade on delete:** Cross-resource FKs must be SetNull or Cascade, never Restrict — causes violations on parent cascade-delete (see `06-persistence.md`)
8. **Response DTO IDs:** Always `string` (not `Guid`) — Mapster maps `Id.Value.ToString()` (see `05-api-layer.md`)
9. **OpenAPI 401:** All protected endpoints must include `.ProducesProblem(401)` (see `05-api-layer.md`)
10. **TDD obligatoire:** Never write production code without tests first — load `tdd-workflow` skill, follow RED→GREEN→REFACTOR→VERIFY, track debt in `.claude/test-debt.md` (see `11-agents-skills.md`)

---

## How Memory Works

- Le **thread principal** lit cet index en début de tâche, puis charge les fichiers thématiques pertinents
- **En fin de tâche**, il met à jour le bon fichier thématique + `changelog.md`
- Le sous-agent **`dream`** se lance **manuellement** via `/dream` (plus de gate auto) ; il met à jour `lastDreamDate`
- **Phases du dream :** Orient -> Gather -> Consolidate -> Prune (fichiers < 150 lignes, changelog < 60 jours)

---

## Merge-main Notes

- 2026-04-15: Merged `origin/main` into `copilot/ddd-002-seal-azure-resource-classes`.
  - Conflicts: `.claude/memory/changelog.md`, `.claude/memory/dream-state.md`
  - Resolution rule: kept both branch and `main` memory updates, preserved latest dream gate date from `main`.
  - Post-merge adaptation: verified DDD-002 domain sealing remains intact for concrete `AzureResource` aggregates.
