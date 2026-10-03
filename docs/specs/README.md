# Spécifications fonctionnelles — InfraFlowSculptor

> **Statut : version 0 (rétro-spécification).** Ces documents décrivent ce que le code fait
> réellement à la date du 2026-10-02 (base : commit `e2346f43` de `feature/apps-pipeline`),
> domaine par domaine. Ils servent de base à relire puis à faire évoluer vers la cible produit.
>
> Ils ne décrivent **pas** l'implémentation (classes, couches, patterns) sauf quand une
> référence au code aide à vérifier une affirmation. Ils décrivent ce que l'utilisateur peut
> faire, les données manipulées, les règles appliquées et ce qui est produit.

## Pourquoi ce dossier

- `docs/stabilization/feature-map.md` dit **si** une feature marche et ce qu'on en garde.
- `.claude/memory/` dit **comment** le code marche (pour les agents).
- **`docs/specs/` dit quoi** : le comportement attendu, assez précis pour reconstruire le
  produit sur une base neuve sans relire le code.

## Sommaire

| # | Document | Contenu |
|---|---|---|
| 01 | [Vision, périmètre, glossaire](01-vision-et-perimetre.md) | Golden path, acteurs, vocabulaire commun |
| 02 | [Accès, utilisateurs et droits](02-acces-et-droits.md) | Authentification, rôles projet, jetons d'accès personnels |
| 03 | [Projets](03-projets.md) | Projet, assistant de création, environnements, membres, tags, variable groups, agent pool |
| 04 | [Topologie et dépôts git](04-topologie-et-depots.md) | Les 3 topologies, dépôts projet et configuration, jetons, test de connexion, navigation dans un dépôt |
| 05 | [Configurations d'infrastructure](05-configurations.md) | Configuration, resource groups, tags, références cross-config |
| 06 | [Nommage](06-nommage.md) | Gabarits, abréviations, contraintes Azure, calcul du nom, disponibilité |
| 07 | [Modèle commun des ressources](07-modele-commun-ressources.md) | Ce que toute ressource possède : nom, région, réglages par environnement, ressource existante, suppression |
| 08 | [Catalogue des ressources Azure](08-catalogue-ressources.md) | Les 20 types : propriétés, réglages par environnement, sous-ressources, valeurs autorisées |
| 09 | [Identités et accès RBAC](09-identites-et-rbac.md) | Identités managées, attributions de rôles, catalogue de rôles, analyse d'impact |
| 10 | [Configuration applicative](10-configuration-applicative.md) | App settings, clés App Configuration, paramètres sécurisés, outputs de ressources |
| 11 | [Réseau et exposition](11-reseau-et-exposition.md) | VNet et subnets, privatisation et Private Endpoint, domaines personnalisés |
| 12 | [Pipeline applicatif](12-pipeline-applicatif.md) | Construction et déploiement des applications : stacks, options d'étapes, détection automatique, image Docker |
| 13 | [Génération Bicep](13-generation-bicep.md) | Ce qui est généré, structure des fichiers, règles de génération |
| 14 | [Génération des pipelines et du bootstrap](14-generation-pipelines-bootstrap.md) | Pipelines infra CI/PR/Release, pipelines applicatifs, bootstrap Azure DevOps |
| 15 | [Livraison git](15-livraison-git.md) | Stockage des artefacts, téléchargement, push par topologie, résultat par dépôt |
| 16 | [Diagnostics](16-diagnostics.md) | Règles de contrôle d'une configuration |
| 17 | [Import ARM](17-import-arm.md) | Analyse d'un template ARM et création d'un projet |
| 18 | [Serveur MCP](18-serveur-mcp.md) | Pilotage du produit par un agent IA |
| 19 | [Interface et préférences](19-interface-et-preferences.md) | Navigation, accueil, favoris, récents, préférences, fonctions annoncées |
| 90 | [Écarts et dette fonctionnelle](90-ecarts.md) | Tout ce que le code fait mal, à moitié ou pas du tout, consolidé |
| A1 | [Annexe — carte des routes API](A1-routes-api.md) | Correspondance fonctionnalité → route HTTP |

## Conventions

### Identifiants

| Préfixe | Sens | Exemple |
|---|---|---|
| `RG-<DOM>-nn` | Règle de gestion : contrainte toujours vraie | `RG-PRJ-04` |
| `UC-<DOM>-nn` | Cas d'utilisation : un parcours avec un résultat | `UC-GEN-02` |
| `EC-<DOM>-nn` | Écart constaté : le code ne fait pas ce qu'il promet, ou fait autre chose | `EC-NOM-01` |
| `Q-<DOM>-nn` | Question ouverte : décision produit à prendre | `Q-DEP-03` |

Codes de domaine : `ACC` accès, `PRJ` projet, `DEP` dépôts, `CFG` configuration, `NOM` nommage,
`RES` ressources (commun), `CAT` catalogue, `RBA` RBAC, `APP` configuration et pipeline applicatifs,
`NET` réseau, `GEN` génération Bicep, `PIP` pipelines et bootstrap, `GIT` livraison, `DIA` diagnostics, `IMP` import, `MCP` serveur MCP, `UI` interface.

Les écarts déjà répertoriés dans `docs/stabilization/feature-map.md` gardent leur identifiant
d'origine entre parenthèses (`D05`, `F15`…).

### Niveau de certitude

Chaque affirmation est tirée du code. Quand le comportement dépend de l'exécution et n'a pas été
vérifié en conditions réelles, c'est indiqué par **(non vérifié à l'exécution)**.

### Ce que ces specs ne tranchent pas

Elles décrivent l'existant. Les choix de cible (garder, corriger, retirer) sont dans
`feature-map.md` (colonne Décision) et seront intégrés lors des révisions de ces documents.
Les sections « Questions ouvertes » listent ce qui demande une décision.

## Comment faire évoluer ces specs

1. Relire un domaine, corriger ce qui ne correspond pas à l'intention.
2. Transformer les écarts `EC-*` en règles cibles (`RG-*`) ou les retirer.
3. Ajouter les nouvelles fonctionnalités dans le domaine concerné, avec des identifiants neufs.
4. Passer le statut du document de « v0 rétro » à « v1 cible » quand il décrit le produit voulu.
