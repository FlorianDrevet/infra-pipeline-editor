# Azure DevOps — Configuration du Process Board

## Organisation et Projet

| Paramètre | Valeur |
|-----------|--------|
| Organisation | `florian-drevet` |
| Projet | `Infra Flow Sculptor` |
| URL | `https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor` |

---

## 1. Process Template recommandé

Utiliser le process **Scrum** ou **Agile** personnalisé. Pour avoir les types `Epic / User Story / Technical Story / Task`, créer un **process hérité** depuis Scrum.

### Étapes dans Azure DevOps

1. Aller dans **Organization Settings → Boards → Process**
2. Cliquer sur **Scrum** → **Create inherited process**
3. Nommer le process : `InfraFlowSculptor`
4. Une fois le process créé, ajouter un type de Work Item personnalisé : **Technical Story**

---

## 2. Types de Work Items à configurer

| Type | Icône suggérée | Niveau | Description |
|------|---------------|--------|-------------|
| **Epic** | ⚡ (violet) | Portfolio | Grande initiative stratégique (ex: "Gestion des ressources Azure") |
| **User Story (US)** | 📋 (bleu) | Requirement | Besoin utilisateur (ex: "En tant que DevOps, je peux créer un KeyVault") |
| **Technical Story (TS)** | 🔧 (gris) | Requirement | Tâche technique pure sans valeur métier directe (ex: "Configurer EF Core + Migrations") |
| **Task** | ✅ (jaune) | Task | Sous-tâche de 1-4h, enfant d'une US ou TS |
| **Bug** | 🐛 (rouge) | Requirement | Défaut à corriger |

### Ajouter le type "Technical Story"

Dans **Organization Settings → Process → InfraFlowSculptor** :

1. Cliquer sur **Work item types → New work item type**
2. Nom : `Technical Story`
3. Icône : engrenage (gear), couleur grise
4. Hériter de : `Task` (pour qu'il apparaisse au niveau Requirement) ou créer un type indépendant
5. Ajouter les champs : `Title`, `Description`, `Acceptance Criteria`, `Story Points`, `Iteration`, `Area`

### Hiérarchie des types

```
Epic
└── User Story (US)
    └── Task
└── Technical Story (TS)
    └── Task
└── Bug
    └── Task
```

---

## 3. Configuration des colonnes du Board

### Colonnes recommandées (Kanban / Sprint Board)

| Colonne | État WI | WIP Limit |
|---------|---------|-----------|
| New / Backlog | New | — |
| Refinement | Active | 5 |
| Sprint Ready | Active | — |
| In Progress | Active | 3 |
| In Review / PR | Active | 3 |
| Done | Closed | — |

### Swimlanes

- **Epics en cours**
- **US prioritaires (sprint courant)**
- **Tech Debt (TS)**

---

## 4. Configuration des sprints (Iterations)

Créer les itérations suivantes dans **Project Settings → Boards → Team Configuration → Iterations** :

| Sprint | Dates suggérées |
|--------|----------------|
| Sprint 0 — Setup | déjà complété |
| Sprint 1 — Core Architecture | déjà complété |
| Sprint 2 — InfraConfig + Members | déjà complété |
| Sprint 3 — Azure Resources (RG, KV, Redis) | déjà complété |
| Sprint 4 — StorageAccount + SubResources | déjà complété |
| Sprint 5 — BicepGenerator | déjà complété |
| Sprint 6 — Auth & RBAC | déjà complété |
| Sprint 7 — En cours | actuel |

---

## 5. Areas (Areas Path)

Configurer les Areas pour aligner avec l'architecture :

```
Infra Flow Sculptor
├── API Core
│   ├── InfrastructureConfig
│   ├── ResourceGroup
│   ├── KeyVault
│   ├── RedisCache
│   └── StorageAccount
├── BicepGenerator
│   ├── Generation Engine
│   └── Artifact Storage
├── Shared
│   ├── Domain
│   ├── Infrastructure
│   └── Auth
└── DevOps & Infra
    ├── Aspire
    └── CI/CD
```

---

## 6. Champs personnalisés à ajouter

Sur `User Story` et `Technical Story` :

| Champ | Type | Description |
|-------|------|-------------|
| `Acceptance Criteria` | HTML | Critères d'acceptation |
| `Story Points` | Integer | Estimation en points |
| `Component` | Picklist | API / BicepGenerator / Shared / Aspire |

---

## 7. Prochaines étapes

Une fois le process configuré :
1. Appliquer le nouveau process **InfraFlowSculptor** au projet dans **Project Settings → Overview → Process**
2. Importer le backlog initial depuis [`board-backlog.md`](./board-backlog.md)
3. Créer les sprints et assigner les work items
