---
name: graphify-corpus
description: "Use when: corpus-level questions, documentation graph, architecture overview from docs+code, onboarding orientation, audit context, cross-file conceptual links, god nodes, community detection, surprising connections, diagram-to-code traceability, or any question that spans documentation and code rather than pure code structure."
---

# Skill : graphify-corpus — Graphe de connaissance corpus pour InfraFlowSculptor

> Charger ce skill pour toute tâche nécessitant une vue transversale entre code et documentation,
> une orientation architecturale rapide, une analyse de communautés, ou une exploration de liens
> conceptuels qui dépassent le graphe de code pur.

---

## Graphe unique

Graphify couvre le code AST, la documentation Markdown, les audits, les diagrammes et les relations entre fichiers. Il sert à orienter l'exploration, comprendre le contexte architectural et relier les concepts du corpus.

Pour les dépendances exactes d'un symbole, compléter la requête Graphify par une lecture ciblée, `git diff`, un build et les tests concernés. La centralité d'un nœud ne remplace pas une validation exécutable.

---

## Pré-requis

1. Graphify installé : `python -m pip install graphifyy` ou `uv tool install graphifyy`
2. Graphe initial construit avec `python -m graphify update .`
3. Le fichier `graphify-out/graph.json` existe et est non vide
4. Le `.graphifyignore` à la racine exclut les sorties build, les dépendances, et les fichiers d'instructions agents
5. Le serveur MCP Graphify est déclaré dans `.vscode/mcp.json` sous l'entrée `graphify`
6. Pour `python -m graphify.serve`, le package `mcp` doit être installé : `python -m pip install --user mcp`

### Notes runtime vérifiées sur ce dépôt [2026-09-01]

- Le lanceur `graphify.exe` est installé dans `%APPDATA%\Python\Python314\Scripts`, mais ce dossier n'est pas dans le `PATH` utilisateur par défaut sur cette machine.
- En pratique, utiliser **`python -m graphify ...`** dans le terminal est plus fiable que `graphify ...`.
- La version PyPI disponible ici est `graphifyy 0.7.16`. Elle supporte `python -m graphify update|query|path|explain|serve` et génère `graphify-out/graph.json` ainsi que `GRAPH_REPORT.md`.
- Sur un gros repo comme celui-ci, la visualisation `graph.html` peut échouer à cause de la taille du graphe. `graph.json` et `GRAPH_REPORT.md` restent suffisants pour MCP et pour les agents.

---

## Intégration VS Code contrôlée

Pour **ce dépôt**, ne pas lancer `graphify vscode install` de manière automatique.

Pourquoi :

- `graphify vscode install` ajoute une section `## graphify` à `.github/copilot-instructions.md`
- ce dépôt possède déjà une orchestration repo-specific plus riche (`dev`, mémoire projet, skill `graphify-corpus`)
- ajouter la section Graphify officielle en mode aveugle crée une deuxième couche always-on moins précise que les instructions du dépôt

Mode contrôlé recommandé :

1. Installer **uniquement** le skill utilisateur Copilot avec `python -m graphify copilot install`
2. Garder `.github/copilot-instructions.md` du dépôt comme source de vérité
3. Utiliser `/graphify` explicitement quand une tâche justifie la couche corpus/semantics
4. Si un jour tu veux la section officielle Graphify dans le repo, la fusionner manuellement au lieu d'exécuter `graphify vscode install`

Conséquence pratique :

- le slash command `/graphify` est disponible côté Copilot utilisateur
- le dépôt conserve ses règles de priorité : mémoire -> Graphify -> Explore

---

## Utiliser Graphify

- Tu dois comprendre **comment la documentation se relie au code** (quels docs parlent de quel module)
- Tu dois identifier les **god nodes** du corpus (concepts qui relient le plus de communautés)
- Tu dois trouver des **connexions surprenantes** entre fichiers qui n'ont pas de lien structurel direct
- Tu dois préparer un **onboarding** ou une **explication d'architecture** couvrant docs + code
- Tu dois contextualiser un **audit** technique par rapport à la documentation existante
- Tu dois naviguer dans des **diagrammes**, **images**, ou **PDFs** qui font partie du corpus
- Tu dois répondre à une question de type "**pourquoi** cette zone du code est conçue ainsi" (rationnel extrait des commentaires et docs)
- Tu dois obtenir une **compression de contexte** pour une question large ("donne-moi une vue d'ensemble de la génération Bicep en incluant la doc")

