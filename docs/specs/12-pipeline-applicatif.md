# 12 — Pipeline applicatif

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F12 (génération des pipelines, `keep`), F06 (navigation dans le dépôt de code, `fix`).
> La génération elle-même (fichiers, emplacements) est décrite en [14](14-generation-pipelines-bootstrap.md).

## 1. Objectif

Pour chaque application déployée par IFS (WebApp, FunctionApp, ContainerApp), produire les
pipelines Azure DevOps qui construisent, testent, analysent et déploient le code, environnement par
environnement. L'utilisateur décrit **comment** son application se construit ; IFS génère les YAML.

## 2. Champ d'application

| Type | Modes de déploiement |
|---|---|
| WebApp | `Code` (défaut) ou `Container` |
| FunctionApp | `Code` (défaut) ou `Container` |
| ContainerApp | `Container` uniquement |

Une ressource existante n'a pas de pipeline applicatif (RG-RES-07).

## 3. Données de build et de déploiement (sur la ressource)

| Champ | Applicable | Règles / défaut |
|---|---|---|
| Nom d'application | Tous | Nom du dossier des pipelines de l'application ; défaut : nom de la ressource. |
| Chemin du code source | Tous | Chemin relatif dans le dépôt de code. Sert de chemin de déclenchement de la CI ; vide ou `.` → `<config>/<ressource>`. |
| Commande de build | Mode Code | Facultative ; sinon déduite du profil de stack (§ 5). |
| Stack d'exécution + version | WebApp, FunctionApp | Voir [08](08-catalogue-ressources.md). Défaut dans la CI : `DOTNETCORE` / `8.0`. |
| Chemin du Dockerfile | Mode Container | Défaut : `Dockerfile`. Contexte de build = chemin du code source. |
| Registre de conteneurs | Mode Container | Ressource ContainerRegistry. |
| Mode d'authentification au registre | Mode Container | `ManagedIdentity` ou `AdminCredentials` ([10 § 6](10-configuration-applicative.md)). |
| Identité de tirage (AcrPull) | `ManagedIdentity` | UserAssignedIdentity ([09](09-identites-et-rbac.md), RG-RBA-04). |
| Nom de l'image | Mode Container | Défaut : nom de la ressource en minuscules. |
| Image validée | Mode Container | Indicateur stocké, fourni par l'écran. |
| Service connection du registre | ContainerApp, par environnement | Réglage par environnement ; la CI utilise celle du **premier** environnement. |

## 4. Options d'étapes de pipeline

| Groupe | Options |
|---|---|
| Stack | `Unknown`, `DotNet`, `NodeJs`, `Angular`, `Java`, `Python`, `Php`, `Go`, `StaticSite`, `Custom` + profil (§ 5). |
| Tests unitaires | Activer ; commande ; framework ; format des résultats ; chemin des résultats ; publier les résultats. |
| Couverture | Publier ; outil ; chemin du rapport. |
| Sonar | Activer ; clé de projet ; organisation ; service connection. |
| Lint | Activer ; commande. |
| Analyse des dépendances | Activer ; outil (OWASP Dependency Check, npm audit…). |
| Divers | Validation du build ; cache des dépendances ; smoke tests + commande. |

**UC-APP-14 — Configurer le pipeline applicatif** (Contributor), dans l'onglet « Pipeline
applicatif » de la ressource.

**RG-APP-10 — Profil cohérent.** Le profil de stack doit correspondre à la stack choisie, sinon refus.

## 5. Profils de stack

| Profil | Réglages (défaut) | Commandes déduites quand le champ est vide |
|---|---|---|
| DotNet | Framework de test (xUnit), couverture (non), glob des projets de test | Gérées par les modèles .NET. |
| NodeJs | Gestionnaire de paquets (npm), framework (Jest), lint (non), scripts `test` / `lint` | Installation adaptée au gestionnaire. |
| Angular | Gestionnaire (npm), `ng test` (oui), `ng lint` (non), build production (oui), projet | `ng build [--configuration production] [--project]`, `ng test --no-watch --code-coverage --browsers=ChromeHeadless`, `ng lint`. |
| Java | Outil (Maven), framework (JUnit 5), couverture (non) | `mvn clean package -DskipTests` / `gradle build -x test` ; `mvn test` / `gradle test` ; JaCoCo si couverture. |
| Python | Gestionnaire (pip), framework (pytest), couverture (non) | `pip install -r requirements.txt` (poetry, uv) ; `python -m pytest --junitxml=results.xml [--cov]`. |
| StaticSite | Commande de build, dossier de sortie (tous deux obligatoires) | Commande de build reprise. |
| Custom | Commandes de test, lint, build | Commandes reprises. |

Php et Go n'ont pas de profil.

## 6. Détection automatique des options

**UC-APP-15 — Détecter les options depuis le dépôt de code.** IFS lit le dépôt de code applicatif
(branche, chemin du code source) et propose des valeurs :

