---
title: Tests de push Git et bootstrap Azure DevOps
description: Recette manuelle des pushes par configuration et par projet, des branches, des résultats partiels et de l'exécution du bootstrap.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests de push Git et bootstrap Azure DevOps

Ces scénarios nécessitent des dépôts de recette et un PAT Git configuré côté serveur. Utiliser des branches dédiées. Ne jamais copier un PAT dans une capture.

## Préparation Git

* [ ] Les dépôts de test sont accessibles par le provider choisi.
* [ ] Le PAT peut lister les branches, créer une branche et créer un commit.
* [ ] Les dépôts ne contiennent pas de données client.
* [ ] Une branche de base et une branche de recette sont connues.
* [ ] Les secrets de recette sont stockés dans le mécanisme prévu par l'application.

## MT-GIT-001 - Push Bicep config-level

**Feature testée** : push Bicep d'une configuration vers son dépôt.

**Comportement attendu** : le site utilise le PAT du dépôt de configuration, pousse les fichiers Bicep sur la branche choisie et retourne les métadonnées du commit.

### Étapes

1. [ ] Générer le Bicep d'une configuration `MultiRepo`.
2. [ ] Ouvrir l'action dédiée `Push to Git` depuis le contexte Bicep.
3. [ ] Attendre le chargement des branches.
4. [ ] Choisir une branche `manual-test/...` ou saisir la branche autorisée.
5. [ ] Saisir un message de commit.
6. [ ] Lancer le push.
7. [ ] Ouvrir l'URL de branche retournée.

### Résultat attendu

* [ ] Le PAT n'est pas visible dans le dialogue ou la réponse.
* [ ] Le push vise le dépôt lié à la configuration, pas un dépôt projet par défaut.
* [ ] Les fichiers Bicep apparaissent avec les chemins attendus.
* [ ] Le résultat affiche succès, SHA, URL de branche et nombre de fichiers.
* [ ] Le contenu du dépôt correspond au viewer avant le push.

## MT-GIT-002 - Push Pipeline et Bootstrap config-level

**Feature testée** : push Pipeline et Bootstrap d'une configuration.

**Comportement attendu** : chaque onglet envoie sa famille d'artefacts vers la cible et le bouton Bootstrap utilise le bootstrap, pas le pipeline ou le Bicep.

### Étapes

1. [ ] Générer le Pipeline et ouvrir son action `Push to Git`.
2. [ ] Choisir une branche et un message de commit distincts.
3. [ ] Pousser le Pipeline et vérifier le dépôt.
4. [ ] Ouvrir l'onglet Bootstrap.
5. [ ] Utiliser le bouton `Push to Git` propre au bootstrap.
6. [ ] Vérifier le dépôt et le fichier `bootstrap.pipeline.yml`.

### Résultat attendu

* [ ] Le push Pipeline n'embarque pas un bootstrap inattendu.
* [ ] Le push Bootstrap envoie bien le fichier bootstrap.
* [ ] En `SplitInfraCode`, le push config-level direct envoie uniquement le bucket `infra/`.
* [ ] Une cible sans secret de dépôt retourne une erreur claire sans essayer un secret projet.

## MT-GIT-003 - Push global AllInOne

**Feature testée** : push projet mono-repository.

**Comportement attendu** : un projet `AllInOne` pousse Bicep, pipelines et bootstrap vers un dépôt unique dans un commit cohérent.

### Étapes

1. [ ] Générer les trois familles au niveau projet.
2. [ ] Ouvrir `/projects/<project-id>/generate`.
3. [ ] Cliquer sur l'action globale `Push to Git`.
4. [ ] Choisir une branche de recette.
5. [ ] Saisir un message de commit.
6. [ ] Confirmer le push.
7. [ ] Vérifier le commit et les chemins du dépôt.

### Résultat attendu

* [ ] Le push global est disponible pour `AllInOne` lorsque les artefacts sont prêts.
* [ ] Les trois familles figurent dans le dépôt attendu.
* [ ] Le commit contient les fichiers sans collisions silencieuses.
* [ ] Les fichiers obsolètes sous les racines générées sont supprimés ou remplacés selon le contrat du provider.

## MT-GIT-004 - Push Infra et Code SplitInfraCode

**Feature testée** : push séparé des deux dépôts du layout `SplitInfraCode`.

**Comportement attendu** : l'utilisateur peut pousser Infra, Code ou les deux, avec un résultat indépendant pour chaque dépôt.

### Étapes

