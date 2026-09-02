---
name: "source-command-run-audit"
description: "Lancer un audit technique complet du dépôt et synchroniser les issues/labels GitHub."
---

# source-command-run-audit

Use this skill when the user asks to run the migrated source command `run-audit`.

## Command Template

# Audit technique du dépôt

## Instructions

1. Lancer le sous-agent `audit-expert` (tool *Agent*) comme agent principal.
2. Charger le skill `audit-workflow` (`.Codex/skills/audit-workflow/SKILL.md`).
3. Produire le rapport markdown dans `audits/`.
4. Synchroniser les findings avec les issues GitHub sur `FlorianDrevet/infra-pipeline-editor` (créer les nouvelles, fermer les résolues).
5. S'assurer que les labels `audit:*` existent sur le repo.
