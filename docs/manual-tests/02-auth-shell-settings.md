---
title: Authentification, shell et préférences
description: Tests manuels de la connexion, de la navigation globale et des réglages utilisateur et projet.
ms.date: 2026-09-02
ms.topic: testing
---

## Authentification, shell et préférences

Cette fiche vérifie les surfaces transverses visibles avant d'entrer dans un projet.

## MT-AUTH-001 - Page de connexion et retour Entra ID

**Feature testée** : connexion Microsoft Entra ID.

**Comportement attendu** : une session absente voit la page de connexion; une session valide revient vers la page demandée ou vers l'accueil.

### Étapes

1. [ ] Supprimer les cookies du site ou ouvrir une fenêtre privée.
2. [ ] Ouvrir l'URL d'une page protégée, par exemple `/projects`.
3. [ ] Vérifier la redirection vers `/login`.
4. [ ] Vérifier la présence du bouton Microsoft et de son état de chargement après clic.
5. [ ] Se connecter.
6. [ ] Revenir à l'URL initiale si l'application le permet.

### Résultat attendu

* [ ] Les pages protégées ne montrent pas de données avant authentification.
* [ ] Le bouton Microsoft n'est pas cliquable plusieurs fois pendant la redirection.
* [ ] Le retour d'authentification ouvre une page fonctionnelle.
* [ ] Une erreur d'authentification affiche un message localisé et permet de réessayer.

## MT-AUTH-002 - Session expirée ou accès refusé

**Feature testée** : gestion des réponses non authentifiées.

**Comportement attendu** : une réponse `401` renvoie vers `/login` sans boucle ni accumulation de redirections.

### Étapes

1. [ ] Se connecter puis ouvrir `/projects`.
2. [ ] Expirer ou révoquer la session dans l'environnement de test, selon la procédure Entra disponible.
3. [ ] Recharger la page.
4. [ ] Répéter une navigation vers une page protégée.

### Résultat attendu

* [ ] L'application revient vers `/login`.
* [ ] Une seule redirection est lancée.
* [ ] Aucun bandeau d'erreur transitoire ne reste bloqué après le retour sur la page de connexion.

## MT-SHELL-001 - Accueil et liste des projets

**Feature testée** : dashboard et page `Projects`.

**Comportement attendu** : l'utilisateur retrouve ses projets, peut les rechercher, les trier et filtrer ses favoris.

### Étapes

1. [ ] Ouvrir la page d'accueil.
2. [ ] Vérifier les panneaux de favoris et d'éléments récents.
3. [ ] Ouvrir `Projects` depuis la navigation.
4. [ ] Saisir une partie du nom d'un projet dans la recherche.
5. [ ] Effacer la recherche.
6. [ ] Modifier le tri.
7. [ ] Activer `Favorites only`.
8. [ ] Marquer un projet comme favori puis désactiver le filtre.
9. [ ] Ouvrir un projet et revenir à la liste.

### Résultat attendu

* [ ] La recherche filtre les cartes sans rechargement incohérent.
* [ ] Le tri change l'ordre des cartes.
* [ ] Le favori est conservé après navigation et apparaît dans la section dédiée.
* [ ] L'état vide et l'état aucun résultat sont distincts et compréhensibles.
* [ ] Un projet récemment ouvert peut être retrouvé dans les éléments récents.

## MT-SHELL-002 - Navigation contextuelle et onglets par URL

**Feature testée** : sidebar globale et contextuelle, breadcrumbs et onglets.

**Comportement attendu** : la navigation change selon le contexte projet ou configuration; un onglet sélectionné reste partageable par URL.

### Étapes

1. [ ] Ouvrir un projet.
2. [ ] Vérifier que la sidebar affiche les rubriques du contexte projet.
3. [ ] Cliquer sur `Génération`.
4. [ ] Revenir au détail projet.
5. [ ] Ouvrir directement une URL avec `?tab=environments`, `?tab=naming`, `?tab=tags` ou `?tab=variables`.
6. [ ] Ouvrir une configuration et tester `?tab=tags`, `?tab=naming`, `?tab=cross-config-refs` et `?tab=variables`.
7. [ ] Sur un projet `MultiRepo`, tester `?tab=git`.
8. [ ] Sur un projet non `MultiRepo`, tester `?tab=git`.
9. [ ] Recharger chaque page.

### Résultat attendu

* [ ] Le breadcrumb reflète le projet et la configuration courants.
* [ ] Le bon onglet reste actif après rechargement.
* [ ] Le Git tab est visible uniquement pour `MultiRepo`.
* [ ] Une URL Git impossible sur un autre layout revient vers l'onglet par défaut.
* [ ] Les liens de la sidebar et l'onglet affiché restent cohérents.

## MT-SETTINGS-001 - Langue de l'application

**Feature testée** : préférences FR/EN.

