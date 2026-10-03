# 09 — Identités managées et accès RBAC

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F10 « Câbler les ressources » — `keep`, partie la plus saine du produit.

## 1. Objectif

Permettre à une ressource d'accéder à une autre sans secret : la ressource source utilise une
**identité managée** (propre ou partagée) qui reçoit un **rôle Azure RBAC** sur la ressource cible.
Le Bicep généré crée les identités et les attributions de rôles.

## 2. Notions

| Notion | Description |
|---|---|
| Identité system-assigned | Identité propre à une ressource, créée et supprimée avec elle. |
| Identité user-assigned | Ressource UserAssignedIdentity autonome, attachable à plusieurs ressources. |
| Identité attribuée | Une ressource peut avoir **une** identité user-assigned attribuée. |
| Attribution de rôle | (ressource source, ressource cible, type d'identité, rôle, identité user-assigned si ce type). |
| Rôle | Rôle Azure intégré, choisi dans un catalogue propre au type de la cible. |

## 3. Attributions de rôles

| Champ | Règles |
|---|---|
| Ressource source | Ressource qui porte l'attribution (celle dont l'identité reçoit le rôle). |
| Ressource cible | Toute ressource ; doit être différente de la source. |
| Type d'identité | `SystemAssigned` ou `UserAssigned`. |
| Identité user-assigned | Obligatoire si `UserAssigned` ; doit exister. |
| Rôle | Identifiant d'un rôle du catalogue de la cible (section 5). |

**RG-RBA-01 — Pas sur soi-même.** Une ressource ne peut pas s'attribuer un rôle sur elle-même.

**RG-RBA-02 — Rôle valide pour la cible.** Le rôle doit appartenir au catalogue du type de la cible.

**RG-RBA-03 — Pas de doublon.** Une attribution identique (même cible, même rôle, même type
d'identité) n'est pas recréée ; l'ajout est sans effet.

**RG-RBA-04 — AcrPull par identité user-assigned seulement.** Le rôle AcrPull ne peut pas être
attribué via une identité system-assigned.

**UC-RBA-01 — Accorder un rôle** (Contributor). L'utilisateur choisit la cible, le rôle (liste
filtrée par type de cible), le type d'identité et, si user-assigned, l'identité.

**UC-RBA-02 — Changer l'identité d'une attribution** (Contributor). Bascule system-assigned ↔
user-assigned, ou change d'identité user-assigned.

**UC-RBA-03 — Retirer un rôle** (Contributor), après analyse d'impact (section 6).

**UC-RBA-04 — Lister les attributions** d'une ressource (comme source).

**UC-RBA-05 — Lister les rôles disponibles** pour un type de cible.

## 4. Identités user-assigned

**UC-RBA-06 — Créer une identité** (comme toute ressource, ou depuis l'éditeur d'une ressource).

**UC-RBA-07 — Attribuer une identité à une ressource.** La ressource reçoit l'identité. Effet de
bord : les attributions user-assigned de cette ressource qui doublonnent une attribution
system-assigned (même cible, même rôle) sont supprimées.

**UC-RBA-08 — Retirer l'identité d'une ressource.** L'identité attribuée est effacée ; les
attributions de rôles existantes ne changent pas.

**UC-RBA-09 — Voir les droits accordés à une identité** : attributions dont elle est l'identité.

**UC-RBA-10 — Voir qui utilise une identité.**

**UC-RBA-11 — Détacher une ressource d'une identité.** Les attributions de la ressource source qui
passaient par cette identité sont **déplacées** sur l'identité elle-même (l'identité devient la
source). Sans attribution concernée, l'opération ne fait rien.

## 5. Catalogue des rôles par type de cible

