---
title: Tests du naming, des tags, des variables et des références
description: Recette manuelle des conventions de nommage, des tags, des variable groups et des références cross-config.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests du naming, des tags, des variables et des références

Ces paramètres influencent les ressources et les pipelines générés. Vérifier les valeurs dans l'UI puis dans un artefact produit.

## MT-NAMING-001 - Naming projet

**Feature testée** : templates et abréviations au niveau projet.

**Comportement attendu** : l'Owner peut définir un template par défaut, des templates par type et des abréviations, puis voir leur effet sur les previews et la génération.

### Étapes

1. [ ] Ouvrir l'onglet `Naming` du projet.
2. [ ] Définir ou modifier le template par défaut.
3. [ ] Ajouter un template pour un type utilisé, par exemple Storage Account.
4. [ ] Modifier l'abréviation de ce type.
5. [ ] Recharger la page.
6. [ ] Ouvrir une configuration et observer la preview d'un groupe ou d'une ressource.
7. [ ] Générer le Bicep.

### Résultat attendu

* [ ] Le template est sauvegardé sans modifier un autre type.
* [ ] L'abréviation est visible dans la liste des overrides.
* [ ] Les previews utilisent la convention projet lorsque la configuration l'hérite.
* [ ] Le Bicep généré utilise la même convention que la preview.

## MT-NAMING-002 - Naming configuration et héritage

**Feature testée** : cascade projet -> configuration.

**Comportement attendu** : la configuration peut utiliser les conventions projet ou définir les siennes, avec un résultat visible et cohérent.

### Étapes

1. [ ] Ouvrir l'onglet `Naming` d'une configuration.
2. [ ] Activer l'héritage des conventions projet.
3. [ ] Vérifier la preview dans l'onglet des groupes de ressources.
4. [ ] Désactiver l'héritage.
5. [ ] Définir un template ou une abréviation locale.
6. [ ] Recharger.
7. [ ] Générer le Bicep et comparer le nom de ressource au nom annoncé dans la preview.

### Résultat attendu

* [ ] L'état d'héritage est explicite.
* [ ] Les valeurs locales prennent effet lorsque l'héritage est désactivé.
* [ ] Les valeurs projet reprennent effet lorsque l'héritage est activé.
* [ ] Les previews et le fichier généré sont identiques.

**Suivi connu** : D05 signale une divergence possible entre la preview et la génération. Si elle est reproduite, classer `KNOWN-ISSUE` et joindre la valeur affichée ainsi que le nom généré.

## MT-NAMING-003 - Disponibilité et validation des noms

**Feature testée** : vérification de nom pour les types Azure sensibles.

**Comportement attendu** : le site indique si le nom est disponible, déjà utilisé, invalide ou en cours de vérification; la sauvegarde respecte le résultat.

### Étapes

1. [ ] Ouvrir une ressource de type soumis à la vérification de nom.
2. [ ] Saisir un nom connu comme disponible dans l'environnement de test.
3. [ ] Attendre la vérification.
4. [ ] Saisir un nom déjà utilisé ou réservé.
5. [ ] Saisir un nom invalide pour le type.
6. [ ] Tenter de sauvegarder dans chaque état.
7. [ ] Tester l'option de bypass si l'interface la propose.

### Résultat attendu

* [ ] Un état de vérification est visible pendant la requête.
* [ ] Les résultats sont présentés par environnement lorsque c'est applicable.
* [ ] Un nom invalide ou indisponible bloque la sauvegarde ou demande un bypass explicite.
* [ ] Le nom courant est distingué d'un nom disponible pour une nouvelle ressource.
* [ ] Un résultat ancien ne remplace pas le résultat du dernier nom saisi.

## MT-TAGS-001 - Tags projet

**Feature testée** : tags au niveau projet.

**Comportement attendu** : les tags projet sont créés, modifiés et supprimés, puis hérités ou utilisés par la génération selon le contrat.

### Étapes

1. [ ] Ouvrir l'onglet `Tags` du projet.
2. [ ] Ajouter une paire clé/valeur.
3. [ ] Modifier sa valeur.
4. [ ] Ajouter un second tag.
5. [ ] Supprimer le premier dans le dialogue de confirmation si disponible.
6. [ ] Recharger puis générer un artefact.

### Résultat attendu

* [ ] Les doublons de clé sont refusés ou gérés explicitement.
* [ ] Les valeurs longues respectent les limites affichées.
* [ ] La suppression actualise la liste.
* [ ] Les tags attendus apparaissent dans le Bicep ou les paramètres générés.

## MT-TAGS-002 - Tags configuration

**Feature testée** : tags propres à une configuration.

**Comportement attendu** : une configuration peut porter des tags qui complètent ou remplacent les valeurs selon la règle annoncée.

### Étapes

1. [ ] Ouvrir l'onglet `Tags` de la configuration.
2. [ ] Ajouter une clé présente au niveau projet avec une valeur différente.
3. [ ] Ajouter une nouvelle clé propre à la configuration.
4. [ ] Sauvegarder et recharger.
5. [ ] Générer le Bicep.

### Résultat attendu

* [ ] La portée du tag est visible depuis la bonne page.
* [ ] La résolution entre tag projet et tag configuration est déterministe.
* [ ] Le fichier généré reflète la valeur effective attendue.

## MT-VARIABLES-001 - Variable groups projet

