# Agents & Skills Registry

## Agents

| Agent | Role | File |
|---|---|---|
| `dev` | Orchestrateur principal, lit MEMORY, route aux agents spécialisés | `.github/agents/dev.agent.md` |
| `dotnet-dev` | Expert C#/.NET 10 | `.github/agents/dotnet-dev.agent.md` |
| `angular-front` | Expert Angular 19, tout travail `src/Front` | `.github/agents/angular-front.agent.md` |
| `architect` | Analyse archi, challenge, plan d'implémentation (ne code pas) | `.github/agents/architect.agent.md` |
| `aspire-debug` | Debug runtime Aspire, MCP diagnostics | `.github/agents/aspire-debug.agent.md` |
| `pr-manager` | Conventions PR (titre, description, template) | `.github/agents/pr-manager.agent.md` |
| `merge-main` | Fusion main sur branche courante | `.github/agents/merge-main.agent.md` |
| `dream` | Consolidation mémoire (4 phases Dream) | `.github/agents/dream.agent.md` |
| `memory` | **DEPRECATED** — redirecteur vers `dev` | `.github/agents/memory.agent.md` |

## Skills

| Skill | When to load | File |
|---|---|---|
| `cqrs-feature` | New aggregate, CQRS feature scaffolding | `.github/skills/cqrs-feature/SKILL.md` |
| `ui-ux-front-saas` | Any frontend UI/UX work | `.github/skills/ui-ux-front-saas/SKILL.md` |
| `new-azure-resource` | New Azure resource type end-to-end | `.github/skills/new-azure-resource/SKILL.md` |
| `gitnexus-workflow` | Code exploration via knowledge graph, impact analysis before modifications, post-change validation, safe refactoring | `.github/skills/gitnexus-workflow/SKILL.md` |
| `draw-io-diagram-generator` | Create or update draw.io diagrams (`.drawio`, `.drawio.svg`, `.drawio.png`) for architecture and technical documentation | `.github/skills/draw-io-diagram-generator/SKILL.md` |

## Skill Concept
A Skill is a `SKILL.md` file of pure knowledge, lazy-loaded via `read_file` when the task justifies it. No tools, composable, lightweight. Skills override pre-training with tested project-specific patterns.

## GitNexus Integration
- Repo indexed as `infra-pipeline-editor` (9569 symbols, 41834 relationships, 300 flows)
- `architect` uses `gitnexus_query/context/impact` in Step 3 (exploration) and Step 4 (challenge)
- `dotnet-dev` runs `gitnexus_impact()` before modifying shared symbols, `detect_changes()` at end
- `angular-front` runs `gitnexus_impact()` before modifying shared services, `detect_changes()` at end
- `dev` uses GitNexus as first exploration step in Research phase, passes impact results in delegation prompts
- `dream` uses `gitnexus_detect_changes()` as signal source, maintains `13-code-graph.md`
