---
title: Smoke test du golden path
description: Vérification rapide du parcours modéliser, générer les artefacts et préparer leur push vers Git.
ms.date: 2026-09-02
ms.topic: testing
---

## Smoke test du golden path

Cette fiche constitue la vérification courte à exécuter après chaque version importante. Elle couvre le chemin principal du produit, de la modélisation au dépôt Git.

## Préconditions

* [ ] [00-preparation.md](00-preparation.md) est terminée.
* [ ] Le compte connecté possède le rôle `Owner`.
* [ ] Un dépôt Git de test est disponible si le push doit être exécuté.
* [ ] Le navigateur ouvre le site sans erreur de chargement.

## MT-SMOKE-001 - Se connecter

**Feature testée** : authentification Microsoft Entra ID.

**Comportement attendu** : le site redirige vers Microsoft, revient vers l'application après une authentification réussie et ouvre la page d'accueil. Une session valide reste utilisable après un rechargement.

### Étapes

1. [ ] Ouvrir l'URL du frontend en navigation privée.
2. [ ] Vérifier que la page `/login` affiche le bouton de connexion Microsoft.
3. [ ] Cliquer sur `Sign in with Microsoft`.
4. [ ] Terminer l'authentification avec le compte de recette.
5. [ ] Vérifier le retour sur l'application.
6. [ ] Recharger la page.

### Résultat attendu

* [ ] La page d'accueil s'affiche.
* [ ] Le nom ou l'avatar du compte est visible dans la barre supérieure.
* [ ] Le rechargement ne renvoie pas vers `/login`.
* [ ] Aucun message d'erreur d'authentification ne reste affiché.

## MT-SMOKE-002 - Créer un projet minimal

**Feature testée** : assistant de création de projet.

**Comportement attendu** : l'assistant crée un projet avec un layout, un dépôt, des environnements et les métadonnées saisies.

### Étapes

1. [ ] Ouvrir `Projects` puis cliquer sur `Create project`.
2. [ ] Dans l'étape d'identité, saisir un nom unique et une description courte.
3. [ ] Choisir `AllInOne`.
4. [ ] Ajouter le dépôt Git de test et saisir son PAT dans le champ prévu.
5. [ ] Lancer la vérification de connexion du dépôt.
6. [ ] Sélectionner la branche par défaut parmi les branches vérifiées.
7. [ ] Ajouter au moins les environnements Development, Staging et Production.
8. [ ] Parcourir l'étape de revue.
9. [ ] Valider la création.

### Résultat attendu

* [ ] Chaque étape bloque le passage si un champ obligatoire est invalide.
* [ ] La branche n'est sélectionnable qu'après une vérification réussie.
* [ ] Le projet apparaît dans la liste des projets.
* [ ] Le projet contient le layout choisi, le dépôt et les environnements.

## MT-SMOKE-003 - Ajouter une configuration et un groupe de ressources

**Feature testée** : configuration d'infrastructure et groupe de ressources.

**Comportement attendu** : une configuration appartient au projet et un groupe de ressources peut être ajouté avec une localisation valide.

### Étapes

1. [ ] Ouvrir le projet créé.
2. [ ] Dans `Configurations`, cliquer sur `Add configuration`.
3. [ ] Saisir `primary` ou un nom unique.
4. [ ] Enregistrer.
5. [ ] Ouvrir la configuration.
6. [ ] Dans `Resource groups`, cliquer sur `Add resource group`.
7. [ ] Saisir un nom et choisir une location.
8. [ ] Enregistrer.
9. [ ] Déplier le groupe de ressources.

### Résultat attendu

* [ ] La configuration apparaît dans le projet.
* [ ] Le groupe de ressources apparaît avec son nom et sa location.
* [ ] L'état vide disparaît après création.
* [ ] Le groupe peut être déplié et affiche l'état des ressources.

## MT-SMOKE-004 - Ajouter une ressource et vérifier son édition

**Feature testée** : catalogue des ressources Azure.

**Comportement attendu** : l'utilisateur choisit un type, remplit ses champs, sauvegarde et retrouve la ressource dans le groupe de ressources.

### Étapes

1. [ ] Dans le groupe de ressources, cliquer sur `Add resource`.
2. [ ] Choisir `Storage Account` ou `Key Vault`.
3. [ ] Saisir un nom et les valeurs obligatoires.
4. [ ] Compléter les environnements demandés.
5. [ ] Enregistrer.
6. [ ] Ouvrir la ressource depuis la liste.
7. [ ] Modifier une valeur non sensible.
8. [ ] Enregistrer puis revenir à la configuration.

### Résultat attendu

* [ ] La ressource apparaît une seule fois dans le groupe.
* [ ] Les valeurs sauvegardées sont conservées après navigation.
* [ ] Le message de succès de sauvegarde s'affiche.
* [ ] La ressource reste rattachée au bon groupe de ressources.

## MT-SMOKE-005 - Générer les artefacts

**Feature testée** : génération Bicep, Pipeline et Bootstrap.

**Comportement attendu** : la génération produit les artefacts correspondant au layout et permet de les consulter et de les télécharger.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>/generate`.
2. [ ] Vérifier le layout et les compteurs affichés.
3. [ ] Cliquer sur `Generate` ou `Generate all`.
4. [ ] Attendre la fin complète de la génération.
5. [ ] Ouvrir les onglets `Bicep`, `Pipeline` et `Bootstrap` disponibles.
6. [ ] Ouvrir un fichier dans l'explorateur.
7. [ ] Utiliser le téléchargement de l'onglet courant.

### Résultat attendu

* [ ] Les états de chargement sont visibles pendant la génération.
* [ ] Les trois familles d'artefacts finissent sans erreur sur un jeu de données valide.
* [ ] L'arborescence contient les fichiers attendus.
* [ ] Le contenu du fichier affiché correspond à son nom.
* [ ] Le ZIP téléchargé est lisible et contient uniquement les artefacts attendus.

## MT-SMOKE-006 - Préparer un push Git

**Feature testée** : push des artefacts générés.

**Comportement attendu** : le dialogue demande une branche et un message de commit, puis affiche le résultat du provider Git sans exposer le PAT.

### Étapes

1. [ ] Ouvrir la fonctionnalité de push depuis l'écran de génération correspondant au layout.
2. [ ] Attendre le chargement des branches.
3. [ ] Choisir une branche de destination de recette.
4. [ ] Saisir un message de commit explicite.
5. [ ] Vérifier que le bouton reste désactivé tant que les champs obligatoires sont invalides.
6. [ ] Lancer le push si le dépôt de test est disponible.

### Résultat attendu

* [ ] Le PAT n'apparaît jamais dans le dialogue ni dans le résultat.
* [ ] Le bouton est désactivé pendant le chargement des branches.
* [ ] Le provider retourne une URL de branche, un SHA et un nombre de fichiers en cas de succès.
* [ ] Une erreur technique affiche un message compréhensible et non un détail interne du provider.

## Verdict smoke

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-SMOKE-001 | | | |
| MT-SMOKE-002 | | | |
| MT-SMOKE-003 | | | |
| MT-SMOKE-004 | | | |
| MT-SMOKE-005 | | | |
| MT-SMOKE-006 | | | |
