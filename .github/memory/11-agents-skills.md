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

## Skill Concept
A Skill is a `SKILL.md` file of pure knowledge, lazy-loaded via `read_file` when the task justifies it. No tools, composable, lightweight. Skills override pre-training with tested project-specific patterns.
