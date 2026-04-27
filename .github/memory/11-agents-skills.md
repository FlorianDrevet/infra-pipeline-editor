# Agents & Skills Registry

## Agents

| Agent | Role | File |
|---|---|---|
| `dev` | Orchestrateur principal, lit MEMORY, route aux agents spécialisés | `.github/agents/dev.agent.md` |
| `dotnet-dev` | Expert C#/.NET 10 | `.github/agents/dotnet-dev.agent.md` |
| `angular-front` | Expert frontend Angular standalone, tout travail `src/Front` (repo en Angular 21) | `.github/agents/angular-front.agent.md` |
| `architect` | Analyse archi, challenge, plan d'implémentation (ne code pas) | `.github/agents/architect.agent.md` |
| `aspire-debug` | Debug runtime Aspire, MCP diagnostics | `.github/agents/aspire-debug.agent.md` |
| `audit-expert` | Audit technique expert, produit des rapports `audits/` et synchronise les issues GitHub d'audit | `.github/agents/audit-expert.agent.md` |
| `pr-manager` | Conventions PR (titre, description, template) | `.github/agents/pr-manager.agent.md` |
| `merge-main` | Fusion main sur branche courante | `.github/agents/merge-main.agent.md` |
| `dream` | Consolidation mémoire (4 phases Dream) | `.github/agents/dream.agent.md` |
| `memory` | **DEPRECATED** — redirecteur vers `dev` | `.github/agents/memory.agent.md` |

## Dream concurrency [2026-04-25]

- `@dev` doit sérialiser `@dream` avec un verrou exclusif PowerShell via le répertoire temporaire `$env:TEMP\infra-pipeline-editor-dream-lock` avant toute invocation du sous-agent.
- Si le verrou existe déjà, l'agent courant doit considérer qu'un autre agent possède déjà le cycle Dream et continuer la tâche utilisateur sans lancer un second dream.
- `@dream` doit sortir sans modifier la mémoire si `dream-state.md` montre déjà un gate fermé pour la date du jour (`lastDreamDate` = aujourd'hui et `sessionsSinceLastDream` = 0).

## Skills

| Skill | When to load | File |
|---|---|---|
| `cqrs-feature` | New aggregate, CQRS feature scaffolding | `.github/skills/cqrs-feature/SKILL.md` |
| `ui-ux-front-saas` | Any frontend UI/UX work | `.github/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | New Azure resource type end-to-end | `.github/skills/new-azure-resource/SKILL.md` |
| `gitnexus-workflow` | Code exploration via knowledge graph, impact analysis before modifications, post-change validation, safe refactoring | `.github/skills/gitnexus-workflow/SKILL.md` |
| `draw-io-diagram-generator` | Create or update draw.io diagrams (`.drawio`, `.drawio.svg`, `.drawio.png`) for architecture and technical documentation | `.github/skills/draw-io-diagram-generator/SKILL.md` |
| `audit-workflow` | Produce expert code audits and reconcile audit findings with GitHub issues and labels | `.github/skills/audit-workflow/SKILL.md` |
| `dotnet-patterns` | Any C#/.NET code generation: naming, XML docs, SOLID, async/await, EF Core, pattern matching, security | `.github/skills/dotnet-patterns/SKILL.md` |
| `xunit-unit-testing` | Any .NET xUnit unit-test work: project placement under `tests/`, naming, AAA, FluentAssertions, NSubstitute, Verify, Bogus, MockQueryable, coverage, mutation | `.github/skills/xunit-unit-testing/SKILL.md` |
| `angular-patterns` | Angular frontend patterns for ce repo : Signals, standalone components, forms, Axios, routing, Material+Tailwind, i18n | `.github/skills/angular-patterns/SKILL.md` |

## Unit Test Routing [2026-04-27]

- `dotnet-dev` must load `xunit-unit-testing` for any xUnit unit-test task (creation, review, bug reproduction, snapshots, coverage, mutation).
- Unit test projects belong under `tests/<TargetAssembly>.Tests/` and target exactly one production assembly.
- `tests/InfraFlowSculptor.GenerationParity.Tests/` is currently just a kept folder without an active `.csproj`; do not place ordinary unit tests there.

## Skill Concept
A Skill is a `SKILL.md` file of pure knowledge, lazy-loaded via `read_file` when the task justifies it. No tools, composable, lightweight. Skills override pre-training with tested project-specific patterns.

## GitHub Operations
- Default GitHub repository for this project is `FlorianDrevet/infra-pipeline-editor` unless the user explicitly names another repository.
- Audit issue workflows use reports under `audits/` (for example `audits/audit-14-04-2026`) together with `scripts/sync-audit-issues.ps1`; on 2026-04-15, 66 findings were recreated as GitHub issues and the `phase:*` / `severity:*` label mojibake was cleaned up.
