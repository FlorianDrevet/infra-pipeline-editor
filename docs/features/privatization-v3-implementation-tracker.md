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
| Lot 3 — CQRS/API | Not started | dotnet-dev | 2026-05-29 | Créer commandes/endpoints `SetPrivateEndpointConfig` et `RemovePrivateEndpointConfig` |
| Lot 4 — cross-config VNet | Not started | dotnet-dev | 2026-05-29 | Valider VNet projet, subnet, et référence cross-configuration explicite |
| Lot 5 — génération Bicep | Not started | dotnet-dev | 2026-05-29 | Remplacer les stages skeleton par la génération Private Endpoint réelle |
| Lot 6 — frontend ressource | Not started | angular-front | 2026-05-29 | Déplacer l'édition dans `resource-edit`, sélectionner VNet/subnet, retirer modes |
| Lot 7 — aide DNS et i18n | Not started | angular-front | 2026-05-29 | Ajouter dialogue d'aide DNS FR/EN avec composants DS |
| Lot 8 — suppression V2 UI/API | Not started | dev | 2026-05-29 | Supprimer `NetworkingProfile` obsolète après remplacement fonctionnel |
| Lot 9 — validation finale | Not started | dev | 2026-05-29 | Build/tests .NET, typecheck/build frontend, GitNexus detect_changes, mémoire projet |

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

## Prochaines étapes

1. Lot 3 : créer les commandes CQRS de configuration Private Endpoint au niveau ressource et les exposer via API.
2. Lot 4 : valider que le VNet sélectionné appartient au même projet que la ressource cible, même s'il vient d'une autre configuration, et que le subnet demandé existe sur ce VNet.
3. Lot 5 : remplacer les stages Bicep skeleton V2 par une génération Private Endpoint réelle basée sur `AzureResource.PrivateEndpointConfiguration`.
4. Lots 6-7 : déplacer l'édition frontend dans `resource-edit`, retirer les modes réseau, ajouter le sélecteur VNet/subnet cross-config et l'aide DNS pédagogique.
5. Lot 8 : supprimer ou déprécier proprement `NetworkingProfile` seulement après remplacement fonctionnel API + Bicep + UI.

## Checklist reprise sur un autre PC

- Branche : vérifier la branche courante avec `git branch --show-current`.
- Restaurer/build backend : `dotnet build .\InfraFlowSculptor.slnx`.
- Tests backend ciblés Lot 1/2 : `dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj`; `dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --filter "FullyQualifiedName~AzureResourcePrivateEndpointConfigurationTests|FullyQualifiedName~CoreStringLengthConfigurationTests"`.
- Validation complète backend en fin de slice : `dotnet test .\InfraFlowSculptor.slnx`.
- Frontend après Lots 6-7 : depuis `src\Front`, lancer `npm run typecheck` puis `npm run build`.
- GitNexus : relancer `npx gitnexus analyze` après les changements structurants, puis `mcp_gitnexus_detect_changes(scope: "all")` avant clôture.
- Mémoire projet : mettre à jour `.github/memory/03-domain-model.md`, `.github/memory/07-bicep-generation.md`, `.github/memory/08-frontend.md` et `.github/memory/changelog.md` selon les lots livrés.