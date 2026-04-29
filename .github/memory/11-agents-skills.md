# Agents & Skills Registry

## Agents

| Agent | Role | File |
|---|---|---|
| `dev` | Orchestrateur principal, lit MEMORY, route aux agents spécialisés | `.github/agents/dev.agent.md` |
| `dotnet-dev` | Expert C#/.NET 10 | `.github/agents/dotnet-dev.agent.md` |
| `angular-front` | Expert frontend Angular standalone, tout travail `src/Front` (repo en Angular 21) | `.github/agents/angular-front.agent.md` |
| `architect` | Analyse archi, challenge, plan d'implémentation (ne code pas) | `.github/agents/architect.agent.md` |
| `documentation-professor` | Rédaction technique pédagogique, onboarding, explication des patterns et guide de lecture du code | `.github/agents/documentation-professor.agent.md` |
| `aspire-debug` | Debug runtime Aspire, MCP diagnostics | `.github/agents/aspire-debug.agent.md` |
| `audit-expert` | Audit technique expert, produit des rapports `audits/` et synchronise les issues GitHub d'audit | `.github/agents/audit-expert.agent.md` |
| `review-expert` | Revue de code pré-merge sur le diff contre `main`, findings sévérisés et backlog de correction | `.github/agents/review-expert.agent.md` |
| `vibe-coding-refractaire` | Seconde passe de revue PR anti-vibe coding, traque les abstractions bidon, duplications masquées, tests creux et code généré fragile | `.github/agents/vibe-coding-refractaire.agent.md` |
| `review-remediator` | Remédiation disciplinée d'un backlog de review approuvé, avec validations et traçabilité des fixes | `.github/agents/review-remediator.agent.md` |
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
| `mcp-dotnet-server` | Design, planning, and implementation guidance for a C#/.NET MCP server integrated with InfraFlowSculptor, including VS Code exposure, transport selection, IaC import/migration workflows, and long-term evolution strategy | `.github/skills/mcp-dotnet-server/SKILL.md` |
| `ui-ux-front-saas` | Any frontend UI/UX work | `.github/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | New Azure resource type end-to-end | `.github/skills/new-azure-resource/SKILL.md` |
| `gitnexus-workflow` | Code exploration via knowledge graph, impact analysis before modifications, post-change validation, safe refactoring | `.github/skills/gitnexus-workflow/SKILL.md` |
| `graphify-corpus` | Corpus-level knowledge graph (docs+code+diagrams+audits), god nodes, community detection, surprising connections, onboarding orientation, architecture overview spanning docs and code | `.github/skills/graphify-corpus/SKILL.md` |
| `draw-io-diagram-generator` | Create or update draw.io diagrams (`.drawio`, `.drawio.svg`, `.drawio.png`) for architecture and technical documentation | `.github/skills/draw-io-diagram-generator/SKILL.md` |
| `audit-workflow` | Produce expert code audits and reconcile audit findings with GitHub issues and labels | `.github/skills/audit-workflow/SKILL.md` |
| `dotnet-patterns` | Any C#/.NET code generation: naming, XML docs, SOLID, async/await, EF Core, pattern matching, security | `.github/skills/dotnet-patterns/SKILL.md` |
| `xunit-unit-testing` | Any .NET xUnit unit-test work: project placement under `tests/`, naming, AAA, FluentAssertions, NSubstitute, Verify, Bogus, MockQueryable, coverage, mutation | `.github/skills/xunit-unit-testing/SKILL.md` |
| `tdd-workflow` | **Any code modification** — enforces TDD Red→Green→Refactor→Verify cycle, test project init, test debt tracking in `.github/test-debt.md` | `.github/skills/tdd-workflow/SKILL.md` |
| `angular-patterns` | Angular frontend patterns for ce repo : Signals, standalone components, forms, Axios, routing, Material+Tailwind, i18n | `.github/skills/angular-patterns/SKILL.md` |
| `bicep-v2-migration` | Migrating an IResourceTypeBicepGenerator from legacy string template to Builder + IR (Vague 2), including TDD tests, emitter parity, review cycle, and skill feedback loop | `.github/skills/bicep-v2-migration/SKILL.md` |

## Code Generation Guardrails [2026-04-29]

- New workspace instruction: `.github/instructions/code-quality-guardrails.instructions.md` auto-attaches on C# and Angular source edits.
- Guardrails now repeated across `dev`, `architect`, `dotnet-dev`, `angular-front`, `vibe-coding-refractaire`, `dotnet-patterns`, and `angular-patterns`.
- Mandatory rules: no magic strings, one public top-level type/class per file, strongly typed contracts/models/persistence before `object` / `Dictionary` / `JsonDocument` / weak JSON, and explicit design-pattern choice based on readability, maintainability, and scalability.

## Unit Test Routing [2026-04-27]

- `dotnet-dev` must load `tdd-workflow` + `xunit-unit-testing` for any code modification task (not just test-only tasks).
- The TDD cycle RED → GREEN → REFACTOR → VERIFY is mandatory before any production code is written.
- If the test project `tests/<Assembly>.Tests/` doesn't exist, create it following `xunit-unit-testing` section 2.
- Test debt (areas with no test coverage) must be tracked in `.github/test-debt.md`.
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

- Workspace prompt `.github/prompts/review-main.prompt.md` standardizes strict pre-merge reviews against `origin/main` / `main`.
- PR creation via `pr-manager` now requires the double technical gate: `review-main` / `review-expert` plus `vibe-coding-refractaire` before submission.
- `review-remediator` consumes the approved corrective backlog from `review-expert` and applies only the requested fixes.
- The expected sequence is: `review-main` prompt -> `review-expert` findings/backlog -> `vibe-coding-refractaire` anti-vibe findings/backlog -> `review-remediator` fixes -> optional second `review-main` pass.

## Skill Concept
A Skill is a `SKILL.md` file of pure knowledge, lazy-loaded via `read_file` when the task justifies it. No tools, composable, lightweight. Skills override pre-training with tested project-specific patterns.

## GitHub Operations
- Default GitHub repository for this project is `FlorianDrevet/infra-pipeline-editor` unless the user explicitly names another repository.
- Audit issue workflows use reports under `audits/` (for example `audits/audit-14-04-2026`) together with `scripts/sync-audit-issues.ps1`; on 2026-04-15, 66 findings were recreated as GitHub issues and the `phase:*` / `severity:*` label mojibake was cleaned up.

## MCP Skill [2026-04-29]

- New skill: `mcp-dotnet-server`.
- Scope: official MCP + C# SDK baseline, `stdio` vs Streamable HTTP, `.vscode/mcp.json`, auth/authorization, tasks, observability, testing, and project-specific integration rules.
- Project stance: MCP must stay an adapter layer over `Application`/generation services; import/migration logic should be reusable outside MCP via canonical import services and contracts.
- **Current state [2026-04-29]:** MCP runs as ASP.NET Core HTTP host (`/mcp`, port 5258) under Aspire, secured with PAT auth. Import preview/apply logic extracted to `Application/Imports/` for shared API+MCP use. `ResourceCommandFactory` + `ProjectSetupOrchestrator` wire end-to-end resource creation. One-class-per-file enforced, `LayoutPresetEnum` replaces magic strings.
- Conversational creation rule: a prompt like "create a project with a Key Vault" must first go through a draft/clarification step; repository topology (`MonoRepo`, `SplitInfraCode`, etc.) must not be guessed by a mutating tool.
- Environment subscription rule: missing environment subscription IDs are optional during MCP draft/project creation. They must remain non-blocking warnings, warnings must be recomputed from the current draft state on revalidation, and `create_project_from_draft` should echo them in the success payload so the caller knows the subscription can be configured later.
- VS Code connection: `.vscode/mcp.json` points to `http://127.0.0.1:5258/mcp` with `Authorization: Bearer ${input:ifs_pat}` header.
- The skill now explicitly requires typed import models instead of weak `Dictionary<string, object>` / `JsonDocument` propagation, one public top-level type per file, and deliberate pattern selection.

