# Guide Claude Code — InfraFlowSculptor

> Tutoriel complet pour utiliser ta flotte agentique **migrée de GitHub Copilot vers Claude Code**.
> Tu ne connais pas Claude Code ? Lis ce guide de haut en bas une fois. Ensuite garde-le comme référence.

---

## 0. TL;DR — Ce qui a été fait

Ta flotte Copilot (`.github/agents`, `.github/skills`, `.github/prompts`, `.github/memory`) a été **dupliquée et adaptée** dans `.claude/`. **Rien dans `.github/` n'a été modifié** : Copilot continue de fonctionner exactement comme avant. Les deux systèmes sont **totalement séparés**.

| Tu veux… | Tu tapes / fais… |
|----------|------------------|
| Lancer une tâche complexe orchestrée | `/dev <ta demande>` |
| Générer une feature CQRS | `/new-cqrs-feature <Nom> Create,Read,…` |
| Ajouter une ressource Azure | `/add-azure-resource <Nom> <typeARM> <abbr>` |
| Review pré-merge | `/review-main` |
| Merger main | `/merge-main` |
| Lancer un audit | `/run-audit` |
| Une tâche backend précise | « utilise le sous-agent **dotnet-dev** pour… » |
| Une tâche frontend précise | « utilise le sous-agent **angular-front** pour… » |

---

## 1. La différence fondamentale Copilot ↔ Claude Code

Tu avais 4 briques dans Copilot. Voici leur équivalent Claude Code et **ce qui change vraiment** :

| Brique Copilot | Équivalent Claude Code | Différence clé |
|----------------|------------------------|----------------|
| `.github/copilot-instructions.md` | **`CLAUDE.md`** (racine) | Auto-chargé à chaque session. Identique dans l'esprit. |
| `.github/agents/*.agent.md` | **`.claude/agents/*.md`** (sous-agents) | ⚠️ Un sous-agent **ne peut pas en appeler un autre**. |
| `.github/skills/*/SKILL.md` | **`.claude/skills/*/SKILL.md`** | Quasi identique. Chargés automatiquement par le modèle. |
| `.github/prompts/*.prompt.md` | **`.claude/commands/*.md`** (slash-commands) | `{{ var }}` → `$ARGUMENTS`. Invoqués par toi avec `/nom`. |
| `.github/memory/*` | **`.claude/memory/*`** | De simples fichiers que Claude lit parce que CLAUDE.md le lui dit. |

### ⚠️ LA différence à comprendre absolument : pas de délégation imbriquée

Dans Copilot, ton agent `@dev` pouvait appeler `@dotnet-dev` qui pouvait lui-même appeler un autre agent. **Claude Code interdit ça** : un sous-agent est un *exécutant isolé en bout de chaîne*. Il a son propre contexte, fait son travail, et rend un résultat. Il **ne peut pas lancer d'autres sous-agents**.

**Conséquence sur ton orchestrateur `dev`** : il ne peut pas être un sous-agent (sinon il ne pourrait déléguer à personne). Il est donc devenu :
- le **thread principal** (la conversation Claude elle-même) qui joue le rôle de chef d'orchestre,
- piloté par la commande **`/dev`** (`.claude/commands/dev.md`) qui contient tout ton protocole (mémoire → dream gate → research → routage → skills → exécution → mise à jour mémoire),
- avec un résumé permanent dans `CLAUDE.md` (toujours en contexte).

```
COPILOT (avant)                      CLAUDE CODE (maintenant)
  @dev                                 Thread principal = toi + Claude (protocole /dev)
   ├─ @dotnet-dev ──┐                    ├─ lance sous-agent dotnet-dev  (isolé, bout de chaîne)
   │   └─ @angular  │ imbriqué           ├─ lance sous-agent angular-front (isolé)
   └─ @pr-manager ──┘                    └─ lance sous-agent pr-manager  (isolé)
                                       (les sous-agents NE relancent PERSONNE)
```

C'est en fait plus simple et plus robuste : la coordination est centralisée dans une seule tête.

---

## 2. Démarrer Claude Code

Dans un terminal, à la racine du repo :

```powershell
claude
```

Au lancement, Claude lit automatiquement **`CLAUDE.md`**. Quelques commandes système utiles (tape-les dans le prompt) :

- `/help` — aide intégrée
- `/agents` — voir / gérer les sous-agents disponibles
- `/mcp` — voir l'état des serveurs MCP (codegraph, sonarqube)
- `/clear` — vider le contexte (nouvelle tâche)
- `/model` — changer de modèle
- `! <commande>` — exécuter une commande shell directement (ex : `! gcloud auth login`)

