---
title: Tests des layouts et des dépôts
description: Recette manuelle des layouts AllInOne, SplitInfraCode et MultiRepo ainsi que des connexions Git.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests des layouts et des dépôts

Cette fiche vérifie la séparation des responsabilités entre les trois topologies supportées et le comportement des dépôts associés.

## MT-TOPO-001 - Changer le layout du projet

**Feature testée** : choix du layout projet.

**Comportement attendu** : le site explique la topologie sélectionnée et applique les slots de dépôts correspondants.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>/generate/config`.
2. [ ] Repérer le preset de layout affiché.
3. [ ] Sélectionner `AllInOne`, puis observer le slot de dépôt principal.
4. [ ] Sélectionner `SplitInfraCode`, puis observer les slots Infrastructure et ApplicationCode.
5. [ ] Sélectionner `MultiRepo`, puis observer le comportement des dépôts par configuration.
6. [ ] Recharger après chaque sauvegarde.

### Résultat attendu

* [ ] Une seule option est sélectionnée à la fois.
* [ ] Le changement est visible immédiatement puis reste après rechargement.
* [ ] Le site avertit si le changement réinitialise ou rend incompatibles les dépôts existants.
* [ ] Les actions proposées correspondent au layout courant.

## MT-TOPO-002 - Configurer un dépôt AllInOne

**Feature testée** : dépôt unique Infrastructure + ApplicationCode.

**Comportement attendu** : un dépôt AllInOne porte les artefacts Bicep, pipeline et bootstrap du projet.

### Étapes

1. [ ] Choisir `AllInOne`.
2. [ ] Cliquer sur le slot de dépôt vide ou sur `Edit`.
3. [ ] Choisir GitHub ou Azure DevOps.
4. [ ] Saisir l'URL du dépôt de test.
5. [ ] Saisir le PAT dans le champ prévu.
6. [ ] Lancer `Verify connection`.
7. [ ] Sélectionner la branche parmi les branches retournées.
8. [ ] Enregistrer.
9. [ ] Ouvrir le lien du dépôt depuis la carte.

### Résultat attendu

* [ ] La vérification retourne les branches du dépôt.
* [ ] Une branche non retournée par le provider n'est pas proposée comme branche vérifiée.
* [ ] La carte affiche provider, URL, branche et types de contenu.
* [ ] Le PAT n'est jamais affiché après sauvegarde.
* [ ] Le lien ouvre le dépôt configuré dans un nouvel onglet.

## MT-TOPO-003 - Configurer les deux dépôts SplitInfraCode

**Feature testée** : slots Infrastructure et ApplicationCode.

**Comportement attendu** : les deux dépôts restent indépendants et reçoivent des artefacts différents.

### Étapes

1. [ ] Choisir `SplitInfraCode`.
2. [ ] Ajouter un dépôt au slot Infrastructure.
3. [ ] Vérifier sa connexion et sa branche.
4. [ ] Ajouter un dépôt au slot ApplicationCode.
5. [ ] Vérifier sa connexion et sa branche.
6. [ ] Modifier la branche de l'un des deux dépôts.
7. [ ] Recharger la page.
8. [ ] Ouvrir `/projects/<project-id>/generate`.

### Résultat attendu

* [ ] Chaque carte conserve son rôle.
* [ ] Une modification du dépôt Infrastructure ne change pas le dépôt ApplicationCode.
* [ ] Les deux branches sont affichées correctement.
* [ ] Le board expose des actions Infra et Code distinctes.

## MT-TOPO-004 - Configurer un dépôt de configuration MultiRepo

**Feature testée** : dépôt possédé par une `InfrastructureConfig`.

**Comportement attendu** : le dépôt, son PAT et son rôle sont stockés au niveau de la configuration, puis utilisés par les générations et push config-level.

### Étapes

1. [ ] Utiliser un projet `MultiRepo` avec deux configurations.
2. [ ] Ouvrir la première configuration puis l'onglet `Git`.
3. [ ] Choisir `AllInOne` ou `SplitInfraCode` pour le sous-mode de la configuration.
4. [ ] Ajouter le dépôt de configuration.
5. [ ] Saisir son PAT puis vérifier la connexion.
6. [ ] Choisir la branche vérifiée et enregistrer.
7. [ ] Ouvrir la deuxième configuration et répéter avec un autre dépôt.
8. [ ] Recharger les deux pages.

### Résultat attendu

* [ ] Les dépôts restent rattachés à la bonne configuration.
* [ ] Chaque dépôt possède son PAT côté serveur sans que le secret soit visible dans l'UI.
* [ ] La branche choisie est conservée par configuration.
* [ ] Le Git tab n'est visible que pour un projet `MultiRepo`.

## MT-TOPO-005 - Vérification d'un dépôt invalide

**Feature testée** : prévalidation d'une connexion Git avant sauvegarde.

**Comportement attendu** : une URL, un provider ou un PAT invalides provoquent une erreur compréhensible et ne remplacent pas une configuration valide.

### Étapes

1. [ ] Ouvrir l'édition d'un dépôt existant de recette.
2. [ ] Remplacer l'URL par une URL mal formée.
3. [ ] Lancer la vérification.
4. [ ] Restaurer l'URL et saisir un PAT invalide.
5. [ ] Lancer la vérification.
6. [ ] Fermer ou annuler le dialogue.
7. [ ] Recharger la page.

### Résultat attendu

* [ ] Le formulaire signale l'erreur sans fermer le dialogue de manière trompeuse.
* [ ] La branche par défaut ne devient pas disponible sans réponse vérifiée.
* [ ] L'ancien dépôt reste inchangé après annulation ou échec.
* [ ] Aucun détail de secret ou d'en-tête d'authentification n'est montré.

## MT-TOPO-006 - Branches du dépôt

**Feature testée** : recherche et sélection de branche.

**Comportement attendu** : le select ou autocomplete propose les branches retournées par le provider et empêche le push vers une branche inconnue.

### Étapes

1. [ ] Ouvrir un dialogue de dépôt ou de push.
2. [ ] Attendre la fin du chargement des branches.
3. [ ] Ouvrir la liste des branches.
4. [ ] Rechercher une branche par son nom.
5. [ ] Sélectionner une branche de recette.
6. [ ] Tester le cas où le dépôt ne renvoie aucune branche.

### Résultat attendu

* [ ] Un état de chargement est visible pendant la recherche.
* [ ] Le bouton d'action reste désactivé pendant le chargement.
* [ ] La recherche filtre la liste.
* [ ] L'état vide indique qu'aucune branche n'est disponible.
* [ ] La branche sélectionnée est utilisée par le push.

## MT-TOPO-007 - Navigation dans le dépôt applicatif

**Feature testée** : branches, recherche de fichiers et recherche de dossiers du dépôt code.

**Comportement attendu** : le site peut lire le dépôt ApplicationCode avec le provider et la branche sélectionnés.

### Étapes

1. [ ] Ouvrir la fonctionnalité de navigation Git depuis le projet ou la ressource compute qui la propose.
2. [ ] Choisir une branche vérifiée.
3. [ ] Rechercher `Dockerfile` ou un nom de fichier connu.
4. [ ] Rechercher un dossier source connu.
5. [ ] Ouvrir la sélection si un picker de Dockerfile est disponible.
6. [ ] Choisir un fichier et vérifier le chemin reporté dans le formulaire.

### Résultat attendu

* [ ] Les résultats appartiennent à la branche et au dépôt sélectionnés.
* [ ] Le filtre de nom ne retourne pas de fichiers d'une autre branche.
* [ ] Le chemin choisi reste relatif au dépôt.
* [ ] Les chemins absolus ou avec remontée `..` sont refusés.

## MT-TOPO-008 - URL Git impossible sur un layout non MultiRepo

**Feature testée** : garde-fou du Git tab config-level.

**Comportement attendu** : un projet `AllInOne` ou `SplitInfraCode` ne présente pas un onglet Git config-level impossible.

### Étapes

1. [ ] Ouvrir une configuration d'un projet `AllInOne`.
2. [ ] Ajouter `?tab=git` à l'URL.
3. [ ] Recharger.
4. [ ] Répéter pour `SplitInfraCode`.

### Résultat attendu

* [ ] L'onglet Git n'est pas affiché dans ces layouts.
* [ ] L'URL revient vers l'onglet par défaut ou la route normale.
* [ ] Les dépôts projet restent accessibles depuis leur écran prévu.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-TOPO-001 | | | |
| MT-TOPO-002 | | | |
| MT-TOPO-003 | | | |
| MT-TOPO-004 | | | |
| MT-TOPO-005 | | | |
| MT-TOPO-006 | | | |
| MT-TOPO-007 | | | |
| MT-TOPO-008 | | | |
