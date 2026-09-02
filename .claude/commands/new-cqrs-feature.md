---
description: Générer une feature CQRS complète (Command/Query, Handler, Validator, Contracts, Endpoint, Migration).
argument-hint: "<NomFeature> [opérations: Create/Read/Update/Delete/List]"
---

# Nouvelle feature CQRS : $ARGUMENTS

## Instructions

1. Charger le skill `cqrs-feature` (`.claude/skills/cqrs-feature/SKILL.md`).
2. Lancer le sous-agent `dotnet-dev` (tool *Agent*) pour toute la génération C#, en lui passant les conventions et les chemins exacts.
3. Créer les fichiers dans l'ordre du skill : Domain → Application (Commands/Queries, Handlers, Validators) → Contracts → Mapping Mapster → API Endpoint.
4. Charger `tdd-workflow` : écrire les tests AVANT le code de production.
5. Créer la migration EF Core : `Add<NomFeature>Aggregate`.
6. Vérifier le build : `dotnet build .\InfraFlowSculptor.slnx` et `dotnet test .\InfraFlowSculptor.slnx`.
7. Mettre à jour `.claude/memory/03-domain-model.md` + `.claude/memory/changelog.md` avec le nouvel agrégat.
