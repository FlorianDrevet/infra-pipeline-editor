# 14 — Génération des pipelines et du bootstrap

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F12 (pipelines, `keep`), F13 (bootstrap, `fix`), D09 (bootstrap MultiRepo, corrigé,
> validation réelle restante).
> Le contenu des pipelines applicatifs est décrit en [12](12-pipeline-applicatif.md).

## 1. Objectif

Produire, pour Azure DevOps :
- les **pipelines d'infrastructure** de chaque configuration (CI, PR, Release du Bicep) ;
- les **pipelines applicatifs** de chaque application ([12](12-pipeline-applicatif.md)) ;
- un **pipeline de bootstrap** idempotent qui crée dans Azure DevOps tout ce dont ces pipelines ont
  besoin : définitions de pipelines, environnements, variable groups, et vérifie les service
  connections.

## 2. Niveaux de génération

| Artefact | Niveau projet | Niveau configuration |
|---|---|---|
| Pipelines (infra + applicatifs) | AllInOne, SplitInfraCode. Refusé en MultiRepo. | Les 3 topologies. |
| Bootstrap | AllInOne, SplitInfraCode. Refusé explicitement en MultiRepo. | Les 3 topologies (mode `FullOwner`). |

**UC-PIP-01 — Générer les pipelines** (Contributor / PAT Generate).
**UC-PIP-02 — Générer le bootstrap.**
**UC-PIP-03 — Télécharger** les fichiers générés, **lire un fichier**.

L'écran n'ouvre la génération par configuration (onglets Bicep, Pipeline, Bootstrap) qu'en
MultiRepo.

## 3. Pipelines d'infrastructure (par configuration)

| Fichier | Rôle |
|---|---|
| `ci.pipeline.yml` | Valide (lint de chaque fichier Bicep) puis construit et publie l'artefact `drop` (fichiers Bicep et paramètres). |
| `pr.pipeline.yml` | Validation sur pull request. |
| `release.pipeline.yml` | Déclenchement manuel (`trigger: none`), consomme l'artefact de la CI. Un stage `Deploy_<env>` par environnement, sur l'environnement Azure DevOps du même nom, déploiement ARM avec `main.<env>.bicepparam`, lecture des outputs du déploiement. |
| `variables/vars.yml` | Variables de la configuration (hors mono-dépôt). |
| `variables/<env>.yml` | Variables par environnement : service connection ARM, abonnement, région (hors mono-dépôt). |

En mono-dépôt (AllInOne, SplitInfraCode au niveau projet), des modèles partagés sont produits sous
`.azuredevops/` (`pipelines/`, `jobs/`, `steps/`, `variables/<env>.variables.yml`) et chaque
configuration a son dossier.

**RG-PIP-01 — Paramètres sécurisés.** La release surcharge les paramètres sensibles par les variables
de pipeline associées ([10 § 6](10-configuration-applicative.md)).

**RG-PIP-02 — Variable groups.** Les variable groups du projet sont référencés ; `{env}` est remplacé
par le nom court de l'environnement. Un nom vide, sur plusieurs lignes ou contenant une expression
`${{ }}` est refusé.

**RG-PIP-03 — Agent pool.** Celui du projet, sinon `ubuntu-latest`.

### 3.1 Noms des définitions Azure DevOps

| Pipeline | Nom |
|---|---|
| Infra | `[Infra] <config> - CI`, `- PR`, `- Release` |
| Application | `[Code] <config> - <ressource> - CI`, `- PR`, `- Release` |

Dossier Azure DevOps : `\<config>` pour l'infra, un sous-dossier par application.

## 4. Bootstrap

Fichier `bootstrap.pipeline.yml`, à la racine du dépôt cible, déclenchement manuel. Il utilise le
jeton du pipeline (`System.AccessToken`) et la CLI `az devops` / l'API REST Azure DevOps.

### 4.1 Modes

