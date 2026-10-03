# 13 — Génération Bicep

> Statut : v0 rétro-spécification (code au 2026-10-02) — **première version, à compléter**
> (structure exacte des dossiers, contenu de chaque fichier, rétention).
> Feature-map : F11 (niveau projet, `keep`), F11b (niveau config, `fix`), F15 (cross-config, `fix`),
> F23 (dernière génération, `keep`), D05, D06, D07.

## 1. Objectif

Transformer le modèle d'une configuration (resource groups, ressources, réglages par environnement,
identités, rôles, app settings, réseau, tags, nommage) en fichiers Bicep déployables, un fichier de
paramètres par environnement.

## 2. Niveaux de génération

| Niveau | Contenu | Topologies |
|---|---|---|
| Projet | Toutes les configurations du projet en une passe : dossier `Common/` partagé + un dossier par configuration. | AllInOne, SplitInfraCode. **Refusé en MultiRepo.** |
| Configuration | Une seule configuration. | Toutes côté serveur ; l'écran ne le propose qu'en MultiRepo (EC-GEN-01). |

**UC-GEN-01 — Générer le Bicep** (Contributor ou PAT de portée Generate).
**UC-GEN-02 — Télécharger les fichiers générés** (archive) et **lire un fichier**.
**UC-GEN-03 — Retrouver la dernière génération** sans regénérer.

## 3. Fichiers produits (par configuration)

| Fichier | Rôle |
|---|---|
| `main.bicep` | Point d'entrée : déclare les resource groups et appelle un module par ressource. |
| `main.<nomCourt>.bicepparam` | Un par environnement : valeurs des réglages par environnement, tags, domaines validés. |
| Modules par type | Un module par ressource (dossier par type). |
| `constants.bicep` | Identifiants des rôles RBAC utilisés. |
| `types.bicep`, `functions.bicep` | Types partagés (environnement…) et fonctions (nommage). |
| Module `kvSecrets` | Écriture des secrets dans les Key Vaults (exports sensibles, `ViaBicepparam`). |
| Modules d'attribution de rôles | Un par type de cible ([09](09-identites-et-rbac.md)). |
| `PrivateEndpoints/`, `Networking/` | Private Endpoints et résolution du subnet ([11](11-reseau-et-exposition.md)). |

Les artefacts sont stockés (Blob) pour téléchargement et push ([15](15-livraison-git.md)).

## 4. Étapes de génération (ordre d'exécution)

| Ordre | Étape | Effet |
|---|---|---|
| 100 | Analyse des identités | Détermine les identités system/user-assigned par ressource. |
| 200 | Analyse des app settings | Classe les app settings par source ([10](10-configuration-applicative.md)). |
| 300 | Construction des modules | Un module par ressource, depuis son générateur de type. |
| 400 | Injection des identités | Bloc `identity`, paramétré si les combinaisons diffèrent. |
| 500 | Injection des outputs | Outputs consommés par d'autres ressources. |
| 520 / 540 / 560 | Réseau | Résolution du subnet, Private Endpoints compagnons, accès public coupé. |
| 600 | Injection des app settings | Paramètres `appSettings` / `envVars`. |
| 700 | Tags | Paramètre de tags (union projet / config / environnement). |
| 800 | Références | Parents et références cross-config (`existing`). |
| 850 | Émission | Modules écrits en texte depuis la représentation intermédiaire. |
| 900 | Assemblage | `main.bicep`, paramètres, constantes, types, fonctions. |
| 950 | Élagage | Retire les outputs non utilisés. |

## 5. Règles

**RG-GEN-01 — Nom Azure** calculé par les fonctions Bicep à partir du gabarit (voir [06 § 4.2](06-nommage.md)).
**RG-GEN-02 — Réglage absent** non écrit dans le fichier de paramètres (RG-RES-02).
**RG-GEN-03 — Ressource existante** déclarée `existing`, jamais déployée.
**RG-GEN-04 — Paramètres sécurisés** jamais écrits en clair ; fournis par la pipeline ([10 § 6](10-configuration-applicative.md)).

## 6. Écarts constatés

- **EC-GEN-01 (F11b) — Génération par configuration joignable par l'API mais sans entrée à l'écran hors MultiRepo.**
- **EC-GEN-02 (D05) — Le nom généré peut différer du nom affiché et validé à l'écran.**
- **EC-GEN-03 (D06) — Réglages par environnement perdus pour DocumentIntelligence (nom de module de base inconnu).**
- **EC-GEN-04 (D07) — Clés App Configuration jamais générées.**
- **EC-GEN-05 (F15) — Seules 6 propriétés de référence cross-config sont branchées ; les autres sont ignorées sans message.**
- **EC-GEN-06 — Pas d'historique ni de rétention des générations : seule la dernière est retrouvable.**
- **EC-GEN-07 — Pas de validation du Bicep produit (`bicep build`, what-if) avant livraison.**

## 7. Questions ouvertes

- **Q-GEN-01** — Valider le Bicep généré (compilation, what-if) dans IFS ?
- **Q-GEN-02** — Historiser les générations et afficher un diff entre deux versions ?
- **Q-GEN-03** — Utiliser les Azure Verified Modules plutôt que des modules maison ?
