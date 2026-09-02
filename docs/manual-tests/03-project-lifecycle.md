---
title: Tests du cycle de vie projet
description: Scénarios manuels de création, administration et suppression d'un projet InfraFlowSculptor.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests du cycle de vie projet

Cette fiche vérifie qu'un projet conserve une structure cohérente entre son identité, son layout, ses dépôts, ses environnements et ses configurations.

## MT-PROJECT-001 - Créer un projet AllInOne

**Feature testée** : assistant de création de projet.

**Comportement attendu** : l'assistant crée un projet complet en quatre étapes : identité, layout et dépôts, environnements, revue.

### Étapes

1. [ ] Ouvrir `Projects` puis `Create project`.
2. [ ] Dans l'étape d'identité, saisir un nom unique et une description.
3. [ ] Sélectionner le layout `AllInOne`.
4. [ ] Ajouter un dépôt principal avec URL, provider, PAT et branche vérifiés.
5. [ ] Dans l'étape des environnements, ajouter Development, Staging et Production avec leur ordre.
6. [ ] Parcourir la revue sans modifier les valeurs.
7. [ ] Valider la création.
8. [ ] Ouvrir le projet créé depuis la liste.

### Résultat attendu

* [ ] Le projet apparaît avec le nom et la description saisis.
* [ ] Le layout affiché est `AllInOne`.
* [ ] Le dépôt et sa branche sont visibles dans la configuration du projet.
* [ ] Les environnements apparaissent dans l'ordre défini.
* [ ] Une configuration peut être ajoutée depuis le détail du projet.

## MT-PROJECT-002 - Validation des champs de l'assistant

**Feature testée** : validations de l'assistant.

**Comportement attendu** : l'assistant refuse les données incomplètes ou incohérentes et garde les valeurs valides déjà saisies.

### Étapes

1. [ ] Ouvrir l'assistant.
2. [ ] Tenter de continuer avec le nom vide.
3. [ ] Saisir un nom puis laisser les champs obligatoires du dépôt incomplets.
4. [ ] Tenter de continuer.
5. [ ] Saisir une URL invalide et un PAT de test invalide.
6. [ ] Lancer la vérification du dépôt.
7. [ ] Ajouter deux environnements portant le même identifiant court si le formulaire le permet.
8. [ ] Vérifier le blocage de la revue ou de la soumission.

### Résultat attendu

* [ ] Chaque champ invalide affiche une erreur proche du champ.
* [ ] Le passage à l'étape suivante reste bloqué tant qu'une erreur persiste.
* [ ] La vérification d'un dépôt invalide ne sauvegarde pas le dépôt.
* [ ] Les environnements ne peuvent pas produire de doublon technique.
* [ ] Les valeurs déjà valides ne sont pas effacées par une erreur sur un autre champ.

## MT-PROJECT-003 - Créer un projet SplitInfraCode

**Feature testée** : topologie projet infrastructure/code.

**Comportement attendu** : le layout `SplitInfraCode` demande deux dépôts distincts, un pour l'infrastructure et un pour l'application.

### Étapes

1. [ ] Ouvrir l'assistant de création.
2. [ ] Choisir `SplitInfraCode`.
3. [ ] Configurer le dépôt Infrastructure.
4. [ ] Configurer le dépôt ApplicationCode.
5. [ ] Vérifier les types de contenu affichés pour chaque dépôt.
6. [ ] Vérifier les branches des deux dépôts.
7. [ ] Créer le projet.
8. [ ] Ouvrir `/projects/<project-id>/generate/config`.

### Résultat attendu

* [ ] Les deux emplacements sont visibles dans la revue.
* [ ] Un dépôt ne peut pas être présenté comme les deux slots si le formulaire exige deux dépôts.
* [ ] Le board de génération affiche les deux cibles.
* [ ] Les actions de génération et de push distinguent Infrastructure et ApplicationCode.

## MT-PROJECT-004 - Créer un projet MultiRepo

**Feature testée** : topologie avec dépôts appartenant aux configurations.

**Comportement attendu** : `MultiRepo` n'utilise pas un unique dépôt projet pour la génération globale; les dépôts sont configurés au niveau de chaque configuration.

### Étapes

1. [ ] Créer un projet avec le layout `MultiRepo`.
2. [ ] Ne pas ajouter de dépôt projet si l'assistant ne propose pas de slot applicable.
3. [ ] Créer au moins deux configurations dans le projet.
4. [ ] Ouvrir chaque configuration.
5. [ ] Ouvrir l'onglet `Git` visible dans le contexte `MultiRepo`.
6. [ ] Ajouter un dépôt propre à chaque configuration.
7. [ ] Revenir au board `/projects/<project-id>/generate`.

### Résultat attendu

