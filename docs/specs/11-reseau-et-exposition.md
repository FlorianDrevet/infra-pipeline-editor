# 11 — Réseau et exposition

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F33 (privatisation, `fix`, D11), F34 (Private Endpoint, `fix`, D02), F35 (profil
> réseau V2, `cut`, D04), F10c (domaines personnalisés, `fix`, D08).

## 1. Objectif

Décrire comment une ressource est exposée : en public (comportement par défaut), en privé derrière
un **Private Endpoint** dans un réseau virtuel, et, pour les applications web, sous un **domaine
personnalisé** avec certificat.

## 2. Réseau virtuel

Le VNet est une ressource du catalogue ([08 § 19](08-catalogue-ressources.md)) : espaces
d'adressage, serveurs DNS et protection DDoS par environnement ; subnets avec délégation, service
endpoints et politiques réseau des Private Endpoints.

**UC-NET-01 — Gérer les subnets** (Contributor) : ajouter, modifier, supprimer un subnet d'un VNet.

**RG-NET-01 — Nom de subnet unique** dans le VNet, 80 caractères max.

## 3. Privatisation d'une ressource

### 3.1 Types privatisables (14)

| Type | Sous-ressources ciblées par le Private Endpoint (`groupIds`) |
|---|---|
| KeyVault | `vault` |
| StorageAccount | `blob`, `file`, `queue`, `table`, `dfs`, `web` |
| AppConfiguration | `configurationStores` |
| CosmosDb | `Sql`, `MongoDB`, `Cassandra`, `Gremlin`, `Table` |
| SqlServer | `sqlServer` |
| RedisCache | `redisCache` |
| ServiceBusNamespace, EventHubNamespace | `namespace` |
| ContainerRegistry | `registry` |
| WebApp, FunctionApp | `sites` |
| ApplicationInsights, LogAnalyticsWorkspace | `azuremonitor` |
| DocumentIntelligence | `account` |

Non privatisables : AppServicePlan, ContainerAppEnvironment, ContainerApp, SqlDatabase,
UserAssignedIdentity, VirtualNetwork. La liste est la même côté écran et côté serveur.

### 3.2 Données

| Champ | Règles |
|---|---|
| Privatisée | Booléen. |
| VNet | Ressource VirtualNetwork du **même projet** (une autre configuration est permise). |
| Subnet | Nom d'un subnet existant de ce VNet. |
| Mode DNS | `AutoManaged`, `ExistingHub`, `Disabled`. |
| Hub DNS | Resource group et abonnement du hub : obligatoires si `ExistingHub`. |

### 3.3 Cas d'usage

**UC-NET-02 — Privatiser / rendre publique une ressource** (Contributor). Bascule indépendante de
la configuration du Private Endpoint (EC-NET-01).

**UC-NET-03 — Configurer le Private Endpoint** (Contributor). L'utilisateur choisit le VNet, le
subnet et le mode DNS. Enregistrer la configuration **privatise** aussi la ressource.

**RG-NET-02 — VNet du même projet.** Refus si le VNet appartient à un autre projet.

**RG-NET-03 — Subnet existant.** Refus si le subnet n'existe pas dans le VNet choisi.

**UC-NET-04 — Retirer le Private Endpoint** (Contributor). La configuration est effacée et la
ressource redevient publique.

**RG-NET-04 — Écran.** L'onglet « Réseau » n'est proposé que pour les types privatisables, et pas
pour une ressource existante.

### 3.4 Génération

**RG-NET-05 — Accès public coupé.** Toute ressource privatisée est générée avec l'accès réseau public
désactivé.

**RG-NET-06 — Private Endpoint compagnon.** Pour chaque ressource privatisée **qui a** une
configuration, un module `pe<Ressource>` est généré dans le dossier `PrivateEndpoints`, dans le
resource group de la ressource, nommé `pe-<nom>`. Il crée le Private Endpoint (connexion vers la
ressource, sous-ressources du § 3.1, subnet) et, si une zone DNS privée est fournie, son groupe de
zones DNS.

**RG-NET-07 — Résolution du subnet.** Un module `networkingResolution` (dossier `Networking`) lit le
VNet et le subnet existants et expose l'identifiant du subnet.

## 4. Domaines personnalisés

Applicables à : **WebApp, FunctionApp, ContainerApp**, non existantes.