## Graphify Runtime Notes [2026-04-29]

- Workspace now includes a local `graphify-corpus` skill and a `graphify` MCP server entry in `.vscode/mcp.json`.
- Verified on this Windows machine with PyPI `graphifyy 0.4.23`: the user-install launcher exists at `%APPDATA%\Python\Python314\Scripts\graphify.exe`, but that folder is not on `PATH`; agents should prefer `python -m graphify ...` in terminal commands.
- Verified bootstrap command for a code-only graph on this repo: `python -c "from pathlib import Path; from graphify.watch import _rebuild_code; import sys; ok = _rebuild_code(Path('.')); sys.exit(0 if ok else 1)"`.
- Verified query command: `python -m graphify query "bicep generation" --graph .\graphify-out\graph.json` works against the generated graph.
- Verified `graphify.serve` requires a separate `mcp` Python package (`python -m pip install --user mcp`).
- Verified large-repo caveat: `graphify-out/graph.json` and `GRAPH_REPORT.md` are generated successfully on this repo, but `graph.html` may fail with "Graph has ... nodes - too large for HTML viz"; agents should treat HTML output as optional.
- Controlled VS Code integration rule: prefer `python -m graphify copilot install` over `graphify vscode install` for this repository. `vscode install` appends a generic `## graphify` section to `.github/copilot-instructions.md`, while this repo already has a stronger custom orchestration for memory, GitNexus, Graphify, and agents.
- 2026-04-29 validation: the Graphify user skill is installed at `%USERPROFILE%\.copilot\skills\graphify\SKILL.md`, and the Python user Scripts directory is now present on the user PATH so `graphify --help` works directly in terminal.

## CQRS Skill [2026-04-29]

- `cqrs-feature` now has valid skill frontmatter and explicitly enforces one public top-level type per file, no magic strings, strong typing over weak objects/dictionaries/JSON, and deliberate pattern selection before introducing abstractions.
