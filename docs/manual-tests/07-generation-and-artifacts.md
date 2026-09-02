---
title: Tests de génération et d'artefacts
description: Recette manuelle des générations Bicep, Pipeline et Bootstrap aux niveaux configuration et projet.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests de génération et d'artefacts

Cette fiche vérifie que le site produit les fichiers correspondant au modèle, au layout, aux environnements et aux rôles des dépôts.

## Règle de lecture des résultats

Un résultat de génération doit être vérifié à trois niveaux :

1. le statut affiché dans le site
2. l'arborescence et le contenu des fichiers
3. le téléchargement ou le push vers le dépôt cible

La génération projet globale est prévue pour `AllInOne` et `SplitInfraCode`. `MultiRepo` utilise la génération au niveau de chaque configuration.

## MT-GEN-001 - Générer le Bicep d'une configuration

**Feature testée** : génération Bicep config-level.

**Comportement attendu** : le site génère le Bicep de la configuration courante, avec `main.bicep`, modules, types et fichiers de paramètres par environnement.

### Étapes

1. [ ] Ouvrir une configuration contenant un groupe de ressources et des ressources valides.
2. [ ] Ouvrir la section ou l'onglet de génération Bicep.
3. [ ] Cliquer sur `Generate`.
4. [ ] Attendre la fin du traitement.
5. [ ] Déplier les dossiers affichés.
6. [ ] Ouvrir `main.bicep`, un module et un fichier `.bicepparam`.
7. [ ] Télécharger l'archive Bicep.

### Résultat attendu

* [ ] Le bouton et le panneau indiquent l'état de chargement.
* [ ] Le résultat contient un fichier principal, les modules attendus et les paramètres des environnements.
* [ ] Les noms de ressources, tags, paramètres et références configurés sont présents.
* [ ] Le viewer affiche le contenu du fichier choisi.
* [ ] Le ZIP s'ouvre et conserve les chemins relatifs.

## MT-GEN-002 - Générer les pipelines d'une configuration

**Feature testée** : génération Pipeline config-level.

**Comportement attendu** : le site génère les pipelines Infrastructure et ApplicationCode selon le layout de la configuration et les ressources compute déclarées.

### Étapes

1. [ ] Ouvrir l'onglet `Pipeline` de la génération configuration.
2. [ ] Cliquer sur `Generate`.
3. [ ] Attendre la fin.
4. [ ] Ouvrir un wrapper Infrastructure.
5. [ ] Ouvrir un wrapper applicatif d'une ressource compute.
6. [ ] Vérifier les templates partagés et les variables d'environnement.
7. [ ] Télécharger l'archive Pipeline.

### Résultat attendu

* [ ] Les YAML sont regroupés sous les chemins attendus, notamment `.azuredevops` et `apps`.
* [ ] Les wrappers applicatifs ne sont présents que pour les ressources compatibles.
* [ ] Les options de tests, couverture, lint, Sonar et scans restent cohérentes avec le formulaire.
* [ ] Les chemins source configurés pour une application sont utilisés dans les triggers et le build.
* [ ] Le ZIP est lisible et ne mélange pas les buckets lorsque le layout est split.

## MT-GEN-003 - Générer le bootstrap d'une configuration

**Feature testée** : génération Bootstrap config-level.

**Comportement attendu** : le site produit `bootstrap.pipeline.yml` et le guide explique comment exécuter le pipeline Azure DevOps et configurer les permissions nécessaires.

### Étapes

1. [ ] Ouvrir l'onglet `Bootstrap` d'une configuration `MultiRepo`.
2. [ ] Cliquer sur `Generate`.
3. [ ] Ouvrir l'arborescence du résultat.
4. [ ] Ouvrir `bootstrap.pipeline.yml`.
5. [ ] Lire la carte de guide et ses étapes de préparation, exécution, permissions et finalisation.
6. [ ] Télécharger le bootstrap.

### Résultat attendu

* [ ] Le bootstrap est visible dans le troisième onglet pour `MultiRepo`.
* [ ] Le fichier contient les étapes de préflight et de provisioning annoncées.
* [ ] Le guide rappelle les permissions Build Service nécessaires.
* [ ] Le fichier téléchargé est distinct des archives Bicep et Pipeline.
* [ ] Le bootstrap n'embarque jamais un PAT en clair.

## MT-GEN-004 - Générer les trois artefacts ensemble

**Feature testée** : action `Generate all`.