**Feature testée** : groupes de variables Azure DevOps au niveau projet.

**Comportement attendu** : l'utilisateur peut déclarer les groupes utilisés par les pipelines et retrouver leurs références dans les artefacts.

### Étapes

1. [ ] Ouvrir l'onglet `Pipeline Variables` du projet.
2. [ ] Ajouter un variable group avec un nom de recette.
3. [ ] Modifier ou supprimer le groupe.
4. [ ] Recharger.
5. [ ] Générer Pipeline et Bootstrap.

### Résultat attendu

* [ ] Le groupe apparaît dans le bon projet.
* [ ] Les noms invalides ou doublons sont refusés.
* [ ] Les YAML référencent le groupe attendu avec le format prévu.
* [ ] La suppression demande confirmation et retire la référence des prochaines générations.

## MT-VARIABLES-002 - Variable groups configuration

**Feature testée** : groupes de variables au niveau configuration.

**Comportement attendu** : une configuration peut déclarer ses variables sans les mélanger avec celles d'une autre configuration.

### Étapes

1. [ ] Ouvrir l'onglet `Pipeline Variables` d'une configuration.
2. [ ] Ajouter un groupe ou une variable de recette.
3. [ ] Ouvrir une seconde configuration et vérifier son contenu.
4. [ ] Générer le Pipeline de la première configuration.

### Résultat attendu

* [ ] Les variables restent limitées à la configuration courante.
* [ ] Le pipeline généré utilise le groupe associé à la bonne configuration.
* [ ] Une configuration sans groupe affiche un état vide compréhensible.

## MT-REFERENCE-001 - Ajouter une référence cross-config

**Feature testée** : ressource d'une configuration qui référence une ressource d'une autre configuration du même projet.

**Comportement attendu** : le dialogue permet de choisir une ressource cible, de saisir un alias et un objectif, puis la référence apparaît dans les deux vues concernées.

### Étapes

1. [ ] Préparer deux configurations du même projet avec des ressources compatibles.
2. [ ] Ouvrir la configuration source.
3. [ ] Ouvrir le dialogue d'ajout de référence cross-config depuis le parcours de ressource prévu.
4. [ ] Rechercher la configuration ou la ressource cible.
5. [ ] Sélectionner la ressource.
6. [ ] Saisir un alias valide et un objectif.
7. [ ] Confirmer.
8. [ ] Ouvrir l'onglet `Cross-config references`.
9. [ ] Cliquer sur le lien vers la configuration cible.
10. [ ] Générer le Bicep.

### Résultat attendu

* [ ] Le dialogue sépare sélection de ressource et configuration de la référence.
* [ ] L'alias obligatoire est validé.
* [ ] La référence apparaît dans la liste source et dans la vue entrante de la cible.
* [ ] Le lien ouvre la bonne configuration.
* [ ] Le Bicep contient une expression `existing` ou la liaison attendue.

## MT-REFERENCE-002 - Doublon et suppression d'une référence

**Feature testée** : intégrité des références cross-config.

**Comportement attendu** : un doublon est refusé avec une erreur métier et la suppression retire la relation des deux côtés.

### Étapes

1. [ ] Rejouer l'ajout de la même référence.
2. [ ] Vérifier le message de doublon.
3. [ ] Supprimer la référence.
4. [ ] Recharger les deux configurations.
5. [ ] Regénérer.

### Résultat attendu

* [ ] Le second ajout ne crée pas de doublon.
* [ ] Aucune erreur PostgreSQL brute n'est affichée.
* [ ] La référence disparaît de la source et des entrantes de la cible.
* [ ] Le résultat généré ne conserve pas une liaison supprimée.

**Suivi connu** : la génération ne couvre pas encore toutes les propriétés de référence possibles. Une référence stockée mais absente du Bicep doit être classée `KNOWN-ISSUE` avec le type de ressource concerné.

## MT-REFERENCE-003 - Clés App Configuration

**Feature testée** : clés de configuration par environnement.

**Comportement attendu** : les clés sont créées et visibles dans la ressource App Configuration avec leurs valeurs d'environnement.

### Étapes

1. [ ] Ouvrir une App Configuration.
2. [ ] Ouvrir l'onglet `Configuration keys`.
3. [ ] Ajouter une clé et une valeur de Development.
4. [ ] Ajouter une valeur de Production différente si le formulaire le propose.
5. [ ] Modifier puis supprimer la clé.
6. [ ] Générer l'artefact de la configuration.

### Résultat attendu

* [ ] La clé apparaît dans la ressource App Configuration.
* [ ] Les valeurs restent séparées par environnement.
* [ ] La modification et la suppression sont persistées.
* [ ] Le résultat de génération respecte le contrat documenté pour les clés.

**Suivi connu** : D07 signale que les clés App Configuration peuvent être persistées mais absentes du Bicep. Vérifier le fichier généré et classer `KNOWN-ISSUE` si la donnée saisie n'y apparaît pas.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-NAMING-001 | | | |
| MT-NAMING-002 | | | |
| MT-NAMING-003 | | | |
| MT-TAGS-001 | | | |
| MT-TAGS-002 | | | |
| MT-VARIABLES-001 | | | |
| MT-VARIABLES-002 | | | |
| MT-REFERENCE-001 | | | |
| MT-REFERENCE-002 | | | |
| MT-REFERENCE-003 | | | |
