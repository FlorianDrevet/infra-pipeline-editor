---
description: Lancer un audit technique complet du dépôt et synchroniser les issues/labels GitHub.
---

# Audit technique du dépôt

## Instructions

1. Lancer le sous-agent `audit-expert` (tool *Agent*) comme agent principal.
2. Charger le skill `audit-workflow` (`.claude/skills/audit-workflow/SKILL.md`).
3. Produire le rapport markdown dans `audits/`.
4. Synchroniser les findings avec les issues GitHub sur `FlorianDrevet/infra-pipeline-editor` (créer les nouvelles, fermer les résolues).
5. S'assurer que les labels `audit:*` existent sur le repo.