---

## 3. Les sous-agents (`.claude/agents/`)

### Qu'est-ce que c'est
Un **fichier markdown** avec un frontmatter (`name`, `description`) et un corps qui est le *system prompt* du sous-agent. Quand il est lancé, il tourne dans un **contexte séparé** (il ne voit pas tout l'historique de ta conversation), fait sa tâche, et renvoie un résultat au thread principal.

### Comment on les déclenche
Trois façons :

1. **Automatique** — Claude lit la `description` de chaque agent et lance le bon tout seul quand ta demande correspond. C'est pour ça que les descriptions contiennent « Use when: … ».
2. **Explicite** — tu nommes l'agent : *« Utilise le sous-agent `dotnet-dev` pour ajouter un handler X »*. Le plus fiable quand tu sais ce que tu veux.
3. **Via `/dev`** — le protocole orchestrateur choisit et lance les bons sous-agents pour toi.

### La liste (13 sous-agents migrés)

| Agent | Rôle |
|-------|------|
| `architect` | Analyse, faisabilité, **plan** d'implémentation, challenge de feature. Ne code pas. |
| `dotnet-dev` | Tout le backend C#/.NET (Domain, Application, Infra, Contracts, API, Shared). |
| `angular-front` | Tout le frontend Angular (`src/Front`). |
| `aspire-debug` | Debug runtime / AppHost Aspire (ressources, logs, traces, recovery). |
| `review-expert` | Review pré-merge stricte (gate qualité avant `main`). |
| `vibe-coding-refractaire` | Seconde passe anti-vibe-coding (smells, abstractions bidon, tests théâtre). |
| `review-remediator` | Applique le backlog de correction issu d'une review. |
| `audit-expert` | Audit technique complet + sync issues GitHub. |
| `documentation-professor` | Documentation technique, onboarding, pédagogie. |
| `pr-manager` | Création/soumission de PR (conventions de titre/description). |
| `merge-main` | Fusionne `main` sur la branche courante. |
| `upgrade-orchestrator` | Montées de version .NET / Angular / NuGet / npm. |
| `dream` | Consolidation mémoire (les 4 phases Orient→Gather→Consolidate→Prune). |

> `Explore` (exploration read-only rapide du code) est un sous-agent **built-in** de Claude Code — pas besoin de fichier, il existe déjà.

### Créer ou modifier un agent
Édite/crée un fichier dans `.claude/agents/`. Frontmatter minimal :
```markdown
---
name: mon-agent
description: "Use when: <mots-clés qui déclenchent l'agent>."
---
Tu es … (le system prompt : rôle, protocole, règles).
```
Champs optionnels : `tools` (liste blanche d'outils ; **omis = hérite de tous**), `model` (`opus`/`sonnet`/`haiku` ; omis = hérite).

---

## 4. Les skills (`.claude/skills/`)

### Qu'est-ce que c'est
De la **connaissance projet pure** (un `SKILL.md` par skill), **chargée à la demande** par le modèle quand la tâche correspond à la `description`. Pas d'outils, réutilisable par n'importe quel agent ou par le thread principal. C'est exactement le même concept que tes skills Copilot.

### Comment on les déclenche
**Automatiquement** : le modèle voit le nom + la description de tous les skills, et charge le bon via le tool *Skill* quand c'est pertinent. Tu peux aussi forcer : *« charge le skill `cqrs-feature` »*.

### Les 15 skills migrés
`cqrs-feature` · `new-azure-resource` · `dotnet-patterns` · `xunit-unit-testing` · `tdd-workflow` · `angular-patterns` · `ui-ux-front-saas` · `bicep-v2-migration` · `dotnet-upgrade` · `angular-upgrade` · `codegraph-workflow` · `graphify-corpus` · `audit-workflow` · `draw-io-diagram-generator` · `mcp-dotnet-server`.

> Les skills `draw-io-diagram-generator` etc. peuvent embarquer des sous-dossiers (`scripts/`, `assets/`, `references/`) — ils ont été copiés tels quels.

### Créer/modifier un skill
Crée `.claude/skills/<nom>/SKILL.md` :
```markdown
---
name: mon-skill
description: "Use when: <quand le charger>."
---
# Contenu : conventions, patterns, exemples de code testés.
```
Le frontmatter `name` + `description` est **obligatoire** dans Claude Code (c'est pour ça que j'ai dû en ajouter à 5 de tes skills qui n'en avaient pas).

---

## 5. Les commandes (`.claude/commands/`)

### Qu'est-ce que c'est
Des **prompts réutilisables** que **tu** déclenches en tapant `/nom`. Équivalent de tes `.prompt.md` Copilot.

| Commande | Effet |
|----------|-------|
| `/dev <tâche>` | **Orchestrateur — optionnel.** Protocole « artillerie lourde » (research Codegraph, contradiction Bicep, scratchpad multi-agents, tracker multi-PC). Pas nécessaire pour une tâche simple : Claude orchestre déjà par défaut. |
| `/dream` | **Consolidation mémoire manuelle** (synthétise/déduplique/prune `.claude/memory/`). À lancer quand tu veux. |
| `/new-cqrs-feature <Nom> <ops>` | Génère une feature CQRS complète. |
| `/add-azure-resource <Nom> <typeARM> <abbr>` | Ajoute une ressource Azure end-to-end. |
| `/review-main [branche] [focus]` | Review pré-merge (review-expert + vibe-coding-refractaire). |
| `/merge-main` | Merge `main` sur la branche courante. |
| `/run-audit` | Audit technique + sync GitHub. |

Les arguments que tu tapes après la commande remplacent `$ARGUMENTS` (ou `$1`, `$2`…) dans le fichier.

### Créer une commande
Crée `.claude/commands/<nom>.md`. Le nom du fichier = le nom de la commande. Frontmatter optionnel (`description`, `argument-hint`). Le corps est le prompt envoyé à Claude.

---

## 6. La mémoire (`.claude/memory/`)

### Le point le plus important à comprendre
Claude Code a un système de mémoire **natif** (des fichiers gérés automatiquement, stockés hors du repo dans ton profil utilisateur). **Ce n'est PAS celui-ci.**

Ta mémoire projet (`.claude/memory/`) est un **ensemble de fichiers dans le repo** que Claude lit **parce que `CLAUDE.md` lui dit de le faire** (« Premier réflexe — Mémoire d'abord »). C'est la copie fidèle de ton système Copilot découpé en thèmes pour économiser les tokens — exactement ta logique d'origine.

### Structure
- `.claude/memory/MEMORY.md` — **l'index léger** (< 80 lignes). Lu en premier. Pointe vers les fichiers thématiques.
- `.claude/memory/01-*.md` … `14-*.md` — le détail par domaine (chargés à la demande).
- `.claude/memory/changelog.md` — historique des changements récents.
- `.claude/memory/dream-state.md` — état des gates du dream.
- `.claude/memory/session/` — scratchpad volatil des tâches multi-agents (gitignoré, supprimé en fin de tâche).

### Le cycle de vie
1. **Début de tâche** : le thread principal lit `MEMORY.md` + les thèmes pertinents.
2. **Fin de tâche** : il met à jour le bon fichier thématique + une ligne dans `changelog.md`.
3. **Consolidation (`dream`) — manuelle** : quand tu le décides, tu lances `/dream`. Le sous-agent `dream` synthétise, déduplique et prune la mémoire (4 phases). **Plus de gate automatique, plus de compteur de sessions, plus de verrou** — c'était de la cérémonie héritée de Copilot, retirée car Claude Code n'en a pas besoin.

### ⚠️ Séparation = divergence à terme
Comme tu as choisi la **duplication** (séparation totale), la mémoire Claude (`.claude/memory/`) et la mémoire Copilot (`.github/memory/`) **vont diverger** au fil de tes sessions : chacune n'est mise à jour que par l'outil qui tourne. C'est le prix de la séparation étanche que tu voulais. Si un jour tu veux resynchroniser, copie manuellement les `changelog.md` / fichiers thématiques de l'un vers l'autre (ou demande-moi de le faire).

---

## 7. Les serveurs MCP

Les MCP (Model Context Protocol) sont des outils externes branchés sur Claude. Ils sont **déjà configurés et partagés** entre Copilot et Claude (ils vivent dans `.mcp.json` à la racine, neutre vis-à-vis des deux outils) :

- **`codegraph`** — intelligence de code (graphe) : `codegraph_query`, `codegraph_context`, `codegraph_impact`, `codegraph_detect_changes`. Utilisé par le protocole `/dev` (phase Research + analyse d'impact).
- **`sonarqube`** — qualité de code.

Activés via `.claude/settings.json` (`enabledMcpjsonServers`, commité pour ton workflow multi-PC). Vérifie leur état avec `/mcp`. Si Codegraph signale un index obsolète : `npx codegraph analyze`.

---

## 8. Workflows types (exemples concrets)

### A. Créer une feature CQRS complète
```
/new-cqrs-feature StorageAccount Create,Read,Update,Delete,List
```
→ charge `cqrs-feature` + `tdd-workflow`, lance `dotnet-dev`, génère Domain→…→API+migration, teste, met à jour la mémoire.

### B. Tâche complexe « à la dev »
```
/dev Ajoute le support des tags personnalisés sur les ressources, backend + frontend, avec migration
```
→ lit la mémoire, fait la passe de contradiction, explore via Codegraph, planifie (lance `architect` si besoin), exécute via `dotnet-dev` puis `angular-front`, vérifie, met à jour la mémoire.

### C. Avant un merge
```
/review-main
```
→ review-expert puis vibe-coding-refractaire, backlog priorisé. Puis, pour appliquer : *« utilise le sous-agent review-remediator sur le backlog »*.

### D. Tâche ciblée sans orchestration
```
Utilise le sous-agent dotnet-dev pour corriger la traduction LINQ du KeyVaultId dans GetKeyVaultQueryHandler
```
→ pas besoin de `/dev` pour un truc précis ; tu nommes directement l'expert.

---

## 9. Faire évoluer ta flotte

- **Ajouter un agent** → fichier dans `.claude/agents/`. Mets à jour la table §6 de `CLAUDE.md` et la table de routage de `.claude/commands/dev.md`.
- **Ajouter un skill** → dossier dans `.claude/skills/<nom>/SKILL.md`. Mets à jour la table §7 de `CLAUDE.md`.
- **Ajouter une commande** → fichier dans `.claude/commands/`.
- **Mémoire** → laisse le sous-agent `dream` la maintenir, ou édite directement les fichiers thématiques.
- Demande-moi *« mets à jour ma flotte Claude »* et je m'occupe de la cohérence entre CLAUDE.md, les agents, et les commandes.

---

## 10. Revenir à GitHub Copilot

Rien à défaire. Copilot lit `.github/` ; Claude lit `.claude/` + `CLAUDE.md`. Ils s'ignorent mutuellement. Tu peux utiliser les deux le même jour sur le même repo. Le seul fichier neutre partagé est `.mcp.json` (MCP), commun aux deux.

`CLAUDE.md` est à la racine mais Copilot ne le lit pas (Copilot lit `.github/copilot-instructions.md`). Aucune interférence.

---

## 11. Tableau de correspondance complet

| Concept | Copilot | Claude Code |
|---------|---------|-------------|
| Instructions globales | `.github/copilot-instructions.md` | `CLAUDE.md` (racine) |
| Orchestrateur | agent `@dev` | thread principal + commande `/dev` |
| Agent spécialisé | `.github/agents/X.agent.md` | `.claude/agents/X.md` |
| Invocation d'agent | `@X` | auto (description) ou « utilise le sous-agent X » |
| Délégation agent→agent | autorisée | **interdite** (thread principal seulement) |
| Skill | `.github/skills/X/SKILL.md` | `.claude/skills/X/SKILL.md` |
| Chargement de skill | `read_file` | tool *Skill* (auto) |
| Prompt réutilisable | `.github/prompts/X.prompt.md` | `.claude/commands/X.md` → `/X` |
| Variables de prompt | `{{ var }}` | `$ARGUMENTS`, `$1`, `$2` |
| Mémoire projet | `.github/memory/` | `.claude/memory/` |
| Dette de tests | `.github/test-debt.md` | `.claude/test-debt.md` |
| Outil lecture fichier | `read_file` | `Read` |
| Outil commit/push | `report_progress` | `Bash` (git) / sous-agent `pr-manager` |
| Config MCP | `.vscode/mcp.json` | `.mcp.json` + `.claude/settings.json` |

---

## 12. Arborescence finale créée

```
CLAUDE.md                      ← instructions auto-chargées (racine)
.mcp.json                      ← MCP partagé (déjà présent)
.claude/
├── GUIDE.md                   ← ce fichier
├── settings.json             ← MCP activés (commité, multi-PC)
├── settings.local.json       ← réglages locaux (gitignoré)
├── test-debt.md              ← dette de tests (snapshot de départ)
├── agents/                   ← 13 sous-agents
├── commands/                 ← 6 slash-commands (dont /dev)
├── skills/                   ← 15 skills
├── memory/                   ← mémoire projet (index + 14 thèmes + changelog + dream-state)
│   └── session/              ← scratchpad volatil (gitignoré, créé à la demande)
└── instructions/             ← guardrails (référence ; repliés dans CLAUDE.md)
```

Bon dev avec Claude Code. 🚀
