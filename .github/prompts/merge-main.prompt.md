---
description: 'Fusionner main sur la branche courante et résoudre les conflits'
mode: 'agent'
---

# Merge main sur la branche courante

## Instructions

1. Utiliser `@merge-main` comme agent.
2. Lire `MEMORY.md` pour connaître l'état du projet.
3. Exécuter `git fetch origin ; git merge origin/main`.
4. Si conflits : résoudre en respectant les conventions du projet, en privilégiant les changements de main pour l'infrastructure et les changements de la branche pour les features en cours.
5. Vérifier le build après merge : `dotnet build .\InfraFlowSculptor.slnx`.
6. Vérifier le frontend : `cd src\Front ; npm run typecheck ; npm run build`.