1. [ ] Générer le projet `SplitInfraCode`.
2. [ ] Ouvrir le switcher `Infra`.
3. [ ] Cliquer sur le push Infra.
4. [ ] Choisir une branche et un message.
5. [ ] Vérifier que le dépôt Infra contient Bicep, pipelines infra et bootstrap Infra.
6. [ ] Ouvrir le switcher `Code`.
7. [ ] Cliquer sur le push Code.
8. [ ] Vérifier que le dépôt Code contient pipelines applicatifs et bootstrap ApplicationOnly.
9. [ ] Tester le mode `both` si le dialogue le propose.

### Résultat attendu

* [ ] Le dépôt Infra ne reçoit pas les pipelines applicatifs.
* [ ] Le dépôt Code ne reçoit pas le Bicep ni le bootstrap FullOwner.
* [ ] Les deux résultats affichent leur propre URL, SHA et nombre de fichiers.
* [ ] Une erreur sur un dépôt n'efface pas le résultat de l'autre.

## MT-GIT-005 - Push bulk MultiRepo

**Feature testée** : push projet `MultiRepo` par configuration et dépôt.

**Comportement attendu** : le dialogue affiche une carte par dépôt de configuration et réalise un commit indépendant par cible.

### Étapes

1. [ ] Utiliser un projet `MultiRepo` avec au moins deux configurations `AllInOne`.
2. [ ] Générer Bicep, Pipeline et Bootstrap pour chaque configuration.
3. [ ] Ouvrir `/projects/<project-id>/generate`.
4. [ ] Cliquer sur le push bulk MultiRepo.
5. [ ] Vérifier une carte par configuration et dépôt.
6. [ ] Choisir une branche et un message pour chaque carte.
7. [ ] Lancer le push.
8. [ ] Ouvrir chaque URL de branche retournée.

### Résultat attendu

* [ ] Les cartes indiquent le nom de la configuration, le dépôt et les types de contenu.
* [ ] Chaque dépôt reçoit uniquement les artefacts de sa configuration.
* [ ] Chaque commit possède son propre SHA.
* [ ] Le résultat ne promet pas une atomicité entre dépôts.
* [ ] Le bouton reste indisponible si une cible ou un champ requis manque.

## MT-GIT-006 - Push bulk SplitInfraCode par configuration

**Feature testée** : routage du bulk selon le mode de configuration.

**Comportement attendu** : une configuration `SplitInfraCode` envoie les artefacts infra et application vers les dépôts correspondant aux rôles `Infrastructure` et `ApplicationCode`.

### Étapes

1. [ ] Utiliser un projet `MultiRepo` contenant une configuration en `SplitInfraCode`.
2. [ ] Ajouter un dépôt Infrastructure et un dépôt ApplicationCode à cette configuration.
3. [ ] Générer les trois familles.
4. [ ] Ouvrir le push bulk.
5. [ ] Vérifier les rôles affichés pour les deux dépôts.
6. [ ] Saisir deux branches et deux messages.
7. [ ] Lancer le push.

### Résultat attendu

* [ ] Le dépôt Infrastructure reçoit Bicep, pipeline infra et bootstrap FullOwner.
* [ ] Le dépôt ApplicationCode reçoit pipeline applicatif et bootstrap ApplicationOnly.
* [ ] Les chemins `infra/` et `app/` sont retirés ou transformés en chemins relatifs au dépôt cible.
* [ ] Les deux commits sont distincts et vérifiables.

## MT-GIT-007 - Résultat partiel

**Feature testée** : gestion d'un échec sur une cible parmi plusieurs.

**Comportement attendu** : le site affiche le succès d'un dépôt et l'échec de l'autre sans prétendre avoir poussé partout.

### Préparation

Utiliser uniquement un environnement de recette. Après avoir sauvegardé deux dépôts valides, rendre temporairement un seul secret inaccessible ou utiliser un dépôt de test volontairement indisponible selon la procédure de l'environnement.

### Étapes

1. [ ] Ouvrir le push bulk avec deux cibles prêtes.
2. [ ] Lancer le push.
3. [ ] Observer les cartes pendant le traitement.
4. [ ] Vérifier la carte réussie dans le dépôt concerné.
5. [ ] Lire la carte en erreur.
6. [ ] Corriger la cible puis utiliser `Retry`.

### Résultat attendu

* [ ] La carte réussie conserve son SHA et son URL.
* [ ] La carte en échec affiche un code ou un message compréhensible.
* [ ] Le bandeau global indique un résultat partiel.
* [ ] Le retry permet de relancer sans supprimer la preuve du premier succès.
* [ ] Aucun message ne révèle un token ou une réponse interne sensible.

