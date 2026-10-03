# 01 — Vision, périmètre et glossaire

> Statut : v0 rétro-spécification (code au 2026-10-02).

## 1. Le produit en une phrase

InfraFlowSculptor (IFS) permet de **modéliser une infrastructure Azure** une seule fois, puis de
**générer** le code Bicep, les pipelines Azure DevOps et le pipeline de bootstrap correspondants, et
de les **pousser** dans les dépôts git du client.

Positionnement retenu (mémoire projet, 2026-05-16) : produit « Azure-first » pour les équipes
plateforme qui veulent une source unique pour le Bicep, les livrables Azure DevOps et le pilotage
par agent IA (MCP). Le produit est destiné à la vente (décision de stabilisation, 2026-08).

## 2. Golden path

```
Modéliser  →  Générer le Bicep  →  Générer les pipelines  →  Pousser dans git
```

1. **Modéliser** : créer un projet, ses environnements et ses dépôts ; créer des configurations,
   des resource groups et des ressources ; les relier (identités, rôles, app settings, réseau).
2. **Générer le Bicep** : produire un `main.bicep`, des modules et un fichier de paramètres par
   environnement, par configuration ou pour tout le projet.
3. **Générer les pipelines** : produire les pipelines CI, PR et Release de l'infrastructure, les
   pipelines des applications et le pipeline de bootstrap qui prépare Azure DevOps.
4. **Pousser** : écrire ces fichiers dans les dépôts git déclarés, un commit par dépôt.

La **topologie des dépôts** (voir [04](04-topologie-et-depots.md)) est un axe transverse : elle
change la génération, les pipelines, le bootstrap et le push. Une fonctionnalité validée pour une
topologie ne dit rien des deux autres.

## 3. Acteurs

| Acteur | Description |
|---|---|
| Utilisateur authentifié | Personne connectée via Microsoft Entra ID. Voit uniquement les projets dont elle est membre. |
| Owner (propriétaire) | Membre d'un projet avec tous les droits, dont membres, dépôts, topologie, suppression. Le créateur du projet est Owner. |
| Contributor (contributeur) | Membre qui peut modéliser, générer et pousser. |
| Reader (lecteur) | Membre qui peut consulter. |
| Agent IA | Client MCP authentifié par un jeton d'accès personnel ; agit avec les droits de l'utilisateur, limités aux portées du jeton. |
| Client API | Script ou outil authentifié par jeton d'accès personnel ou jeton Entra. |

Détail des droits : [02](02-acces-et-droits.md).

## 4. Hiérarchie des objets

```
Utilisateur
└── Projet                     (membres, environnements, nommage, tags, variable groups, dépôts, topologie)
    └── Configuration          (nommage propre ou hérité, tags, dépôts si un dépôt par configuration)
        └── Resource group     (nom, région)
            └── Ressource      (un des 20 types ; réglages par environnement ; sous-ressources)
```

Une ressource peut référencer :
- d'autres ressources de la même configuration (plan App Service, identité, Key Vault…) ;
- des ressources d'autres configurations du même projet, via une **référence cross-config** ;
- des ressources « existantes » qui ne sont pas déployées par IFS.

## 5. Périmètre fonctionnel actuel

| Domaine | Inclus aujourd'hui | Document |
|---|---|---|
| Accès | Connexion Entra ID, rôles projet, jetons d'accès personnels (API, MCP) | 02 |
| Projet | CRUD, assistant de création, environnements, membres, tags, variable groups, agent pool | 03 |
| Dépôts | 3 topologies, dépôts projet et configuration, jetons git dans Key Vault, test de connexion, branches, recherche de fichiers | 04 |
| Configuration | CRUD, resource groups, tags, références cross-config, répartition infra/code par configuration | 05 |
| Nommage | Gabarit par défaut, gabarits par type, abréviations, à deux niveaux ; contraintes Azure ; disponibilité | 06 |
| Ressources | 20 types Azure avec réglages par environnement et sous-ressources | 07, 08 |
| Câblage | Identités managées, RBAC, app settings, clés App Configuration, paramètres sécurisés | 09, 10 |
| Réseau | VNet, subnets, privatisation par Private Endpoint, domaines personnalisés | 11 |
| Pipelines applicatifs | 7 stacks de build, options d'étapes, détection depuis le dépôt | 12 |
| Génération | Bicep, pipelines infra, pipelines applicatifs, bootstrap | 13, 14 |
| Livraison | Téléchargement zip, push git par topologie | 15 |
| Contrôle | 5 règles de diagnostic | 16 |
| Import | Template ARM JSON vers nouveau projet (sans écran) | 17 |
| Agent IA | Serveur MCP (29 outils exposés) | 18 |

