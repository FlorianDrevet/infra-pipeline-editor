# Carte de vérité fonctionnelle — InfraFlowSculptor

> **But** : savoir ce qui existe, ce qui marche, et ce qu'on garde. Reconstruite depuis le
> code, pas depuis `.claude/memory/` (qui s'est révélée partiellement périmée).
>
> **Ambition produit** : produit destiné à la vente. Le tri est sévère — ce qui n'est pas
> sur le golden path doit justifier son existence.
>
> **Golden path** : modéliser une infra Azure → générer le Bicep → générer les pipelines
> Azure DevOps → pousser dans les dépôts git du client.

## Règles de lecture

- **Statut** : `?` inconnu · `🟢` vérifié par exécution · `🟡` partiel · `🔴` cassé.
  Un statut n'est jamais posé sur une simple lecture de code. `🔴` est autorisé quand le
  défaut est structurel et démontrable sans exécuter (route absente, secret jamais écrit).
- **Décision** : `keep` / `fix` / `cut` / `?` — arbitrée avec le porteur du projet.
- **Layout** : `Project.LayoutPreset` (AllInOne / SplitInfraCode / MultiRepo) est un **axe
  transverse**, pas une étape. Il change la génération Bicep, les pipelines, le bootstrap
  et le push. Une feature validée sur un layout ne dit rien des deux autres.
- **Cartographie** : Graphify fournit le contexte transversal code, documentation, audits et
  diagrammes. Les statuts restent fondés sur la lecture du code et la validation par exécution.

## Avancement de la cartographie

| Tranche | État |
|---|---|
| 1. Projet & topologie dépôts/git | cartographiée |
| 2. InfraConfig, ResourceGroup & naming | cartographiée |
| 3. Catalogue des types de ressources Azure | cartographiée |
| 4. Câblage entre ressources | cartographiée |
| 5. Génération Bicep | cartographiée |
| 6. Pipelines, bootstrap & push git × layouts | cartographiée |
| 7. Périphérie : import ARM, MCP/PAT, networking | cartographiée |

---

## GOLDEN PATH

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F01 | Créer un projet (assistant) | Créer projet + topologie + environnements + dépôts en une fois | 🟡 | fix | L'assistant stocke le PAT **sans vérifier la connexion git**, alors que l'ajout de dépôt classique vérifie les branches avant sauvegarde. Un projet peut naître avec une URL ou un PAT invalides, sans erreur. Même motif que D05/D08/D11 — à traiter, priorité après MultiRepo et les 3 mensonges. |
| F02 | Membres et rôles projet | Ajouter/retirer un utilisateur, changer son rôle | ? | keep | Chaîne complète, tests présents. |
| F03 | Environnements de projet | Déclarer dev/preprod/prod avec localisation, abonnement, approbation | ? | keep | Vérifié en tri (2026-08-28) : `ProjectEnvironmentController` (3 routes CRUD complètes, tous les champs promis — localisation, abonnement, approbation), handlers + validators + tests domaine/contracts, exposé aussi côté MCP (`ProjectQueryTools`/`ProjectResources`). Front : écran dédié + modale d'ajout, deux points d'entrée cohérents (wizard de création + détail projet). Note vide en cartographie initiale par omission, pas par risque réel. |
| F04 | Topologie des dépôts (`LayoutPreset`) | Choisir la répartition du code entre les dépôts git du client | ? | keep | Règles réellement validées dans le domaine pour les 3 presets. Changer de preset vide les dépôts déclarés. |
| F05 | Dépôts par config (MultiRepo) | Déclarer les dépôts au niveau `InfrastructureConfig` | 🟡 | fix | PAT config-level, vérification de connexion et routage du secret sont implémentés (D01). Validation contre un vrai fournisseur Git encore attendue. |
| F06 | Connexion git & navigation dépôt | Tester la connexion, lister les branches, chercher fichiers/dossiers du dépôt applicatif | 🟡 | fix | Fonctionne au niveau projet. Les flux de navigation config-level restent à compléter pour le vrai MultiRepo; le push bulk D10 utilise le routage config-level directement. |
| F07 | Config d'infra & resource groups | CRUD des configurations Bicep d'un projet, de leurs resource groups, et des tags de config qui étendent les tags projet | ? | keep | Chaîne complète, front câblé, tests présents. Rien à signaler. |
| F07b | Héritage des conventions de nommage par config | Choisir si une config hérite du nommage projet ou définit le sien (`PUT /infra-config/{id}/inheritance`) | 🔴 | fix | La bascule ne veut pas dire la même chose selon qui la lit. Voir D05. |
| F08 | Modéliser les ressources + settings par env | CRUD des ~20 types Azure catalogués dans un resource group, avec réglages par environnement et sous-ressources (subnets, blob containers, event hubs, bases SQL) | 🟡 | keep | Motif CRUD homogène sur tous les types, front et tests présents. Catalogue back/front identique (`AzureResourceTypes.cs:101` ↔ `resource-type.metadata.ts:4`). Écart isolé : `EventHubNamespace` persiste des settings par env que le front n'expose jamais (absent de `RESOURCE_TYPES_WITH_ENVIRONMENT_SETTINGS`). |
| F09 | Naming (cascade projet → config) | Gabarits de nommage et abréviations par type de ressource, aux deux niveaux, + vérification de disponibilité du nom | 🔴 | fix | **Deux implémentations concurrentes de la cascade** — celle qui répond à l'utilisateur n'est pas celle qui génère le Bicep. Voir D05. La « vérification Azure » est en fait une résolution DNS publique (`DnsNameAvailabilityChecker.cs:36`), pas l'API ARM `checkNameAvailability`. |
| F10 | Câbler les ressources (RBAC, identités, app settings, outputs) | Accorder des rôles Azure entre ressources, attacher des identités managées, définir des variables d'env dérivées d'outputs ou de Key Vault | ? | keep | La partie la plus saine du produit : RBAC, identités et app settings sont câblés de bout en bout jusqu'à la génération (`AppSettingsAnalysisStage`), avec tests. |
| F10b | Clés App Configuration | Définir des clés de configuration par environnement sur une ressource App Configuration | 🔴 | fix | CRUD complet, persisté, front câblé — et **jamais relu par la génération**. Voir D07. |
| F10c | Domaines personnalisés | Rattacher un domaine à une Web App / Function App / Container App, avec instructions DNS et validation | 🔴 | fix | La « validation DNS » ne vérifie rien. Voir D08. |
| F10d | Mappings de paramètres sécurisés | Dire quel variable group Azure DevOps fournit la valeur d'un paramètre Bicep sensible | ? | keep | Chaîne confirmée jusqu'à la génération **pipeline** uniquement (`GenerationRequestBuilder.BuildForPipeline`), pas dans la génération Bicep — cohérent avec l'objet de la feature. |
| F11 | Générer le Bicep (niveau projet, mono-repo) | Génère en une passe le Bicep de toutes les configs du projet : dossier `Common/` partagé + un dossier par config, stocké pour download | 🟡 | keep | Gère `AllInOne` et `SplitInfraCode`. **Refuse explicitement `MultiRepo`** (`GenerateProjectBicepCommandHandler.cs:45`, `Project.cs:477`) — le front est cohérent avec ce refus. Overrides par env perdus pour 2 types : voir D06. |
| F11b | Générer le Bicep (niveau config) | Génère le Bicep d'une seule config | 🟡 | fix | Endpoints exposés sans restriction serveur, mais le front ne les ouvre **que si le projet est MultiRepo** (`config-detail.component.html:20`). En `AllInOne`/`SplitInfraCode` : joignables en API directe, sans point d'entrée UI. Lié au chantier MultiRepo (D01/D09/D10) : c'est l'endpoint dont dépend son push. |
| F12 | Générer les pipelines (infra + applicatifs) | Produit les YAML CI/PR/Release infra et les wrappers applicatifs par ressource compute, au niveau config comme au niveau projet | 🟡 | keep | **Niveau config : les 3 layouts sont réellement gérés.** Niveau projet : `AllInOne` + `SplitInfraCode`, MultiRepo refusé explicitement (`Project.cs:477`), front cohérent. `AppPipelineMode.Combined` ne combine rien (voir défauts secondaires). |
| F13 | Générer le bootstrap | Produit `bootstrap.pipeline.yml`, pipeline idempotent qui provisionne pipelines, environnements, variable groups et service connections via `az devops` | 🟡 | fix | `AllInOne` : bootstrap `FullOwner`. `SplitInfraCode` : bootstrap `FullOwner` infra + `ApplicationOnly` code. `MultiRepo` : génération config-level accessible; validation réelle encore attendue. Voir D09. |
| F14 | Pousser dans git × 3 layouts | Pousser Bicep, pipelines et bootstrap dans les dépôts du client | 🟡 | fix | Push par config : les 3 layouts sont routés. Push projet : `AllInOne` et `SplitInfraCode` conservent leurs flux existants; `MultiRepo` dispose maintenant d'un bulk push par configuration, avec un commit indépendant par dépôt et résultats partiels explicites. Validation contre un vrai fournisseur Git encore attendue. |
| F15 | Références cross-config | Une ressource d'une config référence une ressource d'une autre config du même projet (déclaration Bicep `existing`) | 🟡 | fix | Chaîne complète jusqu'à la génération, mais seules 6 propriétés FK connues sont câblées à une expression Bicep (`ParentReferenceResolutionStage.cs:53-63`). Une référence sur tout autre type est stockée puis **silencieusement ignorée** — le fichier le documente lui-même ligne 15. Même motif que D07 (donnée saisie qui ne part nulle part) ; priorité plus basse que MultiRepo/D05/D08/D11. |

## ADJACENT

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F20 | Gestion du PAT git (endpoint dédié) | Stocker/renouveler le jeton d'un dépôt | 🟡 | cut | `PUT /projects/{id}/repositories/{repoId}/git-pat` est fonctionnel et testé, mais **le composant Angular qui l'appelle n'est ouvert par aucun bouton**. Chemin mort côté UI. Le vrai chemin est la modale de création/édition de dépôt — la gestion du PAT est déjà couverte ailleurs. |
| F22 | Diagnostics de configuration | Exécute des règles de diagnostic (ex. RBAC manquant) sur une config et retourne les constats | ? | keep | Chaîne complète, tests présents. Consommation front à confirmer. |
| F23 | Artefacts de la dernière génération | Retrouver les chemins des fichiers produits par la dernière génération sans regénérer (`GET /projects/{id}/latest-generation`) | ? | keep | Front câblé. Aucun test dédié trouvé pour `GetProjectLatestGenerationQueryHandler`. |
| F21 | Personal Access Tokens (API/MCP) | Créer, lister, révoquer des jetons avec scopes Read/Write/Generate | ? | keep | Seule chaîne trouvée **complète et cohérente** de bout en bout : handler d'authentification, behavior MediatR de vérification de scope, écran Angular, tests. |

## PÉRIPHÉRIQUE

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F30 | Import ARM — preview | Analyser un template ARM JSON, montrer ressources mappées / gaps / dépendances | 🟡 | keep | **Aucun écran front.** Seul consommateur : le serveur MCP (déjà `fix` via F32). Pas sur le golden path, priorité basse. |
| F31 | Import ARM — apply | Créer un projet complet depuis un preview | 🟡 | keep | Aucun front. Crée **toujours un nouveau projet** — impossible d'importer dans un projet existant. Seuls 20 types ARM mappés, le reste tombe en gap. Format `arm-json` uniquement, confirmé en code. Limite assumée pour l'instant, pas un bug à corriger — à revisiter si le besoin d'import dans un projet existant émerge. |
| F32 | Serveur MCP | Piloter le produit depuis un agent IA | 🟡 | fix | **4 classes d'outils sur 17 ne sont pas enregistrées** — voir D03. Aucun outil MCP ne couvre le networking. |
| F33 | Privatisation d'une ressource (V3) | Marquer une ressource comme accessible via Private Endpoint uniquement | 🔴 | fix | Chaîne V3 complète et cohérente (domaine → EF → read model → 3 étages Bicep), mais la bascule est indépendante de la config PE : une ressource peut être générée sans accès public **et** sans Private Endpoint. Voir D11. Toujours aucun test sur `ToggleResourcePrivatizationCommandHandler`. |
| F34 | Configuration Private Endpoint | Attacher VNet / subnet / mode DNS à une ressource privatisée | 🔴 | fix | Voir D02. |
| F35 | Profil réseau `NetworkingProfile` (V2) | Profil réseau au niveau `InfrastructureConfig` | 🔴 | **cut** | Code mort intégral — voir D04. |

---

## Défauts structurels confirmés

Vérifiés directement dans le code, pas seulement rapportés par un agent.

### D01 — Le layout MultiRepo ne peut pas pousser (bloquant produit)

Un dépôt déclaré au niveau `InfrastructureConfig` ne peut structurellement pas porter de PAT :

- `AddInfraConfigRepositoryRequest` n'a **aucun champ PAT**.
- `ProjectGitSecretNames.GetRepositoryPatSecretName(...)` prend un `ProjectRepositoryId` **typé** — il ne peut pas nommer le secret d'un dépôt de config.
- Les 4 seuls chemins qui écrivent un secret (`AddProjectRepositoryCommandHandler.cs:79`, `CreateProjectWithSetupCommandHandler.cs:174`, `SetProjectGitPatCommandHandler.cs:36`, `UpdateProjectRepositoryCommandHandler.cs:61`) passent tous par ce helper, donc écrivent tous sous le préfixe `git-pat-repo-`.
- `RepositoryTargetResolver.cs:73` pose `PatSecretName: null` pour les dépôts de config.
- **10 sites d'appel** compensent avec `target.PatSecretName ?? $"git-pat-{project.Id.Value}"` — préfixe `git-pat-`, que **rien n'écrit jamais**. Le repli pointe vers un secret fantôme.

Conséquence : en MultiRepo, test de connexion, listing de branches, push Bicep, push pipeline et push bootstrap échouent tous sur une erreur Key Vault.
Seul `MultiRepoProjectArtifactsPushService.cs:235` teste explicitement le cas et sort proprement.

**Statut [2026-08-28, dotnet-dev + angular-front] : PAT résolu, D01 fermé, back et front.** `AddInfraConfigRepositoryCommand`/`UpdateInfraConfigRepositoryCommand` portent désormais `PersonalAccessToken` (requis + vérifié via `ProjectRepositoryConnectionVerifier`, exactement comme au niveau projet) et écrivent le secret sous `ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(...)` (même préfixe `git-pat-repo-`, pas de collision — les deux types d'id sont des GUID globalement uniques). `RepositoryTargetResolver.BuildFromConfigRepository` (`RepositoryTargetResolver.cs:73`) ne pose plus `PatSecretName: null` — il retourne le vrai nom de secret, donc les 10 sites d'appel qui repliaient sur le secret fantôme `git-pat-{projectId}` retrouvent un vrai secret. Contracts + `ProjectController` mis à jour. Côté front, `infra-config-repository-dialog` affiche maintenant le champ PAT (déjà présent dans le form model partagé mais jamais rendu ni lu) et le transmet — toujours à la création, seulement si renseigné à l'édition (ne pas écraser le secret existant). Build 0 erreur, tests ajoutés (TDD), aucune régression sur la baseline connue. **Statut reste `🔴`/`🟡` (pas `🟢`) tant que le flux n'a pas été validé par exécution réelle contre un vrai fournisseur git** — le fix est vérifié par tests unitaires + relecture, pas encore par un push MultiRepo réel. Reste hors périmètre : D09 (bootstrap), D10 (push projet MultiRepo).

### D02 — L'onglet réseau échoue à chaque ouverture

`NetworkingProfileController` ne déclare que trois routes : `MapPut:27` (privatization), `MapPut:56` et `MapDelete:86` (private endpoint). **Aucun `MapGet`.**
Or `private-endpoint.service.ts:22` fait un `GET` sur `/infra-config/{id}/resources/{resourceId}/private-endpoint-config`, appelé au chargement de l'onglet réseau. Résultat : 404 systématique, avalé par un `.catch()` qui affiche un message d'erreur générique.

### D03 — 4 outils MCP sur 17 sont injoignables

17 classes portent `[McpServerToolType]`, 13 sont enregistrées via `.WithTools<T>()` dans `src/Mcp/InfraFlowSculptor.Mcp/Program.cs:34-46`.
Jamais enregistrées : `ArchitectureSuggestionTools`, `ContainerAppShortcutTools`, `ProjectQueryTools`, `RoleDefinitionTools`. Implémentées, compilées, invisibles pour tout client MCP.

### D04 — L'agrégat `NetworkingProfile` (V2) est orphelin

Domaine complet (agrégat, 8 value objects, entité d'override), configuration EF Core, `DbSet`, migration `20260528093816_AddNetworkingProfileV2` appliquée. Et **aucun repository, aucun handler, aucun contrôleur** ne le lit ni ne l'écrit.
Le dossier `Application/NetworkingProfiles/` ne contient que les 3 commandes **V3**, qui travaillent toutes sur `AzureResource` — le nom du dossier entretient la confusion.
`GenerationRequest.NetworkingProfile` existe aussi mais n'est jamais assigné.
La migration V2 précède la V3 d'un seul jour : V2 a été abandonné sans nettoyage.

### D05 — Deux cascades de nommage divergentes : l'écran ment sur le nom généré

Deux implémentations concurrentes résolvent le nom d'une ressource, et ce n'est pas la même :

- `ResourceNameResolver.ResolveTemplate` (`ResourceNameResolver.cs:75-138`), qui répond à `check-availability`, fait un **merge fin par type** : override config → override projet → défaut config → défaut projet, avec substitution par `AzureNamingConstraints.GetRecommendedTemplate` quand le type l'exige (ACR, StorageAccount…).
- `InfrastructureConfigReadRepository.BuildNamingContext` (`InfrastructureConfigReadRepository.cs:405-431`), qui alimente la génération Bicep réelle via `GenerationRequestBuilder.BuildCoreRequest:182-200`, fait un **switch tout-ou-rien** : si `UseProjectNamingConventions` est `false`, elle lit exclusivement la config, sans aucun repli sur le projet.

`GetRecommendedTemplate` n'a que **deux sites dans tout le dépôt** : sa définition et son seul appel, dans le résolveur de `check-availability`. Le garde-fou n'existe donc pas dans le chemin de génération.

Conséquence : pour un type non explicitement surchargé au niveau config, l'utilisateur voit un nom validé et disponible, et le Bicep généré porte un autre nom — potentiellement invalide pour Azure (tirets dans un nom d'ACR).

### D06 — `GetBaseModuleName` incomplet : des overrides par environnement disparaissent

`ResourceTypeMetadata.GetBaseModuleName` (`ResourceTypeMetadata.cs:89-114`) couvre 19 types ARM sur les 21 déclarés dans `AzureResourceTypes.ArmTypes`. Manquent `DocumentIntelligenceType` et `PrivateEndpointType`, qui tombent sur `_ => "unknown"`.

Or `DocumentIntelligenceTypeBicepGenerator.cs:16` nomme son module `"documentIntelligence"`. `ParameterFileAssembler.FindMatchingResource:100-112` construit un `expectedModuleName` préfixé `"unknown"` et ne matche jamais le module réel : les `DocumentIntelligenceEnvironmentSettings`, pourtant propagés génériquement par `GenerationRequestBuilder:228`, sont **silencieusement ignorés** à l'assemblage des fichiers de paramètres.

Le test `ResourceTypeMetadataTests.cs:112` fige `"unknown"` comme comportement attendu pour un type inconnu — il verrouille le bug au lieu de le détecter. Il n'existe aucun test d'exhaustivité de la table contre `AzureResourceTypes.ArmTypes`.

### D07 — Les clés App Configuration ne sortent jamais dans le Bicep

`AppConfigurationKeyController` est câblé de bout en bout : domaine, EF Core, front, tests. Mais `GenerationRequestBuilder.cs` ne référence jamais `AppConfigurationKey`, `InfrastructureConfigReadRepository.cs` ne les charge pas dans le read model de génération, et aucun générateur de `BicepGeneration` ne les lit. L'utilisateur saisit des clés qui sont persistées et ne partent nulle part — contrairement aux App Settings, eux repris (`GenerationRequestBuilder.cs:92-133`) et exploités par `AppSettingsAnalysisStage`.

### D08 — La « validation DNS » des domaines personnalisés ne valide rien

`CustomDomain.ValidateDns()` (`CustomDomain.cs:70`) est un `=> DnsValidationStatus = Validated`, sans condition. `ValidateCustomDomainDnsCommandHandler.cs:44` l'appelle directement. Aucun résolveur DNS n'existe dans `src/Api` pour ce chemin. Le bouton « Valider » du front (`resource-edit-custom-domains-section.controller.ts:118`) fait confiance à la déclaration de l'utilisateur.

Ce n'est pas cosmétique : `ParameterFileAssembler.cs:66-95` n'injecte les vraies valeurs de domaine dans le paramètre `customDomains` que pour les domaines `Validated`. Une déclaration fausse produit donc un binding de domaine généré sans qu'aucun contrôle n'ait eu lieu.

### D09 — Le bootstrap est inatteignable en MultiRepo

`GenerateProjectBootstrapPipelineCommandHandler.cs:41` et `PushProjectBootstrapPipelineToGitCommandHandler.cs:44` appellent tous deux `targetResolver.Resolve(project, config: null, ArtifactKind.Bootstrap)`. Or `RepositoryTargetResolver.cs:26-29` retourne `NoRepositoryConfigured` dès que `config is null` en MultiRepo. Le commentaire « AllInOne / MultiRepo: single bootstrap owns everything » (`GenerateProjectBootstrapPipelineCommandHandler.cs:136`) décrit une branche que ce layout n'atteint jamais.

Incohérence de pattern au passage : `GenerateProjectPipelineCommandHandler.cs:41` garde MultiRepo explicitement par le domaine (`Project.CanGenerateAllFromProjectLevel()`), le bootstrap laisse le résolveur échouer en aval.

**Statut [2026-08-31, architect (plan) + dotnet-dev + angular-front] : D09 fermé, back et front.** Diagnostic racine (agent `architect`) : contrairement à Bicep (F11b) et Pipeline (F12), le bootstrap n'avait qu'un seul niveau (projet) — pas de niveau config, alors que c'est justement le niveau config qui fonctionne nativement en MultiRepo (chaque `InfrastructureConfig` porte ses propres `InfraConfigRepository`, avec PAT désormais fonctionnel depuis D01). Correction : nouveau slice `InfrastructureConfig/Commands/GenerateBootstrap` + `PushBootstrapToGit` (+ Download/GetFileContent), symétrique à `GenerateBicep`/`GeneratePipeline`, réutilisant `IProjectBootstrapDefinitionBuilder.BuildAsync` tel quel (aucune modification) avec `configs: [configReadModel]` et `Mode = BootstrapMode.FullOwner`. Le bootstrap projet (`GenerateProjectBootstrapPipelineCommandHandler`/`PushProjectBootstrapPipelineToGitCommandHandler`) est maintenant gardé explicitement contre MultiRepo via `Project.CanGenerateAllFromProjectLevel()` (`Errors.GitRouting.AmbiguousProjectLevelGeneration`), au lieu de laisser le résolveur échouer silencieusement — même pattern que Bicep/Pipeline, incohérence corrigée. Front : 3ᵉ onglet « Bootstrap » dans l'écran de génération config, visible seulement en MultiRepo (même garde que F11b), bouton Push to Git dédié posant `isBootstrap: true`. `dotnet build` + `npm run build` : 0 erreur. `dotnet test` : 51 échecs = baseline exacte du 2026-08-28, aucune régression. **Statut reste `🔴`/`🟡` (pas `🟢`) tant que le flux n'a pas été validé par exécution réelle contre un vrai fournisseur git** — comme D01, le fix est vérifié par tests unitaires + relecture, pas encore par un push MultiRepo réel. Revalidation en conditions réelles différée jusqu'à D10 (push combiné, même campagne de validation). Détail complet : `docs/features/multirepo-bootstrap-d09-implementation-tracker.md`. **Défaut adjacent découvert pendant l'implémentation, hors périmètre D09 : voir D12.**

Le push config-level direct sélectionne uniquement le bucket `infra/` en `SplitInfraCode`, refuse
une cible sans secret au lieu d'utiliser un secret projet et propage l'annulation par Key Vault,
Blob et les providers Git.

### D10 — Le push projet MultiRepo était absent

Avant la réparation, `PushProjectArtifactsToMultiRepoCommandHandler.cs:33` rejetait tout layout
autre que `SplitInfraCode`, malgré le nom de la commande et de la route. Le vrai `MultiRepo`
n'avait donc aucun push bulk au niveau projet.

**Statut [2026-09-01] : implémenté côté backend, API et frontend, validation réelle restante.**
Un nouveau contrat `PushProjectMultiRepoArtifactsCommand` reçoit les cibles par
`InfrastructureConfig` et `InfraConfigRepository`. Le service charge les artefacts config-level,
déduit le rôle depuis `ConfigLayoutMode` et `ContentKinds`, prévalide toutes les cibles avant le
premier effet Git, puis pousse chaque dépôt avec son PAT config-level. `AllInOne` regroupe Bicep,
pipelines et bootstrap dans un commit par dépôt; `SplitInfraCode` sépare les artefacts infra et
applicatifs, y compris les bootstraps `FullOwner`/`ApplicationOnly`. Les erreurs Git sont renvoyées
par cible sans prétendre à une atomicité entre dépôts.

L'ancien flux est exposé sous `push-split-infra-code-artifacts-to-git`; le nouveau flux utilise
`push-multi-repo-artifacts-to-git`. Un dialogue frontend dédié alimente les dépôts de chaque
configuration. Les plans sont prévalidés avant le premier push, l'annulation arrête la séquence
sans tenter la cible suivante, et les tests automatisés et builds passent. Il reste à exécuter la
campagne sur un vrai fournisseur Git.

### D12 — Le bouton « Push to Git » unique de l'en-tête config ne pousse jamais le Pipeline

Découvert pendant l'implémentation de D09 (agent `angular-front`, 2026-08-31). Le bouton unique de `config-detail.component.html:29-36` construit un `PushToGitDialogData` qui n'active jamais `isPipeline: true` — il ne pousse donc que le Bicep, jamais le Pipeline, quel que soit l'onglet actif dans l'écran de génération. Non corrigé (hors périmètre D09) ; le nouvel onglet Bootstrap (D09) évite délibérément ce piège avec un bouton dédié posant `isBootstrap: true` explicitement, plutôt que de réutiliser le bouton d'en-tête ambigu. À trier : `fix` (le bouton devrait suivre l'onglet actif) ou retrait au profit de boutons dédiés par onglet, cohérent avec ce que fait déjà Bootstrap.

La collision de vocabulaire est un piège en soi : lire le code laisse croire que MultiRepo est couvert.

### D11 — Privatiser une ressource sans Private Endpoint la coupe du réseau

`AzureResource.Privatize()` (`AzureResource.cs:150`) met `IsPrivatized = true` sans exiger de `PrivateEndpointConfiguration`, et `ToggleResourcePrivatizationCommandHandler` l'appelle directement. Côté front, la bascule (`togglePrivatization`) est indépendante de la configuration VNet/subnet (`saveConfig`).

À la génération, les deux étages divergent : `PublicNetworkAccessStage.cs:23` désactive l'accès public de **toute** ressource `IsPrivatized`, tandis que `PrivateEndpointCompanionStage.cs:32` ne génère le Private Endpoint que si `PrivateEndpointConfig is not null`. Une ressource privatisée sans config est donc déployée sans accès public et sans point d'entrée privé.

## Défauts secondaires relevés

- `CreateProjectWithSetup` ne vérifie pas la connexion git avant de stocker le PAT, alors que `AddProjectRepository` vérifie les branches. Deux niveaux de rigueur pour la même opération.
- Composant Angular `ProjectGitPatDialogComponent` référencé nulle part hors de son propre spec.
- Pas de test pour `SearchCodeRepoDirectoriesQueryHandler` ni pour `ToggleResourcePrivatizationCommandHandler`.
- `ApplyImportPreviewCommandHandler` ne sait créer que de nouveaux projets.
- `ResolvedRepositoryTarget.BasePath` et `PipelineBasePath` sont **codés en dur à `null`** dans les deux constructeurs (`RepositoryTargetResolver.cs:51-73`), alors que leur propre XML doc affirme qu'ils sont peuplés selon l'`ArtifactKind`. Deux conséquences : `bicepBasePath` passé aux pipelines release est toujours `null` (`GeneratePipelineCommandHandler.cs:49-55`), et aucun sous-chemin de dépôt n'est jamais honoré. *Nuance vérifiée :* le nettoyage des fichiers obsolètes au push **fonctionne quand même**, car `MultiScopeGitPushRequestBuilder.SplitRootScopedPath:120` re-découpe par dossier de premier niveau ; seuls les fichiers générés **à la racine** du dépôt échappent au nettoyage.
- `AppPipelineGenerationEngine.GenerateCombined` (`AppPipelineGenerationEngine.cs:74-78,137-160`) ne combine rien : elle appelle `Generate()` par ressource et se contente de renommer les chemins de sortie. Son XML doc promet « a single CI + release pipeline with parallel jobs ».
- Deux enums `AppPipelineMode` concurrents (Domain vs GenerationCore), reliés par un `Enum.TryParse` sur chaîne avec repli silencieux sur `Isolated` (`ConfigPipelineGenerationService.cs:75-77`).
- `SetInfraConfigLayoutMode` est exposé sous le groupe de routes `Projects` (`ProjectController.cs:440`) alors qu'il mute un état de l'agrégat `InfrastructureConfig`.
- Références cross-config : `ParameterReferenceResolutionStage` documente ligne 15 que les références non résolues sont « silently dropped ». Seules 6 propriétés FK connues sont câblées (`ParentReferenceResolutionStage.cs:53-63`).
- `EventHubNamespace` persiste des `EnvironmentSettings` que le front n'expose jamais (absent de `RESOURCE_TYPES_WITH_ENVIRONMENT_SETTINGS`, `resource-type.metadata.ts:27-46`).
- `AddEventHubRequest` / `AddEventHubConsumerGroupRequest` sont déclarées dans `EventHubNamespaceController.cs:206-220` au lieu du projet `Contracts`.
- `check-availability` présente une résolution DNS publique (`DnsNameAvailabilityChecker.cs:36-77`) là où l'utilisateur peut comprendre « API Azure `checkNameAvailability` ». Résultat divergent possible selon firewall ou zone privée.
- Types morts confirmés, arbitrés en session de tri (voir « Décisions de tri » ci-dessous) : `NetworkSecurityGroup` ne survit que comme `Subnet.NsgId` + une interface Angular inutilisée — **conservé**, roadmap future ; `PrivateDnsZone` n'existe plus que dans d'anciennes migrations et une constante d'erreur jamais appelée — **à purger**, couvert par le V3 (`PrivateEndpointDnsMode`) ; `FrontDoor` n'apparaît nulle part dans le code, seulement dans la mémoire et `docs/features/` — **rien à purger**, roadmap future.
- Aucun test dédié pour `GetProjectLatestGenerationQueryHandler`.

## Décisions de tri — session 2026-08-28

Premiers arbitrages posés avec le porteur du projet (phase 2, ordre 1 à 3 de `NEXT.md`).

1. **Layout `MultiRepo` → `fix`, priorité produit forte.** Réparer D01 (PAT) + D09 (bootstrap) +
   D10 (push projet) plutôt que retirer le layout. Confirmé indispensable au produit.
2. **D05, D08, D11 → `fix`, confirmé.** Les trois mensonges à l'utilisateur (nom généré, DNS,
   privatisation) sont traités en priorité, quel que soit l'arbitrage MultiRepo.
