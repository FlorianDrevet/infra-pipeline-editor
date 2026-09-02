---
title: Dossier de tests manuels
description: Campagne complète de recette manuelle pour InfraFlowSculptor, de la connexion au push des artefacts générés.
ms.date: 2026-09-02
ms.topic: testing
---

## Dossier de tests manuels

Ce dossier décrit les vérifications à effectuer dans le site InfraFlowSculptor. Chaque fiche indique la feature contrôlée, le comportement attendu, les actions à réaliser et les preuves à conserver.

Le périmètre couvre le parcours complet :

* connexion et navigation
* création et administration d'un projet
* layouts `AllInOne`, `SplitInfraCode` et `MultiRepo`
* configurations, groupes de ressources et ressources Azure
* environnements, naming, tags, variables et références cross-config
* génération Bicep, pipelines Azure DevOps et bootstrap
* téléchargement, prévisualisation et push vers Git
* sécurité, identités, domaines et réseau privé
* serveur MCP et import ARM, qui ne disposent pas d'écran web

## Ordre recommandé

1. Préparer l'environnement avec [00-preparation.md](00-preparation.md).
2. Exécuter [01-smoke-golden-path.md](01-smoke-golden-path.md) après chaque version importante.
3. Exécuter les fiches détaillées concernées par la modification.
4. Exécuter [12-regression-responsive.md](12-regression-responsive.md) avant une recette complète.
5. Reporter chaque résultat dans [13-session-report.md](13-session-report.md).

## Fiches

| Document | Périmètre | Prérequis principal |
|---|---|---|
| [00-preparation.md](00-preparation.md) | Installation, données, comptes, preuves | Stack locale et navigateur |
| [01-smoke-golden-path.md](01-smoke-golden-path.md) | Parcours critique modéliser -> générer -> pousser | Compte Owner et dépôt Git de test |
| [02-auth-shell-settings.md](02-auth-shell-settings.md) | Connexion, navigation, préférences et PAT applicatifs | Compte Entra ID |
| [03-project-lifecycle.md](03-project-lifecycle.md) | Assistant projet, membres, environnements et configuration | Compte Owner |
| [04-topologies-and-repositories.md](04-topologies-and-repositories.md) | Layouts, dépôts, vérification Git et navigation dépôt | Dépôts Git de test |
| [05-configurations-and-resource-groups.md](05-configurations-and-resource-groups.md) | Configurations, groupes de ressources et états de page | Compte Contributor ou Owner |
| [06-azure-resource-catalog.md](06-azure-resource-catalog.md) | Catalogue des ressources et sous-ressources | Config avec environnements |
| [07-generation-and-artifacts.md](07-generation-and-artifacts.md) | Générations Bicep, Pipeline, Bootstrap et artefacts | Ressources configurées |
| [08-git-push-and-bootstrap.md](08-git-push-and-bootstrap.md) | Push par configuration et par projet, résultats partiels | PAT Git et dépôts accessibles |
| [09-naming-tags-variables-references.md](09-naming-tags-variables-references.md) | Naming, tags, variables et références cross-config | Deux configurations dans un projet |
| [10-security-networking-domains.md](10-security-networking-domains.md) | Rôles, identités, Key Vault, réseau privé et domaines | Compte Owner, ressources compatibles |
| [11-mcp-and-arm-import.md](11-mcp-and-arm-import.md) | Fonctions sans écran web : MCP et import ARM | Client MCP et PAT `ifs_...` |
| [12-regression-responsive.md](12-regression-responsive.md) | Erreurs, droits, rechargement et responsive | Navigateur desktop et mobile |
| [13-session-report.md](13-session-report.md) | Modèle de compte-rendu de recette | Une campagne exécutée |

## Statuts à utiliser

| Statut | Signification |
|---|---|
| `PASS` | Le comportement observé correspond au résultat attendu. |
| `FAIL` | Le comportement observé ne correspond pas au résultat attendu. Créer ou référencer un ticket. |
| `BLOCKED` | Le test ne peut pas démarrer à cause d'un prérequis indisponible. |
| `N/A` | Le scénario ne s'applique pas au jeu de données utilisé. |
| `KNOWN-ISSUE` | Le défaut est déjà documenté, mais doit rester vérifié pour suivre sa résolution. |

## Ce qu'il faut conserver comme preuve

* identifiant du test et date d'exécution
* navigateur et résolution utilisés
* layout et identifiant du projet testé
* capture de l'écran avant et après l'action importante
* message d'erreur complet, sans inclure de secret
* nom de branche et message de commit utilisés
* URL de branche et SHA de commit retournés par le push
* fichier généré ou extrait pertinent
* statut final et lien vers le ticket en cas d'échec

## Limites connues

Les fiches indiquent lorsqu'un test demande Azure, Entra ID, Docker, Azure DevOps, GitHub ou un client MCP. Un test local vert ne prouve pas qu'un fournisseur externe acceptera le fichier généré.

Les défauts déjà identifiés dans [feature-map.md](../stabilization/feature-map.md) restent des scénarios de recette importants. Lorsqu'une fiche les mentionne, le résultat attendu décrit le comportement produit souhaité et le résultat actuel doit être reporté séparément.
