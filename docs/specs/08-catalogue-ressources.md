# 08 — Catalogue des ressources Azure

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Règles communes à tous les types : [07](07-modele-commun-ressources.md). Rôles RBAC : [09](09-identites-et-rbac.md). Outputs : [10](10-configuration-applicative.md).

Pour chaque type : propriétés de la ressource (identiques dans tous les environnements),
réglages par environnement, sous-ressources, liaisons obligatoires, sorties exposées, rôles RBAC
applicables quand la ressource est **cible** d'une attribution.

Les champs communs (nom logique, région, resource group, ressource existante) ne sont pas répétés.
« Valeurs proposées » = valeurs offertes par l'écran ; « valeurs acceptées » = valeurs contrôlées
par le serveur. Quand le serveur accepte un texte libre, seule la liste de l'écran limite la saisie.

---

## 1. KeyVault (Key Vault)

| Propriété | Valeurs | Défaut |
|---|---|---|
| Autorisation RBAC | Booléen | Oui |
| Activé pour le déploiement de VM | Booléen | Non |
| Activé pour le chiffrement de disque | Booléen | Non |
| Activé pour le déploiement de template | Booléen | Non |
| Protection contre la purge | Booléen | Oui |
| Suppression réversible (soft delete) | Booléen | Oui |

| Réglage par environnement | Valeurs acceptées |
|---|---|
| SKU | `Standard`, `Premium` |

Sorties : `vaultUri`, `vaultName`.
Rôles applicables : Key Vault Administrator, Contributor, Reader, Secrets Officer, Secrets User,
Crypto Officer, Crypto User, Crypto Service Encryption User, Certificates Officer, Certificate User.
Privatisable (groupe `vault`).

## 2. RedisCache (Azure Cache for Redis)

| Propriété | Valeurs |
|---|---|
| Version Redis | Entier ; proposées : 4, 6 |
| Port non TLS activé | Booléen |
| Version TLS minimale | `Tls10`, `Tls11`, `Tls12` |
| Désactiver l'authentification par clé d'accès | Booléen |
| Authentification Entra ID | Booléen |

