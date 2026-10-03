# 04 — Topologie et dépôts git

> Statut : v0 rétro-spécification (code au 2026-10-02).
> La génération et le push par topologie sont détaillés dans [13](13-generation-bicep.md), [14](14-generation-pipelines-bootstrap.md) et [15](15-livraison-git.md).

## 1. Objectif

Dire **où** vont les fichiers générés : dans quels dépôts git du client, avec quels droits
d'accès. Le choix de topologie est structurant : il change ce qui peut être généré au niveau
projet, la forme des pipelines, le bootstrap et le push.

## 2. Les trois topologies (LayoutPreset)

| Topologie | Nom à l'écran | Dépôts | Où sont déclarés les dépôts |
|---|---|---|---|
| `AllInOne` | Tout-en-un | 1 dépôt qui reçoit l'infrastructure **et** le code applicatif | Projet |
| `SplitInfraCode` | Infra / code séparés | 2 dépôts : un Infrastructure, un Code applicatif | Projet |
| `MultiRepo` | Un dépôt par configuration | Chaque configuration déclare ses propres dépôts | Configuration |

### Contenu d'un dépôt (ContentKinds)

Un dépôt déclare ce qu'il reçoit : `Infrastructure`, `ApplicationCode`, ou les deux.

- **Infrastructure** reçoit : le Bicep, les pipelines d'infrastructure, le bootstrap
  d'infrastructure.
- **ApplicationCode** reçoit : les pipelines applicatifs et le bootstrap applicatif.

**RG-DEP-01 — Au moins un contenu.** Un dépôt sans contenu est refusé.

### Règles par topologie (dépôts projet)

**RG-DEP-02 — Tout-en-un.** Exactement un dépôt projet, qui porte Infrastructure **et**
ApplicationCode. Un deuxième dépôt est refusé.

**RG-DEP-03 — Infra / code séparés.** Au plus deux dépôts projet ; chacun porte un seul contenu ;
les deux contenus ne peuvent pas être portés deux fois. Un dépôt qui porte les deux contenus est
refusé.

**RG-DEP-04 — Un dépôt par configuration.** Aucun dépôt projet n'est autorisé. Les dépôts sont
déclarés sur chaque configuration.

**RG-DEP-05 — Changer de topologie vide les dépôts.** Passer d'une topologie à une autre supprime
tous les dépôts projet déclarés (pas les dépôts git eux-mêmes). Choisir la même topologie ne fait
rien. Réservé à l'Owner.

### Répartition par configuration (topologie Un dépôt par configuration)

En `MultiRepo`, chaque configuration choisit sa propre répartition (ConfigLayoutMode) :

| Répartition | Dépôts de la configuration |
|---|---|
| `AllInOne` | 1 dépôt Infrastructure + ApplicationCode |
| `SplitInfraCode` | 2 dépôts : un Infrastructure, un ApplicationCode |

**RG-DEP-06 — Répartition obligatoire.** Une configuration doit avoir une répartition avant de
recevoir un dépôt.

**RG-DEP-07 — Mêmes règles qu'au niveau projet.** Les règles RG-DEP-02 et RG-DEP-03 s'appliquent
aux dépôts d'une configuration selon sa répartition.

**RG-DEP-08 — Changer de répartition vide les dépôts.** Changer la répartition d'une configuration
(y compris vers « aucune ») supprime ses dépôts déclarés. Réservé à l'Owner.

## 3. Dépôt

| Champ | Type / valeurs | Obligatoire | Règles |
|---|---|---|---|
| Fournisseur | `GitHub`, `AzureDevOps` | Voir RG-DEP-09 | — |
| URL | URL absolue | Voir RG-DEP-09 | Le propriétaire et le nom du dépôt sont extraits de l'URL (section 3.1). |
| Branche par défaut | Texte | Voir RG-DEP-09 | Branche de base des pushes. |
| Contenu | Infrastructure, ApplicationCode | Oui | RG-DEP-01 |
| Jeton git | Texte secret | Voir RG-DEP-10 | Stocké dans Azure Key Vault, jamais renvoyé. |

**RG-DEP-09 — Dépôt projet : complet ou emplacement.** Un dépôt projet a soit fournisseur, URL et
branche tous renseignés, soit aucun (« emplacement » à compléter). Un dépôt de configuration doit
toujours être complet.

**RG-DEP-10 — Jeton obligatoire et vérifié à l'ajout.** À l'ajout d'un dépôt complet (projet ou
configuration), le jeton est obligatoire. IFS liste les branches du dépôt avec ce jeton et vérifie
que la branche par défaut existe (comparaison insensible à la casse). En cas d'échec, le dépôt
n'est pas enregistré. À la modification, le jeton n'est remplacé que s'il est fourni.

**RG-DEP-11 — Un secret par dépôt.** Chaque dépôt a son propre secret dans Key Vault, nommé à
partir de l'identifiant du dépôt (`git-pat-repo-<id>`). Un dépôt n'utilise jamais le jeton d'un
autre.

### 3.1 Lecture de l'URL

| Fournisseur | Format reconnu | Propriétaire extrait | Nom extrait |
|---|---|---|---|
| GitHub | `github.com/<owner>/<repo>` | `<owner>` | `<repo>` |
| Azure DevOps | `dev.azure.com/<org>/<project>/_git/<repo>` | `<org>/<project>` | `<repo>` |
| Autre forme | — | avant-dernier segment du chemin | dernier segment |

Le suffixe `.git` et la barre finale sont retirés.

## 4. Cas d'utilisation

