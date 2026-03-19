---
description: 'Conventions obligatoires pour la création de Pull Requests par les agents GitHub Copilot.'
---
# Conventions Pull Request — InfraFlowSculptor

> Ce fichier est lu par tous les agents GitHub Copilot avant de créer ou de soumettre une Pull Request.  
> Ces conventions sont **obligatoires** et non-négociables.

---

## 1. Format du titre de PR

### Règle absolue

Le titre de la PR doit **toujours** décrire le **but principal** de la PR, jamais la dernière tâche effectuée.

### Format

```
type(scope): description courte du but principal
```

- **type** : un des types listés ci-dessous (obligatoire)
- **scope** : composant ou aggregate concerné, en kebab-case (optionnel, mais recommandé)
- **description** : phrase courte au présent, sans majuscule initiale, sans point final

### Types autorisés

| Type       | Quand l'utiliser |
|------------|-----------------|
| `feat`     | Nouvelle fonctionnalité ou feature CQRS complète |
| `fix`      | Correction d'un bug ou d'un comportement incorrect |
| `refactor` | Restructuration sans changement fonctionnel |
| `perf`     | Amélioration des performances |
| `docs`     | Documentation uniquement (README, MEMORY.md, commentaires) |
| `test`     | Ajout ou modification de tests |
| `chore`    | Maintenance, mise à jour des dépendances, Dockerfile |
| `ci`       | Changements liés aux pipelines CI/CD / GitHub Actions |
| `style`    | Formatage, indentation, lint (aucun impact fonctionnel) |
| `revert`   | Annulation d'un commit ou d'une feature précédente |

### Exemples corrects

```
feat(storage-account): add StorageAccount aggregate with full CRUD
fix(key-vault): correct EF Core LINQ translation for KeyVaultId comparison
refactor(member): extract MemberCommandHelper to reduce duplication
feat(role-assignment): add role assignment management on Azure resources
chore: update Directory.Packages.props to latest package versions
docs: update MEMORY.md with StorageAccount conventions
```

### Exemples incorrects ❌

```
Add StorageAccountConfiguration.cs          ← dernière tâche, pas le but
WIP                                         ← pas de type ni description
Update files                                ← trop vague
feat: done                                  ← pas de description
```

---

## 2. Description de la PR

Utiliser le **template `.github/PULL_REQUEST_TEMPLATE.md`** fourni dans ce dépôt.

### Ce qui est obligatoire dans la description

1. **But principal** — une phrase résumant l'objectif global de la PR
2. **Type de changement** — coche la ou les cases correspondantes
3. **Changements par couche** — pour chaque couche impactée :
   - Domaine : nouveaux agrégats, entités, value objects, erreurs
   - Application : commandes, queries, handlers, validators
   - Infrastructure : configurations EF Core, repositories, migrations
   - Contrats : requests, responses
   - API : endpoints, mappings Mapster
   - Frontend : components/routes/services/facades/guards/environments dans `src/Front`
4. **Migration EF Core** — indiquer si une migration a été ajoutée et son nom
5. **Checklist** — valider chaque point avant de soumettre

### Règle : exhaustivité

Chaque fichier créé ou modifié doit apparaître dans au moins une section de la description.  
Ne pas regrouper tous les changements en une seule ligne vague comme "ajout de la feature".

---

## 3. Protocole de création de PR pour les agents

Quand un agent crée une PR, il doit :

1. **Identifier le but principal** de l'ensemble du travail effectué (pas la dernière modification).
2. **Construire le titre** selon le format `type(scope): description`.
3. **Remplir la description** en utilisant le template et en listant **tous** les fichiers créés ou modifiés, groupés par couche.
4. **S'assurer que le build passe** (`dotnet build .\InfraFlowSculptor.slnx`) avant de soumettre.
5. **Mettre à jour `MEMORY.md`** et inclure cette mise à jour dans la même PR.
6. **Exécuter le protocole Azure DevOps** (section 5) : chercher ou créer Epic/US, ajouter les Tasks, lier la PR aux work items, mettre à jour les statuts.

---

## 4. Scope recommandés par aggregate / composant

| Scope              | Concerne |
|--------------------|---------|
| `infra-config`     | InfrastructureConfig aggregate |
| `resource-group`   | ResourceGroup aggregate |
| `key-vault`        | KeyVault aggregate |
| `redis-cache`      | RedisCache aggregate |
| `role-assignment`  | RoleAssignment feature |
| `storage-account`  | StorageAccount aggregate (futur) |
| `member`           | Member management within InfrastructureConfig |
| `bicep`            | BicepGenerator API |
| `aspire`           | Aspire AppHost / service orchestration |
| `auth`             | Authentication / authorization |
| `shared`           | Shared.* projects |
| `ef-core`          | Migrations, configurations EF Core transversales |
| `ci`               | GitHub Actions, Dockerfile |

