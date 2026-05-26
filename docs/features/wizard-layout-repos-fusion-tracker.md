# Wizard Layout + Repositories Fusion — Implementation Tracker

## Contexte

Fusionner la configuration des repositories directement dans l'étape layout du wizard de création de projet, avec l'UX de vérification de connexion inline (provider → URL → PAT → Vérifier → branches autocomplete → branche par défaut).

## Statut des lots

| Lot | Contenu | Statut | Owner | Dernière mise à jour | Reste à faire |
|-----|---------|--------|-------|----------------------|---------------|
| Lot 1 | Backend — `POST /git/verify-connection` | Done | dotnet-dev | 2026-05-26 | — |
| Lot 2 | Backend — PAT dans `CreateProjectWithSetup` | Done | dotnet-dev | 2026-05-26 | — |
| Lot 3 | Frontend — `RepositoryConnectionFormComponent` shared | Done | angular-front | 2026-05-26 | — |
| Lot 4 | Frontend — Refonte `LayoutStepComponent` inline slots | Done | angular-front | 2026-05-26 | — |
| Lot 5 | Frontend — Adapter wizard + nettoyage | Done | angular-front | 2026-05-26 | — |

## Journal d'implémentation

- 2026-05-26: Plan validé par architect. Lot 1 lancé.
- 2026-05-26: Lots 1+2 backend terminés — endpoint `POST /git/verify-connection` créé, PAT ajouté à `CreateProjectWithSetup`.
- 2026-05-26: Lots 3+4+5 frontend terminés — `RepositoryConnectionFormComponent` créé, LayoutStep avec inline repos, wizard simplifié à 4 étapes.
- 2026-05-26: Validation complète — typecheck OK, build OK, 1474 tests passent.

## Prochaines étapes

Tous les lots sont terminés. Suppression du fichier `repositories-step` (dead code) possible en cleanup ultérieur.

## Checklist reprise sur un autre PC

- [x] `dotnet build .\InfraFlowSculptor.slnx` passe (hors lock Aspire)
- [x] `npm install && npm run typecheck` passe (src/Front)
- [x] Nouvel endpoint `POST /git/verify-connection` répond 200 avec credentials valides
- [x] Wizard de création de projet a 4 étapes (Identity, Layout+Repos, Environments, Review)
