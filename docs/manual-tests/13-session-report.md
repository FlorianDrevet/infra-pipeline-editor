---
title: Compte-rendu de campagne de tests manuels
description: Modèle à remplir pour conserver les résultats, preuves et anomalies d'une recette InfraFlowSculptor.
ms.date: 2026-09-02
ms.topic: testing
---

## Compte-rendu de campagne de tests manuels

Copier ce modèle dans un emplacement de suivi de recette avant l'exécution. Ne pas enregistrer de secret dans le compte-rendu.

## Informations générales

| Champ | Valeur |
|---|---|
| Date de début | |
| Date de fin | |
| Testeur | |
| Version, commit ou build | |
| URL testée | |
| Environnement | local / recette / autre |
| Navigateur | |
| Résolution principale | |
| Tenant Entra ID | |
| Projet de test | |
| Layouts testés | |
| Provider Git | |

## Prérequis

* [ ] [00-preparation.md](00-preparation.md) exécutée.
* [ ] Stack disponible.
* [ ] Compte Owner disponible.
* [ ] Comptes Contributor/Reader testés ou marqués `N/A`.
* [ ] Dépôts Git de recette disponibles ou tests Git marqués `BLOCKED`.
* [ ] Données Azure de recette disponibles pour les scénarios concernés.
* [ ] Client MCP disponible ou tests MCP marqués `N/A`.

## Résumé

| Indicateur | Valeur |
|---|---|
| Tests `PASS` | |
| Tests `FAIL` | |
| Tests `BLOCKED` | |
| Tests `KNOWN-ISSUE` | |
| Tests `N/A` | |
| Tickets créés | |
| Validation de release | go / no-go |

## Registre des résultats

| ID | Feature | Statut | Preuve | Ticket | Commentaire |
|---|---|---|---|---|---|
| MT-SMOKE-001 | Authentification | | | | |
| MT-SMOKE-002 | Création projet | | | | |
| MT-SMOKE-003 | Configuration | | | | |
| MT-SMOKE-004 | Ressource | | | | |
| MT-SMOKE-005 | Génération | | | | |
| MT-SMOKE-006 | Push Git | | | | |
| MT-AUTH-001 | Entra ID | | | | |
| MT-SHELL-001 | Accueil et projets | | | | |
| MT-PROJECT-001 | Assistant projet | | | | |
| MT-TOPO-001 | Layout | | | | |
| MT-CONFIG-001 | Configuration | | | | |
| MT-RES-001 | Catalogue ressources | | | | |
| MT-GEN-001 | Bicep | | | | |
| MT-GIT-001 | Push config-level | | | | |
| MT-NAMING-001 | Naming | | | | |
| MT-SEC-001 | Autorisations | | | | |
| MT-NET-001 | Réseau | | | | |
| MT-DOMAIN-001 | Domaine personnalisé | | | | |
| MT-MCP-001 | MCP | | | | |
| MT-REG-001 | Non-régression | | | | |
| MT-REG-005 | Responsive | | | | |

Ajouter les autres IDs des fiches lorsque le périmètre de la campagne est complet.

## Fiche d'anomalie

À remplir pour chaque `FAIL` ou `KNOWN-ISSUE` :

| Champ | Valeur |
|---|---|
| Test ID | |
| Date et heure | |
| Gravité | blocker / critical / major / minor |
| Projet et configuration | |
| Layout | |
| Étapes pour reproduire | |
| Résultat attendu | |
| Résultat observé | |
| Message d'erreur | |
| URL ou écran | |
| Capture ou artefact | |
| Ticket | |
| Contournement | |

## Preuves Git

| Cible | Repository | Branche | SHA | Fichiers | Résultat |
|---|---|---|---|---:|---|
| | | | | | |
| | | | | | |
| | | | | | |

## Preuves de génération

| Artefact | Projet/configuration | Fichier contrôlé | Téléchargement | Résultat |
|---|---|---|---|---|
| Bicep | | | | |
| Pipeline | | | | |
| Bootstrap | | | | |

## Clôture

* [ ] Les captures ne contiennent aucun secret.
* [ ] Les branches Git de recette sont supprimées.
* [ ] Les tokens applicatifs sont révoqués.
* [ ] Les ressources Azure temporaires sont supprimées ou listées.
* [ ] Les tickets bloquants sont liés au compte-rendu.
* [ ] Le verdict de campagne est partagé avec l'équipe.