| Stack | Détection |
|---|---|
| DotNet | Projet `*Test*.csproj` → framework (lu dans le projet), commande `dotnet test` avec TRX + couverture Cobertura ; `.editorconfig` → `dotnet format --verify-no-changes` ; outil de dépendances : OWASP. |
| Node / Angular | `package.json` : vitest / jest / mocha → commande et format JUnit ; eslint ou prettier → commande de lint ; outil : npm audit. |
| Python | `requirements.txt` / `pyproject.toml` : pytest ; ruff / flake8 / pylint. |
| Java | Maven / Gradle (fichiers de build). |
| Toutes | `sonar-project.properties` → Sonar détecté et clé de projet extraite. |

L'écran propose ces valeurs ; l'utilisateur les accepte ou les modifie.

**UC-APP-16 — Parcourir le dépôt de code** : chercher des fichiers et des dossiers du dépôt
applicatif (pour choisir le chemin du code source ou du Dockerfile).

## 7. Pipelines produits par application

Trois fichiers par application : `ci.app-pipeline.yml`, `pr.app-pipeline.yml`,
`release.app-pipeline.yml`. Chacun **étend** un modèle partagé du dossier `Common/pipelines`.

### 7.1 CI

- Déclenchement : branches `main` et `release/*`, sur le chemin du code source, le dossier commun et le
  dossier de l'application.
- Nom de build : `AAAAMMJJ.r`.
- **Mode Container** : build et push de l'image (buildx) avec le tag `{buildNumber}-{shortSha}`,
  connexion au registre, scans de sécurité (Trivy, SBOM Syft) **toujours activés**.
- **Mode Code** : installation du SDK de la stack, build, tests, publication de l'artefact.
- Étapes optionnelles selon le § 4 : tests et résultats, couverture, Sonar, lint, analyse des
  dépendances, cache, validation du build.
- Variable groups : ceux du projet, résolus pour le premier environnement.
- Agent pool : celui du projet, sinon `ubuntu-latest`.

### 7.2 PR

Pipeline de validation sur pull request (build et vérifications, sans déploiement).

### 7.3 Release

- Un stage `Deploy_<env>` par environnement, dans l'ordre, verrouillage séquentiel.
- Variables de l'environnement et variable groups résolus pour cet environnement.
- **Mode Container** : promotion de l'image vers le registre de l'environnement (stratégie
  `AcrImport`, fixe), puis déploiement (Azure CLI pour ContainerApp, tâche App Service pour
  WebApp / FunctionApp en conteneur).
- **Mode Code** : déploiement du paquet (`AzureWebApp@1`, `AzureFunctionApp@2`).
- Smoke tests si activés.
- Le nom de la définition CI attendue est noté en tête de fichier (la release consomme les artefacts de
  la CI).

## 8. Mode de pipelines d'une configuration

| Mode | Sens annoncé |
|---|---|
| `Isolated` (défaut) | Un jeu de pipelines par application. |
| `Combined` | Un pipeline commun pour toutes les applications de la configuration. |

## 9. Écarts constatés

- **EC-APP-07 — `Combined` ne combine rien.** Le mode génère les mêmes pipelines par application et
  ne change que les chemins de sortie. Il est en outre impossible à choisir à l'écran (EC-CFG).
- **EC-APP-08 — Deux énumérations du mode.** Le mode est converti par son libellé, avec un repli
  silencieux sur `Isolated`.
- **EC-APP-09 — Options générées mais pas saisissables.** Le moteur sait produire une vérification des
  licences, des notifications par webhook, un motif de tag d'image et une stratégie de promotion
  `TagOnly` ; aucune de ces options n'est stockée ni modifiable (valeurs fixes).
- **EC-APP-10 — Scans de sécurité forcés.** Trivy et Syft sont toujours activés en mode Container,
  sans option.
- **EC-APP-11 — Détection partielle.** Pas de détection pour Php, Go, StaticSite, Custom ; la
  détection lit **tout** le dépôt puis filtre par chemin.
- **EC-APP-12 — « Image validée » non vérifiée.** L'indicateur est fourni par l'écran ; le serveur ne
  vérifie pas l'existence de l'image (**non vérifié côté écran**).
- **EC-APP-13 — Premier environnement implicite.** La CI prend le premier environnement (ordre de la
  liste) pour la service connection du registre et les variable groups, sans que ce choix soit
  explicite.
- **EC-APP-14 — Navigation de dépôt au niveau projet seulement** (F06) : en MultiRepo, la
  navigation dans le dépôt applicatif d'une configuration n'est pas complète.

## 10. Questions ouvertes

- **Q-APP-05** — Rendre configurables la promotion (`TagOnly` / `AcrImport`), le tag d'image, les
  scans, les licences et les notifications ?
- **Q-APP-06** — Implémenter réellement `Combined` (un pipeline multi-jobs) ou le retirer ?
- **Q-APP-07** — Désigner explicitement l'environnement de build (au lieu du premier) ?
- **Q-APP-08** — Ajouter des profils Php et Go, et la détection associée ?
- **Q-APP-09** — Supporter GitHub Actions en plus d'Azure DevOps ?