3. **Code mort vs. roadmap future — arbitré ressource par ressource :**
   - **D04 / F35 — `NetworkingProfile` (V2) → `cut`.** Géré ailleurs : le V3
     (`AzureResource.IsPrivatized` + `PrivateEndpointConfiguration`) couvre déjà ce même concept,
     câblé jusqu'à la génération. L'agrégat V2 est un reste de migration jamais nettoyé.
   - **F20 — dialogue PAT git autonome → `cut`.** Géré ailleurs : la modale de création/édition
     de dépôt fait déjà tout le travail.
   - **`PrivateDnsZone` → `cut` (purge des résidus).** Géré ailleurs : sa fonction (résolution DNS
     pour un Private Endpoint) est reprise par `PrivateEndpointDnsMode` (V3, `AutoManaged` /
     `ExistingHub`). Résidus à purger au moment du repair : constante d'erreur jamais appelée,
     mentions dans d'anciennes migrations (ne pas réécrire l'historique de migration EF Core,
     seulement nettoyer ce qui est vivant).
   - **`NetworkSecurityGroup` → conservé, roadmap future.** Rien aujourd'hui ne permet d'éditer
     des règles NSG — ce n'est pas couvert ailleurs. Le porteur du projet confirme vouloir
     supporter cette ressource plus tard. Ne pas purger `Subnet.NsgId` ni y toucher avant que la
     feature soit reprise.
   - **`FrontDoor` → conservé, roadmap future.** N'existe plus du tout dans le code (seulement
     mentionné en mémoire/docs) — rien à purger. Confirmé comme ressource à supporter plus tard.

