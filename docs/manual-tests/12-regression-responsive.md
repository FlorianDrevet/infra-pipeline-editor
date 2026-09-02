---
title: Tests de non-régression et responsive
description: Checklist transversale des états d'erreur, droits, rechargements, accessibilité, responsive et défauts connus.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests de non-régression et responsive

Cette fiche complète les tests fonctionnels. Elle vérifie que le site reste utilisable quand les données manquent, qu'un appel échoue, qu'un rôle change ou qu'un écran est ouvert sur une petite largeur.

## MT-REG-001 - Rechargement et navigation directe

**Feature testée** : stabilité des routes Angular et des contextes projet/configuration/ressource.

**Comportement attendu** : chaque route directe recharge le bon contexte et ne conserve pas l'état d'une page précédente.

### Étapes

1. [ ] Ouvrir directement `/projects`.
2. [ ] Ouvrir `/projects/<project-id>`.
3. [ ] Ouvrir `/projects/<project-id>/generate`.
4. [ ] Ouvrir `/projects/<project-id>/generate/config`.
5. [ ] Ouvrir `/config/<config-id>`.
6. [ ] Ouvrir `/config/<config-id>/resource/<type>/<resource-id>`.
7. [ ] Recharger chaque page.
8. [ ] Changer de projet puis de configuration sans fermer l'onglet.

### Résultat attendu

* [ ] Le bon nom, breadcrumb et contenu sont affichés après chaque navigation.
* [ ] Les données d'une page précédente ne restent pas visibles pendant ou après le chargement.
* [ ] Une URL inconnue revient vers `/login` ou la route prévue.
* [ ] Les query params d'onglet restent cohérents.

## MT-REG-002 - États loading, empty, error et success

**Feature testée** : feedback visuel et récupération.

**Comportement attendu** : chaque opération longue ou échouée expose un état lisible et une action de récupération adaptée.

### Étapes

1. [ ] Ouvrir une page avec plusieurs appels réseau.
2. [ ] Ralentir le réseau avec les outils du navigateur dans un environnement de test.
3. [ ] Observer les spinners et les zones de contenu.
4. [ ] Tester une page sans projet, configuration, groupe ou ressource.
5. [ ] Simuler une réponse serveur en erreur.
6. [ ] Cliquer sur `Retry` lorsqu'il existe.

### Résultat attendu

* [ ] Un spinner ne masque pas le mauvais contenu.
* [ ] Les états vides expliquent l'action possible.
* [ ] Les erreurs sont localisées et non silencieuses.
* [ ] Retry relance la bonne opération sans dupliquer les données.
* [ ] Un succès retire l'erreur précédente.

## MT-REG-003 - Isolation des projets et configurations

**Feature testée** : cloisonnement des données affichées.

**Comportement attendu** : changer de projet ou de configuration ne révèle pas des ressources ou paramètres d'un autre périmètre.

### Étapes

1. [ ] Préparer deux projets avec des noms et tags très différents.
2. [ ] Ouvrir le projet A, sa configuration et une ressource.
3. [ ] Naviguer vers le projet B.
4. [ ] Recharger.
5. [ ] Utiliser un lien direct vers une configuration du projet A avec une session autorisée uniquement sur B.
6. [ ] Observer les réponses et les écrans.

### Résultat attendu

* [ ] Les compteurs, tags, environnements et ressources appartiennent au contexte courant.
* [ ] Une URL non autorisée retourne une erreur d'accès ou une page vide contrôlée.
* [ ] Aucun nom, secret ou artefact du projet A ne s'affiche dans le projet B.

## MT-REG-004 - Double clic, retry et modifications non sauvegardées

**Feature testée** : idempotence de l'UI.

**Comportement attendu** : les actions d'écriture ne sont pas déclenchées plusieurs fois et les modifications non sauvegardées ne disparaissent pas sans avertissement.

### Étapes

1. [ ] Ouvrir un formulaire de ressource.
2. [ ] Modifier plusieurs champs.
3. [ ] Cliquer rapidement plusieurs fois sur Save.
4. [ ] Vérifier le nombre d'éléments ou la valeur sauvegardée.
5. [ ] Modifier une valeur puis naviguer vers une autre page.
6. [ ] Revenir au formulaire.
7. [ ] Déclencher un retry après une erreur réseau.

### Résultat attendu

* [ ] Un seul enregistrement est créé.
* [ ] Le bouton passe en état occupé pendant l'appel.
* [ ] Les champs ne reviennent pas à une ancienne valeur après un succès.
* [ ] Une navigation n'efface pas silencieusement une modification en cours.
* [ ] Retry ne double pas les ressources, tags, variables ou références.

## MT-REG-005 - Responsive desktop et mobile

**Feature testée** : adaptation des pages et dialogues.

**Comportement attendu** : les textes, formulaires, cartes et actions restent utilisables sans chevauchement ni défilement horizontal involontaire.

### Étapes

1. [ ] Tester une largeur de 1440 px.
2. [ ] Tester une largeur de 1024 px.
3. [ ] Tester une largeur de 768 px.
4. [ ] Tester une largeur mobile de 390 px.
5. [ ] Parcourir l'accueil, Projects, le wizard, le détail projet, le détail configuration, l'édition ressource et les dialogues de push.
6. [ ] Ouvrir le picker des types de ressources.
7. [ ] Ouvrir le dialogue de push bulk avec deux ou plusieurs cartes.

### Résultat attendu

