# Agents & Skills Registry
## Agents

| Agent | Role | File |
|---|---|---|
| `dev` | Orchestrateur principal, lit MEMORY, route aux agents spécialisés | `.claude/agents/dev.md` |
| `dotnet-dev` | Expert C#/.NET 10 | `.claude/agents/dotnet-dev.md` |
| `angular-front` | Expert frontend Angular standalone, tout travail `src/Front` (repo en Angular 21) | `.claude/agents/angular-front.md` |
| `architect` | Analyse archi, challenge, plan d'implémentation (ne code pas) | `.claude/agents/architect.md` |
| `documentation-professor` | Rédaction technique pédagogique, onboarding, explication des patterns et guide de lecture du code | `.claude/agents/documentation-professor.md` |
| `aspire-debug` | Debug runtime Aspire, MCP diagnostics | `.claude/agents/aspire-debug.md` |
| `audit-expert` | Audit technique expert, produit des rapports `audits/` et synchronise les issues GitHub d'audit | `.claude/agents/audit-expert.md` |
| `review-expert` | Revue de code pré-merge sur le diff contre `main`, findings sévérisés et backlog de correction | `.claude/agents/review-expert.md` |
| `vibe-coding-refractaire` | Seconde passe de revue PR anti-vibe coding, traque les abstractions bidon, duplications masquées, tests creux et code généré fragile | `.claude/agents/vibe-coding-refractaire.md` |
| `review-remediator` | Remédiation disciplinée d'un backlog de review approuvé, avec validations et traçabilité des fixes | `.claude/agents/review-remediator.md` |
| `pr-manager` | Conventions PR (titre, description, template) | `.claude/agents/pr-manager.md` |
| `merge-main` | Fusion main sur branche courante | `.claude/agents/merge-main.md` |
| `dream` | Consolidation mémoire (4 phases Dream) | `.claude/agents/dream.md` |
| `upgrade-orchestrator` | Orchestrateur montées de version (.NET, Angular, NuGet, npm, TS) — lit release notes, planifie, délègue | `.claude/agents/upgrade-orchestrator.md` |
> Note : l'agent `memory` (déprécié dans Copilot) n'a **pas** été migré vers Claude — son rôle est tenu par le thread principal + la commande `/dev`.

## Dream — déclenchement manuel [2026-06-02]

- La consolidation mémoire se lance **manuellement** via la commande `/dream` (Claude Code). Plus de gate 24h/5-sessions, plus de compteur `sessionsSinceLastDream`, plus de verrou `$env:TEMP`.
- Le thread principal lance le sous-agent `dream` (un sous-agent ne peut pas en lancer un autre dans Claude Code).
- `dream-state.md` ne trace plus que `lastDreamDate` à titre informatif.

## Skills