**RG-CAT-01** — Si l'authentification par clé est désactivée, l'authentification Entra ID doit être
activée.

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Basic`, `Standard`, `Premium` (famille `C`, ou `P` pour Premium) |
| Capacité | Entier |
| Politique d'éviction mémoire | `NoEviction`, `AllKeysLru`, `VolatileLru`, `AllKeysRandom`, `VolatileRandom`, `VolatileTtl`, `AllKeysLfu`, `VolatileLfu` |

Sorties : `hostName`, `sslPort`, `connectionString` (sensible), `primaryKey` (sensible).
Rôles : Redis Cache Contributor, Redis Enterprise Cluster Data Access Administrator, Redis Enterprise
Cluster Data Reader. Privatisable (`redisCache`).

**EC-CAT-01 — Politique d'éviction non générée.** Elle est saisie et stockée mais n'apparaît pas dans
le Bicep.

## 3. StorageAccount (compte de stockage)

| Propriété | Valeurs proposées |
|---|---|
| Type (Kind) | `StorageV2`, `BlobStorage`, `BlockBlobStorage` (le serveur accepte aussi `FileStorage`, `Storage`) |
| Niveau d'accès | `Hot`, `Cool` (le serveur accepte aussi `Premium`) |
| Autoriser l'accès public aux blobs | Booléen |
| HTTPS uniquement | Booléen |
| Version TLS minimale | `Tls10`, `Tls11`, `Tls12` |

| Réglage par environnement | Valeurs |
|---|---|
| SKU (redondance) | `Standard_LRS`, `Standard_GRS`, `Standard_RAGRS`, `Standard_ZRS`, `Premium_LRS`, `Premium_ZRS` |

Sous-ressources :

| Sous-ressource | Champs | Règles |
|---|---|---|
| Conteneur blob | Nom ; accès public `None`, `Blob`, `Container` | Nom unique dans le compte (insensible à la casse). Accès modifiable. |
| File d'attente | Nom | Nom unique. |
| Table | Nom | Nom unique. |
| Règle CORS | Service (`Blob` ou `Table`), origines autorisées, méthodes (`DELETE`, `GET`, `HEAD`, `MERGE`, `OPTIONS`, `PATCH`, `POST`, `PUT`), en-têtes autorisés, en-têtes exposés, durée de cache (secondes) | Liste remplacée en bloc par service. |
| Règle de cycle de vie | Nom, conteneurs ciblés, durée de vie en jours | Liste remplacée en bloc. |

Sorties : `storageAccountName`, `primaryBlobEndpoint`, `primaryQueueEndpoint`,
`primaryTableEndpoint`, `connectionString` (sensible), `primaryKey` (sensible).
Rôles : Storage Blob Data Contributor / Reader / Owner, Storage Table Data Contributor / Reader,
Storage Queue Data Contributor / Reader, Storage Account Contributor.
Privatisable (`blob`, `file`, `queue`, `table`, `dfs`, `web`).

## 4. AppServicePlan (plan App Service)

| Propriété | Valeurs |
|---|---|
| Système | `Windows`, `Linux` |

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `F1`, `B1`, `B2`, `B3`, `S1`, `S2`, `S3`, `P1v3`, `P2v3`, `P3v3` |
| Nombre d'instances | Entier |

Sortie : `id`. Rôles : Website Contributor, Contributor, Reader.
Dépendants : WebApp et FunctionApp hébergées.

## 5. WebApp (application web App Service)

| Propriété | Valeurs | Règles |
|---|---|---|
| Plan App Service | Référence à un AppServicePlan | Obligatoire ; doit exister. |
| Pile d'exécution | `DotNet`, `Node`, `Python`, `Java`, `Php` | Obligatoire. |
| Version | Selon la pile : DotNet `10`, `9`, `8` ; Node `22-lts`, `20-lts` ; Python `3.13`, `3.12`, `3.11`, `3.10` ; Java `21`, `17`, `11` ; Php `8.4`, `8.3`, `8.2` | Doit appartenir à la liste de la pile. |
| Always On | Booléen | — |
| HTTPS uniquement | Booléen | — |
| Mode de déploiement | `Code`, `Container` | Obligatoire. |
| Registre de conteneurs | Référence à un ContainerRegistry | Facultatif. |
| Authentification au registre | `ManagedIdentity`, `AdminCredentials` | Effacée s'il n'y a pas de registre. |
| Identité de pull | Référence à une UserAssignedIdentity | Gardée seulement en mode `ManagedIdentity`. |
| Nom d'image Docker | Texte | — |
| Image validée | Booléen | Posé par l'écran après vérification (voir [12](12-pipeline-applicatif.md)). |
| Chemin du Dockerfile, chemin du code source, commande de build, nom d'application | Texte | Utilisés par le pipeline applicatif. |
| Options d'étapes du pipeline | Voir [12](12-pipeline-applicatif.md) | — |

| Réglage par environnement | Valeurs |
|---|---|
| Always On | Booléen |
| HTTPS uniquement | Booléen |
| Tag de l'image Docker | Texte |

Sortie : `defaultHostName`. Rôles : Website Contributor, Contributor, Reader.
Liaisons possibles : app settings, domaines personnalisés, identité, rôles, Private Endpoint (`sites`),
pipeline applicatif.

## 6. FunctionApp (application de fonctions)

Comme WebApp, sauf :

| Différence | Détail |
|---|---|
| Pas d'Always On | Ni en propriété ni par environnement. |
| Piles | `DotNet`, `Node`, `Python`, `Java`, `PowerShell` |
| Versions | DotNet `10-isolated`, `9-isolated`, `8-isolated`, `8-in-process` ; Node `22`, `20` ; Python `3.12`, `3.11`, `3.10` ; Java `21`, `17`, `11` ; PowerShell `7.4`, `7.2` |
| Réglages par environnement | HTTPS uniquement, nombre maximal d'instances, tag de l'image Docker |

Sortie : `defaultHostName`. Rôles : Website Contributor, Contributor, Reader. Privatisable (`sites`).

## 7. UserAssignedIdentity (identité managée)

Aucune propriété propre, aucun réglage par environnement. 3 à 80 caractères pour le nom logique.

Sorties : `principalId`, `clientId`. Rôles : Managed Identity Operator, Managed Identity Contributor.
Voir [09](09-identites-et-rbac.md) pour son usage.

## 8. AppConfiguration (App Configuration)

Aucune propriété propre.

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Free`, `Standard` (texte libre côté serveur) |
| Rétention en suppression réversible (jours) | Entier |
| Protection contre la purge | Booléen |
| Désactiver l'authentification locale | Booléen |
| Accès réseau public | `Enabled`, `Disabled` |

