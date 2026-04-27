---
description: 'Ajouter une nouvelle ressource Azure end-to-end (Domain → App → Infra → Contracts → API → Bicep → Frontend → i18n)'
mode: 'agent'
---

# Ajouter une ressource Azure : {{ resourceName }}

- **Type ARM** : `{{ armType }}`
- **Abréviation** : `{{ abbr }}`

## Instructions

1. Charger le skill `new-azure-resource` (`.github/skills/new-azure-resource/SKILL.md`).
2. Suivre la checklist du skill étape par étape.
3. Utiliser `@dotnet-dev` pour tout le code C# et `@angular-front` pour le frontend.
4. Nommer l'agrégat : `{{ resourceName }}` (PascalCase).
5. Nommer le fichier de migration EF Core : `Add{{ resourceName }}Aggregate`.