* [ ] Aucun titre, label ou bouton ne sort de son conteneur.
* [ ] Les cartes de ressources restent lisibles et passent à la grille prévue.
* [ ] Les dialogues restent scrollables et leurs actions restent accessibles.
* [ ] Les tables et résultats Git peuvent être lus sur mobile.
* [ ] Aucun contenu important ne se retrouve sous la sidebar ou le footer.

## MT-REG-006 - Clavier et accessibilité de base

**Feature testée** : navigation clavier et états accessibles.

**Comportement attendu** : les actions principales peuvent être atteintes au clavier et les états sont compréhensibles par un lecteur d'écran.

### Étapes

1. [ ] Recharger la page et naviguer avec `Tab`.
2. [ ] Ouvrir un dialogue avec le clavier.
3. [ ] Parcourir les champs et les boutons.
4. [ ] Fermer avec `Escape` lorsque le dialogue le permet.
5. [ ] Déclencher une erreur de validation.
6. [ ] Observer le focus et le message associé.

### Résultat attendu

* [ ] Le focus reste visible.
* [ ] L'ordre de tabulation suit l'ordre visuel.
* [ ] Les boutons icon-only ont un nom accessible ou une tooltip utile.
* [ ] Les messages d'erreur sont annoncés ou reliés au champ.
* [ ] Les tabs indiquent l'onglet actif.

## MT-REG-007 - FR/EN sur les parcours complets

**Feature testée** : localisation des états et des dialogues.

**Comportement attendu** : la langue choisie s'applique aux écrans, boutons, erreurs, spinners, téléchargements et messages Git.

### Étapes

1. [ ] Exécuter le smoke test en français.
2. [ ] Passer en anglais depuis Settings.
3. [ ] Rejouer un formulaire invalide, un dialogue de confirmation, une génération et un push.
4. [ ] Recharger pendant un onglet de détail.
5. [ ] Revenir au français.

### Résultat attendu

* [ ] Les labels et erreurs sont localisés.
* [ ] Les dates et compteurs restent lisibles.
* [ ] Les clés de traduction brutes ne s'affichent pas dans les chemins supportés.
* [ ] La langue ne réinitialise pas les données saisies.

## MT-REG-008 - Secrets dans l'interface et les logs visibles

**Feature testée** : hygiène des secrets côté UI.

**Comportement attendu** : aucun PAT Git, token `ifs_...`, secret Key Vault ou mot de passe n'est affiché dans les écrans, URLs ou messages de résultat.

### Étapes

1. [ ] Configurer un dépôt avec un PAT de recette.
2. [ ] Ouvrir les dialogues de dépôt, de vérification de connexion et de push.
3. [ ] Ouvrir les outils réseau du navigateur.
4. [ ] Générer et pousser un artefact.
5. [ ] Rechercher les préfixes de token dans le texte de la page, les URLs et les réponses visibles.

### Résultat attendu

* [ ] Le PAT est transmis uniquement au mécanisme d'authentification prévu.
* [ ] Les réponses et messages ne contiennent pas le secret.
* [ ] Les erreurs provider affichent une description générique côté client.
* [ ] Les captures de recette restent exploitables sans masquer des informations métier non sensibles.

## MT-REG-009 - Persistance après fermeture et réouverture

**Feature testée** : persistance des modifications métier.

**Comportement attendu** : les données sauvegardées survivent à une fermeture de l'onglet ou à une nouvelle session, tandis que les états temporaires ne réapparaissent pas comme des données persistées.

### Étapes

1. [ ] Créer ou modifier un projet, une configuration, un tag et une ressource.
2. [ ] Fermer l'onglet.
3. [ ] Rouvrir l'URL après reconnexion.
4. [ ] Vérifier les données.
5. [ ] Comparer avec une valeur modifiée mais non sauvegardée dans une autre session.

### Résultat attendu

* [ ] Les données sauvegardées sont présentes.
* [ ] Les formulaires non sauvegardés ne sont pas présentés comme enregistrés.
* [ ] Les états de génération historiques correspondent aux artefacts réellement disponibles.

## MT-REG-010 - Vérifier les défauts de stabilisation suivis

Ces tests ne doivent pas être supprimés tant que les défauts correspondants ne sont pas fermés dans la carte de stabilisation.

| Défaut suivi | Scénario à rejouer | Résultat produit attendu | Résultat actuel à documenter |
|---|---|---|---|
| D05 | Comparer preview naming et nom dans le Bicep après bascule d'héritage | Même nom effectif | Noter toute divergence |
| D07 | Ajouter une App Configuration key puis générer | La donnée est traitée selon le contrat annoncé | Noter son absence éventuelle du Bicep |
| D08 | Valider un custom domain sans preuve DNS externe | La validation attend une preuve réelle | Noter toute validation automatique |
| D11 | Privatiser sans Private Endpoint | Le site bloque ou avertit clairement | Noter toute génération inaccessible |
| D12 | Utiliser le bouton d'en-tête Push depuis Pipeline et Bootstrap | Le push suit l'artefact courant ou l'action est supprimée | Noter si Bicep est poussé à la place |

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-REG-001 | | | |
| MT-REG-002 | | | |
| MT-REG-003 | | | |
| MT-REG-004 | | | |
| MT-REG-005 | | | |
| MT-REG-006 | | | |
| MT-REG-007 | | | |
| MT-REG-008 | | | |
| MT-REG-009 | | | |
| MT-REG-010 | | | |
