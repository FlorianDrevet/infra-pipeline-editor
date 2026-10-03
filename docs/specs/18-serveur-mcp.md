# 18 — Serveur MCP (pilotage par un agent IA)

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F32 (`fix`, D03), F21 (PAT, `keep`).

## 1. Objectif

Permettre à un agent IA (Claude, Copilot…) de créer et modifier des projets IFS en langage naturel,
par le protocole MCP, avec les mêmes règles métier que l'écran.

## 2. Accès

**RG-MCP-01 — Authentification par PAT IFS** ([02](02-acces-et-droits.md)) : chaque appel porte un
jeton `ifs_…` ; ses portées (Read, Write, Generate) et le rôle de l'utilisateur sur le projet
s'appliquent.

**RG-MCP-02 — Transport HTTP sans état.**

**RG-MCP-03 — Limitation de débit** par fenêtre fixe de 60 s : 100 appels (global), 10 (appels MCP,
politique « coûteuse »), 30 (contrôles de santé). Pas de file d'attente.

## 3. Outils exposés

| Groupe | Outils |
|---|---|
| Découverte | `list_repository_topologies`, `list_supported_resource_types` |
| Brouillon de projet | `draft_project_from_prompt` (extrait l'intention d'une demande libre, liste ce qui manque et les questions à poser, ne crée rien), `validate_project_draft` (applique des compléments, renvoie champs manquants / erreurs / avertissements) |
| Création | `create_project_from_draft` (brouillon au statut `ReadyToCreate`) |
| Projet | `add_project_environment`, `set_git_configuration` |
| Infrastructure | `create_infrastructure_config`, `create_resource_group`, `add_cross_config_reference` |
| Ressources | `create_resource`, `set_resource_environment_settings`, `add_blob_container`, `add_custom_domain` |
| Accès | `add_role_assignment` |
| App settings | `add_app_setting`, `add_output_reference_app_setting`, `add_keyvault_secret_app_setting`, `list_app_settings`, `remove_app_setting` |
| Nommage | `set_project_naming_template`, `set_project_resource_naming_template`, `remove_project_resource_naming_template`, `set_project_resource_abbreviation`, `remove_project_resource_abbreviation` |
| Génération | `generate_project_bicep` |
| Import | `preview_iac_import`, `apply_import_preview` ([17](17-import-arm.md)) |

**Ressource MCP** : `ifs://projects/{projectId}/summary` (synthèse d'un projet), aperçus d'import.
**Prompt MCP** : `project_creation_guide` (parcours guidé de création).

**RG-MCP-04 — Brouillons et aperçus en mémoire.** Les brouillons de projet et les aperçus d'import sont
gardés en mémoire du serveur, avec un nombre limité et une expiration ; ils sont perdus au
redémarrage.

## 4. Parcours type

1. L'agent envoie la demande de l'utilisateur à `draft_project_from_prompt`.
2. Il pose à l'utilisateur les questions renvoyées, puis appelle `validate_project_draft` jusqu'au
   statut `ReadyToCreate`.
3. `create_project_from_draft`, puis ajout des ressources, rôles, app settings.
4. `generate_project_bicep`.

## 5. Écarts constatés

- **EC-MCP-01 (D03) — 4 groupes d'outils non enregistrés**, donc invisibles :
  `suggest_architecture`, `link_container_app_to_acr` / `set_container_app_docker_config`,
  `get_project_structure` / `list_project_resources`, `list_available_role_definitions`.
- **EC-MCP-02 — Couverture partielle.** Pas d'outil pour : réseau et Private Endpoints, identités
  user-assigned, clés App Configuration, paramètres sécurisés, pipelines (options, génération),
  bootstrap, push git, diagnostics, membres, suppression / modification de la plupart des objets.
- **EC-MCP-03 — Génération Bicep projet seulement** (refusée en MultiRepo, comme l'écran).
- **EC-MCP-04 — Brouillons non persistés** (RG-MCP-04) : non partagés entre plusieurs instances.
- **EC-MCP-05 — Lecture limitée** : sans les outils de requête (EC-MCP-01), l'agent ne peut lire un
  projet que par la ressource `summary`.

## 6. Questions ouvertes

- **Q-MCP-01** — Couvrir tout le produit (parité avec l'API), ou un sous-ensemble choisi ?
- **Q-MCP-02** — Générer automatiquement les outils MCP depuis l'API (OpenAPI) ?
- **Q-MCP-03** — Persister les brouillons ?
- **Q-MCP-04** — Authentification OAuth (Entra) en plus des PAT ?
