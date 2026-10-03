# 02 — Accès, utilisateurs et droits

> Statut : v0 rétro-spécification (code au 2026-10-02).

## 1. Objectif

Garantir que seules les personnes autorisées voient et modifient un projet, et permettre à des
outils (scripts, agents IA) d'agir au nom d'un utilisateur avec des droits limités.

## 2. Authentification

### 2.1 Utilisateurs interactifs

- La connexion se fait avec **Microsoft Entra ID** (OAuth 2.0 / OpenID Connect, MSAL côté front,
  jeton JWT Bearer côté API).
- Il n'existe pas de compte local ni de mot de passe IFS.

**RG-ACC-01 — Création automatique de l'utilisateur.** À la première requête authentifiée, IFS
crée l'utilisateur à partir des revendications Entra : identifiant d'objet Entra (`oid`) et nom
d'affichage. Le nom d'affichage est découpé au premier espace en prénom et nom ; sans espace, tout
part dans le nom. Aucune adresse e-mail n'est stockée.

**RG-ACC-02 — Identité stable.** Un utilisateur est identifié par son identifiant d'objet Entra.
Un changement de nom dans Entra ne met pas à jour le nom stocké (**non vérifié à l'exécution**).

### 2.2 Clients non interactifs : jetons d'accès personnels IFS

Un utilisateur peut créer des jetons pour appeler l'API ou le serveur MCP sans passer par Entra.

| Donnée | Règle |
|---|---|
| Nom | Obligatoire, 100 caractères maximum. |
| Portées | Une ou plusieurs parmi `Read`, `Write`, `Generate`. Sans portée fournie : `Read` seul. Une valeur inconnue est ignorée silencieusement. |
| Expiration | Facultative. Si fournie, doit être dans le futur. Sans expiration, le jeton n'expire jamais. L'écran propose au plus aujourd'hui + 1 an. |
| Valeur | Format `ifs_` + secret aléatoire. Affichée **une seule fois** à la création. |
| Stockage | Seul un hachage SHA-256 et le préfixe affichable sont conservés. |
| Suivi | Date de création, date de dernière utilisation (mise à jour à chaque appel authentifié), état révoqué. |

**RG-ACC-03 — Utilisation.** Le jeton est envoyé dans l'en-tête `Authorization: Bearer ifs_…`.
Il est refusé s'il est révoqué ou expiré.

**RG-ACC-04 — Portées.** Quand la requête est authentifiée par un jeton IFS :

| Type d'opération | Portée exigée |
|---|---|
| Lecture (requête) | `Read` ou `Write` |
| Modification (commande) | `Write` |
| Génération (commandes marquées « génération ») | `Generate` |

Sans la portée, la réponse est une erreur « portée manquante ». Une connexion Entra interactive
n'est pas soumise aux portées.

**RG-ACC-05 — Les portées limitent, elles n'élargissent pas.** Un jeton agit avec les droits
projet de son propriétaire (section 3) ; la portée ne donne jamais plus que le rôle.

**UC-ACC-01 — Créer un jeton.** L'utilisateur saisit un nom, coche des portées, choisit une
expiration. IFS affiche la valeur une fois, avec un bouton de copie. Le jeton apparaît ensuite dans
la liste avec son préfixe, ses portées, ses dates.

**UC-ACC-02 — Révoquer un jeton.** L'utilisateur révoque un jeton depuis la liste. Il devient
inutilisable immédiatement. Il reste affiché comme révoqué (**non vérifié à l'exécution**).

**UC-ACC-03 — Lister ses jetons.** Chaque utilisateur ne voit que ses propres jetons.

## 3. Droits sur un projet

### 3.1 Rôles

| Rôle | Peut |
|---|---|
| **Owner** | Tout ce que fait Contributor, plus : gérer les membres et leurs rôles, ajouter/modifier/retirer les dépôts projet et leurs jetons, changer la topologie, ajouter/modifier les dépôts de configuration, changer la répartition infra/code d'une configuration, supprimer une configuration, supprimer le projet. |
| **Contributor** | Modifier le projet (environnements, nommage, tags, variable groups, agent pool), créer et modifier configurations, resource groups et ressources, générer, télécharger, pousser. |
| **Reader** | Consulter le projet, ses configurations, ses ressources et les fichiers générés. |

**RG-ACC-06 — Créateur Owner.** Le créateur d'un projet en devient automatiquement Owner.

**RG-ACC-07 — Invisibilité.** Un utilisateur non membre qui accède à un projet reçoit « projet
introuvable » (pas « accès refusé »), pour ne pas révéler son existence.

**RG-ACC-08 — Lecteur.** Un Reader qui tente une modification reçoit « interdit ».

**RG-ACC-09 — Droits hérités.** Les droits sur une configuration, un resource group ou une
ressource sont ceux du projet parent.

### 3.2 Opérations réservées à l'Owner (constat du code)

Ajouter, modifier le rôle de, retirer un membre · ajouter, modifier, retirer un dépôt projet ·
remplacer le jeton d'un dépôt projet · changer la topologie du projet · supprimer le projet ·
ajouter ou modifier un dépôt de configuration · changer la répartition infra/code d'une
configuration · supprimer une configuration.

### 3.3 Membres

**UC-ACC-04 — Ajouter un membre.** L'Owner choisit un utilisateur dans la liste des utilisateurs
connus d'IFS et un rôle. L'utilisateur doit s'être connecté au moins une fois à IFS pour exister.

**UC-ACC-05 — Changer le rôle d'un membre.** L'Owner change le rôle d'un membre.

**UC-ACC-06 — Retirer un membre.** L'Owner retire un membre.

## 4. Écarts constatés

- **EC-ACC-01 — Lectures sans contrôle d'appartenance.** Plusieurs lectures ne vérifient pas que
  l'utilisateur est membre du projet : diagnostics d'une configuration, contenu d'un fichier généré
  au niveau configuration (Bicep, pipeline, bootstrap), détection des options de pipeline,
  vérification de l'accès Key Vault et de l'accès AcrPull, liste des sorties disponibles. Toute
  personne authentifiée qui connaît l'identifiant peut les lire.
- **EC-ACC-02 — Liste de tous les utilisateurs.** La liste des utilisateurs (pour ajouter un
  membre) renvoie tous les utilisateurs connus d'IFS, sans filtre d'organisation.
- **EC-ACC-03 — Dernier Owner.** Rien n'empêche de retirer ou de rétrograder le dernier Owner d'un
  projet (**non vérifié à l'exécution** ; aucune règle trouvée dans le domaine).
- **EC-ACC-04 — Doublon de membre.** Rien dans le domaine n'empêche d'ajouter deux fois le même
  utilisateur au même projet (**non vérifié à l'exécution**).
- **EC-ACC-05 — Portées inconnues.** Une portée mal orthographiée à la création d'un jeton est
  ignorée sans erreur ; si toutes le sont, le jeton n'a aucune portée.
- **EC-ACC-06 — Invitation impossible.** On ne peut ajouter que quelqu'un qui s'est déjà connecté ;
  il n'existe pas d'invitation par e-mail.

## 5. Questions ouvertes

- **Q-ACC-01** — Faut-il un niveau organisation (tenant client) au-dessus des projets, pour la vente
  multi-clients et pour filtrer la liste des utilisateurs ?
- **Q-ACC-02** — Faut-il des rôles plus fins (par configuration, ou un rôle « Déployeur » qui peut
  pousser sans modéliser) ?
- **Q-ACC-03** — Le jeton IFS doit-il pouvoir être limité à un projet ?
- **Q-ACC-04** — Invitation par e-mail d'une personne qui ne s'est jamais connectée ?