* [ ] Le layout est affiché comme `MultiRepo`.
* [ ] Chaque configuration peut posséder son dépôt.
* [ ] Les dépôts d'une configuration ne se mélangent pas avec ceux d'une autre.
* [ ] Le board propose le push bulk MultiRepo lorsque les cibles et artefacts sont prêts.
* [ ] Le bouton de génération projet global n'est pas présenté comme disponible pour `MultiRepo`.

## MT-PROJECT-005 - Ajouter et supprimer une configuration

**Feature testée** : CRUD des configurations depuis le détail projet.

**Comportement attendu** : une configuration est créée dans le projet courant et peut être supprimée avec confirmation.

### Étapes

1. [ ] Ouvrir l'onglet `Configurations` du projet.
2. [ ] Cliquer sur `Add configuration`.
3. [ ] Saisir un nom unique et sauvegarder.
4. [ ] Vérifier la nouvelle carte de configuration.
5. [ ] Ouvrir la configuration.
6. [ ] Revenir au projet.
7. [ ] Cliquer sur l'action de suppression de la configuration.
8. [ ] Annuler dans le dialogue.
9. [ ] Recommencer et confirmer.

### Résultat attendu

* [ ] La nouvelle configuration est visible sans rechargement incohérent.
* [ ] L'annulation laisse la configuration intacte.
* [ ] La confirmation supprime la configuration de la liste.
* [ ] Les ressources liées suivent la politique de suppression annoncée par le dialogue.

## MT-PROJECT-006 - Gérer les environnements du projet

**Feature testée** : environnements Development, Staging, Production ou équivalents.

**Comportement attendu** : l'Owner peut créer, modifier et supprimer les environnements avec location, subscription, ordre et approbation.

### Étapes

1. [ ] Ouvrir l'onglet `Environments` du projet.
2. [ ] Ajouter un environnement de recette avec nom, short name, location et subscription de test.
3. [ ] Activer ou désactiver l'approbation selon le cas de test.
4. [ ] Enregistrer.
5. [ ] Modifier sa location ou son approbation.
6. [ ] Recharger la page.
7. [ ] Supprimer l'environnement dans le dialogue de confirmation.

### Résultat attendu

* [ ] Les champs sont visibles dans le panneau de l'environnement.
* [ ] L'ordre affiché respecte l'ordre technique.
* [ ] Une modification réapparaît après rechargement.
* [ ] La suppression demande une confirmation et met à jour les compteurs.
* [ ] Une configuration de ressource ne peut pas masquer silencieusement un environnement manquant.

## MT-PROJECT-007 - Vérifier membres et rôles

**Feature testée** : membres du projet et autorisations.

**Comportement attendu** : les rôles Owner, Contributor et Reader produisent des droits différents dans les écrans et sur les actions d'écriture.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>/members` ou l'entrée `Members` de la sidebar.
2. [ ] Ajouter un utilisateur de test avec le rôle `Reader`.
3. [ ] Vérifier sa ligne puis modifier son rôle en `Contributor`.
4. [ ] Se connecter avec le compte concerné dans une autre session.
5. [ ] Vérifier qu'il peut consulter le projet et les artefacts.
6. [ ] Vérifier les actions d'ajout, modification, suppression et génération.
7. [ ] Tester le rôle `Owner` dans une session autorisée.
8. [ ] Retirer l'utilisateur de test.

### Résultat attendu

* [ ] Reader conserve l'accès de lecture et ne voit pas d'action d'écriture utilisable.
* [ ] Contributor peut modifier les données autorisées mais ne réalise pas les actions réservées à Owner.
* [ ] Owner peut gérer membres, suppression et configuration du projet.
* [ ] La suppression d'un membre demande une confirmation et actualise la liste.

## MT-PROJECT-008 - Vérifier les onglets de détail projet

**Feature testée** : agrégation des paramètres projet.

**Comportement attendu** : les onglets Configurations, Environments, Naming, Tags et Pipeline Variables affichent le bon périmètre projet.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>`.
2. [ ] Parcourir chaque onglet.
3. [ ] Ajouter un tag projet et le sauvegarder.
4. [ ] Ajouter un template de naming ou une abréviation.
5. [ ] Ajouter un groupe de variables de pipeline si les données de recette le permettent.
6. [ ] Recharger la page et vérifier les compteurs des onglets.

### Résultat attendu

* [ ] Chaque onglet ne montre que les données du projet.
* [ ] Les compteurs reflètent le nombre d'éléments présents.
* [ ] Les modifications restent après navigation et rechargement.
* [ ] Les erreurs de sauvegarde restent localisées à la section concernée.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-PROJECT-001 | | | |
| MT-PROJECT-002 | | | |
| MT-PROJECT-003 | | | |
| MT-PROJECT-004 | | | |
| MT-PROJECT-005 | | | |
| MT-PROJECT-006 | | | |
| MT-PROJECT-007 | | | |
| MT-PROJECT-008 | | | |
