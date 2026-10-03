# 05 — Configurations d'infrastructure

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Hors de ce document : nommage ([06](06-nommage.md)), dépôts de configuration ([04](04-topologie-et-depots.md)), ressources ([07](07-modele-commun-ressources.md)).

## 1. Objectif

Une configuration regroupe des resource groups et des ressources qui seront générés ensemble :
un `main.bicep`, un jeu de paramètres par environnement, des pipelines CI/PR/Release. Exemples
typiques : un socle partagé (réseau, logs, registre), un service applicatif.

## 2. Données d'une configuration

| Champ | Type / valeurs | Obligatoire | Règles | Défaut |
|---|---|---|---|---|
| Nom | Texte | Oui | 1 à 100 caractères. Sert de nom de dossier dans les dépôts (après nettoyage) et dans les noms de pipelines. | — |
| Projet | Référence | Oui | Fixé à la création. | — |
| Utiliser le nommage du projet | Booléen | — | Voir [06](06-nommage.md) | Oui |
| Gabarit par défaut, gabarits par type, abréviations | — | — | Voir [06](06-nommage.md) | Aucun |
| Tags | Liste (clé, valeur) | — | Remplacée en bloc. Fusion : voir RG-PRJ-11. | Aucun |
| Répartition infra/code (ConfigLayoutMode) | `AllInOne`, `SplitInfraCode`, aucune | — | Utilisée en topologie `MultiRepo` seulement. Voir [04](04-topologie-et-depots.md). | Aucune |
| Dépôts de configuration | Liste | — | `MultiRepo` seulement. | Aucun |
| Références cross-config | Liste | — | Section 5 | Aucune |
| Mode des pipelines applicatifs | `Isolated`, `Combined` | — | Voir EC-CFG-04 | `Isolated` |
| Resource groups | Liste | — | Section 4 | Aucun |

## 3. Cycle de vie

**UC-CFG-01 — Créer une configuration** (Contributor). Saisie : nom. La configuration hérite du
nommage du projet.

**UC-CFG-02 — Consulter une configuration.** Resource groups et ressources, nommage, tags,
références, variables, dépôts, diagnostics.

**UC-CFG-03 — Lister mes configurations.** Toutes les configurations des projets dont
l'utilisateur est membre.

**UC-CFG-04 — Modifier les tags** (Contributor).

**UC-CFG-05 — Supprimer une configuration** (Owner). Ses resource groups et ressources sont
supprimés en cascade en base (**non vérifié à l'exécution**).

**RG-CFG-01 — Pas d'unicité du nom.** Rien n'empêche deux configurations de même nom dans un projet
(**non vérifié à l'exécution**). Or le nom sert de dossier dans les dépôts : deux configurations
homonymes écriraient au même endroit.

## 4. Resource groups

| Champ | Type / valeurs | Obligatoire | Règles |
|---|---|---|---|
| Nom | Texte | Oui | 1 à 90 caractères. C'est un **nom logique** : le nom Azure est calculé par le gabarit `ResourceGroup` (voir [06](06-nommage.md)). |
| Région | Une des 31 régions supportées | Oui | — |

**UC-CFG-06 — Créer un resource group** (Contributor). Saisie : nom, région.

**UC-CFG-07 — Modifier un resource group** (Contributor). Nom et région.

**UC-CFG-08 — Supprimer un resource group** (Contributor). Ses ressources sont supprimées en
cascade en base (**non vérifié à l'exécution**).

**UC-CFG-09 — Lister les ressources d'un resource group / d'une configuration.**

**RG-CFG-02 — Région du resource group et des ressources.** Chaque ressource a sa propre région ;
rien ne l'aligne sur celle de son resource group (voir EC-CFG-02).

## 5. Références cross-config

Une configuration peut utiliser une ressource déclarée dans une autre configuration du même
projet (exemple : le Log Analytics du socle partagé utilisé par l'Application Insights d'un
service).

| Champ | Description |
|---|---|
| Configuration cible | Déduite de la ressource cible. |
| Ressource cible | Ressource d'une autre configuration du même projet. |

**RG-CFG-03 — Même projet, autre configuration.** La ressource cible doit appartenir à une
configuration du même projet, différente de la configuration courante.

**RG-CFG-04 — Pas de doublon.** Une configuration ne référence qu'une fois la même ressource.

**RG-CFG-05 — Effet sur la génération.** Une ressource référencée est déclarée dans le Bicep de la
configuration comme ressource `existing` (avec son resource group), et peut alors servir de cible à
des propriétés de liaison (voir [13](13-generation-bicep.md)) : rôles RBAC, app settings, Private
Endpoint vers un VNet partagé.

**UC-CFG-10 — Ajouter une référence** (Contributor). L'utilisateur choisit une ressource d'une
autre configuration.

**UC-CFG-11 — Lister les références sortantes** d'une configuration.

**UC-CFG-12 — Lister les références entrantes** : quelles autres configurations utilisent les
ressources de celle-ci.

**UC-CFG-13 — Retirer une référence** (Contributor).

## 6. Écarts constatés

- **EC-CFG-01 (F15) — Références partiellement câblées.** Une référence est stockée et déclarée
  `existing`, mais seules six propriétés de liaison connues savent pointer vers une ressource d'une
  autre configuration. Toute autre utilisation est **ignorée silencieusement** à la génération.
  Exception constatée : à la génération **projet**, le registre de conteneurs utilisé par une
  application d'une autre configuration est déduit automatiquement.
- **EC-CFG-02 — Région libre.** La règle « une ressource doit être dans la même région que son
  resource group » existe dans le domaine mais n'est jamais appliquée. De même, l'unicité du nom
  d'une ressource dans son resource group et la limite de 800 ressources par resource group ne sont
  jamais vérifiées.
- **EC-CFG-03 — Renommer ou déplacer un resource group** ne met pas à jour la région des ressources
  qu'il contient.
- **EC-CFG-04 — Mode des pipelines applicatifs inatteignable.** La configuration porte un mode
  `Isolated` / `Combined`, mais aucune action ne permet de le changer : il vaut toujours `Isolated`.
  Le mode `Combined`, s'il était choisi, ne combine rien (il renomme seulement les fichiers).
- **EC-CFG-05 — Paramètres de configuration morts.** Le modèle contient des « définitions de
  paramètres » (nom, type, secret, valeur par défaut) et leurs usages par les ressources ; aucune
  action ne les crée, rien ne les génère.
- **EC-CFG-06 — Pas d'alias ni d'usage déclaré.** Une référence cross-config ne porte qu'une cible ;
  aucun alias ni description d'usage n'est stocké.
- **EC-CFG-07 — Route mal rangée.** La répartition infra/code d'une configuration se modifie par une
  route du groupe « projets » alors qu'elle modifie la configuration.

## 7. Questions ouvertes

- **Q-CFG-01** — Rendre le nom de configuration unique dans un projet (il sert de dossier) ?
- **Q-CFG-02** — Imposer la région du resource group aux ressources, ou au contraire supprimer la
  région du resource group ?
- **Q-CFG-03** — Câbler toutes les références cross-config ou limiter explicitement les usages
  possibles à l'écran (et refuser le reste) ?
- **Q-CFG-04** — Dupliquer une configuration (pour créer un service sur le modèle d'un autre) ?
- **Q-CFG-05** — Garder les « paramètres de configuration » comme concept (variables Bicep
  partagées) ou les supprimer ?
