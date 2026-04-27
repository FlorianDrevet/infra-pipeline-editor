---
description: 'Lancer un audit technique complet du dépôt et synchroniser les issues GitHub'
mode: 'agent'
---

# Audit technique du dépôt

## Instructions

1. Utiliser `@audit-expert` comme agent principal.
2. Charger le skill `audit-workflow` (`.github/skills/audit-workflow/SKILL.md`).
3. Produire le rapport markdown dans `audits/`.
4. Synchroniser les findings avec les issues GitHub sur `FlorianDrevet/infra-pipeline-editor` (créer les nouvelles, fermer les résolues).
5. S'assurer que les labels `audit:*` existent sur le repo.