---

## 5. Gestion Azure DevOps — Protocole obligatoire

> Chaque PR créée par un agent **doit** être tracée dans Azure DevOps Boards.  
> Projet : `Infra Flow Sculptor`.  
> **Toutes les opérations ADO utilisent les outils MCP Azure DevOps** — aucune commande `az` CLI.

### 5.1 Hiérarchie des work items

```
Epic
└── User Story (US)
    └── Task (une par action significative réalisée)
```

La chaîne minimale est **Epic → US → Tasks**.

---

### 5.2 Étape 1 — Rechercher une Epic et une US existantes

Utiliser **`mcp_microsoft_azu_search_workitem`** avec les mots-clés du scope ou du titre de la PR.

**Recherche d'une Epic :**
```
outil : mcp_microsoft_azu_search_workitem
  searchText  : "<scope de la PR, ex: agent, storage-account, bicep>"
  project     : ["Infra Flow Sculptor"]
  workItemType: ["Epic"]
  state       : ["New", "Active", "In Progress"]
  top         : 10
```

**Recherche d'une User Story :**
```
outil : mcp_microsoft_azu_search_workitem
  searchText  : "<scope de la PR>"
  project     : ["Infra Flow Sculptor"]
  workItemType: ["User Story"]
  state       : ["New", "Active"]
  top         : 10
```

**Critères de correspondance :**
- Le titre contient le scope ou le sujet principal de la PR.
- L'état n'est pas `Closed`, `Resolved` ou `Done`.

---

### 5.3 Étape 2 — Créer les items manquants

#### Si aucune Epic correspondante n'existe

```
outil : mcp_microsoft_azu_wit_create_work_item
  project      : "Infra Flow Sculptor"
  workItemType : "Epic"
  fields:
    - name: "System.Title"
      value: "<Titre de l'Epic — domaine fonctionnel>"
    - name: "System.Description"
      value: "<Description du domaine couvert>"
      format: "Html"
```

Récupérer l'`id` retourné — c'est l'`ID_EPIC`.

#### Si aucune US correspondante n'existe

```
outil : mcp_microsoft_azu_wit_create_work_item
  project      : "Infra Flow Sculptor"
  workItemType : "User Story"
  fields:
    - name: "System.Title"
      value: "<Titre décrivant le besoin couvert par la PR>"
    - name: "System.Description"
      value: "En tant que développeur, je veux <objectif> afin de <valeur>."
      format: "Html"
```

Puis lier l'US à l'Epic en tant qu'enfant :

```
outil : mcp_microsoft_azu_wit_work_items_link
  project : "Infra Flow Sculptor"
  updates:
    - id      : <ID_US>
      linkToId: <ID_EPIC>
      type    : "child"
```

---

### 5.4 Étape 3 — Créer les Tâches sur l'US

Utiliser **`mcp_microsoft_azu_wit_add_child_work_items`** pour créer toutes les Tasks en un appel :

```
outil : mcp_microsoft_azu_wit_add_child_work_items
  project      : "Infra Flow Sculptor"
  parentId     : <ID_US>
  workItemType : "Task"
  items:
    - title      : "<Action réalisée — ex: Créer merge-main.agent.md>"
      description: "<Description de ce qui a été fait>"
      format     : "Html"
    - title      : "<Autre action>"
      description: "<Description>"
```

**Exemples de Tasks selon le type de PR :**

| Type de PR | Tasks typiques |
|-----------|---------------|
| `feat(aggregate)` | Créer l'agrégat domaine, Ajouter commandes/queries/handlers, Configurer EF Core, Créer migration, Ajouter endpoints, Aligner frontend |
| `fix` | Analyser le bug, Corriger le handler/repository, Vérifier le build |
| `chore(agents)` | Créer fichier agent, Mettre à jour MEMORY.md, Supprimer fichiers obsolètes |
| `docs` | Rédiger documentation, Créer pages wiki ADO |
| `refactor` | Restructurer le code, Extraire helper, Vérifier non-régression |

---

### 5.5 Étape 4 — Lier les work items à la PR GitHub

