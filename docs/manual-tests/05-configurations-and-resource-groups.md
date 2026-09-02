---
title: Tests des configurations et groupes de ressources
description: Recette manuelle du détail d'une configuration, de ses onglets et de ses groupes de ressources Azure.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests des configurations et groupes de ressources

Cette fiche couvre la page `/config/<config-id>` et les opérations qui structurent une configuration avant l'ajout des ressources.

## MT-CONFIG-001 - Charger le détail d'une configuration

**Feature testée** : lecture d'une configuration.

**Comportement attendu** : le site charge la configuration, le projet parent, les groupes de ressources et les données secondaires sans mélanger deux configurations.

### Étapes

1. [ ] Ouvrir une carte de configuration depuis le détail projet.
2. [ ] Vérifier le breadcrumb projet > configuration.
3. [ ] Recharger la page.
4. [ ] Ouvrir directement `/config/<config-id>` dans un nouvel onglet.
5. [ ] Naviguer vers une autre configuration via un lien cross-config si disponible.

### Résultat attendu

* [ ] Le nom de la configuration et celui du projet sont corrects.
* [ ] Les groupes de ressources et leurs compteurs appartiennent à la configuration courante.
* [ ] Le chargement affiche un état d'attente puis le contenu ou une erreur claire.
* [ ] Le changement de configuration ne conserve pas les ressources de la précédente.

## MT-CONFIG-002 - Onglets de configuration

**Feature testée** : onglets Resource groups, Tags, Naming templates, Cross-config references et Pipeline variables.

**Comportement attendu** : chaque onglet affiche et modifie uniquement le périmètre configuration; `Git` apparaît seulement en `MultiRepo`.

### Étapes

1. [ ] Ouvrir chaque onglet depuis les tabs.
2. [ ] Recharger après un onglet sélectionné.
3. [ ] Tester les URLs `?tab=tags`, `?tab=naming`, `?tab=cross-config-refs` et `?tab=variables`.
4. [ ] Sur un projet `MultiRepo`, tester `?tab=git`.
5. [ ] Sur un projet non `MultiRepo`, tester `?tab=git`.

### Résultat attendu

* [ ] L'onglet demandé reste actif après rechargement.
* [ ] Les compteurs sont cohérents avec le contenu visible.
* [ ] Le Git tab est conditionné au layout `MultiRepo`.
* [ ] Une URL d'onglet invalide revient à la vue de la configuration sans écran vide.

## MT-CONFIG-003 - Créer un groupe de ressources

**Feature testée** : création d'un Resource Group.

**Comportement attendu** : le groupe est créé dans la configuration et utilise une location cohérente avec les environnements du projet.

### Étapes

1. [ ] Dans `Resource groups`, cliquer sur `Add resource group`.
2. [ ] Saisir un nom unique.
3. [ ] Choisir une location.
4. [ ] Enregistrer.
5. [ ] Recharger la configuration.
6. [ ] Déplier le groupe.

### Résultat attendu

* [ ] Le nom et la location sont affichés.
* [ ] Le nouveau groupe apparaît dans le compteur.
* [ ] Le groupe appartient à la configuration courante.
* [ ] Une erreur de doublon ou de validation reste visible dans le dialogue.

## MT-CONFIG-004 - Modifier et supprimer un groupe

**Feature testée** : maintenance d'un Resource Group.

**Comportement attendu** : la modification est sauvegardée et la suppression est protégée par une confirmation et une information sur les dépendances.

### Étapes

1. [ ] Ouvrir l'action `Edit` d'un groupe vide.
2. [ ] Modifier le nom ou la location autorisée.
3. [ ] Enregistrer puis recharger.
4. [ ] Créer un groupe contenant une ressource de recette.
5. [ ] Ouvrir `Delete`.
6. [ ] Annuler.
7. [ ] Rouvrir et confirmer sur les données de recette.

### Résultat attendu

* [ ] La modification réapparaît après rechargement.
* [ ] L'annulation ne supprime rien.
* [ ] La suppression confirme clairement son périmètre.
* [ ] La liste et les compteurs sont rafraîchis après suppression.
* [ ] Les dépendances ne produisent pas une erreur technique non expliquée.

## MT-CONFIG-005 - Aperçu par environnement

**Feature testée** : select `Preview environment` des groupes de ressources.

**Comportement attendu** : l'aperçu de naming ou de propriétés s'adapte à l'environnement sélectionné.

### Étapes

1. [ ] Ouvrir une configuration contenant au moins deux environnements.
2. [ ] Sélectionner `Development` dans le select d'aperçu.
3. [ ] Observer les previews de noms affichées dans les groupes ou ressources.
4. [ ] Sélectionner `Production`.
5. [ ] Choisir l'option sans environnement si elle existe.

### Résultat attendu

* [ ] Les valeurs de preview changent avec l'environnement.
* [ ] Le nom affiché dans l'aperçu ne modifie pas la donnée sauvegardée.
* [ ] L'absence d'environnement sélectionné ne produit pas de valeur incohérente.

## MT-CONFIG-006 - Déplier les ressources d'un groupe

**Feature testée** : chargement paresseux des ressources et regroupement parent/enfant.

**Comportement attendu** : le groupe charge ses ressources à l'ouverture et affiche les enfants sous leur parent.

### Étapes

1. [ ] Déplier un groupe de ressources.
2. [ ] Observer le spinner de chargement si la réponse prend du temps.
3. [ ] Déplier un parent comme Storage Account, App Service Plan, Container App Environment ou SQL Server.
4. [ ] Déplier un parent sans enfant.
5. [ ] Replier puis déplier à nouveau.

### Résultat attendu

* [ ] Le chargement n'affiche pas deux listes concurrentes.
* [ ] Les ressources restent dans leur groupe.
* [ ] Les enfants sont visuellement rattachés au parent.
* [ ] Un parent sans enfant affiche un état vide compréhensible.
* [ ] Le second dépliage réutilise les données ou recharge proprement sans doublon.

## MT-CONFIG-007 - États vides et erreurs

**Feature testée** : feedback de la page configuration.

**Comportement attendu** : une configuration sans groupe, un groupe sans ressource et une erreur de chargement sont distingués.

### Étapes

1. [ ] Ouvrir une configuration sans groupe.
2. [ ] Vérifier l'état vide et son CTA.
3. [ ] Créer un groupe vide.
4. [ ] Ouvrir le groupe.
5. [ ] Simuler une erreur réseau ou couper temporairement l'API dans un environnement de test.
6. [ ] Recharger et utiliser le bouton de retour disponible.

### Résultat attendu

* [ ] L'état vide propose une action adaptée.
* [ ] L'absence de ressource n'est pas présentée comme une erreur serveur.
* [ ] Une erreur réseau affiche un message et une navigation de récupération.
* [ ] Aucun ancien groupe ou ressource ne reste affiché après un échec de rechargement.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-CONFIG-001 | | | |
| MT-CONFIG-002 | | | |
| MT-CONFIG-003 | | | |
| MT-CONFIG-004 | | | |
| MT-CONFIG-005 | | | |
| MT-CONFIG-006 | | | |
| MT-CONFIG-007 | | | |
