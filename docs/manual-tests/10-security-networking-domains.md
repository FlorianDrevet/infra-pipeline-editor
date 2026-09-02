---
title: Tests de sécurité, réseau et domaines
description: Recette manuelle des rôles, identités, paramètres sécurisés, privatisation, Private Endpoint, ACR et domaines personnalisés.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests de sécurité, réseau et domaines

Ces scénarios mélangent le modèle métier affiché dans le site et des prérequis Azure réels. Les opérations Azure destructives doivent rester limitées à une subscription de test.

## MT-SEC-001 - Droits Reader, Contributor et Owner

**Feature testée** : contrôle d'accès au niveau projet.

**Comportement attendu** : le site adapte les actions disponibles au rôle et le serveur refuse toute écriture non autorisée.

### Étapes

1. [ ] Avec un Owner, ouvrir le projet et noter les actions visibles.
2. [ ] Se connecter avec un Reader.
3. [ ] Ouvrir le projet, une configuration, une ressource et une génération existante.
4. [ ] Vérifier les actions Ajouter, Modifier, Supprimer, Générer et Pousser.
5. [ ] Recommencer avec un Contributor.
6. [ ] Tenter une action interdite directement si un bouton reste visible par erreur.

### Résultat attendu

* [ ] Reader peut consulter les données autorisées sans action d'écriture utilisable.
* [ ] Contributor peut modifier les zones prévues mais ne gère pas les opérations Owner.
* [ ] Owner peut gérer membres, suppression et topologie.
* [ ] Une tentative interdite retourne une erreur d'autorisation, pas un succès.

## MT-SEC-002 - Rôles et identités Azure

**Feature testée** : Identity and access, User Assigned Identity et role assignments.

**Comportement attendu** : une identité peut être attachée à une ressource compatible et les rôles accordés sont visibles et réversibles.

### Étapes

1. [ ] Créer une User Assigned Identity.
2. [ ] Ouvrir une ressource compatible et son onglet `Identity and access`.
3. [ ] Attacher l'identité.
4. [ ] Ajouter un rôle depuis `Add role assignment`.
5. [ ] Vérifier le statut du rôle dans la liste.
6. [ ] Ouvrir l'analyse d'impact si disponible.
7. [ ] Retirer le rôle puis l'identité.

### Résultat attendu

* [ ] L'identité sélectionnée est affichée avec son nom.
* [ ] Le rôle choisi apparaît sur la bonne ressource.
* [ ] Un rôle nécessitant une identité signale cette exigence.
* [ ] La suppression actualise la liste et demande confirmation.
* [ ] Les rôles générés sont visibles dans le Bicep attendu.

## MT-SEC-003 - App settings et références Key Vault

**Feature testée** : app settings statiques, outputs et secrets Key Vault.

**Comportement attendu** : l'utilisateur peut définir la source d'une valeur et le site signale les droits Key Vault manquants.

### Étapes

1. [ ] Ouvrir une Web App, Function App ou Container App.
2. [ ] Ouvrir l'onglet `App settings`.
3. [ ] Ajouter un setting statique.
4. [ ] Ajouter un setting basé sur un output ou un secret Key Vault si l'option est disponible.
5. [ ] Modifier puis supprimer le setting.
6. [ ] Tester la ressource sans rôle Key Vault suffisant.
7. [ ] Utiliser l'action d'aide ou d'affectation de rôle affichée.
8. [ ] Regénérer le Bicep et Pipeline.

### Résultat attendu

* [ ] Les modes statique, output et secret sont distingués.
* [ ] La liste des settings est rafraîchie après chaque opération.
* [ ] Un rôle Key Vault manquant donne un warning explicite.
* [ ] Le Bicep et le pipeline reprennent la source choisie sans afficher la valeur secrète.

## MT-SEC-004 - Mapping de paramètre SQL sécurisé

**Feature testée** : mot de passe SQL Server et variable group.

**Comportement attendu** : le mot de passe n'est jamais saisi en clair dans un artefact et peut être fourni aléatoirement ou par un variable group.

### Étapes