| Mode | Quand | Ce qu'il fait |
|---|---|---|
| `FullOwner` | AllInOne ; dépôt infra en SplitInfraCode ; chaque configuration en MultiRepo. | Crée environnements, variable groups, définitions de pipelines ; vérifie les service connections. |
| `ApplicationOnly` | Dépôt de code en SplitInfraCode. | Vérifie que environnements et variable groups existent déjà, puis crée seulement les définitions des pipelines applicatifs. |

### 4.2 Jobs (dans l'ordre)

| Job | Condition | Effet |
|---|---|---|
| ValidateSharedResources | `ApplicationOnly` | Échoue si un environnement ou un variable group attendu manque. |
| ValidateServiceConnections | Service connections attendues | Échoue si une service connection n'existe pas. |
| ProvisionPipelineDefinitions | Pipelines à créer | Crée chaque définition absente (nom, chemin YAML, dossier, branche par défaut). |
| ProvisionEnvironments | `FullOwner` et environnements | Crée chaque environnement absent. |
| ProvisionVariableGroups | `FullOwner` et variable groups | Crée chaque groupe absent, ajoute les variables manquantes (secrètes vides pour les paramètres sécurisés), retire la variable `PLACEHOLDER`. |
| NothingToProvision | Aucun autre job | Job vide. |

**RG-PIP-04 — Idempotent.** Chaque création vérifie d'abord l'existence ; relancer le bootstrap ne
duplique rien et ne modifie pas l'existant.

**RG-PIP-05 — Environnements.** Ceux du projet, dans leur ordre, dédoublonnés par nom court (en
minuscules) ; à défaut, ceux des configurations.

**RG-PIP-06 — Variable groups.** Un groupe par environnement si le nom contient `{env}` ; variables
issues des associations de paramètres sécurisés.

**RG-PIP-07 — Service connections attendues.** Une `AzureRM` par environnement qui en déclare une ;
une `DockerRegistry` par réglage d'environnement de ContainerApp qui en déclare une.

## 5. Écarts constatés

- **EC-PIP-01 — Approbations non créées.** Un environnement « avec approbation » est créé sans
  contrôle d'approbation ; le bootstrap affiche seulement un message demandant de l'ajouter à la
  main (les approbateurs ne sont pas modélisés).
- **EC-PIP-02 — Service connections non créées.** Elles sont seulement vérifiées : il faut les créer
  à la main avant le bootstrap.
- **EC-PIP-03 — Valeurs des variables vides.** Les variables secrètes sont créées vides ; il faut les
  renseigner dans Azure DevOps.
- **EC-PIP-04 — Pas de mise à jour.** Une définition ou un groupe existant n'est jamais mis à jour
  (renommage, changement de chemin YAML).
- **EC-PIP-05 — Chemins de base ignorés.** Le sous-chemin de dépôt (`BasePath`) n'est jamais
  renseigné : le chemin Bicep passé aux releases est toujours vide et les sous-dossiers de dépôt ne
  sont pas honorés.
- **EC-PIP-06 — Validation MultiRepo réelle** non faite (D09, D10).
- **EC-PIP-07 — Azure DevOps uniquement.** Pas de GitHub Actions, alors que GitHub est un fournisseur
  git supporté ([15](15-livraison-git.md)).
- **EC-PIP-08 — Pas de what-if** dans la release d'infrastructure avant déploiement
  (**non vérifié dans tous les modèles**).

## 6. Questions ouvertes

- **Q-PIP-01** — Modéliser les approbateurs et créer les contrôles d'approbation ?
- **Q-PIP-02** — Créer les service connections (fédération d'identité de charge de travail) ?
- **Q-PIP-03** — Mettre à jour les définitions existantes quand le modèle change ?
- **Q-PIP-04** — Générer des workflows GitHub Actions ?
- **Q-PIP-05** — Ajouter une étape what-if avec validation manuelle avant le déploiement ?
