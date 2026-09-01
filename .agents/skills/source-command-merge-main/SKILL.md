---
name: "source-command-merge-main"
description: "Fusionner main sur la branche courante et résoudre les conflits selon les conventions du projet."
---

# source-command-merge-main

Use this skill when the user asks to run the migrated source command `merge-main`.

## Command Template

# Merge main sur la branche courante

## Instructions

1. Lancer le sous-agent `merge-main` (tool *Agent*).
2. Lire `.Codex/memory/MEMORY.md` pour l'état du projet.
3. Exécuter `git fetch origin ; git merge origin/main`.
4. Conflits : résoudre selon les conventions — privilégier les changements de `main` pour l'infrastructure, ceux de la branche pour les features en cours.
5. Vérifier le build : `dotnet build .\InfraFlowSculptor.slnx`.
6. Vérifier le frontend : `cd src\Front ; npm run typecheck ; npm run build`.