1. [ ] Ouvrir un SQL Server.
2. [ ] Ouvrir la section `Secure parameter`.
3. [ ] Tester le mode de génération aléatoire.
4. [ ] Tester le mode `Variable group`.
5. [ ] Sélectionner un groupe de test et un nom de variable.
6. [ ] Essayer d'enregistrer sans groupe ou sans nom de variable.
7. [ ] Générer le Pipeline et le Bicep.

### Résultat attendu

* [ ] Le mode choisi est explicite.
* [ ] Le mode variable group exige ses deux valeurs.
* [ ] Le mot de passe n'apparaît pas dans le viewer, le YAML ou les captures.
* [ ] Le mapping apparaît dans le pipeline attendu.

## MT-NET-001 - Configurer un Virtual Network

**Feature testée** : Virtual Network et paramètres par environnement.

**Comportement attendu** : le site gère les address spaces, DNS servers et DDoS protection au niveau de chaque environnement.

### Étapes

1. [ ] Créer ou ouvrir un Virtual Network.
2. [ ] Ouvrir l'étape `Environments`.
3. [ ] Saisir une address space CIDR valide pour Development.
4. [ ] Saisir plusieurs address spaces si le contrôle le permet.
5. [ ] Saisir un DNS server IPv4 facultatif.
6. [ ] Activer DDoS pour Production seulement.
7. [ ] Saisir une valeur invalide et tenter de sauvegarder.
8. [ ] Recharger puis générer le Bicep.

### Résultat attendu

* [ ] Les address spaces et DNS servers utilisent les contrôles IP prévus.
* [ ] DDoS est indépendant pour chaque environnement.
* [ ] Les formats invalides sont refusés.
* [ ] Les valeurs apparaissent dans les fichiers de paramètres de l'environnement concerné.

## MT-NET-002 - Privatisation et Private Endpoint

**Feature testée** : accès privé d'une ressource.

**Comportement attendu** : l'utilisateur configure le VNet, le subnet et le mode DNS avant de privatiser une ressource; le Bicep ne désactive pas l'accès public sans chemin privé associé.

### Étapes

1. [ ] Ouvrir une ressource privatisable non existante, comme Web App, Function App ou Container App.
2. [ ] Ouvrir l'onglet `Networking`.
3. [ ] Configurer le Private Endpoint avec VNet, subnet et mode DNS.
4. [ ] Sauvegarder.
5. [ ] Activer la privatisation.
6. [ ] Vérifier le résumé de la configuration.
7. [ ] Générer le Bicep.
8. [ ] Tester le cas inverse : privatiser sans configuration Private Endpoint.

### Résultat attendu

* [ ] Les champs VNet, subnet et DNS sont requis ou expliqués selon le mode choisi.
* [ ] La configuration est visible après rechargement.
* [ ] La génération contient le Private Endpoint et la désactivation d'accès public cohérente.
* [ ] Le site bloque ou avertit clairement une privatisation sans point d'entrée privé.

**Suivi connu** : D11 signale le risque d'une ressource privatisée sans Private Endpoint. Si le site accepte encore ce cas et génère une ressource inaccessible, classer `KNOWN-ISSUE`.

## MT-NET-003 - DNS Private Endpoint

**Feature testée** : modes DNS du Private Endpoint.

**Comportement attendu** : le mode DNS choisi est conservé et influence le module généré.

### Étapes

1. [ ] Tester le mode automatique ou géré.
2. [ ] Tester le mode hub existant avec les valeurs de recette.
3. [ ] Recharger la ressource.
4. [ ] Regénérer le Bicep.
5. [ ] Comparer les modules générés.

### Résultat attendu

* [ ] Le mode sélectionné est affiché après rechargement.
* [ ] Les valeurs de réseau ne se mélangent pas entre les deux modes.
* [ ] Le Bicep ne génère pas une zone DNS contradictoire avec le mode choisi.

## MT-DOMAIN-001 - Ajouter un domaine personnalisé

**Feature testée** : domaines personnalisés sur Web App, Function App et Container App.

**Comportement attendu** : un domaine est rattaché à l'environnement et son état de validation est visible.

### Étapes

