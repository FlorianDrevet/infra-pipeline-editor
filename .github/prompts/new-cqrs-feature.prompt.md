---
description: 'Générer une feature CQRS complète (Command/Query, Handler, Validator, Contracts, Endpoint)'
mode: 'agent'
---

# Nouvelle feature CQRS : {{ featureName }}

- **Opérations** : {{ operations }}

## Instructions

1. Charger le skill `cqrs-feature` (`.github/skills/cqrs-feature/SKILL.md`).
2. Utiliser `@dotnet-dev` pour toute la génération C#.
3. Créer les fichiers dans l'ordre du skill : Domain → Application (Commands/Queries, Handlers, Validators) → Contracts → API Endpoint → Mapping Mapster.
4. Vérifier le build : `dotnet build .\InfraFlowSculptor.slnx`.
5. Mettre à jour `MEMORY.md` avec le nouvel agrégat/feature.