| Skill | When to load | File |
|---|---|---|
| `cqrs-feature` | New aggregate, CQRS feature scaffolding | `.claude/skills/cqrs-feature/SKILL.md` |
| `mcp-dotnet-server` | Design, planning, and implementation guidance for a C#/.NET MCP server integrated with InfraFlowSculptor, including VS Code exposure, transport selection, IaC import/migration workflows, and long-term evolution strategy | `.claude/skills/mcp-dotnet-server/SKILL.md` |
| `ui-ux-front-saas` | Any frontend UI/UX work | `.claude/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | New Azure resource type end-to-end | `.claude/skills/new-azure-resource/SKILL.md` |
| `gitnexus-workflow` | Code exploration via knowledge graph, impact analysis before modifications, post-change validation, safe refactoring | `.claude/skills/gitnexus-workflow/SKILL.md` |
| `graphify-corpus` | Corpus-level knowledge graph (docs+code+diagrams+audits), god nodes, community detection, surprising connections, onboarding orientation, architecture overview spanning docs and code | `.claude/skills/graphify-corpus/SKILL.md` |
| `draw-io-diagram-generator` | Create or update draw.io diagrams (`.drawio`, `.drawio.svg`, `.drawio.png`) for architecture and technical documentation | `.claude/skills/draw-io-diagram-generator/SKILL.md` |
| `audit-workflow` | Produce expert code audits and reconcile audit findings with GitHub issues and labels | `.claude/skills/audit-workflow/SKILL.md` |
| `dotnet-patterns` | Any C#/.NET code generation: naming, XML docs, SOLID, async/await, EF Core, pattern matching, security | `.claude/skills/dotnet-patterns/SKILL.md` |
| `xunit-unit-testing` | Any .NET xUnit unit-test work: project placement under `tests/`, naming, AAA, FluentAssertions, NSubstitute, Verify, Bogus, MockQueryable, coverage, mutation | `.claude/skills/xunit-unit-testing/SKILL.md` |
| `tdd-workflow` | **Any code modification** — enforces TDD Red→Green→Refactor→Verify cycle, test project init, test debt tracking in `.claude/test-debt.md` | `.claude/skills/tdd-workflow/SKILL.md` |
| `angular-patterns` | Angular frontend patterns for ce repo : Signals, standalone components, forms, Axios, routing, Material+Tailwind, i18n | `.claude/skills/angular-patterns/SKILL.md` |
| `bicep-v2-migration` | Migrating an IResourceTypeBicepGenerator from legacy string template to Builder + IR (Vague 2), including TDD tests, emitter parity, review cycle, and skill feedback loop | `.claude/skills/bicep-v2-migration/SKILL.md` |
| `dotnet-upgrade` | .NET version upgrade: SDK, TFM, NuGet, EF Core, ASP.NET Core, Aspire — breaking changes detection, migration procedure, new feature proposals | `.claude/skills/dotnet-upgrade/SKILL.md` |
| `angular-upgrade` | Angular version upgrade: CLI, framework, TypeScript, RxJS, Material, npm packages — breaking changes detection, ng update, new syntax proposals | `.claude/skills/angular-upgrade/SKILL.md` |

## Code Generation Guardrails [2026-04-29]

- New workspace instruction: `.claude/instructions/code-quality-guardrails.instructions.md` auto-attaches on C# and Angular source edits.
- Guardrails now repeated across `dev`, `architect`, `dotnet-dev`, `angular-front`, `vibe-coding-refractaire`, `dotnet-patterns`, and `angular-patterns`.
- Mandatory rules: no magic strings, one public top-level type/class per file, strongly typed contracts/models/persistence before `object` / `Dictionary` / `JsonDocument` / weak JSON, and explicit design-pattern choice based on readability, maintainability, and scalability.

## Frontend DS-first hard gate [2026-05-27]

- `.github/copilot-instructions.md` now states the DS rule in hard-gate form for frontend work: if `app-ds-*` covers the need, agents must use it instead of crafting ad hoc feature UI.
- `.claude/agents/angular-front.md` now treats DS-first as an absolute rule: no handcrafted feature-level button/field/select/chip/tabs/table/menu/card/dialog/banner/accordion patterns when the design system already covers them.
- `.claude/agents/dev.md` now requires every frontend delegation to repeat that DS-first rule explicitly; if a needed pattern is missing, the agent must create or extend a reusable primitive in `src/Front/src/app/shared/components/ds/` before touching the screen.
- Only exceptions explicitly documented in project memory remain allowed.

## Request Contradiction Pass [2026-05-20]

- `.github/copilot-instructions.md`, `.claude/agents/dev.md`, and `.github/prompts/InfraFlowProject.prompt.md` now require an explicit contradiction pass for Bicep generation, Azure DevOps pipelines, bootstrap flows, service connections, repository layouts, and multi-environment configuration requests.
- Agents must challenge technically false or architecture-breaking asks before coding, instead of treating the latest user wording as a sufficient specification.
- Hidden compatibility fallbacks or UI shortcuts that weaken environment isolation are now treated as risks to question, not conveniences to preserve by default.

## Plan vivant multi-PC [2026-05-21]

- `dev.md` impose désormais un **tracker de plan vivant** pour toute implémentation par lots/phases (`plan`, `roadmap`, backlog) : réutiliser le fichier existant ou créer `docs/features/<slug>-implementation-tracker.md`.
- Le suivi doit être **mis à jour pendant l'exécution** (avant étape = `In progress`, après incrément = validations et résultat, blocage = cause + prochaine action), pas seulement en fin de tâche.
- Le protocole `dev` inclut maintenant une vérification dédiée (`step 4ter`) pour garantir que le tracker reflète réellement l'état des lots et qu'une reprise sur un autre PC est possible sans contexte oral.
- `architect.md` doit inclure dans son plan un bloc `Fichier de suivi vivant` avec table de statuts par lot et règles de mise à jour.
- `dotnet-dev.md` et `angular-front.md` doivent synchroniser le tracker pendant et en fin d'implémentation backend/frontend (`Done` / `In progress` / `Blocked` + validations exécutées).

## Unit Test Routing [2026-04-27]

- `dotnet-dev` must load `tdd-workflow` + `xunit-unit-testing` for any code modification task (not just test-only tasks).
- The TDD cycle RED → GREEN → REFACTOR → VERIFY is mandatory before any production code is written.
- If the test project `tests/<Assembly>.Tests/` doesn't exist, create it following `xunit-unit-testing` section 2.
- Test debt (areas with no test coverage) must be tracked in `.claude/test-debt.md`.
- Unit test projects belong under `tests/<TargetAssembly>.Tests/` and target exactly one production assembly.
- `tests/InfraFlowSculptor.GenerationParity.Tests/` is currently just a kept folder without an active `.csproj`; do not place ordinary unit tests there.

## Pre-merge Review Routing [2026-04-27]

- `review-expert` is the strict code-review gate for branch diffs intended for `main`.
- Its review scope is the merge diff by default (`origin/main...HEAD`, fallback `main...HEAD`), not the full repository.
- It must output severity-ranked findings first, then questions/assumptions, then a corrective backlog ready to delegate.
- It should prioritize maintainability, security, scalability, architecture, and test gaps over speed of implementation.
- It must explicitly flag weak typing, dump files with multiple public top-level types, magic strings in structural code, and decorative patterns as real findings rather than style nits.

## Anti-vibe Review Routing [2026-04-29]

- `vibe-coding-refractaire` is the mandatory second-pass reviewer for generated or suspicious diffs that may hide vibe-coding smells.
- It focuses on unnecessary abstractions, hidden duplication, copy-paste adaptation, weak or theatrical tests, guessed design, and repository-convention drift.
- It complements `review-expert` rather than replacing it: merge-readiness first, anti-vibe pass second.

## Documentation Routing [2026-04-29]

- `documentation-professor` is the dedicated agent for technical documentation, onboarding guides, code-reading guides, and pedagogy-oriented explanations of project patterns.
- It must ground explanations in the real codebase and connect theory to actual files, layers, and execution flows instead of producing generic framework prose.
- It should keep documentation aligned with repository conventions from `docs/README.md`: concepts and architecture in `docs/architecture/`, feature docs in `docs/features/`, Azure docs in `docs/azure/`.

## Review Workflow [2026-04-27]

- Workspace prompt `.github/prompts/review-main.prompt.md` standardizes pre-merge reviews against `origin/main`.
- PR creation requires double gate: `review-expert` + `vibe-coding-refractaire` before submission. Sequence: review-main → review-expert → vibe-coding-refractaire → review-remediator → optional second pass.

## Skill Concept
A Skill is a `SKILL.md` file of pure knowledge, lazy-loaded via `read_file` when the task justifies it. No tools, composable, lightweight. Skills override pre-training with tested project-specific patterns.

## GitHub Operations
- Default GitHub repository for this project is `FlorianDrevet/infra-pipeline-editor` unless the user explicitly names another repository.
- Audit issue workflows use reports under `audits/` (for example `audits/audit-14-04-2026`) together with `scripts/sync-audit-issues.ps1`; on 2026-04-15, 66 findings were recreated as GitHub issues and the `phase:*` / `severity:*` label mojibake was cleaned up.

## Audit Triage Canonical Source [2026-05-13]

- `audits/triage-2026-05-13.md` is now the canonical local backlog for the 2026-05 audit wave, backed by `audits/correction-plan-13-05-2026.md`.
- Current durable status after the latest resync: all P1/P2 issues are closed, P3 was requalified/resynchronized, wave P4 closed the targeted guardrail slice, and `13` issues remain open.
- Treat the older `audits/triage-2026-05-12.md` file as historical context for the previous audit cycle, not as the active source of truth for current remediation tracking.

## Local Audit 2026-05-15

- `audits/audit-15-05-2026.md`: slice audit (privatization/networking + pipeline step options). Phased remediation closed same-day (commits `c52fc926..dc06a28b`).

## Parallel Worktree Workflow [2026-04-30]

- Isolation unit: `1 feature = 1 branch = 1 git worktree = 1 VS Code window = 1 PR`. Prefer 1 hub worktree on `origin/main` + 2-3 active feature worktrees.
- Avoid multi-root workspaces for agentic coding. Local runtime is shared (MCP endpoint `127.0.0.1:5258`, Aspire ports).
- Branch naming: `copilot/<slot>/<scope>-<slug>`, folder: `ifs-<slot>-<scope>-<slug>`.
- Prefer regular merges from `origin/main` into long-lived feature branches (no continuous rebases).

## MCP Skill [2026-04-29]

- Project stance: MCP must stay an adapter layer over `Application`/generation services; import/migration logic should be reusable outside MCP via canonical import services and contracts.
- **Current state [2026-04-29]:** MCP runs as ASP.NET Core HTTP host (`/mcp`, port 5258) under Aspire, secured with PAT auth. Import preview/apply logic extracted to `Application/Imports/` for shared API+MCP use. `ResourceCommandFactory` + `ProjectSetupOrchestrator` wire end-to-end resource creation. One-class-per-file enforced, `LayoutPresetEnum` replaces magic strings.
- Conversational creation rule: a prompt like "create a project with a Key Vault" must first go through a draft/clarification step; repository topology (`MonoRepo`, `SplitInfraCode`, etc.) must not be guessed by a mutating tool.
- Environment subscription rule: missing environment subscription IDs are optional during MCP draft/project creation. They must remain non-blocking warnings, warnings must be recomputed from the current draft state on revalidation, and `create_project_from_draft` should echo them in the success payload so the caller knows the subscription can be configured later.
- VS Code connection: `.vscode/mcp.json` points to `http://127.0.0.1:5258/mcp` with `Authorization: Bearer ${input:ifs_pat}` header.
- The skill now explicitly requires typed import models instead of weak `Dictionary<string, object>` / `JsonDocument` propagation, one public top-level type per file, and deliberate pattern selection.

## Graphify Runtime Notes [2026-04-29]

- Installed via PyPI `graphifyy`; prefer `python -m graphify ...` in terminal (user Scripts not always on PATH).
- Bootstrap: `python -c "from pathlib import Path; from graphify.watch import _rebuild_code; import sys; ok = _rebuild_code(Path('.')); sys.exit(0 if ok else 1)"`.
- Query: `python -m graphify query "<topic>" --graph .\graphify-out\graph.json`.
- `graphify.serve` requires `mcp` pip package. `graph.html` may fail on large repos (too many nodes).
- Controlled VS Code integration: prefer `python -m graphify copilot install` over `graphify vscode install` (the latter appends a generic section to copilot-instructions.md).
- User skill installed at `%USERPROFILE%\.copilot\skills\graphify\SKILL.md`.
- `.graphifyignore` excludes `MEMORY.md` and `.claude/memory/` but not the `ifs/` Obsidian vault.