1. [ ] Ouvrir une ressource compatible.
2. [ ] Ouvrir la section `Custom domains`.
3. [ ] Ajouter un domaine de recette.
4. [ ] Sélectionner l'environnement si le dialogue le demande.
5. [ ] Enregistrer.
6. [ ] Ouvrir les instructions DNS.
7. [ ] Pour Container App, vérifier que les instructions ciblent la Container App, son ingress et la section Domain validation.
8. [ ] Ajouter une valeur invalide et tester la validation du formulaire.

### Résultat attendu

* [ ] Le domaine apparaît avec son environnement et son statut.
* [ ] Les instructions sont localisées et ne recopient pas un titre backend brut lorsqu'un libellé connu existe.
* [ ] Le bandeau rappelle que l'infrastructure doit être déployée avant de récupérer les valeurs DNS Azure.
* [ ] Les domaines non validés ne sont pas présentés comme déployables.

## MT-DOMAIN-002 - Valider un domaine et générer

**Feature testée** : workflow DNS puis génération.

**Comportement attendu** : un domaine validé peut être inclus dans la génération; un domaine en attente est exclu ou bloque selon le diagnostic annoncé.

### Étapes

1. [ ] Déployer ou préparer le domaine dans l'environnement Azure de test.
2. [ ] Récupérer les valeurs de validation demandées par Azure.
3. [ ] Mettre à jour la zone DNS.
4. [ ] Utiliser `Validate DNS` dans le site.
5. [ ] Regénérer le Bicep.
6. [ ] Refaire le scénario avec un domaine non validé.

### Résultat attendu

* [ ] Le statut passe à `Validated` uniquement après le parcours prévu.
* [ ] Le domaine validé apparaît dans les paramètres générés.
* [ ] Le domaine en attente génère un warning et n'est pas injecté silencieusement dans les paramètres.

**Suivi connu** : D08 indique que l'ancien handler pouvait valider sans vérifier le DNS. Si la validation passe sans preuve DNS externe, classer `KNOWN-ISSUE`.

## MT-ACR-001 - Accès ACR par identité managée

**Feature testée** : Container Registry, identité de pull et rôle AcrPull.

**Comportement attendu** : le site vérifie l'accès ACR et permet de corriger une identité ou un rôle manquant.

### Étapes

1. [ ] Ouvrir une ressource compute en mode container.
2. [ ] Sélectionner un Container Registry.
3. [ ] Choisir `Managed Identity`.
4. [ ] Observer le diagnostic d'accès ACR.
5. [ ] Sélectionner ou créer une User Assigned Identity.
6. [ ] Ajouter le rôle AcrPull.
7. [ ] Relancer la vérification.
8. [ ] Sauvegarder et générer le pipeline.

### Résultat attendu

* [ ] Le diagnostic distingue identité absente, rôle absent et accès confirmé.
* [ ] L'action d'ajout de rôle utilise le rôle attendu.
* [ ] L'état passe à OK après correction.
* [ ] Le pipeline généré utilise la service connection par environnement lorsqu'elle est configurée.

## MT-ACR-002 - Mode Admin Credentials

**Feature testée** : alternative d'authentification ACR.

**Comportement attendu** : le mode Admin Credentials ne force pas le workflow d'identité managée.

### Étapes

1. [ ] Choisir `Admin Credentials`.
2. [ ] Vérifier les sections affichées.
3. [ ] Passer de nouveau à `Managed Identity` avec un ACR sélectionné.
4. [ ] Observer le retour du diagnostic.
5. [ ] Effacer le Container Registry.

### Résultat attendu

* [ ] Le mode Admin Credentials masque ou désactive les contrôles AcrPull non applicables.
* [ ] Le retour à Managed Identity relance le contrôle d'accès.
* [ ] Effacer l'ACR efface les valeurs dépendantes.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-SEC-001 | | | |
| MT-SEC-002 | | | |
| MT-SEC-003 | | | |
| MT-SEC-004 | | | |
| MT-NET-001 | | | |
| MT-NET-002 | | | |
| MT-NET-003 | | | |
| MT-DOMAIN-001 | | | |
| MT-DOMAIN-002 | | | |
| MT-ACR-001 | | | |
| MT-ACR-002 | | | |
