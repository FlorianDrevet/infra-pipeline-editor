# Wizard Layout + Repositories Fusion — Implementation Tracker

## Contexte

Fusionner la configuration des repositories directement dans l'étape layout du wizard de création de projet, avec l'UX de vérification de connexion inline (provider → URL → PAT → Vérifier → branches autocomplete → branche par défaut).

## Statut des lots

| Lot | Contenu | Statut | Owner | Dernière mise à jour | Reste à faire |
|-----|---------|--------|-------|----------------------|---------------|
| Lot 1 | Backend — `POST /git/verify-connection` | Done | dotnet-dev | 2026-05-26 | — |
| Lot 2 | Backend — PAT dans `CreateProjectWithSetup` | Done | dotnet-dev | 2026-05-26 | — |
| Lot 3 | Frontend — `RepositoryConnectionFormComponent` shared | Not started | angular-front | — | Component + service method |
| Lot 4 | Frontend — Refonte `LayoutStepComponent` inline slots | Not started | angular-front | — | Inline slots + validation |
| Lot 5 | Frontend — Adapter wizard + nettoyage | Not started | angular-front | — | Supprimer step 4, adapter nav |

## Journal d'implémentation

- 2026-05-26: Plan validé par architect. Lot 1 lancé.

## Prochaines étapes

1. Implémenter Lot 1 (backend endpoint stateless)
2. Implémenter Lot 2 (PAT dans CreateProjectWithSetup)
3. Implémenter Lot 3 (composant shared frontend)
4. Implémenter Lot 4 (refonte layout step)
5. Implémenter Lot 5 (adapter wizard principal + suppression step repos)

## Checklist reprise sur un autre PC

- [ ] `dotnet build .\InfraFlowSculptor.slnx` passe
- [ ] `npm install && npm run typecheck` passe (src/Front)
- [ ] Nouvel endpoint `POST /git/verify-connection` répond 200 avec credentials valides
- [ ] Wizard de création de projet a 4 étapes (Identity, Layout+Repos, Environments, Review)
