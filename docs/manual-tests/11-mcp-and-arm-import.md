---
title: Tests MCP et import ARM
description: Recette manuelle des capacités MCP et d'import ARM qui ne disposent pas d'écran dans le site Angular.
ms.date: 2026-09-02
ms.topic: testing
---

## Tests MCP et import ARM

Ces fonctionnalités appartiennent au produit mais ne sont pas accessibles depuis une page web Angular. Le testeur utilise un client MCP compatible, par exemple VS Code avec Copilot Chat, et conserve les résultats structurels sans exposer de secret.

## Prérequis

* [ ] L'AppHost ou le serveur MCP est démarré.
* [ ] L'endpoint local est accessible sur `http://127.0.0.1:5258/mcp`, ou l'URL annoncée par l'environnement.
* [ ] Un PAT interne `ifs_...` a été créé dans `Settings`.
* [ ] Le PAT possède le scope nécessaire : `Read` pour consulter, `Write` pour modifier et `Generate` pour générer.
* [ ] La configuration MCP du client cible le serveur de recette.
* [ ] Un fichier ARM JSON de test ne contient aucune donnée client.

## MT-MCP-001 - Connexion du client MCP

**Feature testée** : exposition HTTP du serveur MCP.

**Comportement attendu** : le client découvre le serveur et ses capacités après authentification Bearer PAT.

### Étapes

1. [ ] Ouvrir la configuration MCP du workspace.
2. [ ] Vérifier que l'URL cible `/mcp`.
3. [ ] Fournir le PAT interne au client par son mécanisme sécurisé.
4. [ ] Redémarrer ou reconnecter le serveur MCP dans le client.
5. [ ] Demander au client d'afficher les outils, resources et prompts disponibles.

### Résultat attendu

* [ ] Le serveur apparaît comme connecté.
* [ ] Une requête sans PAT est refusée.
* [ ] Un PAT révoqué ou expiré est refusé.
* [ ] Le client voit les tools, les resources et le prompt de création exposés.
* [ ] Aucun PAT n'apparaît dans la réponse de découverte.

## MT-MCP-002 - Draft de projet depuis un prompt

**Feature testée** : `draft_project_from_prompt` et workflow de clarification.

**Comportement attendu** : un prompt libre produit un draft structuré et signale les informations manquantes avant toute création.

### Étapes

1. [ ] Demander au client : `Crée un projet de recette avec Key Vault, Container App et SQL Server.`
2. [ ] Examiner le draft retourné.
3. [ ] Vérifier les questions de clarification proposées.
4. [ ] Fournir les valeurs manquantes, notamment environnements, locations, subscriptions et topologie.
5. [ ] Demander une nouvelle validation du draft.

### Résultat attendu

* [ ] Le draft contient un nom, une topologie et des ressources structurées.
* [ ] Le serveur ne crée pas de projet avec des informations obligatoires manquantes.
* [ ] Les clarifications sont spécifiques et exploitables.
* [ ] Les valeurs déjà fournies sont conservées après clarification.

## MT-MCP-003 - Valider un draft

**Feature testée** : validation métier d'un draft.

**Comportement attendu** : les incohérences sont retournées avant la création et les règles métier communes à l'API sont appliquées.

### Étapes

1. [ ] Envoyer un draft sans environnement.
2. [ ] Envoyer un draft avec un type de ressource non supporté.
3. [ ] Envoyer un draft avec des doublons de dépôt ou de nom.
4. [ ] Corriger le draft.
5. [ ] Valider le draft complet.

### Résultat attendu

* [ ] Chaque erreur indique la partie à corriger.
* [ ] Aucun projet n'est créé pour un draft invalide.
* [ ] Un draft valide retourne un résultat de validation positif et réutilisable par l'étape suivante.

## MT-MCP-004 - Créer un projet depuis un draft

**Feature testée** : `create_project_from_draft`.

**Comportement attendu** : seul un draft valide peut créer le projet et les éléments autorisés par le workflow.

### Étapes

1. [ ] Utiliser le draft validé du test précédent.
2. [ ] Demander la création.
3. [ ] Ouvrir le projet créé dans le site web.
4. [ ] Comparer le projet, le layout, les environnements et les ressources avec le draft.
5. [ ] Rejouer la création avec le même identifiant de draft si le client le permet.

### Résultat attendu

* [ ] Le projet créé correspond au draft validé.
* [ ] Les ressources créées apparaissent dans la configuration attendue.
* [ ] La création ne contourne pas les règles de nommage et d'accès.
* [ ] Le serveur ne crée pas silencieusement un second projet inattendu lors d'un retry.

## MT-MCP-005 - Découverte et lecture des resources

**Feature testée** : résumé de projet et preview d'import.

**Comportement attendu** : les resources MCP donnent un contexte en lecture seule et respectent l'utilisateur authentifié.

### Étapes

1. [ ] Demander la resource de résumé d'un projet autorisé.
2. [ ] Vérifier les configurations, environnements et compteurs retournés.
3. [ ] Demander le résumé d'un projet non autorisé.
4. [ ] Créer une preview d'import pour obtenir une resource de preview.
5. [ ] Lire cette preview avec le client.

### Résultat attendu

* [ ] Le résumé correspond au site.
* [ ] Une resource inconnue ou non autorisée retourne une erreur adaptée.
* [ ] Les données d'un autre projet ne sont pas exposées.
* [ ] La preview contient ressources mappées, gaps, dépendances et résumé.