**Comportement attendu** : le site lance Bicep, Pipeline et Bootstrap ensemble et ne révèle pas un résultat partiel comme s'il était complet.

### Étapes

1. [ ] Ouvrir une configuration `MultiRepo` correctement configurée.
2. [ ] Cliquer sur `Generate all`.
3. [ ] Observer les trois états de chargement.
4. [ ] Attendre que les trois traitements soient terminés.
5. [ ] Ouvrir successivement les trois onglets.
6. [ ] Provoquer une erreur sur une des familles avec un jeu de données de recette incomplet.
7. [ ] Relancer `Generate all` après correction.

### Résultat attendu

* [ ] Le bouton reste désactivé pendant la génération globale.
* [ ] Les arbres, erreurs et boutons de téléchargement n'apparaissent pas comme finaux avant la fin du batch.
* [ ] Une erreur précise la famille concernée.
* [ ] Une nouvelle génération réouvre le panneau si celui-ci était replié.
* [ ] Une génération réussie remplace le contexte historique éventuel.

## MT-GEN-005 - Génération projet AllInOne

**Feature testée** : génération mono-repository au niveau projet.

**Comportement attendu** : le projet `AllInOne` génère l'ensemble des configurations dans une arborescence commune puis par configuration.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>/generate` pour un projet `AllInOne`.
2. [ ] Vérifier les compteurs de configurations, groupes de ressources et ressources.
3. [ ] Lancer `Generate all`.
4. [ ] Ouvrir les onglets Bicep, Pipeline et Bootstrap.
5. [ ] Vérifier la présence de `Common` lorsque le résultat doit partager des fichiers.
6. [ ] Télécharger chaque archive projet.

### Résultat attendu

* [ ] Les artefacts de toutes les configurations sont présents.
* [ ] Les références vers `Common` restent relatives et cohérentes.
* [ ] Les noms de configurations ne s'écrasent pas entre eux.
* [ ] Les trois onglets affichent les fichiers du projet, pas seulement la première configuration.

## MT-GEN-006 - Génération projet SplitInfraCode

**Feature testée** : génération projet avec séparation Infra/Code.

**Comportement attendu** : le site distingue les artefacts destinés au dépôt Infrastructure de ceux destinés au dépôt ApplicationCode.

### Étapes

1. [ ] Ouvrir le board d'un projet `SplitInfraCode`.
2. [ ] Lancer la génération complète.
3. [ ] Sélectionner l'onglet extérieur `Infra`.
4. [ ] Parcourir Bicep, Pipeline et Bootstrap.
5. [ ] Sélectionner l'onglet extérieur `Code`.
6. [ ] Parcourir Pipeline et Bootstrap applicatifs.
7. [ ] Télécharger le ZIP Infra puis le ZIP Code.

### Résultat attendu

* [ ] Le switcher affiche les deux périmètres.
* [ ] Le dépôt Infra reçoit Bicep, pipelines infra et bootstrap `FullOwner`.
* [ ] Le dépôt Code reçoit les pipelines applicatifs et le bootstrap `ApplicationOnly`.
* [ ] Les fichiers communs requis par les pipelines applicatifs sont inclus dans le bucket Code.
* [ ] Les ZIP Infra et Code ne contiennent pas les fichiers de l'autre dépôt.

## MT-GEN-007 - Génération projet MultiRepo

**Feature testée** : garde-fou de génération globale.

**Comportement attendu** : `MultiRepo` génère au niveau de chaque configuration et ne prétend pas pouvoir produire un mono-repo global.

### Étapes

1. [ ] Ouvrir `/projects/<project-id>/generate` pour un projet `MultiRepo`.
2. [ ] Vérifier la présentation des configurations et dépôts.
3. [ ] Vérifier l'absence d'une action globale Bicep/Pipeline/Bootstrap qui promettrait un dépôt unique.
4. [ ] Ouvrir une configuration.
5. [ ] Générer Bicep, Pipeline et Bootstrap à son niveau.
6. [ ] Répéter pour une deuxième configuration.

### Résultat attendu

* [ ] Le site explique ou reflète le périmètre par configuration.
* [ ] Les artefacts de la première configuration ne contiennent pas ceux de la seconde.
* [ ] Le push bulk devient disponible quand les générations et dépôts sont prêts.

## MT-GEN-008 - Charger la dernière génération

**Feature testée** : historique de génération.

**Comportement attendu** : le site peut afficher les artefacts existants sans lancer une nouvelle génération et identifie clairement ce contexte historique.