## MT-GIT-008 - Validation des champs de push

**Feature testée** : branche et message de commit.

**Comportement attendu** : le push demande les valeurs obligatoires et évite les clics multiples pendant l'opération.

### Étapes

1. [ ] Ouvrir un dialogue de push.
2. [ ] Laisser le message vide et toucher le champ.
3. [ ] Vérifier le message d'erreur.
4. [ ] Laisser la branche vide ou choisir une valeur non autorisée.
5. [ ] Cliquer plusieurs fois sur le bouton si l'interface le permet.
6. [ ] Saisir les valeurs valides.

### Résultat attendu

* [ ] Le message de commit est obligatoire.
* [ ] Le bouton est désactivé si le formulaire est invalide.
* [ ] Le bouton passe en état de traitement après le premier clic.
* [ ] Un seul push est envoyé.
* [ ] Les cartes remplacent le formulaire par un état de chargement pendant l'opération.

## MT-GIT-009 - Nettoyage des fichiers obsolètes

**Feature testée** : remplacement d'un résultat généré dans une branche existante.

**Comportement attendu** : les anciens fichiers appartenant aux racines générées sont supprimés lorsqu'ils ne font plus partie du résultat actuel.

### Étapes

1. [ ] Pousser une première génération sur une branche de recette.
2. [ ] Modifier ou supprimer une ressource de manière à retirer un fichier généré.
3. [ ] Regénérer.
4. [ ] Pousser à nouveau sur la même branche.
5. [ ] Comparer l'ancien et le nouveau contenu du dépôt.

### Résultat attendu

* [ ] Les fichiers retirés du résultat ne restent pas comme fichiers actifs dans les racines gérées.
* [ ] Les fichiers conservés ne sont pas supprimés.
* [ ] Les chemins à la racine du dépôt sont traités selon le contrat documenté du push.

## MT-GIT-010 - Exécuter le bootstrap dans Azure DevOps

**Feature testée** : bootstrap de provisioning des pipelines et ressources Azure DevOps.

**Prérequis** : organisation Azure DevOps de test, permissions Build Service, service connections et variable groups autorisés.

### Étapes

1. [ ] Pousser le fichier bootstrap dans le dépôt attendu.
2. [ ] Ouvrir le fichier dans Azure DevOps.
3. [ ] Créer ou lancer le pipeline bootstrap selon le guide affiché par le site.
4. [ ] Vérifier le préflight des connexions ARM et ACR.
5. [ ] Vérifier la création ou mise à jour des pipelines, environnements, variable groups et service connections annoncés.
6. [ ] Relancer le bootstrap pour tester l'idempotence.

### Résultat attendu

* [ ] Le pipeline démarre avec les permissions indiquées.
* [ ] Le préflight signale une connexion manquante avant les opérations dépendantes.
* [ ] Les noms de pipelines correspondent aux noms utilisés par les releases générées.
* [ ] Une seconde exécution ne crée pas de doublons inattendus.
* [ ] Les secrets ne sont pas écrits en clair dans le YAML ou les logs.

## MT-GIT-011 - Vérifier le défaut D12 du bouton d'en-tête

**Feature testée** : action générique `Push to Git` de l'en-tête configuration.

**Comportement attendu** : une action générique devrait pousser l'artefact correspondant à l'onglet courant, ou être remplacée par des boutons dédiés.

### Étapes

1. [ ] Ouvrir une configuration `MultiRepo`.
2. [ ] Ouvrir l'onglet Pipeline.
3. [ ] Cliquer sur le bouton `Push to Git` de l'en-tête de la configuration, et non sur le bouton de l'onglet Pipeline.
4. [ ] Observer le dialogue puis le dépôt après le push de recette.
5. [ ] Répéter depuis l'onglet Bootstrap.

### Résultat attendu

* [ ] Le push correspond à l'artefact choisi par l'utilisateur.
* [ ] Si le bouton reste générique, son libellé et son comportement doivent être explicitement documentés.

**Suivi connu** : D12 est actuellement documenté comme défaut adjacent. Si le bouton pousse encore Bicep depuis les onglets Pipeline ou Bootstrap, classer le résultat `KNOWN-ISSUE` et joindre une capture.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-GIT-001 | | | |
| MT-GIT-002 | | | |
| MT-GIT-003 | | | |
| MT-GIT-004 | | | |
| MT-GIT-005 | | | |
| MT-GIT-006 | | | |
| MT-GIT-007 | | | |
| MT-GIT-008 | | | |
| MT-GIT-009 | | | |
| MT-GIT-010 | | | |
| MT-GIT-011 | | | |