Sous-ressources : **clés de configuration**, voir [10](10-configuration-applicative.md).
Sortie : `endpoint`. Rôles : App Configuration Data Reader, App Configuration Data Owner, Contributor.
Privatisable (`configurationStores`).

## 9. ContainerAppEnvironment (environnement Container Apps)

| Propriété | Valeurs |
|---|---|
| Log Analytics | Référence à un LogAnalyticsWorkspace (facultative) |

| Réglage par environnement | Valeurs proposées |
|---|---|
| SKU | `Consumption`, `Premium` |
| Profil de charge | `Consumption`, `D4`, `D8`, `D16`, `D32`, `E4`, `E8`, `E16`, `E32` |
| Load balancer interne | Booléen |
| Redondance de zone | Booléen |

Sorties : `defaultDomain`, `staticIp`. Rôles : ContainerApp Environment Contributor, Contributor, Reader.

## 10. ContainerApp (Container App)

| Propriété | Valeurs | Règles |
|---|---|---|
| Environnement Container Apps | Référence | Obligatoire. |
| Registre, authentification, identité de pull, image, image validée, Dockerfile, nom d'application, chemin du code source | Comme WebApp | — |
| Options d'étapes du pipeline | Voir [12](12-pipeline-applicatif.md) | La pile de build est portée par ces options. |

| Réglage par environnement | Valeurs | Règles |
|---|---|---|
| CPU | `0.25`, `0.5`, `1.0`, `2.0`, `4.0` | — |
| Mémoire | `0.5Gi`, `1.0Gi`, `2.0Gi`, `4.0Gi`, `8.0Gi` | — |
| Réplicas min / max | Entiers | — |
| Ingress activé, ingress externe | Booléens | — |
| Port cible de l'ingress | Entier | — |
| Transport | `auto`, `http`, `http2`, `tcp` | — |
| Sondes readiness, liveness, startup | Chemin HTTP + port | Port 1–65535 ; chemin commençant par `/`. |
| Service connection du registre | Texte | Nom de la service connection Docker Registry de l'environnement ; reprise par le bootstrap. |

Sortie : `fqdn`. Rôles : ContainerApp Contributor, Contributor, Reader.

