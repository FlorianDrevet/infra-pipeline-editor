# Privatization V3 Implementation Tracker

## Contexte

La configuration de privatisation réseau V2 est portée par `NetworkingProfile`, donc par `InfrastructureConfig`. Cette granularité est insuffisante : chaque ressource doit pouvoir choisir son propre Virtual Network et sa stratégie DNS de Private Endpoint. La refonte V3 déplace donc l'intention de privatisation au niveau ressource.

Décisions validées le 2026-05-29 :
- Supprimer la notion de mode réseau `Simplified` / `Standard` / `Advanced`.
- Traiter cette refonte comme le périmètre Private Endpoint uniquement.
- Ne pas confondre Private Endpoint avec l'intégration VNet de Container App Environment, qui restera une configuration spécifique CAE dans une slice séparée.
- Sélectionner le VNet depuis les `VirtualNetwork` configurés sur le projet, y compris depuis une autre configuration du même projet.
- Conserver `AzureResource.IsPrivatized` dans un premier temps pour limiter le blast radius, et l'enrichir avec une configuration privée typée au niveau ressource.
- Ajouter une aide DNS pédagogique côté frontend pour expliquer les modes et les choix de configuration.

## Statut des lots

| Lot | Statut | Owner | Dernière mise à jour | Reste à faire |
| --- | --- | --- | --- | --- |
| Lot 0 — cadrage et tracker | Done | dev | 2026-05-29 | Aucun |
| Lot 1 — domaine ressource | Done | dotnet-dev | 2026-05-29 | Aucun pour la fondation PE ressource ; le rename éventuel de `IsPrivatized` reste hors scope |
| Lot 2 — persistance EF Core | Done | dotnet-dev | 2026-05-29 | Aucun pour la migration additive ; la suppression V2 reste au Lot 8 |
| Lot 3 — CQRS/API | Done | dotnet-dev | 2026-05-30 | Aucun |
| Lot 4 — cross-config VNet | Done | dotnet-dev | 2026-05-30 | Aucun |
| Lot 5 — génération Bicep | Done | dotnet-dev | 2026-05-30 | Aucun |
| Lot 6 — frontend ressource | Done | angular-front | 2026-05-30 | Aucun |
| Lot 7 — aide DNS et i18n | Done | angular-front | 2026-05-30 | Aucun |
| Lot 8 — suppression V2 UI/API | Done | dev | 2026-05-30 | Dead code backend conservé volontairement |
| Lot 9 — validation finale | Done | dev | 2026-05-30 | Aucun |

## Journal d'implémentation

### 2026-05-29

- Lecture mémoire projet et skills obligatoires : TDD, .NET, xUnit, Angular, UI/UX, GitNexus.
- GitNexus réindexé avec `npx gitnexus analyze` car les symboles V2 réseau n'étaient pas visibles malgré un index récent.
- Constat : GitNexus voit `AzureResource` comme symbole à fort blast radius, mais ne remonte pas encore `NetworkingProfile`; l'impact V2 est donc complété par lecture de fichiers et recherche textuelle.
- Exploration confirmée : V2 actuelle est configuration-scoped, les stages Bicep réseau sont des skeletons, et aucun test actif ne couvre `NetworkingProfile`.
- Correction du plan d'architecture : ne pas baser la V3 sur un `PrivateEndpointConfig` actif, car il n'existe plus dans le domaine courant ; créer une nouvelle configuration typée au niveau ressource.
- Lot 1 livré en TDD : `AzureResource` porte désormais `PrivateEndpointConfiguration?`, expose `ConfigurePrivateEndpoint(...)` et `DisablePrivateEndpoint()`, et `Deprivatize()` nettoie la configuration privée.
- Lot 1 livré : ajout du mode DNS typé `PrivateEndpointDnsMode` avec `AutoManaged`, `ExistingHub`, `Disabled`, plus validation domaine des champs hub DNS pour `ExistingHub`.
- Lot 2 livré : mapping EF owned one-to-one de `PrivateEndpointConfiguration` sur `AzureResource`, migration additive `20260529090353_AddResourcePrivateEndpointConfiguration`, et conservation volontaire de `IsPrivatized` pour compatibilité incrémentale.
- Validation Lot 1/2 : `dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj` vert (636 tests) ; `dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --filter "FullyQualifiedName~AzureResourcePrivateEndpointConfigurationTests|FullyQualifiedName~CoreStringLengthConfigurationTests"` vert (9 tests, 1 warning préexistant Testcontainers).
- Validation large : `dotnet build .\InfraFlowSculptor.slnx` vert. `dotnet test .\InfraFlowSculptor.slnx` relancé ; résultat 4110 passés / 9 ignorés / 5 échecs préexistants hors scope (`UpdateProjectEnvironmentRequestTests.Given_EmptySubscriptionId_When_Validate_Then_ReturnsError` et 4 `SecurityMiddlewareIntegrationTests` bloqués par connection string health check nulle).
- GitNexus `detect_changes(scope: all)` exécuté : risque bas, 0 flux affecté remonté. Limite observée : l'outil n'a pas listé les nouveaux fichiers non indexés de cette slice ; utiliser `git status` comme source de vérité jusqu'à une prochaine analyse incluant les nouveaux symboles.