### Hors périmètre aujourd'hui (constaté)

- Fournisseurs git autres que GitHub et Azure DevOps (GitLab et Bitbucket acceptés par un seul
  validateur, mais aucune implémentation).
- Pipelines GitHub Actions : seuls les pipelines Azure DevOps sont générés.
- Déploiement direct sur Azure : IFS ne déploie rien, il produit des fichiers.
- Lecture de l'état réel Azure (drift, import depuis un abonnement).
- Facturation, abonnements, quotas par client.
- Network Security Group, Front Door, Private DNS Zone comme ressources éditables (retirés en
  2026-05 ; NSG et Front Door prévus sur la roadmap).

## 6. Glossaire

| Terme | Définition |
|---|---|
| **Projet** | Conteneur racine : un produit ou un périmètre, son équipe, ses environnements, ses conventions et ses dépôts. |
| **Configuration** (InfrastructureConfig) | Ensemble de resource groups et de ressources qui produit son propre `main.bicep` et ses propres pipelines. Exemple : un socle partagé, un service. |
| **Environnement** | Cible de déploiement définie au niveau projet (dev, staging, prod…) avec région, abonnement, ordre et approbation. Toutes les configurations du projet déploient dans tous les environnements. |
| **Resource group** | Groupe de ressources Azure d'une configuration, avec un nom et une région. |
| **Ressource** | Instance d'un type Azure du catalogue, modélisée dans un resource group. |
| **Réglages par environnement** | Valeurs d'une ressource qui varient selon l'environnement (SKU, capacité, nombre d'instances…). |
| **Ressource existante** | Ressource déjà déployée hors IFS, déclarée pour être référencée sans être déployée. |
| **Référence cross-config** | Lien d'une configuration vers une ressource d'une autre configuration du même projet. |
| **Topologie** (LayoutPreset) | Répartition des fichiers générés entre les dépôts git : Tout-en-un, Infra/code séparés, Un dépôt par configuration. |
| **Dépôt projet** | Dépôt git déclaré au niveau du projet (topologies Tout-en-un et Infra/code séparés). |
| **Dépôt de configuration** | Dépôt git déclaré au niveau d'une configuration (topologie Un dépôt par configuration). |
| **Contenu d'un dépôt** (ContentKinds) | Infrastructure, Code applicatif, ou les deux. |
| **Gabarit de nommage** | Modèle de nom avec des jetons (`{name}`, `{env}`…) appliqué pour calculer le nom Azure. |
| **Abréviation** | Code court d'un type de ressource utilisé dans les noms (`kv`, `app`, `sql`…). |
| **App setting** | Variable d'environnement d'une ressource de calcul (Web App, Function App, Container App). |
| **Clé App Configuration** | Clé de configuration stockée dans une ressource App Configuration. |
| **Output** | Propriété d'une ressource exposée par la génération Bicep pour être lue par une autre (URI d'un Key Vault, chaîne de connexion…). |
| **Variable group** | Groupe de variables Azure DevOps déclaré au niveau projet, utilisable par les pipelines. |
| **Paramètre sécurisé** | Paramètre Bicep sensible dont la valeur est fournie par une variable de pipeline. |
| **Privatisation** | Désactivation de l'accès public d'une ressource au profit d'un Private Endpoint. |
| **Bootstrap** | Pipeline `bootstrap.pipeline.yml` à lancer une fois pour créer dans Azure DevOps les pipelines, environnements et variable groups attendus par les pipelines générés. |
| **Génération** | Production des fichiers (Bicep, pipelines, bootstrap) et leur stockage pour téléchargement ou push. |
| **Push** | Écriture des fichiers générés dans un dépôt git, sous forme d'un commit sur une branche. |
| **Jeton git** (PAT git) | Jeton d'accès du fournisseur git d'un dépôt, stocké dans Azure Key Vault. |
| **Jeton d'accès personnel IFS** (PAT IFS) | Jeton créé par un utilisateur pour appeler l'API ou le serveur MCP d'IFS. Ne pas confondre avec le jeton git. |
| **Diagnostic** | Constat automatique sur une configuration (rôle manquant, nom indisponible…). |