## 11. LogAnalyticsWorkspace (espace Log Analytics)

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Free`, `PerGB2018`, `PerNode`, `Premium`, `Standard`, `Standalone`, `CapacityReservation` |
| Rétention (jours) | Entier |
| Quota journalier (Go) | Décimal |

Sortie : `workspaceId`. Rôles : Log Analytics Contributor, Log Analytics Reader, Contributor, Reader.
Privatisable (`azuremonitor`). Dépendants : ApplicationInsights.

## 12. ApplicationInsights

| Propriété | Valeurs |
|---|---|
| Log Analytics | Référence obligatoire |

| Réglage par environnement | Valeurs |
|---|---|
| Échantillonnage (%) | Décimal |
| Rétention (jours) | `30`, `60`, `90`, `120`, `180`, `270`, … |
| Désactiver le masquage IP | Booléen |
| Désactiver l'authentification locale | Booléen |
| Mode d'ingestion | `ApplicationInsights`, `LogAnalytics`, `ApplicationInsightsWithDiagnosticSettings` |

Sorties : `connectionString`, `instrumentationKey`. Rôles : Application Insights Component Contributor,
Monitoring Contributor, Monitoring Reader, Reader. Privatisable (`azuremonitor`).

## 13. CosmosDb (Azure Cosmos DB)

| Réglage par environnement | Valeurs |
|---|---|
| API | `SQL`, `MongoDB`, `Cassandra`, `Table`, `Gremlin` |
| Niveau de cohérence | `Eventual`, `ConsistentPrefix`, `Session`, `BoundedStaleness`, `Strong` |
| Préfixe de péremption max, intervalle max (s) | Entiers (pour `BoundedStaleness`) |
| Basculement automatique | Booléen |
| Écritures multi-régions | Booléen |
| Politique de sauvegarde | `Periodic`, `Continuous` |
| Niveau gratuit | Booléen |

Sorties : `documentEndpoint`, `primaryConnectionString` (sensible), `primaryKey` (sensible).
Rôles : Cosmos DB Account Reader Role, DocumentDB Account Contributor, Cosmos DB Operator, Contributor,
Reader. Privatisable (`Sql`, `MongoDB`, `Cassandra`, `Gremlin`, `Table`).

## 14. SqlServer (serveur Azure SQL)

| Propriété | Valeurs | Règles |
|---|---|---|
| Version | `V12` | Obligatoire. |
| Login administrateur | Texte | Obligatoire, 128 caractères max. |

| Réglage par environnement | Valeurs |
|---|---|
| Version TLS minimale | `1.0`, `1.1`, `1.2` |

Sorties : `fullyQualifiedDomainName`, `connectionString`. Rôles : SQL Server Contributor, SQL
Security Manager, Contributor, Reader. Privatisable (`sqlServer`). Dépendants : SqlDatabase.

## 15. SqlDatabase (base Azure SQL)

| Propriété | Valeurs | Règles |
|---|---|---|
| Serveur SQL | Référence obligatoire | — |
| Classement (collation) | Texte | Obligatoire, 128 caractères max. |

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Basic`, `Standard`, `Premium`, `GeneralPurpose`, `BusinessCritical`, `Hyperscale` |
| Taille max (Go) | Entier |
| Redondance de zone | Booléen |

Sortie : `id`. Rôles : SQL DB Contributor, SQL Server Contributor, Contributor, Reader.

