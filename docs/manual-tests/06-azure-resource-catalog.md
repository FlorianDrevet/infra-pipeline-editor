---
title: Tests du catalogue de ressources Azure
description: Matrice de recette manuelle des ressources Azure, de leurs formulaires d'environnement et de leurs sous-ressources.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests du catalogue de ressources Azure

Cette fiche sert de matrice pour les ressources disponibles dans le picker du site. Le parcours commun est décrit une fois, puis chaque famille précise les contrôles propres au type.

## Parcours commun de création, édition et suppression

Pour chaque ligne de la matrice :

1. [ ] Ouvrir une configuration puis un groupe de ressources de recette.
2. [ ] Cliquer sur `Add resource`.
3. [ ] Choisir la catégorie puis le type demandé.
4. [ ] Saisir les champs d'identité et les champs spécifiques.
5. [ ] Compléter l'étape `Environments` lorsque le type la propose.
6. [ ] Enregistrer.
7. [ ] Vérifier la ressource dans la liste du groupe.
8. [ ] Ouvrir la ressource par son action d'édition.
9. [ ] Modifier une valeur de recette et sauvegarder.
10. [ ] Recharger la ressource et vérifier la valeur.
11. [ ] Supprimer la ressource de recette et confirmer.

### Résultat commun attendu

* [ ] Le picker affiche le type dans la bonne catégorie.
* [ ] Les champs obligatoires empêchent une soumission incomplète.
* [ ] Les erreurs restent associées au champ ou à la section concernée.
* [ ] La ressource est créée une seule fois et rattachée au bon groupe.
* [ ] La valeur modifiée survit au rechargement.
* [ ] La suppression demande une confirmation et retire la ressource de la liste.

## MT-RES-001 - Catégories et types affichés

**Feature testée** : catalogue du dialogue `Add resource`.

**Comportement attendu** : les types supportés sont accessibles par catégorie sans types supprimés ou non câblés.

### Matrice des types

| Catégorie | Types à tester |
|---|---|
| Compute | App Service Plan, Web App, Function App, Container App Environment, Container App |
| Storage and data | Storage Account, Cosmos DB, Redis Cache, SQL Server, SQL Database, Container Registry |
| Security | Key Vault, User Assigned Identity |
| Messaging | Service Bus Namespace, Event Hub Namespace |
| Monitoring and configuration | Log Analytics Workspace, Application Insights, App Configuration |
| Networking | Virtual Network |
| AI | Document Intelligence |

### Étapes

1. [ ] Ouvrir le picker.
2. [ ] Parcourir chaque catégorie.
3. [ ] Vérifier que chaque type de la matrice peut être sélectionné.
4. [ ] Vérifier qu'un type retiré du produit n'est pas présenté comme disponible.
5. [ ] Annuler le dialogue sans créer de ressource.

### Résultat attendu

* [ ] Les 20 types du catalogue actuel sont présentés une fois chacun.
* [ ] Les labels longs restent lisibles sur desktop et mobile.
* [ ] L'annulation ne crée aucune ressource.

## MT-RES-002 - Environnements obligatoires ou absents

**Feature testée** : étape de configuration par environnement.

**Comportement attendu** : les types avec de vrais champs par environnement affichent cette étape; les types sans configuration environnementale la sautent.

### Étapes

1. [ ] Créer un type avec environnements, par exemple Storage Account, Web App ou Virtual Network.
2. [ ] Vérifier la présence de l'étape `Environments`.
3. [ ] Laisser un environnement obligatoire incomplet et tenter de sauvegarder.
4. [ ] Compléter les valeurs et sauvegarder.
5. [ ] Créer User Assigned Identity et Event Hub Namespace.
6. [ ] Vérifier que l'écran ne réclame pas de configuration environnementale inexistante.

### Résultat attendu

