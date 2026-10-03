# 15 — Livraison dans git

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F06 (connexion et navigation, `fix`), F14 (push × 3 topologies, `fix`), F20 (PAT
> dédié, `cut`), D01 (corrigé), D10 (corrigé, validation réelle restante), D12.
> Les dépôts, jetons et règles de routage sont décrits en [04](04-topologie-et-depots.md).

## 1. Objectif

Pousser les fichiers générés (Bicep, pipelines, bootstrap) dans les dépôts git du client, sur une
branche choisie, en un commit par dépôt, en remplaçant les fichiers générés précédents.

## 2. Fournisseurs supportés

| Fournisseur | Opérations |
|---|---|
| Azure DevOps Repos | Tester la connexion, lister les branches, chercher fichiers / dossiers, lire un fichier, pousser. |
| GitHub | Idem. |

Le jeton d'accès du dépôt est lu dans le Key Vault d'IFS au moment de l'opération ([04](04-topologie-et-depots.md)).

## 3. Cas d'usage

**UC-GIT-01 — Tester la connexion** d'un dépôt : nom complet et branche par défaut, ou erreur.

**UC-GIT-02 — Lister les branches** (nom, protégée ou non).

**UC-GIT-03 — Pousser un artefact** (Contributor / PAT Generate) :

| Ce qui est poussé | Niveau projet | Niveau configuration |
|---|---|---|
| Bicep | Oui (hors MultiRepo) | Oui |
| Pipelines | Oui (hors MultiRepo) | Oui |
| Bootstrap | Oui (hors MultiRepo) | Oui |
| Tout l'ensemble généré | Oui, AllInOne / SplitInfraCode | — |
| Tout l'ensemble, une cible par configuration | Oui, MultiRepo | — |

Saisie : branche cible et message de commit.

**RG-GIT-01 — Branche.** Obligatoire, 200 caractères max, lettres, chiffres, `/`, `_`, `.`, `-`.

**RG-GIT-02 — Message de commit.** Obligatoire, 500 caractères max.

**RG-GIT-03 — Branche créée si absente.** La branche cible est créée depuis la branche par défaut du
dépôt si elle n'existe pas ; sinon le commit est ajouté dessus.

**RG-GIT-04 — Un commit par dépôt.** Tous les fichiers d'un dépôt partent dans un seul commit.

**RG-GIT-05 — Nettoyage des fichiers obsolètes.** Dans les dossiers gérés par IFS (dossiers de
premier niveau des fichiers poussés), un fichier qui n'est plus généré est supprimé. Les fichiers
hors de ces dossiers ne sont jamais touchés.

**RG-GIT-06 — Routage par contenu.** Artefacts d'infrastructure vers le dépôt `Infrastructure`,
pipelines et bootstrap applicatifs vers le dépôt `ApplicationCode` (SplitInfraCode, MultiRepo).

**RG-GIT-07 — MultiRepo : pré-validation et résultats partiels.** Toutes les cibles sont vérifiées
avant le premier push ; chaque dépôt est poussé avec son jeton ; le résultat est donné par cible,
sans atomicité entre dépôts. Une annulation arrête la séquence.

**RG-GIT-08 — Source des fichiers.** Les fichiers poussés sont ceux de la **dernière génération
stockée** ; il faut générer avant de pousser.

**Résultat** : nom de la branche et lien vers elle chez le fournisseur.

## 4. Écarts constatés

- **EC-GIT-01 (D12) — Le bouton « Push to Git » de l'en-tête de configuration ne pousse que le
  Bicep**, jamais les pipelines, quel que soit l'onglet.
- **EC-GIT-02 — Pas de pull request.** IFS pousse sur une branche mais ne crée jamais de pull
  request, même vers une branche protégée.
- **EC-GIT-03 — Fichiers à la racine non nettoyés.** Un fichier généré à la racine du dépôt (hors
  dossier) qui n'est plus produit n'est pas supprimé.
- **EC-GIT-04 — Sous-chemin de dépôt ignoré.** Le chemin de base d'un dépôt n'est jamais renseigné :
  tout est poussé depuis la racine (EC-PIP-05).
- **EC-GIT-05 — Validation réelle MultiRepo** non faite (D01, D09, D10).
- **EC-GIT-06 (F20) — Écran mort de renouvellement du jeton.** Une route dédiée existe, son écran
  n'est ouvert par aucun bouton ; le jeton se change dans la modale du dépôt. Décision : `cut`.
- **EC-GIT-07 — Génération et push non liés.** Rien n'empêche de pousser une génération ancienne,
  antérieure aux dernières modifications du modèle.

## 5. Questions ouvertes

- **Q-GIT-01** — Créer une pull request après le push (titre, description, relecteurs) ?
- **Q-GIT-02** — Générer et pousser en une seule action ?
- **Q-GIT-03** — Afficher le diff avec le contenu actuel du dépôt avant de pousser ?
- **Q-GIT-04** — Supporter GitLab, Bitbucket ?
- **Q-GIT-05** — Remplacer les jetons personnels par une application GitHub / une identité managée ?