## 16. ServiceBusNamespace (Service Bus)

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Basic`, `Standard`, `Premium` |
| Capacité | Entier |
| Redondance de zone | Booléen |
| Désactiver l'authentification locale | Booléen |
| Version TLS minimale | `1.0`, `1.1`, `1.2` |

| Sous-ressource | Champs | Règles |
|---|---|---|
| File (queue) | Nom | Nom unique dans le namespace. |
| Abonnement de topic | Nom du topic, nom de l'abonnement | Couple unique. Le topic n'existe pas comme objet distinct. |

Sortie : `connectionString`. Rôles : Azure Service Bus Data Owner, Data Sender, Data Receiver,
Contributor, Reader. Privatisable (`namespace`).

## 17. EventHubNamespace (Event Hubs)

| Réglage par environnement (serveur seulement, voir EC-RES-03) | Valeurs |
|---|---|
| SKU, capacité, redondance de zone, désactiver l'authentification locale, TLS minimal | Comme Service Bus |
| Auto-inflate, unités de débit max | Booléen, entier |

| Sous-ressource | Champs | Règles |
|---|---|---|
| Event hub | Nom | Nom unique. |
| Groupe de consommateurs | Nom de l'event hub, nom du groupe | Couple unique. |

Rôles : Azure Event Hubs Data Owner, Data Sender, Data Receiver, Contributor, Reader.
Privatisable (`namespace`). Aucune sortie déclarée.

## 18. ContainerRegistry (registre de conteneurs)

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `Basic`, `Standard`, `Premium` |
| Utilisateur admin activé | Booléen |
| Accès réseau public | `Enabled`, `Disabled` |
| Redondance de zone | Booléen |

Rôles : AcrPush, AcrPull, Contributor, Reader. Privatisable (`registry`). Aucune sortie déclarée.
Utilisé par WebApp, FunctionApp, ContainerApp (voir [12](12-pipeline-applicatif.md)).

## 19. VirtualNetwork (réseau virtuel)

| Réglage par environnement | Valeurs | Règles |
|---|---|---|
| Espaces d'adressage | Liste de CIDR | Saisie segmentée par octet à l'écran. |
| Serveurs DNS | Liste d'IPv4 | Facultatif. |
| Protection DDoS | Booléen | — |

| Sous-ressource : subnet | Valeurs | Règles |
|---|---|---|
| Nom | Texte | Obligatoire, 80 caractères max, unique dans le VNet. |
| Préfixe d'adresse | CIDR `a.b.c.d/n` | Obligatoire. Pas de contrôle d'inclusion dans l'espace d'adressage (EC-CAT-02). |
| Délégation | `None`, `WebServerFarms`, `AppEnvironments`, `ContainerGroups`, `SqlManagedInstances`, `PostgresFlexible` | — |
| Service endpoints | Liste au format fournisseur ARM (`Microsoft.Storage`…) | Format contrôlé. |
| Politiques réseau des Private Endpoints | `Disabled`, `Enabled`, `NetworkSecurityGroupEnabled`, `RouteTableEnabled` | Obligatoire. |
| NSG | Référence | Vestige : plus modifiable (NSG retiré, prévu sur la roadmap). |

Aucune sortie déclarée. Voir [11](11-reseau-et-exposition.md).

## 20. DocumentIntelligence (Document Intelligence)

| Propriété | Valeurs |
|---|---|
| Sous-domaine personnalisé | Texte |

| Réglage par environnement | Valeurs |
|---|---|
| SKU | `F0`, `S0` |
| Accès réseau public | `Enabled`, `Disabled` |
| Désactiver l'authentification locale | Booléen |

Sorties : `endpoint`, `id`. Rôles : Cognitive Services User, Cognitive Services Contributor,
Contributor, Reader. Privatisable (`account`).

**EC-CAT-03 (D06) — Réglages par environnement perdus.** Le module généré et la table de
correspondance des modules ne concordent pas : les réglages par environnement de Document
Intelligence ne sont pas écrits dans les fichiers de paramètres.

---

## 21. Écarts transverses du catalogue

- **EC-CAT-02 — Pas de contrôle d'adressage.** Un subnet peut sortir de l'espace d'adressage du VNet ;
  deux subnets peuvent se chevaucher.
- **EC-CAT-04 — Textes libres.** Plusieurs SKU et modes sont des textes libres côté serveur (App
  Configuration, Container Apps, Log Analytics, Cosmos DB, Service Bus, Event Hubs, Container
  Registry) : seule la liste de l'écran limite la saisie ; l'API et le MCP acceptent n'importe quelle
  valeur.
- **EC-CAT-05 — Valeurs serveur sans écran.** Le serveur accepte des valeurs que l'écran ne propose
  pas (Storage `FileStorage`, `Storage`, niveau `Premium`).
- **EC-CAT-06 — Pas de sortie pour trois types.** Event Hubs, Container Registry et Virtual Network
  n'exposent aucune sortie utilisable dans un app setting (exemple : serveur de connexion du registre).
- **EC-CAT-07 — Versions figées.** Les listes de versions d'exécution sont codées en dur et devront
  être maintenues à la main.

## 22. Questions ouvertes

- **Q-CAT-01** — Quels types ajouter pour la v1 commerciale (Front Door, NSG, PostgreSQL, Static Web
  Apps, API Management, Container Instances…) ?
- **Q-CAT-02** — Ressources enfants à modéliser comme objets (topics Service Bus, bases Cosmos,
  secrets Key Vault) ?
- **Q-CAT-03** — Centraliser les valeurs autorisées côté serveur (une seule source pour l'écran,
  l'API et le MCP) ?