| Type cible | Rôles proposés |
|---|---|
| KeyVault | Administrator, Contributor, Reader, Secrets Officer, Secrets User, Crypto Officer, Crypto User, Crypto Service Encryption User, Certificates Officer, Certificate User |
| RedisCache | Redis Cache Contributor, Redis Enterprise Cluster Data Access Administrator, Redis Enterprise Cluster Data Reader |
| StorageAccount | Blob Data Contributor / Reader / Owner, Table Data Contributor / Reader, Queue Data Contributor / Reader, Storage Account Contributor |
| AppServicePlan, WebApp, FunctionApp | Website Contributor, Contributor, Reader |
| UserAssignedIdentity | Managed Identity Operator, Managed Identity Contributor |
| AppConfiguration | App Configuration Data Reader, App Configuration Data Owner, Contributor |
| ContainerAppEnvironment | ContainerApp Environment Contributor, Contributor, Reader |
| ContainerApp | ContainerApp Contributor, Contributor, Reader |
| LogAnalyticsWorkspace | Log Analytics Contributor, Log Analytics Reader, Contributor, Reader |
| CosmosDb | Cosmos DB Account Reader Role, DocumentDB Account Contributor, Cosmos DB Operator, Contributor, Reader |
| ApplicationInsights | Application Insights Component Contributor, Monitoring Contributor, Monitoring Reader, Reader |
| SqlServer | SQL Server Contributor, SQL Security Manager, Contributor, Reader |
| SqlDatabase | SQL DB Contributor, SQL Server Contributor, Contributor, Reader |
| ServiceBusNamespace | Azure Service Bus Data Owner, Data Sender, Data Receiver, Contributor, Reader |
| EventHubNamespace | Azure Event Hubs Data Owner, Data Sender, Data Receiver, Contributor, Reader |
| ContainerRegistry | AcrPush, AcrPull, Contributor, Reader |
| DocumentIntelligence | Cognitive Services User, Cognitive Services Contributor, Contributor, Reader |
| VirtualNetwork | Aucun |

Chaque rôle porte un identifiant Azure, une description et un lien vers la documentation Microsoft.

## 6. Analyse d'impact avant retrait

**UC-RBA-12 — Analyser l'impact d'un retrait.** Avant de retirer une attribution, IFS liste les
conséquences :

| Impact | Gravité | Condition |
|---|---|---|
| AcrPull requis | Critique | Rôle AcrPull, cible ContainerRegistry, source WebApp / FunctionApp / ContainerApp : l'application ne pourra plus tirer son image. |
| Secrets Key Vault requis | Critique | Rôle d'accès aux secrets (Secrets User, Secrets Officer, Administrator), cible KeyVault, aucun autre rôle de secrets de la même source vers la même cible, et la source a des app settings (ou, pour une App Configuration, des clés) qui référencent ce Key Vault. |
| Dernier rôle vers la cible | Avertissement | C'est la seule attribution de la source vers cette cible. |

## 7. Génération

**RG-RBA-05 — Identités dans le Bicep.** Chaque ressource qui utilise une identité system-assigned
et/ou user-assigned reçoit le bloc `identity` correspondant. Quand des ressources d'un même type
ont des combinaisons différentes, le bloc est paramétré.

**RG-RBA-06 — Attributions dans le Bicep.** Chaque attribution produit un module
d'attribution de rôle propre au type de cible (19 types sur 20, pas VirtualNetwork) et les
identifiants de rôles sont regroupés dans `constants.bicep`.

**RG-RBA-07 — Cible dans une autre configuration.** Une attribution vers une ressource d'une autre
configuration utilise la déclaration `existing` issue de la référence cross-config
([05](05-configurations.md)).

## 8. Diagnostics liés

- Rôle AcrPull manquant pour une application qui tire depuis un registre.
- Rôle Key Vault Secrets User manquant pour une ressource dont des app settings référencent un Key
  Vault.

Voir [16](16-diagnostics.md).

## 9. Écarts constatés

- **EC-RBA-01 — Cible hors projet.** La cible d'une attribution est cherchée par identifiant sans
  vérifier qu'elle appartient au même projet (ni que l'utilisateur y a accès).
- **EC-RBA-02 — Source sans identité utile.** Toute ressource peut être source d'une attribution
  system-assigned, même un type qui ne peut pas porter d'identité managée côté Azure
  (**liste des types porteurs non vérifiée**).
- **EC-RBA-03 — Diagnostic Key Vault strict.** Le diagnostic ne reconnaît que Secrets User ; une
  attribution Secrets Officer ou Administrator, pourtant suffisante, déclenche quand même
  l'avertissement. L'analyse d'impact, elle, reconnaît les trois.
- **EC-RBA-04 — Retrait de l'identité silencieux.** Retirer l'identité attribuée laisse les
  attributions user-assigned qui la citent : elles pointent vers une identité que la ressource
  n'a plus.
- **EC-RBA-05 — Portée unique.** Une attribution porte toujours sur la ressource cible entière ; pas de
  portée resource group, abonnement ou sous-ressource (conteneur blob, file).

## 10. Questions ouvertes

- **Q-RBA-01** — Ajouter des portées (resource group, abonnement, sous-ressource) ?
- **Q-RBA-02** — Créer automatiquement le rôle nécessaire quand l'utilisateur ajoute un app setting
  Key Vault ou choisit un registre (au lieu d'un diagnostic) ?
- **Q-RBA-03** — Rôles personnalisés et rôles hors catalogue ?
- **Q-RBA-04** — Attributions pour des identités humaines ou des groupes Entra (accès des équipes aux
  ressources) ?