* [ ] Les types avec environnements demandent les valeurs nécessaires.
* [ ] Un environnement manquant est signalé avant la génération ou à l'endroit prévu.
* [ ] User Assigned Identity et Event Hub Namespace ne montrent pas une étape environnement vide.
* [ ] Les environnements existants ne sont pas réclamés pour une ressource marquée `existing`.

## MT-RES-003 - Compute et applications

**Types** : App Service Plan, Web App, Function App, Container App Environment, Container App.

### Contrôles spécifiques

| Type | Vérifications manuelles |
|---|---|
| App Service Plan | OS type, SKU, location et rattachement des Web Apps/Function Apps |
| Web App | mode code/container, runtime stack/version, Docker image, ACR, Always On, HTTPS only |
| Function App | runtime stack/version, mode code/container, Docker image, ACR, HTTPS only |
| Container App Environment | configuration générale et rattachement des Container Apps |
| Container App | image, ingress, scaling, probes, variables et rattachement à l'environnement |

### Étapes complémentaires

1. [ ] Créer un App Service Plan.
2. [ ] Ajouter une Web App ou Function App enfant et vérifier son regroupement.
3. [ ] Créer un Container App Environment puis une Container App.
4. [ ] Pour une ressource conteneur, saisir une image non validée.
5. [ ] Observer l'avertissement et tenter une génération.
6. [ ] Valider l'image puis sauvegarder.
7. [ ] Vérifier l'onglet `App Pipeline` lorsqu'il est disponible.

### Résultat attendu

* [ ] Les ressources enfants apparaissent sous le parent correspondant.
* [ ] Le mode de déploiement affiche seulement les champs pertinents.
* [ ] Une image non validée produit un diagnostic avant génération.
* [ ] Une image validée retire l'état d'avertissement prévu.
* [ ] Les options de pipeline restent éditables seulement pour une ressource compatible.

## MT-RES-004 - Stockage, bases et registre

**Types** : Storage Account, Cosmos DB, Redis Cache, SQL Server, SQL Database, Container Registry.

### Contrôles spécifiques

| Type | Vérifications manuelles |
|---|---|
| Storage Account | SKU/réplication, HTTPS, TLS, accès public, CORS, lifecycle et services enfants |
| Cosmos DB | API, cohérence, paramètres par environnement et génération des valeurs saisies |
| Redis Cache | version Redis, TLS, non-SSL, authentification par clé et AAD; vérifier l'avertissement AAD |
| SQL Server | version, administrator login et rattachement des SQL Databases |
| SQL Database | paramètres de base, serveur parent et environnement |
| Container Registry | SKU, réseau public, authentification ACR et accès par identité |

### Étapes complémentaires

1. [ ] Créer un Storage Account et ouvrir sa zone de services.
2. [ ] Ajouter un Blob Container, une Queue et une Table.
3. [ ] Modifier l'accès public d'un Blob Container et vérifier la valeur après rechargement.
4. [ ] Créer SQL Server puis SQL Database.
5. [ ] Ouvrir la section du mot de passe SQL Server et tester les modes aléatoire et variable group.
6. [ ] Pour un Container Registry utilisé par une application, tester le mode Managed Identity puis Admin Credentials.
7. [ ] Vérifier que vider l'ACR nettoie les champs dépendants.

### Résultat attendu

* [ ] Les sous-ressources Storage sont visibles et comptées.
* [ ] Les contraintes de nom et de contenu sont signalées.
* [ ] Le mode Admin Credentials ne réclame pas le diagnostic d'identité managée.
* [ ] Le mode Managed Identity signale une identité ou un rôle ACR manquant.
* [ ] Le mapping de mot de passe SQL Server indique clairement la source de la valeur.

## MT-RES-005 - Sécurité et identités

**Types** : Key Vault, User Assigned Identity.

### Étapes

1. [ ] Créer un Key Vault.
2. [ ] Tester ses options RBAC, purge protection, soft delete et déploiement.
3. [ ] Créer une User Assigned Identity.
4. [ ] Ouvrir l'onglet `Identity and access` d'une ressource compatible.
5. [ ] Affecter une identité et un rôle.
6. [ ] Retirer le rôle ou l'identité dans une confirmation.
7. [ ] Ouvrir les états de droits accordés si la ressource le propose.