### Étapes

1. [ ] Générer des artefacts puis recharger le board du projet.
2. [ ] Cliquer sur `Show last generation` si l'action est disponible.
3. [ ] Vérifier la date affichée dans le bandeau historique.
4. [ ] Ouvrir un fichier historique.
5. [ ] Cliquer sur `Run generation again`.
6. [ ] Tester un projet sans génération précédente.

### Résultat attendu

* [ ] Les chemins historiques apparaissent sans nouvelle génération réseau inutile.
* [ ] La date compacte est affichée dans un format local lisible.
* [ ] Le bandeau indique que l'utilisateur consulte une génération précédente.
* [ ] Une génération fraîche retire immédiatement le contexte historique.
* [ ] L'absence de génération affiche un état vide; une erreur serveur réelle reste une erreur.

## MT-GEN-009 - Viewer et navigation dans les fichiers

**Feature testée** : explorateur de fichiers générés.

**Comportement attendu** : sélectionner un fichier ouvre son contenu et l'action de retour restaure l'arborescence utile.

### Étapes

1. [ ] Ouvrir un résultat généré.
2. [ ] Déplier plusieurs dossiers.
3. [ ] Cliquer sur un fichier.
4. [ ] Cliquer rapidement sur un deuxième fichier avant la fin du chargement du premier.
5. [ ] Utiliser `Back to file list` ou l'action équivalente.
6. [ ] Modifier le thème du viewer dans Settings et recommencer.

### Résultat attendu

* [ ] Le contenu affiché correspond au dernier fichier sélectionné.
* [ ] Un chargement ancien ne remplace pas le contenu plus récent.
* [ ] Le retour réouvre les dossiers parents du fichier courant.
* [ ] L'élément actif est repérable dans l'arbre.
* [ ] Le thème n'altère pas le contenu ni les chemins.

## MT-GEN-010 - Diagnostics avant génération

**Feature testée** : préflight de génération.

**Comportement attendu** : le site signale les problèmes connus avant de produire un résultat trompeur.

### Étapes

1. [ ] Créer une ressource non compatible avec un environnement du projet.
2. [ ] Ajouter un domaine personnalisé non validé sur une ressource compatible.
3. [ ] Saisir une image Docker sans la valider.
4. [ ] Créer une configuration nécessitant un rôle ou une identité manquante.
5. [ ] Lancer la génération.
6. [ ] Ouvrir chaque entrée du dialogue de diagnostics.
7. [ ] Corriger un problème puis relancer.

### Résultat attendu

* [ ] Le dialogue sépare les catégories de warnings et d'erreurs.
* [ ] Le lien d'une entrée ramène à la ressource concernée.
* [ ] Les ressources existantes ne sont pas signalées comme des ressources nouvelles incomplètes.
* [ ] Une correction retire le diagnostic correspondant.
* [ ] La génération ne se présente pas comme réussie si un blocage critique persiste.

## MT-GEN-011 - Détection des options de pipeline

**Feature testée** : détection de stack et options depuis un dépôt code.

**Comportement attendu** : le site propose des options adaptées au stack sans empêcher une sélection manuelle.

### Étapes

1. [ ] Ouvrir une Web App, Function App ou Container App non existante.
2. [ ] Ouvrir l'onglet `App Pipeline`.
3. [ ] Sélectionner le dépôt et la branche code.
4. [ ] Lancer l'action de détection.
5. [ ] Vérifier les suggestions de tests, lint, Sonar, cache et scans.
6. [ ] Modifier manuellement le stack ou une option.
7. [ ] Sauvegarder puis regénérer le pipeline.

### Résultat attendu

* [ ] L'action de détection montre un état de chargement puis un résultat.
* [ ] Le sélecteur manuel reste visible et prioritaire.
* [ ] Les options détectées sont cohérentes avec les fichiers du dépôt.
* [ ] Une modification manuelle n'est pas écrasée sans action explicite.
* [ ] Le YAML généré reprend les options finales.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-GEN-001 | | | |
| MT-GEN-002 | | | |
| MT-GEN-003 | | | |
| MT-GEN-004 | | | |
| MT-GEN-005 | | | |
| MT-GEN-006 | | | |
| MT-GEN-007 | | | |
| MT-GEN-008 | | | |
| MT-GEN-009 | | | |
| MT-GEN-010 | | | |
| MT-GEN-011 | | | |