## MT-MCP-006 - Générer du Bicep par MCP

**Feature testée** : génération Bicep exposée au client MCP.

**Comportement attendu** : MCP déclenche le même moteur de génération que l'API et retourne un résultat exploitable.

### Étapes

1. [ ] Demander la génération Bicep d'un projet ou d'une configuration autorisée.
2. [ ] Attendre le résultat.
3. [ ] Ouvrir le projet dans le site.
4. [ ] Comparer `main.bicep`, les modules et les paramètres avec le résultat MCP.
5. [ ] Tester un PAT sans le scope `Generate`.

### Résultat attendu

* [ ] Le résultat est cohérent avec la génération web.
* [ ] Les fichiers sont identifiables par leurs chemins.
* [ ] Un PAT sans scope Generate est refusé sans exécuter la génération.
* [ ] Les erreurs de validation sont retournées de manière structurée.

## MT-MCP-007 - Configurer une ressource par MCP

**Feature testée** : tools de création/configuration de ressources, rôles et app settings.

**Comportement attendu** : les opérations MCP modifient le même modèle que l'UI et deviennent visibles dans le site après rafraîchissement.

### Étapes

1. [ ] Demander l'ajout d'un tag ou d'un app setting à une ressource de recette.
2. [ ] Demander l'ajout d'une identité ou d'un role assignment compatible.
3. [ ] Ouvrir la ressource dans le site.
4. [ ] Vérifier les onglets concernés.
5. [ ] Réessayer avec un PAT limité à `Read`.

### Résultat attendu

* [ ] La modification est visible dans le site.
* [ ] Les règles de validation de l'API sont identiques à celles du workflow MCP.
* [ ] Le PAT Read-only ne peut pas modifier les données.
* [ ] Une ressource inexistante ne provoque pas un succès vide.

## MT-MCP-008 - Preview d'import ARM

**Feature testée** : `preview_iac_import`.

**Comportement attendu** : le serveur analyse un template ARM JSON sans créer de projet et retourne les mappings, gaps et dépendances.

### Étapes

1. [ ] Préparer un template ARM JSON minimal contenant plusieurs ressources supportées.
2. [ ] Ajouter une ressource volontairement non supportée.
3. [ ] Demander une preview d'import.
4. [ ] Examiner les ressources mappées, gaps, dépendances et métadonnées.
5. [ ] Vérifier qu'aucun projet n'est créé pendant la preview.

### Résultat attendu

* [ ] Le format `arm-json` est accepté.
* [ ] Les ressources supportées sont classées comme mappées.
* [ ] La ressource non supportée devient un gap explicite.
* [ ] Les dépendances sont visibles.
* [ ] La preview reste disponible pour l'étape d'application prévue.

## MT-MCP-009 - Appliquer une preview ARM

**Feature testée** : `apply_import_preview`.

**Comportement attendu** : une preview valide crée un nouveau projet et les ressources mappées auto-créables; l'outil n'affirme pas modifier un projet existant.

### Étapes

1. [ ] Utiliser la preview du test précédent.
2. [ ] Fournir les éléments de configuration demandés.
3. [ ] Demander l'application.
4. [ ] Ouvrir le projet créé dans le site.
5. [ ] Comparer les ressources présentes avec la preview.
6. [ ] Vérifier le traitement des gaps.

### Résultat attendu

* [ ] Un nouveau projet est créé conformément à la limite documentée du workflow.
* [ ] Les ressources mappées sont créées lorsqu'elles sont auto-créables.
* [ ] Les gaps ne sont pas présentés comme des ressources créées.
* [ ] Le résultat donne les identifiants nécessaires pour poursuivre dans l'UI.

## MT-MCP-010 - Tools déclarés mais non exposés

**Feature testée** : cohérence entre l'inventaire des outils et leur exposition réelle.

**Comportement attendu** : tout outil annoncé comme disponible doit être appelable par le client; un outil non enregistré doit être absent de la documentation d'utilisation ou marqué comme non disponible.

### Étapes

1. [ ] Comparer la liste de découverte MCP avec la liste annoncée dans la documentation.
2. [ ] Tester les outils de découverte, draft, création, gestion, infrastructure, ressources, rôles, app settings, sous-ressources, naming, Bicep et import.
3. [ ] Noter chaque outil introuvable.
4. [ ] Vérifier qu'un outil absent ne produit pas une réponse vide ou un succès simulé.

### Résultat attendu

* [ ] Les outils exposés sont réellement appelables.
* [ ] Les outils absents sont signalés comme indisponibles.
* [ ] Une capability non enregistrée ne doit pas être présentée comme livrée.

**Suivi connu** : la carte de stabilisation mentionne des classes MCP historiques non enregistrées. Classer le résultat `KNOWN-ISSUE` si un outil attendu manque encore dans la découverte.

## Verdict

| Test | Statut | Preuve | Ticket |
|---|---|---|---|
| MT-MCP-001 | | | |
| MT-MCP-002 | | | |
| MT-MCP-003 | | | |
| MT-MCP-004 | | | |
| MT-MCP-005 | | | |
| MT-MCP-006 | | | |
| MT-MCP-007 | | | |
| MT-MCP-008 | | | |
| MT-MCP-009 | | | |
| MT-MCP-010 | | | |