### Résultat attendu

* [ ] Les options Key Vault restent dans la ressource courante.
* [ ] L'identité sélectionnée apparaît après sauvegarde.
* [ ] Les rôles ajoutés et supprimés actualisent la liste.
* [ ] Une opération refusée affiche un message de droits plutôt qu'un succès trompeur.

## MT-RES-006 - Messagerie

**Types** : Service Bus Namespace, Event Hub Namespace.

### Étapes

1. [ ] Créer un Service Bus Namespace.
2. [ ] Ajouter une queue.
3. [ ] Ajouter un topic et une subscription si les actions sont disponibles.
4. [ ] Créer un Event Hub Namespace.
5. [ ] Ajouter un Event Hub et un consumer group.
6. [ ] Ouvrir les parents et vérifier les compteurs enfants.

### Résultat attendu

* [ ] Les enfants sont rattachés au namespace correct.
* [ ] Les actions de suppression demandent confirmation.
* [ ] Event Hub Namespace ne réclame pas une étape environnement inexistante.
* [ ] Les modifications restent après rechargement.

## MT-RES-007 - Monitoring, configuration, réseau et AI

**Types** : Log Analytics Workspace, Application Insights, App Configuration, Virtual Network, Document Intelligence.

### Étapes

1. [ ] Créer un Log Analytics Workspace et une Application Insights.
2. [ ] Vérifier le regroupement ou les liens entre les ressources lorsque proposés.
3. [ ] Créer une App Configuration et ajouter une configuration key.
4. [ ] Créer un Virtual Network avec plusieurs address spaces.
5. [ ] Ajouter des DNS servers facultatifs et configurer DDoS par environnement.
6. [ ] Créer une ressource Document Intelligence et tester ses paramètres environnementaux.

### Résultat attendu

* [ ] Les champs spécifiques sont visibles dans la bonne section.
* [ ] Les listes d'adresses réseau acceptent uniquement les formats prévus.
* [ ] Le DDoS est géré par environnement pour le Virtual Network.
* [ ] Les clés App Configuration sont persistées et visibles dans la ressource.
* [ ] Document Intelligence apparaît dans la génération et dans le picker.

## MT-RES-008 - Ressource existante

**Feature testée** : mode `existing` d'une ressource.

**Comportement attendu** : une ressource existante reste identifiable et ne déclenche pas de diagnostic d'environnement comme une nouvelle ressource.

### Étapes

1. [ ] Ajouter ou ouvrir une ressource configurée comme existante dans l'environnement de test.
2. [ ] Vérifier le bandeau `Existing resource`.
3. [ ] Observer les champs désactivés ou non applicables.
4. [ ] Vérifier la génération ou le diagnostic de configuration.

### Résultat attendu

* [ ] Le statut existing est visible.
* [ ] Le formulaire ne demande pas des paramètres de création inutiles.
* [ ] Les warnings d'environnement ne comptent pas cette ressource comme une nouvelle ressource incomplète.
* [ ] Le résultat généré respecte le statut existing.

## Verdict matrice

| Type ou famille | Statut | Preuve | Ticket |
|---|---|---|---|
| App Service Plan | | | |
| Web App | | | |
| Function App | | | |
| Container App Environment | | | |
| Container App | | | |
| Storage Account et enfants | | | |
| Cosmos DB | | | |
| Redis Cache | | | |
| SQL Server et SQL Database | | | |
| Container Registry | | | |
| Key Vault | | | |
| User Assigned Identity | | | |
| Service Bus Namespace et enfants | | | |
| Event Hub Namespace et enfants | | | |
| Log Analytics Workspace | | | |
| Application Insights | | | |
| App Configuration et keys | | | |
| Virtual Network | | | |
| Document Intelligence | | | |