### 2026-05-30

- Lot 3 livré : commandes CQRS `ToggleResourcePrivatizationCommand`, `SetPrivateEndpointConfigCommand`, `RemovePrivateEndpointConfigCommand` avec handlers, validators (FluentValidation), et endpoints Minimal API (PUT privatization, PUT/DELETE private-endpoint).
- Lot 4 livré : validation cross-config VNet dans `SetPrivateEndpointConfigCommandValidator` — vérifie que le VNet sélectionné appartient au même projet, même s'il vient d'une autre configuration.
- Lot 5 livré : 3 stages Bicep PE réels (`PrivateEndpointCompanionStage` ordre 540, `NetworkingResolutionStage` ordre 560, `PublicNetworkAccessTransformerStage` ordre 520) avec `PrivateEndpointGroupIdCatalog` dans GenerationCore. 26 tests PE stages verts, 1067 tests BicepGeneration verts au total.
- Lot 6 livré : section networking dans `resource-edit` (composant standalone, controller signal-based, interface view-model), sélection VNet cross-config, sélection subnet, DNS mode, états chargement/erreur.
- Lot 7 livré : dialog aide DNS pédagogique (`DnsHelpDialogComponent`), i18n FR/EN complète sous `RESOURCE_EDIT.NETWORKING.*`.
- Lot 8 livré : suppression V2 API (controller réécrit V3-only), suppression frontend V2 (tab networking retiré de config-detail, section component/controller/constants/view-model supprimés), `PRIVATIZABLE_RESOURCE_TYPES` relocalisé dans `resource-edit/sections/networking/networking.constants.ts`. Dead code backend (agrégat, commandes, repo) conservé.
- Lot 9 validé : `dotnet build .\InfraFlowSculptor.slnx` vert (0 erreur). `dotnet test` : 1067 BicepGen + 1381 Application + 636 Domain + 150 Mcp + 156 PipelineGen + 36 Api + 99 GenerationCore + 302 Contracts = ~3827 tests verts. 1 échec pré-existant (`UpdateProjectEnvironmentRequestTests`). Frontend `npm run typecheck` + `npm run build` verts. Architecture boundary test fixé en approuvant `PrivateEndpointGroupIdCatalog`.

## Prochaines étapes

Tous les lots sont livrés. Dead code backend V2 (`NetworkingProfile` agrégat, commandes/queries V2, repo) conservé volontairement pour nettoyage futur en tâche séparée.

## Checklist reprise sur un autre PC

- Branche : vérifier la branche courante avec `git branch --show-current`.
- Restaurer/build backend : `dotnet build .\InfraFlowSculptor.slnx`.
- Tests backend ciblés Lot 1/2 : `dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj`; `dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --filter "FullyQualifiedName~AzureResourcePrivateEndpointConfigurationTests|FullyQualifiedName~CoreStringLengthConfigurationTests"`.
- Validation complète backend en fin de slice : `dotnet test .\InfraFlowSculptor.slnx`.
- Frontend après Lots 6-7 : depuis `src\Front`, lancer `npm run typecheck` puis `npm run build`.
- GitNexus : relancer `npx gitnexus analyze` après les changements structurants, puis `mcp_gitnexus_detect_changes(scope: "all")` avant clôture.
- Mémoire projet : mettre à jour `.github/memory/03-domain-model.md`, `.github/memory/07-bicep-generation.md`, `.github/memory/08-frontend.md` et `.github/memory/changelog.md` selon les lots livrés.