Pour une question de dépendance ou d'impact, utiliser `query`, `path` et `explain`, puis confirmer les relations par lecture ciblée et validation exécutable.

---

## Workflows ciblés pour ce dépôt

Le plein potentiel de Graphify sur InfraFlowSculptor ne consiste pas à lancer un graphe sémantique géant sur tout le repo à chaque fois. Il consiste à cibler le bon corpus selon la question.

### 1. Architecture / onboarding

Quand : nouveau contributeur, vue d'ensemble, lecture transversale code+docs.

Corpus conseillé :

- `docs/architecture`
- `README.md`
- `src/Api`
- `src/Mcp`

Utilisation :

- `/graphify docs/architecture`
- lire `graphify-out/GRAPH_REPORT.md`
- compléter par lecture ciblée des handlers et validation des flows critiques

### 2. Audit / review transverse

Quand : relier findings, docs, diagrammes, zones de code.

Corpus conseillé :

- `audits`
- `docs`
- la slice de code concernée (`src/Api`, `src/Mcp`, `src/Front`)

Utilisation :

- `/graphify audits`
- `python -m graphify query "what connects PAT auth to MCP tools?" --graph .\graphify-out\graph.json`
- compléter par lecture ciblée, `git diff` et les tests concernés

### 3. MCP / IA tooling

Quand : comprendre les tools MCP, leur documentation, leurs prompts, et leur rattachement au produit.

Corpus conseillé :

- `docs/architecture/mcp-integration.md`
- `src/Mcp`
- `src/Api/InfraFlowSculptor.Application`

Utilisation :

- Graphify pour relier doc, outil et concept métier, puis pour tracer le flux `Tool -> Handler -> Service -> Repository`

### 4. Génération Bicep / pipeline

Quand : questions de design, rationale, dette d'architecture, lecture transversale.

Corpus conseillé :

- `docs/architecture/bicep-generation.md`
- `docs/architecture/pipeline-generation.md`
- `src/Api/InfraFlowSculptor.BicepGeneration`
- `src/Api/InfraFlowSculptor.PipelineGeneration`

Utilisation :

- Graphify pour les communautés, god nodes, liens entre docs et moteurs, et relations autour de `BicepGenerationEngine`, `BicepAssembler`, `AppPipelineGenerationEngine` et `MonoRepoPipelineAssembler`

### 5. Frontend / UX / design system

Quand : relier les docs UX, conventions DS, écrans et composants.

Corpus conseillé :

- `src/Front/src`
- `.claude/memory/14-frontend-design-system.md`
- `docs`

Utilisation :

- Graphify pour les patterns transverses, la cohérence de vocabulaire et les connexions entre écrans et composants

---

## Commandes Graphify

### Depuis un assistant (VS Code Copilot Chat, Claude Code, etc.)

```
/graphify .                          # construire ou reconstruire le graphe complet, si le skill officiel Graphify est installé pour cet assistant
/graphify . --update                 # mise à jour incrémentale sémantique (docs/images), si l'assistant l'expose
/graphify query "bicep generation"  # chercher un concept dans le graphe
/graphify path "BicepAssembler" "docs/architecture/bicep-generation.md"  # chemin entre deux nœuds
/graphify explain "BicepGenerationEngine"  # explication en langage naturel d'un nœud
```

### Depuis le terminal PowerShell

```powershell
# Bootstrap code-only validé sur ce dépôt
python -c "from pathlib import Path; from graphify.watch import _rebuild_code; import sys; ok = _rebuild_code(Path('.')); sys.exit(0 if ok else 1)"

# Commandes CLI fiables avec la version installée
python -m graphify update .
python -m graphify query "architecture overview" --graph .\graphify-out\graph.json
python -m graphify path "AzureResource" "docs/architecture/ddd-concepts.md" --graph .\graphify-out\graph.json
python -m graphify explain "MonoRepoPipelineAssembler" --graph .\graphify-out\graph.json

# MCP Graphify
python -m graphify.serve .\graphify-out\graph.json
```

