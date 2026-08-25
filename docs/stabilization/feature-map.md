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

## Avancement de la cartographie

| Tranche | État |
|---|---|
| 1. Projet & topologie dépôts/git | cartographiée |
| 2. InfraConfig, ResourceGroup & naming | à faire |
| 3. Catalogue des types de ressources Azure | à faire |
| 4. Câblage entre ressources | à faire |
| 5. Génération Bicep | à faire |
| 6. Pipelines, bootstrap & push git × layouts | à faire — **la plus importante** |
| 7. Périphérie : import ARM, MCP/PAT, networking | cartographiée |

---

## GOLDEN PATH

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F01 | Créer un projet (assistant) | Créer projet + topologie + environnements + dépôts en une fois | 🟡 | ? | L'assistant stocke le PAT **sans vérifier la connexion git**, alors que l'ajout de dépôt classique vérifie les branches avant sauvegarde. Un projet peut naître avec une URL ou un PAT invalides, sans erreur. |
| F02 | Membres et rôles projet | Ajouter/retirer un utilisateur, changer son rôle | ? | ? | Chaîne complète, tests présents. |
| F03 | Environnements de projet | Déclarer dev/preprod/prod avec localisation, abonnement, approbation | ? | ? | |
| F04 | Topologie des dépôts (`LayoutPreset`) | Choisir la répartition du code entre les dépôts git du client | ? | ? | Règles réellement validées dans le domaine pour les 3 presets. Changer de preset vide les dépôts déclarés. |
| F05 | Dépôts par config (MultiRepo) | Déclarer les dépôts au niveau `InfrastructureConfig` | 🔴 | fix | **Aucun PAT possible.** Voir D01. |
| F06 | Connexion git & navigation dépôt | Tester la connexion, lister les branches, chercher fichiers/dossiers du dépôt applicatif | 🟡 | fix | Fonctionne au niveau projet. Échoue systématiquement en MultiRepo (D01). |
| F07 | Config d'infra & resource groups | — | ? | ? | tranche 2 |
| F08 | Modéliser les ressources + settings par env | — | ? | ? | tranche 3 |
| F09 | Naming (cascade projet → config) | — | ? | ? | tranche 2 |
| F10 | Câbler les ressources (deps, app settings, RBAC, identités) | — | ? | ? | tranche 4 |
| F11 | Générer le Bicep | — | ? | ? | tranche 5 |
| F12 | Générer les pipelines applicatifs | — | ? | ? | tranche 6 |
| F13 | Générer le bootstrap | — | ? | ? | tranche 6 |
| F14 | Pousser dans git × 3 layouts | — | 🔴 | fix | MultiRepo cassé par D01. AllInOne / SplitInfraCode non cartographiés. |

## ADJACENT

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F20 | Gestion du PAT git (endpoint dédié) | Stocker/renouveler le jeton d'un dépôt | 🟡 | cut ? | `PUT /projects/{id}/repositories/{repoId}/git-pat` est fonctionnel et testé, mais **le composant Angular qui l'appelle n'est ouvert par aucun bouton**. Chemin mort côté UI. Le vrai chemin est la modale de création/édition de dépôt. |
| F21 | Personal Access Tokens (API/MCP) | Créer, lister, révoquer des jetons avec scopes Read/Write/Generate | ? | keep | Seule chaîne trouvée **complète et cohérente** de bout en bout : handler d'authentification, behavior MediatR de vérification de scope, écran Angular, tests. |

## PÉRIPHÉRIQUE

| # | Feature | Prétend faire | Statut | Décision | Note |
|---|---|---|---|---|---|
| F30 | Import ARM — preview | Analyser un template ARM JSON, montrer ressources mappées / gaps / dépendances | 🟡 | ? | **Aucun écran front.** Seul consommateur : le serveur MCP. |
| F31 | Import ARM — apply | Créer un projet complet depuis un preview | 🟡 | ? | Aucun front. Crée **toujours un nouveau projet** — impossible d'importer dans un projet existant. Seuls 20 types ARM mappés, le reste tombe en gap. Format `arm-json` uniquement, confirmé en code. |
| F32 | Serveur MCP | Piloter le produit depuis un agent IA | 🟡 | fix | **4 classes d'outils sur 17 ne sont pas enregistrées** — voir D03. Aucun outil MCP ne couvre le networking. |
| F33 | Privatisation d'une ressource (V3) | Marquer une ressource comme accessible via Private Endpoint uniquement | 🟡 | keep | Chaîne V3 complète et cohérente (domaine → EF → read model → 3 étages Bicep). Mais aucun test sur `ToggleResourcePrivatizationCommandHandler`. |
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

## Défauts secondaires relevés

- `CreateProjectWithSetup` ne vérifie pas la connexion git avant de stocker le PAT, alors que `AddProjectRepository` vérifie les branches. Deux niveaux de rigueur pour la même opération.
- Composant Angular `ProjectGitPatDialogComponent` référencé nulle part hors de son propre spec.
- Pas de test pour `SearchCodeRepoDirectoriesQueryHandler` ni pour `ToggleResourcePrivatizationCommandHandler`.
- `ApplyImportPreviewCommandHandler` ne sait créer que de nouveaux projets.

## Mémoire projet à corriger

`.claude/memory/` contient des affirmations que le code contredit. À traiter avant de refaire confiance à ces fichiers.

- `12-api-endpoints.md` annonce `GET/PUT /infra-config/{id}/networking-profile` et un CRUD `/resources/{resourceId}/private-endpoints`. **Ni l'un ni l'autre n'existe.** La route réelle est singulière (`private-endpoint-config`) et n'a que PUT/DELETE.
- `08-frontend.md:51` suppose des points d'entrée hérités vers la modale PAT autonome. Il n'en existe aucun.
- Aucun fichier mémoire ne mentionne le trou de PAT en MultiRepo (D01), alors que c'est bloquant.

## Motif transverse

Les défauts trouvés ne sont pas des features « qui marchent mal ». Ce sont des features
**jamais raccordées** : un maillon manquant à la jointure entre deux couches — une route
absente, un secret jamais écrit, un enregistrement DI oublié, un agrégat sans repository.
Chacune compile, chacune a l'air complète en lecture. Hypothèse à confirmer sur les
tranches restantes.