**UC-DEP-01 — Choisir la topologie** (Owner). Voir RG-DEP-05.

**UC-DEP-02 — Ajouter un dépôt projet** (Owner). Saisie : contenu, fournisseur, URL, branche,
jeton. Contrôles RG-DEP-02/03/09/10, puis enregistrement du jeton dans Key Vault.

**UC-DEP-03 — Modifier un dépôt projet** (Owner). Même contrôles ; jeton remplacé seulement s'il
est fourni.

**UC-DEP-04 — Retirer un dépôt projet** (Owner).

**UC-DEP-05 — Vérifier une connexion avant d'enregistrer.** À partir d'un fournisseur, d'une URL et
d'un jeton saisis, IFS liste les branches pour que l'utilisateur choisisse la branche par défaut.
Rien n'est enregistré.

**UC-DEP-06 — Tester la connexion d'un dépôt enregistré.** IFS relit le jeton dans Key Vault et
teste l'accès au dépôt.

**UC-DEP-07 — Choisir la répartition d'une configuration** (Owner, `MultiRepo`).

**UC-DEP-08 — Ajouter / modifier / retirer un dépôt de configuration** (Owner).

**UC-DEP-09 — Lister les branches du dépôt Infrastructure du projet.**

**UC-DEP-10 — Naviguer dans le dépôt de code.** Lister les branches du dépôt ApplicationCode,
rechercher des fichiers (motif de nom, 100 caractères max) et des dossiers sur une branche. Utilisé
pour choisir un Dockerfile ou un dossier source dans le pipeline applicatif ([12](12-pipeline-applicatif.md)).

## 5. Routage d'un artefact vers un dépôt

**RG-DEP-12 — Rôle.** Les pipelines applicatifs et le bootstrap applicatif vont vers le dépôt qui
porte ApplicationCode. Tout le reste (Bicep, pipelines d'infrastructure, bootstrap d'infrastructure)
va vers le dépôt qui porte Infrastructure.

**RG-DEP-13 — Où chercher.** En `MultiRepo`, IFS cherche dans les dépôts de la configuration
concernée ; sans configuration (opération de niveau projet), aucun dépôt n'est trouvé. Dans les deux
autres topologies, il cherche dans les dépôts projet.

**RG-DEP-14 — Erreurs de routage.** Aucun dépôt déclaré ; aucun dépôt ne porte le contenu attendu ;
le dépôt est un emplacement non configuré.

**RG-DEP-15 — Pas de sous-dossier.** Les fichiers sont toujours écrits à partir de la racine du
dépôt (voir EC-DEP-03).

## 6. Écarts constatés

- **EC-DEP-01 (F20) — Remplacement de jeton isolé sans écran.** Une route permet de remplacer le jeton
  d'un dépôt projet, mais aucun bouton du front ne l'ouvre. Le remplacement passe par la modification
  du dépôt. Décision de tri : `cut`.
- **EC-DEP-02 — Secret fantôme.** Plusieurs traitements utilisent, si le dépôt n'a pas de nom de
  secret, un secret `git-pat-<projectId>` que rien n'écrit jamais (test de connexion du projet,
  détection des options de pipeline). Depuis la correction D01, chaque dépôt a son secret ; ces
  replis ne servent plus mais restent dans le code.
- **EC-DEP-03 — Sous-chemin de dépôt jamais honoré.** Le modèle prévoit un chemin de base dans le
  dépôt (pour le Bicep et les pipelines), mais il vaut toujours « aucun ». On ne peut pas écrire dans
  un sous-dossier d'un dépôt partagé.
- **EC-DEP-04 — Droits par niveau incohérents.** Les dépôts de configuration sont réservés à
  l'Owner, mais un Contributor peut créer la configuration elle-même. Un Contributor peut donc créer
  une configuration `MultiRepo` qu'il ne pourra jamais livrer sans un Owner.
- **EC-DEP-05 — Fournisseurs annoncés.** Un validateur de vérification de connexion accepte
  `GitLab` et `Bitbucket`, qui n'ont aucune implémentation.
- **EC-DEP-06 — Validation réelle MultiRepo manquante.** Le flux complet `MultiRepo` (jeton par
  configuration, bootstrap, push en lot) est implémenté et testé unitairement, mais n'a pas encore
  été validé contre un vrai fournisseur git (D01, D09, D10).
- **EC-DEP-07 — Navigation de dépôt partielle en MultiRepo (F06).** La navigation dans le dépôt de
  code (branches, fichiers, dossiers) part du niveau projet ; en `MultiRepo`, elle n'a pas de dépôt
  à cibler.
- **EC-DEP-08 — Changement de topologie destructif sans avertissement serveur.** Les dépôts sont
  effacés immédiatement ; la confirmation dépend du seul front.

## 7. Questions ouvertes

- **Q-DEP-01** — Faut-il permettre un sous-dossier par dépôt (exemple : un dossier `infra/` dans un
  monorepo applicatif existant) ?
- **Q-DEP-02** — GitLab, Bitbucket, GitHub Enterprise Server, Azure DevOps Server : lesquels pour la
  vente ?
- **Q-DEP-03** — Authentification par application (GitHub App, identité managée / service principal
  Azure DevOps) plutôt que par jeton personnel ?
- **Q-DEP-04** — Alerte d'expiration des jetons git (IFS ne connaît pas leur date d'expiration
  aujourd'hui) ?
- **Q-DEP-05** — En `MultiRepo`, une configuration doit-elle pouvoir partager le dépôt d'une autre ?