4. **Reste de la carte, arbitré en un seul passage (2026-08-28) :**
   - **`keep`** : F02 (membres/rôles), F03 (environnements de projet — vérifié en tri, chaîne
     CRUD complète back/front/MCP, tests présents), F04 (topologie des dépôts), F07 (config
     d'infra & resource groups), F22 (diagnostics), F30 et F31 (import ARM preview/apply —
     consommés uniquement par le MCP, hors golden path, priorité basse).
   - **`fix`** : F01 (assistant de création — ne vérifie pas la connexion git avant de stocker
     le PAT, même motif que D05/D08/D11), F11b (génération Bicep niveau config — lié au
     chantier MultiRepo), F15 (références cross-config — seules 6 propriétés FK câblées, le
     reste silencieusement ignoré, même motif que D07).

**Phase 2 (Tri) terminée.** Toutes les lignes de la carte portent une décision. Place à la
phase 3 (réparation), golden path d'abord.

**Note historique (trouvée en creusant le contexte de la question 3) :** ces trois ressources
(`NetworkSecurityGroup`, `PrivateDnsZone`, `FrontDoor`) ont existé en V1, complètes, puis ont été
**démolies volontairement** le 2026-05-28 (18 dossiers, ~37 fichiers) au profit du modèle V2/V3 de
privatisation, plus simple. Ce ne sont donc pas des features « oubliées en cours de route » : ce
sont des retraits délibérés — dont deux (NSG, FrontDoor) le porteur du projet souhaite réintroduire
plus tard sous une forme à concevoir. Voir `.claude/memory/03-domain-model.md`, section
« V1 Privatization Demolition ».

