---
title: Préparation d'une campagne de tests manuels
description: Prérequis techniques, comptes, données de test et méthode de preuve pour la recette InfraFlowSculptor.
ms.date: 2026-09-02
ms.topic: testing
---

## Préparation d'une campagne de tests manuels

Cette fiche prépare un environnement de recette reproductible. Elle ne valide pas encore une feature.

## En-tête de campagne

À remplir avant de commencer :

| Champ | Valeur |
|---|---|
| Date | |
| Testeur | |
| Version ou branche | |
| Navigateur et version | |
| Résolution desktop | |
| Résolution mobile simulée | |
| URL du frontend | |
| Environnement | local / recette / autre |
| Projet de test | |
| Tenant Entra ID | |

## Prérequis locaux

* [ ] Le dépôt est à jour sur la version à tester.
* [ ] Le SDK .NET `10.0.100` est installé.
* [ ] Node.js et npm sont disponibles pour le frontend.
* [ ] Docker Desktop est démarré si l'AppHost doit lancer PostgreSQL, Azurite ou les services associés.
* [ ] Chrome ou un navigateur Chromium est installé.
* [ ] Les popups de connexion Microsoft sont autorisées pour le site de recette.
* [ ] Le testeur dispose d'un moyen de capturer des écrans et de copier les URLs de branche.

## Démarrer l'application

Depuis la racine du dépôt, lancer l'AppHost documenté par le projet :

```pwsh
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj
```

L'AppHost affiche les URLs disponibles dans son tableau de bord. Ouvrir l'URL du frontend. Pour un frontend lancé séparément, utiliser le terminal `src\Front` :

```pwsh
Set-Location .\src\Front
npm install
npm run start
```

Dans ce second cas, l'URL de développement Angular est généralement `http://localhost:4200`.

## Comptes nécessaires

### Compte Entra ID

Préparer au minimum :

* un compte `Owner` du projet de test
* un compte `Contributor` pour vérifier les droits d'écriture
* un compte `Reader` pour vérifier les écrans en lecture seule, si l'environnement en fournit un

Le compte doit pouvoir se connecter au tenant configuré par l'application. Ne jamais mettre un mot de passe ou un jeton dans ce dossier.

### Dépôts Git de test

Préparer des dépôts isolés, sans contenu client :

* un dépôt GitHub ou Azure DevOps pour `AllInOne`
* un dépôt Infrastructure et un dépôt ApplicationCode pour `SplitInfraCode`
* au moins deux dépôts de configuration pour `MultiRepo`
* une branche par scénario de push, par exemple `manual-test/<date>-<scenario>`

Le PAT Git doit pouvoir lire le dépôt, lister les branches, créer une branche et créer un commit. Utiliser un secret dédié à la recette et le révoquer après la campagne.

### Données Azure

La modélisation peut être testée avec des valeurs fictives, mais préparer des valeurs cohérentes pour les écrans qui les valident :

* subscription IDs de test
* locations comme `westeurope` ou `francecentral`
* noms de ressources disponibles pour le test de naming
* un Key Vault de test si une vérification d'accès réelle est prévue
* un Container Registry de test si le scénario ACR doit aller jusqu'à Azure
* un VNet et un subnet de test pour les scénarios Private Endpoint

## Jeu de données minimal

Créer ou réutiliser un projet contenant :

* trois environnements : Development, Staging et Production
* une configuration `primary`
* un groupe de ressources dans cette configuration
* au moins une ressource compute, une ressource de stockage et une ressource de sécurité
* une ressource parent avec une sous-ressource, par exemple Storage Account et Blob Container
* une variable de pipeline et un tag au niveau projet et configuration
* deux configurations si les références cross-config ou `MultiRepo` sont testées

Pour les scénarios de génération, utiliser des noms simples et stables afin de retrouver les fichiers dans les téléchargements.

## Règles de sécurité de la campagne

* [ ] Aucun PAT Git, PAT `ifs_...`, secret Key Vault ou mot de passe n'apparaît dans une capture.
* [ ] Les dépôts de recette ne contiennent pas de données client.
* [ ] Les branches de test sont identifiables et supprimables.
* [ ] Les ressources Azure réellement créées utilisent une subscription de test.
* [ ] Les actions destructives sont exécutées uniquement sur les données de recette.
* [ ] Les tokens applicatifs créés dans Settings sont révoqués en fin de campagne.

## Preuve standard

Pour chaque test, conserver :

1. une capture montrant la page et l'état avant l'action
2. une capture montrant le résultat ou l'erreur
3. les valeurs non sensibles utilisées
4. le statut `PASS`, `FAIL`, `BLOCKED`, `N/A` ou `KNOWN-ISSUE`
5. le lien du ticket si le résultat est `FAIL`

## Nettoyage

À la fin de la campagne :

* [ ] supprimer les projets de test et leurs configurations si la base est dédiée
* [ ] supprimer les groupes de ressources de test
* [ ] supprimer les branches créées dans les dépôts
* [ ] supprimer les commits de test si la politique du dépôt le permet
* [ ] révoquer les PAT Git et les tokens applicatifs
* [ ] noter les ressources Azure conservées volontairement