### Via le serveur MCP (quand actif)

Les tools MCP exposés par Graphify :

| Tool MCP | Usage |
|----------|-------|
| `query_graph` | Rechercher un concept, retourne sous-graphe pertinent |
| `get_node` | Détail d'un nœud spécifique (label, type, source, communauté) |
| `get_neighbors` | Voisins directs d'un nœud (entrants + sortants) |
| `shortest_path` | Chemin le plus court entre deux nœuds |

### Sorties clés

| Fichier | Contenu | Usage |
|---------|---------|-------|
| `graphify-out/GRAPH_REPORT.md` | God nodes, connexions surprenantes, communautés, questions suggérées | Lire pour orientation rapide |
| `graphify-out/graph.json` | Graphe complet sérialisé (NetworkX JSON) | Base pour queries MCP et CLI |
| `graphify-out/graph.html` | Visualisation interactive | Optionnelle. Peut ne pas être générée si le graphe est trop gros. |

---

## Intégration avec les agents du dépôt

### `@dev` (orchestrateur)

- Lors de la **phase Research (step 2bis)**, si la tâche touche à la documentation, l'architecture, l'onboarding, ou un audit :
  1. Lire `graphify-out/GRAPH_REPORT.md` pour identifier les god nodes et communautés pertinentes
  2. Utiliser `graphify query` ou le MCP Graphify pour des questions ciblées
  3. Compléter par lecture ciblée et validation exécutable pour confirmer la structure exacte

### `@architect`

- Avant de produire un plan d'implémentation, consulter `GRAPH_REPORT.md` pour :
  - Identifier les communautés fonctionnelles impactées
  - Vérifier que le plan ne crée pas de couplage surprenant entre communautés isolées
  - Utiliser les god nodes pour identifier les points d'ancrage naturels de la feature

### `@documentation-professor`

- Utiliser Graphify comme source primaire pour :
  - Construire l'ordre de lecture recommandé à partir des communautés et des liens code-docs
  - Identifier les zones sous-documentées (nœuds code sans lien vers des docs)
  - Relier les explications aux fichiers source via les chemins du graphe

### `@audit-expert`

- Utiliser Graphify pour :
  - Relier les findings d'audits précédents (`audits/`) aux zones de code concernées
  - Identifier les communautés à risque via les god nodes (haute centralité = haut risque)
  - Compléter par lecture ciblée et validation exécutable avant de produire des recommandations

### `@review-expert` et `@vibe-coding-refractaire`

- Consultation optionnelle de `GRAPH_REPORT.md` pour :
  - Vérifier que le diff ne crée pas de dépendances surprenantes entre communautés
  - Utiliser les connexions surprenantes comme signal de review

---

## Maintenance du graphe

### Quand reconstruire

| Événement | Action |
|-----------|--------|
| Après modification de fichiers code | `python -m graphify update .` (AST only, instantané, pas de LLM) |
| Après modification de docs/Markdown | relancer un build sémantique via un assistant compatible Graphify, ou reconstruire le corpus si tu disposes d'une version plus récente/outillée de Graphify |
| Après ajout de nouveaux diagrammes/images | même règle que pour docs/Markdown |
| Build complet périodique | rebuild corpus via assistant Graphify ou bootstrap ciblé selon le besoin |

### Git commit policy

Ajouter à `.gitignore` :
```
graphify-out/cache/
graphify-out/manifest.json
graphify-out/cost.json
```

Committer `graphify-out/graph.json`, `graphify-out/GRAPH_REPORT.md`, et `graphify-out/graph.html` pour que tout le monde bénéficie du graphe sans le reconstruire.

---

## Anti-patterns

- **Ne pas** déduire un impact exact de la seule centralité du graphe → confirmer avec lecture ciblée, `git diff`, build et tests
- **Ne pas** considérer le rapport Graphify comme une validation de comportement → exécuter le contrôle le plus ciblé disponible
- **Ne pas** forcer tous les agents à lire `GRAPH_REPORT.md` systématiquement → seulement quand le skill s'applique
- **Ne pas** remplacer la mémoire projet par le rapport Graphify → la mémoire est normative et curée, le rapport est descriptif et auto-généré