## Mémoire projet à corriger

`.claude/memory/` contenait des affirmations que le code contredit. **Les trois points ci-dessous
ont été corrigés le 2026-08-25.**

- ✅ `12-api-endpoints.md` annonçait `GET/PUT /infra-config/{id}/networking-profile` et un CRUD `/resources/{resourceId}/private-endpoints`. Ni l'un ni l'autre n'existe. Section remplacée par les 3 routes réelles (`private-endpoint-config` PUT/DELETE + `privatization` PUT), avec renvoi vers D04.
- ✅ `08-frontend.md` supposait des points d'entrée hérités vers la modale PAT autonome. `ProjectGitPatDialogComponent` n'est référencé nulle part hors de son fichier et de son spec — la phrase est corrigée et pointe vers F20.
- ✅ Le trou de PAT en MultiRepo (D01) est désormais consigné dans `07-bicep-generation.md`, section « Multi-Repo Git Routing ».
- ✅ `03-domain-model.md:29` décrivait `NetworkingProfile` comme l'agrégat réseau V2 **actif**, avec un pipeline Bicep qui l'exploiterait, et la V3 comme « commençant ». Le code montre l'inverse : les 3 étages (`NetworkingResolutionStage.cs:28`, `PrivateEndpointCompanionStage.cs:32`, `PublicNetworkAccessStage.cs:23`) ne lisent que `AzureResource.IsPrivatized` / `PrivateEndpointConfig`. La bascule V3 est terminée, seul le nettoyage de V2 ne l'a jamais été. Ligne corrigée avec renvoi vers D04 et D11.