**Comportement attendu** : le changement de langue met à jour les libellés et reste présent après rechargement.

### Étapes

1. [ ] Ouvrir `Settings` depuis la barre supérieure.
2. [ ] Sélectionner `EN`.
3. [ ] Vérifier le titre, la navigation, les boutons et un dialogue.
4. [ ] Recharger la page.
5. [ ] Sélectionner `FR`.
6. [ ] Ouvrir un projet puis une configuration pour vérifier les onglets.

### Résultat attendu

* [ ] Les textes visibles basculent dans la langue choisie.
* [ ] Les libellés des selects et onglets ne restent pas dans l'ancienne langue.
* [ ] Le choix survit au rechargement.
* [ ] Aucun identifiant de clé i18n brut ne s'affiche pour un parcours supporté.

## MT-SETTINGS-002 - Thème de l'application

**Feature testée** : thème clair/sombre ou options de contraste de l'application.

**Comportement attendu** : le thème sélectionné s'applique au shell et aux pages sans rendre les textes ou les contrôles illisibles.

### Étapes

1. [ ] Dans `Settings`, repérer la préférence `App theme`.
2. [ ] Sélectionner chaque option disponible.
3. [ ] Vérifier l'accueil, la liste des projets, un détail, un dialogue et un formulaire.
4. [ ] Recharger la page.

### Résultat attendu

* [ ] Le thème s'applique sans flash prolongé ni page blanche.
* [ ] Les textes, bordures, boutons, tabs et messages d'erreur restent lisibles.
* [ ] La préférence est conservée après rechargement.

## MT-SETTINGS-003 - Thème du lecteur Bicep

**Feature testée** : préférence de thème du viewer Bicep.

**Comportement attendu** : le choix modifie le rendu du lecteur de fichiers générés, sans modifier le contenu du fichier.

### Étapes

1. [ ] Dans `Settings`, repérer les aperçus de thèmes Bicep.
2. [ ] Sélectionner un thème.
3. [ ] Ouvrir un projet ayant une génération disponible.
4. [ ] Ouvrir un fichier Bicep.
5. [ ] Revenir dans `Settings` et sélectionner un autre thème.
6. [ ] Revenir au viewer.

### Résultat attendu

* [ ] Les couleurs de syntaxe ou de surface changent.
* [ ] Le texte Bicep et l'arborescence restent inchangés.
* [ ] Le thème sélectionné est conservé après rechargement.

## MT-SETTINGS-004 - PAT applicatif `ifs_...`

**Feature testée** : gestion des Personal Access Tokens de l'application.

**Comportement attendu** : un Owner peut créer un token avec ses scopes, voir son préfixe et le révoquer; le secret complet n'est affiché qu'au moment prévu.

### Étapes

1. [ ] Dans `Settings`, ouvrir la section des tokens.
2. [ ] Cliquer sur `Create token`.
3. [ ] Saisir un nom, une expiration si le formulaire la propose et les scopes `Read`, `Write` et/ou `Generate`.
4. [ ] Valider.
5. [ ] Copier le token uniquement dans un emplacement sécurisé de recette, jamais dans une capture.
6. [ ] Vérifier la ligne du token et son état.
7. [ ] Révoquer le token.
8. [ ] Confirmer l'action dans le dialogue.

### Résultat attendu

* [ ] La création affiche le token selon le contrat one-time.
* [ ] La liste affiche le nom, le préfixe, la date, l'expiration et le statut.
* [ ] Un token révoqué n'est plus présenté comme actif.
* [ ] La confirmation empêche une révocation accidentelle.

## MT-PROJECT-SETTINGS-001 - Agent pool

**Feature testée** : paramètres du projet.

**Comportement attendu** : l'Owner peut activer un agent pool personnalisé et le sauvegarder; les autres cartes indiquent clairement qu'elles sont à venir.

### Étapes

1. [ ] Depuis un projet, ouvrir `Settings`.
2. [ ] Activer `Use custom agent pool`.
3. [ ] Saisir un nom de pool.
4. [ ] Sauvegarder.
5. [ ] Recharger la page.
6. [ ] Vérifier les sections Notifications, Auto-generation, Retention et Service connections.

### Résultat attendu

* [ ] Le nom du pool est conservé.
* [ ] Le bouton Save apparaît uniquement lorsqu'une modification existe.
* [ ] Les sections non livrées sont marquées `Upcoming` et ne prétendent pas être configurables.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-AUTH-001 | | | |
| MT-AUTH-002 | | | |
| MT-SHELL-001 | | | |
| MT-SHELL-002 | | | |
| MT-SETTINGS-001 | | | |
| MT-SETTINGS-002 | | | |
| MT-SETTINGS-003 | | | |
| MT-SETTINGS-004 | | | |
| MT-PROJECT-SETTINGS-001 | | | |
