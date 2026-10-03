# 16 — Diagnostics et contrôles avant génération

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F22 (diagnostics de configuration, `keep`).

## 1. Objectif

Signaler à l'utilisateur, avant qu'il génère ou déploie, les incohérences probables de son modèle :
droit manquant, image absente, nom déjà pris, environnement incomplet, domaine non validé.

## 2. Diagnostics serveur (par configuration)

**UC-DIA-01 — Lire les diagnostics d'une configuration.** Chaque constat porte : ressource (id, nom,
type), gravité (`Info`, `Warning`), code de règle, cible ou détail.

| Code | Gravité | Condition |
|---|---|---|
| `ACR_PULL_MISSING` | Warning | WebApp / FunctionApp / ContainerApp liée à un registre, authentification autre que `AdminCredentials`, et ni la ressource ni son identité de tirage n'ont le rôle AcrPull sur ce registre. |
| `KEY_VAULT_ACCESS_MISSING` | Warning | Une ressource a des app settings qui référencent un Key Vault sans avoir le rôle Key Vault Secrets User sur ce Key Vault. |
| `DOCKER_IMAGE_NOT_SET` | Info | Ressource en mode conteneur (ContainerApp, ou WebApp / FunctionApp en mode `Container`), non existante, sans nom d'image. |
| `DOCKER_IMAGE_NOT_VALIDATED` | Warning | Même ressource, image renseignée mais non marquée validée. |
| `NAME_UNAVAILABLE` | Info | Pour chaque environnement qui a un abonnement : le nom calculé est déjà résolu dans le DNS public Azure (« attendu si la configuration est déjà déployée »). Types couverts : ceux de la vérification de disponibilité ([06](06-nommage.md)). |

**RG-DIA-01 — Non bloquant.** Les diagnostics informent ; ils n'empêchent ni la génération ni le
push côté serveur.

## 3. Contrôles de l'écran avant génération projet

Avant une génération au niveau projet, l'écran rassemble, pour chaque configuration :

| Contrôle | Condition |
|---|---|
| Diagnostics serveur | Section 2. |
| Réglages par environnement manquants | Ressource non existante, d'un type qui a des réglages, sans jeu pour un des environnements du projet. |
| Domaines personnalisés en attente | Domaine au statut `Pending` (il ne sera pas généré). |
| Images Docker en attente | Image absente ou non validée. |
| Environnements incomplets | Environnement du projet sans abonnement ou sans service connection ARM. |

**UC-DIA-02 — Confirmer malgré les constats.** S'il y a au moins un constat, une boîte de dialogue
les présente regroupés par configuration ; l'utilisateur peut corriger (liens vers les ressources)
ou continuer.

Les diagnostics apparaissent aussi sur la liste des ressources d'une configuration (pastille par
ressource).

## 4. Analyse d'impact

L'analyse d'impact avant retrait d'un rôle est décrite en [09 § 6](09-identites-et-rbac.md).

## 5. Écarts constatés

- **EC-DIA-01 — Règle Key Vault stricte** : seul Secrets User est reconnu (EC-RBA-03).
- **EC-DIA-02 — Diagnostics éclatés.** Une partie des contrôles est faite par le serveur, une autre
  seulement par l'écran (réglages manquants, domaines, environnements incomplets) : ni l'API ni le
  serveur MCP ne les voient.
- **EC-DIA-03 — Contrôles absents** : privatisée sans Private Endpoint (D11), références
  cross-config ignorées (F15), clés App Configuration non générées (D07), paramètre sécurisé sans
  variable group, CIDR de subnet hors de l'espace d'adressage (EC-CAT-02), nom qui diffère entre
  l'écran et le Bicep (D05).
- **EC-DIA-04 — Disponibilité du nom par DNS public**, pas par l'API Azure `checkNameAvailability`.
- **EC-DIA-05 — Avant génération projet seulement.** La génération par configuration ne passe pas par
  la boîte de contrôle (**non vérifié pour tous les points d'entrée**).

## 6. Questions ouvertes

- **Q-DIA-01** — Centraliser tous les contrôles côté serveur, avec gravité `Error` bloquante ?
- **Q-DIA-02** — Proposer des corrections automatiques (accorder le rôle manquant en un clic) ?
- **Q-DIA-03** — Ajouter les contrôles listés en EC-DIA-03 ?
