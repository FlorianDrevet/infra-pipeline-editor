---
description: Ajouter une nouvelle ressource Azure end-to-end (Domain → App → Infra → Contracts → API → Bicep → Frontend → i18n).
argument-hint: "<NomRessource> <type ARM> <abréviation>"
---

# Ajouter une ressource Azure : $ARGUMENTS

## Instructions

1. Charger le skill `new-azure-resource` (`.claude/skills/new-azure-resource/SKILL.md`).
2. Suivre la checklist du skill étape par étape.
3. Lancer le sous-agent `dotnet-dev` pour tout le code C# (et le générateur Bicep) et `angular-front` pour le frontend, via le tool *Agent*.
4. Nommer l'agrégat en PascalCase ; nommer la migration EF Core `Add<NomRessource>Aggregate`.
5. Appliquer la passe de contradiction Bicep/pipeline (voir `/dev` §2a) avant de générer le code.
6. Mettre à jour la mémoire (`.claude/memory/03-domain-model.md`, `07-bicep-generation.md`, `changelog.md`).