| Champ | Règles |
|---|---|
| Environnement | Obligatoire, 100 caractères max. Un domaine vaut pour un seul environnement. |
| Nom de domaine | Obligatoire, FQDN valide, 253 caractères max. Mis en minuscules. |
| Mode de certificat | `ManagedCertificate` (défaut), `KeyVaultCertificate`, `ManualCertificate`, `Disabled`. |
| URL Key Vault + identité managée | Obligatoires en `KeyVaultCertificate`. |
| Nom du certificat | Obligatoire en `ManualCertificate`. |
| Statut DNS | `Pending` à la création, puis `Validated`. |

**RG-NET-08 — Pas de doublon** (même environnement, même domaine) sur une ressource.

**UC-NET-05 — Ajouter un domaine** (Contributor).

**UC-NET-06 — Lire les instructions DNS.** IFS indique les enregistrements à créer :

| Type | Étapes |
|---|---|
| WebApp, FunctionApp | 1. CNAME du domaine vers `<nom>.azurewebsites.net`. 2. TXT `asuid.<domaine>` avec l'identifiant de vérification du portail. 3. Valider. |
| ContainerApp | 1. Déployer d'abord sans domaine, puis CNAME vers l'URL de l'application (ou A vers l'IP). 2. TXT `asuid.<domaine>`. 3. Valider ; le prochain déploiement crée la liaison. |

**UC-NET-07 — Valider le DNS** (Contributor). Le domaine passe à `Validated`.

**UC-NET-08 — Supprimer un domaine.**

**UC-NET-09 — Lister les domaines** d'une ressource.

**RG-NET-09 — Génération.** Seuls les domaines `Validated` de l'environnement sont écrits dans le
paramètre `customDomains` de `main.<env>.bicepparam`. Type de liaison : `Auto` (ContainerApp en
certificat managé), `SniEnabled` (autres modes avec certificat), `Disabled`.

## 5. Écarts constatés

- **EC-NET-01 (D11) — Privatisée sans Private Endpoint.** La bascule de privatisation n'exige pas de
  configuration. Une ressource privatisée sans configuration est générée sans accès public et sans
  Private Endpoint : elle est injoignable.
- **EC-NET-02 (D02) — L'onglet Réseau ne charge pas.** L'écran lit la configuration du Private
  Endpoint par une route qui n'existe pas ; l'onglet affiche une erreur à chaque ouverture.
- **EC-NET-03 — Mode DNS ignoré.** Le mode DNS et le hub sont enregistrés mais la génération ne les
  lit pas : la zone DNS privée reste vide, aucun groupe de zones DNS n'est créé, même en
  `AutoManaged`. La résolution DNS du Private Endpoint n'est donc pas assurée.
- **EC-NET-04 — Plusieurs sous-ressources dans un seul Private Endpoint.** StorageAccount et CosmosDb
  reçoivent toutes leurs sous-ressources dans une seule connexion, alors qu'Azure attend un Private
  Endpoint par sous-ressource (**non vérifié à l'exécution**, risque d'échec du déploiement).
- **EC-NET-05 — Branchement des modules non vérifié.** Le câblage, dans `main.bicep`, des paramètres
  du Private Endpoint (identifiant de la ressource, du subnet) n'a pas été suivi jusqu'au bout
  (**non vérifié à l'exécution**).
- **EC-NET-06 (D08) — Validation DNS fictive.** « Valider » passe le domaine à `Validated` sans aucune
  résolution DNS. Comme seuls les domaines validés sont générés, une déclaration fausse produit une
  liaison qui échouera au déploiement.
- **EC-NET-07 (D04) — Profil réseau V2 mort.** Un profil réseau par configuration (modes
  Simplified/Standard/Advanced, VNet à créer / existant / hub-spoke, DNS AutoManaged / hub / custom,
  surcharges par environnement) existe en base et dans le modèle, sans aucune action. Décision :
  `cut`.
- **EC-NET-08 — Aucun outil MCP** ne couvre le réseau.
- **EC-NET-09 — Pas de NSG ni de pare-feu IP.** Le NSG d'un subnet n'est plus modifiable ; aucune
  règle d'accès IP ou de réseau autorisé n'est modélisée sur les ressources.

## 6. Questions ouvertes

- **Q-NET-01** — Rendre la privatisation indissociable de la configuration du Private Endpoint ?
- **Q-NET-02** — Générer les zones DNS privées (mode `AutoManaged`) et les liens VNet ?
- **Q-NET-03** — Choisir les sous-ressources ciblées (blob seul, par exemple) ?
- **Q-NET-04** — Intégration VNet sortante des applications (WebApp/FunctionApp/ContainerAppEnvironment
  dans un subnet délégué) ?
- **Q-NET-05** — Valider réellement le DNS (résolution CNAME/TXT) ou retirer le statut ?
- **Q-NET-06** — Front Door / Application Gateway, NSG, pare-feux IP (roadmap) ?
