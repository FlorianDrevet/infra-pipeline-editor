# 19 — Interface et préférences

> Statut : v0 rétro-spécification (code au 2026-10-02). Constat de l'écran Angular actuel ; la
> refonte visuelle cible est la maquette Claude Design (design system « Strata »).

## 1. Navigation (écrans)

| Écran | Adresse | Contenu |
|---|---|---|
| Connexion | `/login` | Connexion Entra ID. |
| Accueil | `/` | Salutation, actions (nouveau projet, voir tous les projets), favoris, consultés récemment, statistiques. |
| Projets | `/projects` | Liste des projets, création (simple ou assistant). |
| Projet | `/projects/:id` | Arborescence configurations → resource groups → ressources ; sections environnements, topologie et dépôts, nommage, tags, variable groups, agent pool ; tableau de génération et guide du bootstrap. |
| Membres | `/projects/:id/members` | Membres et rôles ([02](02-acces-et-droits.md)). |
| Paramètres du projet | `/projects/:id/settings` | Agent pool ; sections annoncées « à venir » (EC-UI-01). |
| Génération projet | `/projects/:id/generate`, `/generate/config` | Génération et téléchargement Bicep / pipelines / bootstrap, push git. |
| Configuration | `/config/:id` | Ressources, resource groups, références cross-config, nommage, tags, variable groups, dépôts (MultiRepo), diagnostics. |
| Génération config | `/config/:id/generate` | Onglets Bicep, Pipeline, Bootstrap (MultiRepo). |
| Ressource | `/config/:configId/resource/:type/:id` | Éditeur à onglets ([07 § 7](07-modele-commun-ressources.md)). |
| Paramètres utilisateur | `/settings` | Préférences et PAT IFS. |
| Design system | `/design-system` | Catalogue des composants (développement). |

Toutes les adresses sauf `/login` exigent d'être connecté. Fil d'Ariane et barre latérale de contexte
(projet courant).

## 2. Visualisation du code généré

**UC-UI-01 — Lire un fichier généré** dans un visualiseur avec coloration Bicep / YAML, arborescence
des fichiers, téléchargement de l'archive.

## 3. Préférences utilisateur (stockées dans le navigateur)

| Préférence | Valeurs | Défaut |
|---|---|---|
| Langue | Français, anglais | Langue du navigateur (**non vérifié**) |
| Thème de l'application | `dark`, `light` | `dark` |
| Thème du visualiseur Bicep | `azure-night`, `graphite-frost`, `sand-dusk` | `azure-night` |

## 4. Mémoire locale de l'écran

| Donnée | Règle |
|---|---|
| Projets favoris | Liste d'identifiants. |
| Consultés récemment | 8 éléments max, le plus récent d'abord. |
| Brouillon de l'assistant de création | Reprise d'une création interrompue. |
| Dernière branche de push | Par projet, par dépôt (MultiRepo, infra / code). Défaut : branche par défaut du dépôt. |

**RG-UI-01 — Local au poste.** Ces données ne suivent pas l'utilisateur d'un navigateur à l'autre.

## 5. Règles transverses

**RG-UI-02 — Design system obligatoire** : tout écran part des composants `app-ds-*`.
**RG-UI-03 — Internationalisation** : tous les textes passent par les fichiers de traduction (fr, en).

## 6. Écarts constatés

- **EC-UI-01 — Paramètres de projet fictifs.** Génération automatique, notifications, rétention et
  service connections sont affichées comme sections « à venir » : rien n'est stocké.
- **EC-UI-02 — Préférences non synchronisées** (RG-UI-01).
- **EC-UI-03 — Pas de vue graphe** des ressources et de leurs liens (prévue par la maquette).
- **EC-UI-04 — Pas d'écran d'import ARM** ([17](17-import-arm.md)).
- **EC-UI-05 — Pas de recherche globale** ni de palette de commandes.

## 7. Questions ouvertes

- **Q-UI-01** — Stocker les préférences côté serveur (profil utilisateur) ?
- **Q-UI-02** — Implémenter ou retirer les sections « à venir » des paramètres de projet ?
- **Q-UI-03** — Vue graphe éditable (création de liens par glisser-déposer) ?
- **Q-UI-04** — Historique d'activité (qui a modifié quoi) ?