Utiliser **`mcp_microsoft_azu_wit_add_artifact_link`** pour lier l'US et l'Epic à la PR GitHub.

> Prérequis : récupérer d'abord le `projectId` (GUID) et le `repositoryId` (GUID) via `mcp_microsoft_azu_repo_get_repo_by_name_or_id` avec `repositoryName: "infra-pipeline-editor"`.

**Lier l'US à la PR :**
```
outil : mcp_microsoft_azu_wit_add_artifact_link
  workItemId    : <ID_US>
  project       : "Infra Flow Sculptor"
  linkType      : "Pull Request"
  pullRequestId : <N° de la PR GitHub>
  projectId     : "<GUID du projet ADO>"
  repositoryId  : "<GUID du repo GitHub dans ADO>"
```

**Lier l'Epic à la PR (même appel, workItemId différent) :**
```
outil : mcp_microsoft_azu_wit_add_artifact_link
  workItemId    : <ID_EPIC>
  project       : "Infra Flow Sculptor"
  linkType      : "Pull Request"
  pullRequestId : <N° de la PR GitHub>
  projectId     : "<GUID du projet ADO>"
  repositoryId  : "<GUID du repo GitHub dans ADO>"
```

#### Mentionner les items dans la description de la PR GitHub

Dans la section "Issues / tickets liés" de la PR, ajouter :
```
ADO Epic #<ID_EPIC> : <Titre Epic>
ADO User Story #<ID_US> : <Titre US>
```

---

### 5.6 Étape 5 — Mettre à jour les statuts

**Règles de mise à jour :**

| Situation | État cible US | État cible Tasks existantes |
|-----------|--------------|----------------------------|
| PR créée, travail en cours | `Active` | `Active` |
| PR fusionnée vers main | `Resolved` | `Closed` |
| Bug corrigé et validé | `Resolved` | `Closed` |
| Feature complète | `Resolved` | `Closed` |

**Mettre à jour l'état d'un item :**
```
outil : mcp_microsoft_azu_wit_update_work_item
  id      : <ID_US ou ID_TASK>
  updates :
    - path : "/fields/System.State"
      value: "Active"   ← ou "Resolved" / "Closed"
```

**Mettre à jour plusieurs items en une fois :**
```
outil : mcp_microsoft_azu_wit_update_work_items_batch
  updates:
    - id   : <ID_TASK_1>
      path : "/fields/System.State"
      value: "Closed"
    - id   : <ID_TASK_2>
      path : "/fields/System.State"
      value: "Closed"
```

---

### 5.7 Résumé du protocole DevOps — ordre d'exécution

```
1. mcp_microsoft_azu_search_workitem  → chercher Epic correspondante
   ├─ Trouvée → retenir son ID
   └─ Non trouvée → mcp_microsoft_azu_wit_create_work_item (Epic)

2. mcp_microsoft_azu_search_workitem  → chercher US correspondante
   ├─ Trouvée → retenir son ID + mettre à jour son état → Active
   └─ Non trouvée → mcp_microsoft_azu_wit_create_work_item (User Story)
                  + mcp_microsoft_azu_wit_work_items_link (US enfant de l'Epic)

3. mcp_microsoft_azu_wit_add_child_work_items → créer toutes les Tasks sur l'US

4. mcp_microsoft_azu_wit_add_artifact_link → lier US et Epic à la PR GitHub

5. mcp_microsoft_azu_wit_update_work_item (ou _batch) → mettre à jour les statuts

6. Ajouter les références ADO dans la section "Issues / tickets liés" de la PR
```

---

### 5.8 Référence des outils MCP utilisés

| Outil MCP | Usage |
|-----------|-------|
| `mcp_microsoft_azu_search_workitem` | Rechercher Epics et US existantes par mots-clés |
| `mcp_microsoft_azu_wit_create_work_item` | Créer une Epic ou une US |
| `mcp_microsoft_azu_wit_add_child_work_items` | Créer toutes les Tasks en batch sous une US |
| `mcp_microsoft_azu_wit_work_items_link` | Lier l'US à l'Epic (relation enfant/parent) |
| `mcp_microsoft_azu_wit_add_artifact_link` | Lier un work item à la PR GitHub |
| `mcp_microsoft_azu_wit_update_work_item` | Mettre à jour l'état d'un item unique |
| `mcp_microsoft_azu_wit_update_work_items_batch` | Mettre à jour l'état de plusieurs items |
| `mcp_microsoft_azu_repo_get_repo_by_name_or_id` | Récupérer le GUID du repo (requis pour artifact link) |