## Motif transverse

**Hypothèse confirmée sur les 7 tranches.** Les défauts trouvés ne sont pas des features
« qui marchent mal ». Ce sont des features **jamais raccordées** : un maillon manquant à la
jointure entre deux couches — une route absente (D02), un secret jamais écrit (D01), un
enregistrement DI oublié (D03), un agrégat sans repository (D04), une table de correspondance
incomplète (D06), un read model qui ne charge pas ce que l'écran a saisi (D07). Chacune
compile, chacune a l'air complète en lecture.

Deux motifs s'ajoutent après les 5 dernières tranches :

1. **Deux implémentations du même concept, non synchronisées.** Le nommage (D05), les deux
   enums `AppPipelineMode`, les deux niveaux de génération Bicep. La divergence est invisible
   tant qu'on ne compare pas les deux chemins ligne à ligne.
2. **Le nom promet ce que le code ne fait pas.** `ValidateDns()` ne valide pas (D08),
   `GenerateCombined` ne combine pas, `PushProjectArtifactsToMultiRepo` refuse MultiRepo (D10),
   et le XML doc de `ResolvedRepositoryTarget` décrit des champs codés à `null`. Ici la
   documentation interne est activement trompeuse : la lire coûte plus qu'elle ne rapporte.

**Conséquence produit à retenir pour la session de tri :** le layout `MultiRepo` n'est pas
« partiellement supporté », il est **non livrable** — pas de PAT (D01), pas de bootstrap (D09),
pas de push projet (D10). C'est le premier arbitrage à poser.
