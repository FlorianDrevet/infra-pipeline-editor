# 07 — Modèle commun des ressources

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Le détail de chaque type est dans [08](08-catalogue-ressources.md).

## 1. Objectif

Décrire ce que **toute** ressource possède et les règles qui s'appliquent quel que soit son type :
identification, région, réglages par environnement, ressource existante, liaisons, suppression.

## 2. Catalogue des types

20 types sont modélisables, regroupés ici par catégorie (la catégorie est une convention
d'affichage, pas une donnée) :

| Catégorie | Types |
|---|---|
| Calcul | AppServicePlan, WebApp, FunctionApp, ContainerAppEnvironment, ContainerApp |
| Données | StorageAccount, SqlServer, SqlDatabase, CosmosDb, RedisCache |
| Messagerie | ServiceBusNamespace, EventHubNamespace |
| Sécurité | KeyVault, UserAssignedIdentity |
| Observabilité | LogAnalyticsWorkspace, ApplicationInsights |
| Réseau | VirtualNetwork |
| Plateforme | ContainerRegistry, AppConfiguration |
| IA | DocumentIntelligence |

Le catalogue est identique côté serveur et côté écran.

## 3. Données communes

| Champ | Type / valeurs | Obligatoire | Règles |
|---|---|---|---|
| Nom logique | Texte | Oui | Saisi par l'utilisateur. Le nom Azure est calculé (voir [06](06-nommage.md)). Pas d'unicité imposée (EC-RES-01). |
| Type | Un des 20 types | Oui | Fixé à la création. |
| Resource group | Référence | Oui | Peut changer (déplacement prévu par le modèle, **sans action exposée**). |
| Région | Une des 31 régions | Oui | Indépendante de celle du resource group (EC-CFG-02). |
| Ressource existante | Booléen | Non (défaut : non) | Fixé à la création. Section 6. |
| Privatisée | Booléen | — | Voir [11](11-reseau-et-exposition.md). |
| Configuration Private Endpoint | VNet, subnet, mode DNS | — | Voir [11](11-reseau-et-exposition.md). |
| Identité managée attribuée | Référence à une UserAssignedIdentity | Non | Voir [09](09-identites-et-rbac.md). |
| Attributions de rôles | Liste | — | Voir [09](09-identites-et-rbac.md). |
| App settings | Liste | — | Calcul uniquement. Voir [10](10-configuration-applicative.md). |
| Domaines personnalisés | Liste | — | Calcul uniquement. Voir [11](11-reseau-et-exposition.md). |
| Correspondances de paramètres sécurisés | Liste | — | Voir [10](10-configuration-applicative.md). |
| Réglages par environnement | Un enregistrement par environnement | Non | Section 4. |
| Options de pipeline applicatif | — | — | Calcul uniquement. Voir [12](12-pipeline-applicatif.md). |

### 3.1 Régions supportées (31)

`francecentral`, `francesouth`, `uksouth`, `westeurope`, `germanywestcentral`, `switzerlandnorth`,
`italynorth`, `northeurope`, `spaincentral`, `norwayeast`, `polandcentral`, `swedencentral`,
`qatarcentral`, `uaenorth`, `canadaeast`, `canadacentral`, `eastus`, `eastus2`, `centralindia`,
`centralus`, `southcentralus`, `westus2`, `southafricanorth`, `westus3`, `westus`, `koreacentral`,
`brazilsouth`, `eastasia`, `japaneast`, `southeastasia`, `australiaeast`.

Région de repli si une valeur n'est pas reconnue : `westeurope`.

## 4. Réglages par environnement

**RG-RES-01 — Un jeu par environnement.** Pour les types qui en ont, une ressource porte au plus
un jeu de réglages par environnement du projet. Le jeu est identifié par le **nom** de
l'environnement (voir RG-PRJ-09 et EC-PRJ-03).

**RG-RES-02 — Réglage absent = valeur par défaut.** Chaque réglage est facultatif. Un réglage non
renseigné n'est pas écrit dans le fichier de paramètres de cet environnement ; la valeur par défaut
du module Bicep s'applique.

**RG-RES-03 — Saisie à la création ou après.** Les réglages peuvent être saisis à la création de la
ressource (étape « environnements » de l'ajout) ou modifiés ensuite (onglet « Environnements »).
Une modification complète remplace l'ensemble des jeux.

**RG-RES-04 — Destination.** Les réglages d'un environnement sont écrits dans
`main.<nomCourt>.bicepparam` (voir [13](13-generation-bicep.md)).

Types **sans** réglages par environnement : UserAssignedIdentity. EventHubNamespace en a côté
serveur mais l'écran ne les propose pas (EC-RES-03).

## 5. Cycle de vie commun

**UC-RES-01 — Ajouter une ressource** (Contributor). Parcours à l'écran :
1. choix du type dans le catalogue ;
2. saisie des propriétés du type (nom logique, région, liaisons obligatoires du type) ;
3. option « ressource existante » ;
4. réglages par environnement (étape sautée pour une ressource existante et pour les types sans
   réglages).

**UC-RES-02 — Modifier une ressource** (Contributor). Propriétés du type, réglages par
environnement, liaisons (onglets, section 7).

**UC-RES-03 — Supprimer une ressource** (Contributor). Section 8.

**UC-RES-04 — Consulter une ressource.** Lecture avec ses réglages, liaisons et noms calculés.

## 6. Ressource existante

Une ressource existante représente une ressource déjà déployée hors IFS, déclarée pour que d'autres
ressources puissent s'y relier.

**RG-RES-05 — Gelée.** Pour une ressource existante, seuls le nom et la région sont modifiables ;
les autres propriétés et les réglages par environnement sont ignorés silencieusement.

**RG-RES-06 — Pas de domaine.** On ne peut pas ajouter de domaine personnalisé à une ressource
existante (erreur explicite).

**RG-RES-07 — Pas déployée.** Une ressource existante n'est pas déployée par le Bicep : elle y est
déclarée en `existing` pour être référencée. Elle n'a ni pipeline applicatif ni entrée dans le
bootstrap.

**RG-RES-08 — Écran.** Pour une ressource existante, l'écran masque les onglets « Environnements »,
« Pipeline applicatif » et « Réseau », et affiche un bandeau d'information.

## 7. Onglets de l'éditeur de ressource (constat de l'écran)

| Onglet | Affiché pour |
|---|---|
| Général | Tous |
| Environnements | Tous sauf UserAssignedIdentity et ressource existante |
| Identité & accès | Tous sauf UserAssignedIdentity |
| Stockage (conteneurs, files, tables) | StorageAccount |
| App settings | WebApp, FunctionApp, ContainerApp |
| Clés de configuration | AppConfiguration |
| Droits accordés, Utilisée par | UserAssignedIdentity |
| Pipeline applicatif | WebApp, FunctionApp, ContainerApp (non existantes) |
| Réseau | Types privatisables (voir [11](11-reseau-et-exposition.md)), non existants |

Les domaines personnalisés sont proposés pour WebApp, FunctionApp, ContainerApp.

## 8. Suppression et dépendances

**RG-RES-09 — Suppression directe.** Supprimer une ressource ne vérifie pas si d'autres ressources
la référencent. Les références en base sont soit mises à vide, soit supprimées en cascade selon le
lien (**comportement exact par lien non vérifié à l'exécution**).

**UC-RES-05 — Voir les ressources dépendantes.** Avant suppression, l'écran peut lister les
dépendants pour trois cas seulement :

| Ressource supprimée | Dépendants listés |
|---|---|
| LogAnalyticsWorkspace | ApplicationInsights qui l'utilisent |
| AppServicePlan | WebApp et FunctionApp hébergées |
| SqlServer | SqlDatabase du serveur |

## 9. Écarts constatés

- **EC-RES-01 — Noms en double.** Deux ressources peuvent avoir le même nom logique, dans le même
  resource group ou non (aucune contrainte, aucune règle appliquée).
- **EC-RES-02 — Dépendances explicites mortes.** Le modèle prévoit qu'une ressource « dépende »
  d'autres (avec refus des cycles et des resource groups différents) ; aucune action ne crée de
  dépendance et la suppression d'une dépendance n'est jamais bloquée.
- **EC-RES-03 (F08) — Event Hub sans réglages à l'écran.** Les réglages par environnement de
  l'EventHubNamespace existent côté serveur mais ne sont pas proposés par l'écran.
- **EC-RES-04 — Liens d'entrée/sortie morts.** Le modèle prévoit des liens typés entre sorties et
  entrées de ressources ; rien ne les crée.
- **EC-RES-05 — Dépendants incomplets.** La liste des dépendants ignore les rôles, app settings,
  références cross-config, identités, Private Endpoints, domaines : supprimer un Key Vault référencé
  par des app settings ne prévient de rien.
- **EC-RES-06 — Modifications ignorées sans message.** Modifier une ressource existante renvoie un
  succès même si les modifications sont ignorées (RG-RES-05).
- **EC-RES-07 — Trois régions par défaut.** `westeurope` (serveur), `EastUS2` (repli de l'écran
  ressource), `westeurope` (assistant).

## 10. Questions ouvertes

- **Q-RES-01** — Unicité du nom logique : par resource group, par configuration, par projet ?
- **Q-RES-02** — Bloquer la suppression d'une ressource référencée, ou proposer de retirer les liens ?
- **Q-RES-03** — Une ressource existante doit-elle pouvoir désigner une ressource réelle par son
  identifiant Azure (abonnement, resource group, nom) plutôt que par le calcul de nom ?
- **Q-RES-04** — Déplacer une ressource d'un resource group à un autre ?
- **Q-RES-05** — Dupliquer une ressource ?
