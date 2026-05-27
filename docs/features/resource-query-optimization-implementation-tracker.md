# Resource Query Optimization Implementation Tracker

## Contexte

La navigation detail/edit ressource est lente hors cold start. Les traces Aspire montrent que `GET /container-app/{id}` est domine par une commande EF Core issue de la materialisation TPT du graphe `AzureResource`, et que `GET /projects/{id}/resources` amplifie le probleme dans la page Angular `resource-edit`.

## Statut des lots

| Lot | Statut | Owner | Derniere mise a jour | Reste a faire |
| --- | --- | --- | --- | --- |
| Lot 1 - Container App detail projection | Done | dev / dotnet-dev | 2026-05-22 - Projection + handler valides | Aucun |
| Lot 2 - Project resources projection | Done | dev / dotnet-dev | 2026-05-22 - Projection + handler valides | Aucun |
| Lot 3 - Frontend project resources cache | Done | angular-front | 2026-05-22 - Cache/coalescing implemente | Aucun (tests passes, typecheck OK) |
| Lot 4 - Validation finale | Done | dev | 2026-05-22 - Gates locaux passes | Aucun |

## Journal d'implementation

- 2026-05-22 - Plan corrige apres revue : autorisation via `InfrastructureConfigId` porte par le read model detail, `EnvironmentSettings` Container App chargees par requete ciblee separee, liste projet basee sur la colonne persistante `ResourceType` au lieu de `GetType().Name`, cache frontend court avec coalescing des requetes concurrentes.
- 2026-05-22 - Demarrage TDD RED backend pour `GetContainerAppQueryHandler`, `ContainerAppReadRepository`, `ListProjectResourcesQueryHandler`, et `ProjectResourceReadRepository`.
- 2026-05-22 - Backend handlers et read repositories cibles verts : tests Application handlers et Infrastructure read repositories passes.
- 2026-05-22 - **Lot 3 frontend complete** : Cache/coalescing implemente dans `ProjectService.getProjectResources()` avec TTL 30s, invalidation explicite via `invalidateProjectResourcesCache()`, et invalidation automatique apres mutations projet. 7 tests Jasmine verts (coalescing concurrent, cache hit TTL, expiration TTL, invalidation explicite, invalidation in-flight anti-cache stale, retry apres erreur, invalidation post-mutation). TypeScript typecheck passe.
- 2026-05-22 - Validation finale : `dotnet test .\InfraFlowSculptor.slnx` vert (4374 tests, 9 ignores), `dotnet build .\InfraFlowSculptor.slnx` vert, spec `project.service.spec.ts` vert (30 tests), `npm run typecheck` vert, `npm run build` vert avec warnings budgets/CommonJS preexistants.

## Prochaines etapes

1. Mesurer sous Aspire le gain reel sur `GET /container-app/{id}` et `GET /projects/{id}/resources` sur une session chaude authentifiee.
2. Si la page reste lente apres les deux endpoints optimises, planifier un lot separe pour lazy-loader les sections secondaires de `resource-edit` par onglet.

## Checklist reprise sur un autre PC

- Branche courante : utiliser la branche active du depot local, aucun commit cree par l'agent.
- Commandes backend ciblees :
  - `dotnet test .\tests\InfraFlowSculptor.Application.Tests\InfraFlowSculptor.Application.Tests.csproj --filter "FullyQualifiedName~GetContainerAppQueryHandlerTests|FullyQualifiedName~ListProjectResourcesQueryHandlerTests"`
  - `dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --filter "FullyQualifiedName~ReadRepositories"`
- Commandes frontend ciblees depuis `src\Front` :
  - `npm run typecheck`
  - `npm run build`
- Aucun changement de migration EF prevu : les optimisations utilisent des colonnes et tables existantes.