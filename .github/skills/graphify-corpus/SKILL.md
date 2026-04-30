---
name: graphify-corpus
description: "Use when: corpus-level questions, documentation graph, architecture overview from docs+code, onboarding orientation, audit context, cross-file conceptual links, god nodes, community detection, surprising connections, diagram-to-code traceability, or any question that spans documentation and code rather than pure code structure."
---

# Skill : graphify-corpus — Graphe de connaissance corpus pour InfraFlowSculptor

> Charger ce skill pour toute tâche nécessitant une vue transversale entre code et documentation,
> une orientation architecturale rapide, une analyse de communautés, ou une exploration de liens
> conceptuels qui dépassent le graphe de code pur.

---

## Règle cardinale : GitNexus pour le code, Graphify pour le corpus

Ce dépôt utilise **deux graphes de connaissance complémentaires** :

| Dimension | GitNexus | Graphify |
|-----------|----------|----------|
| **Périmètre** | Code source uniquement (symboles, appels, héritages, flows) | Corpus complet (code AST + docs Markdown + audits + diagrammes + images) |
| **Force principale** | Impact analysis, blast radius, rename-safe, execution flows | Communautés conceptuelles, god nodes, connexions surprenantes, compression de contexte |
| **Transport MCP** | `gitnexus mcp` (stdio) | `python -m graphify.serve graphify-out/graph.json` (stdio) |
| **Mutations** | `rename()`, `detect_changes()` | Aucune — lecture seule |
| **Précision code** | Symbolique (callers/callees exacts, Cypher) | AST + inférence sémantique (moins précis sur les appels, plus riche sur les concepts) |
| **Docs / images / audits** | Non couvert | Couvert nativement |

**Aucun des deux ne remplace l'autre.** Un agent qui a besoin de comprendre "qui appelle quoi et que casse un changement" utilise GitNexus. Un agent qui a besoin de comprendre "comment la documentation, les audits, les diagrammes et le code se relient" utilise Graphify.

---

## Pré-requis

1. Graphify installé : `pip install graphifyy` ou `uv tool install graphifyy`
2. Graphe initial construit :
   - soit via une intégration assistant compatible Graphify (`/graphify .` si tu as installé le skill officiel Graphify pour cet assistant)
   - soit, sur cette machine/ce dépôt avec PyPI `graphifyy 0.4.23`, via un bootstrap code-only validé :
     `python -c "from pathlib import Path; from graphify.watch import _rebuild_code; import sys; ok = _rebuild_code(Path('.')); sys.exit(0 if ok else 1)"`
3. Le fichier `graphify-out/graph.json` existe et est non vide
4. Le `.graphifyignore` à la racine exclut les sorties build, les dépendances, et les fichiers d'instructions agents
5. Le serveur MCP Graphify est déclaré dans `.vscode/mcp.json` sous l'entrée `graphify`
6. Pour `python -m graphify.serve`, le package `mcp` doit être installé : `python -m pip install --user mcp`

### Notes runtime vérifiées sur ce dépôt [2026-04-29]

- Le lanceur `graphify.exe` est installé dans `%APPDATA%\Python\Python314\Scripts`, mais ce dossier n'est pas dans le `PATH` utilisateur par défaut sur cette machine.
- En pratique, utiliser **`python -m graphify ...`** dans le terminal est plus fiable que `graphify ...`.
- La version PyPI disponible ici est `graphifyy 0.4.23`. Elle supporte `python -m graphify query|update|path|explain|serve`, mais **pas** le `graphify .` direct en terminal tel que documenté dans les versions plus récentes du README GitHub.
- Sur un gros repo comme celui-ci, la visualisation `graph.html` peut échouer à cause de la taille du graphe. `graph.json` et `GRAPH_REPORT.md` restent suffisants pour MCP et pour les agents.

---

## Intégration VS Code contrôlée

Pour **ce dépôt**, ne pas lancer `graphify vscode install` de manière automatique.

Pourquoi :

- `graphify vscode install` ajoute une section `## graphify` à `.github/copilot-instructions.md`
- ce dépôt possède déjà une orchestration repo-specific plus riche (`dev`, mémoire projet, GitNexus, skill `graphify-corpus`)
- ajouter la section Graphify officielle en mode aveugle crée une deuxième couche always-on moins précise que les instructions du dépôt

Mode contrôlé recommandé :

1. Installer **uniquement** le skill utilisateur Copilot avec `python -m graphify copilot install`
2. Garder `.github/copilot-instructions.md` du dépôt comme source de vérité
3. Utiliser `/graphify` explicitement quand une tâche justifie la couche corpus/semantics
4. Si un jour tu veux la section officielle Graphify dans le repo, la fusionner manuellement au lieu d'exécuter `graphify vscode install`

Conséquence pratique :

- le slash command `/graphify` est disponible côté Copilot utilisateur
- le dépôt conserve ses règles de priorité : mémoire -> GitNexus -> Graphify -> Explore

---

## Quand utiliser Graphify vs GitNexus

### Utiliser GitNexus quand :

- Tu dois savoir **qui appelle** un handler, un service, ou une interface
- Tu dois évaluer le **blast radius** d'un changement avant de modifier du code
- Tu dois tracer un **flux d'exécution** complet (ex: HTTP request → handler → repository → DB)
- Tu dois **renommer** un symbole en toute sécurité
- Tu dois **valider** que tes changements n'impactent que les fichiers/flux attendus (`detect_changes`)
- Tu dois écrire une **requête Cypher** précise sur les relations de code

### Utiliser Graphify quand :

- Tu dois comprendre **comment la documentation se relie au code** (quels docs parlent de quel module)
- Tu dois identifier les **god nodes** du corpus (concepts qui relient le plus de communautés)
- Tu dois trouver des **connexions surprenantes** entre fichiers qui n'ont pas de lien structurel direct
- Tu dois préparer un **onboarding** ou une **explication d'architecture** couvrant docs + code
- Tu dois contextualiser un **audit** technique par rapport à la documentation existante
- Tu dois naviguer dans des **diagrammes**, **images**, ou **PDFs** qui font partie du corpus
- Tu dois répondre à une question de type "**pourquoi** cette zone du code est conçue ainsi" (rationnel extrait des commentaires et docs)
- Tu dois obtenir une **compression de contexte** pour une question large ("donne-moi une vue d'ensemble de la génération Bicep en incluant la doc")

### Utiliser les deux quand :

- **Architecture review complète** : Graphify pour la vue d'ensemble corpus + GitNexus pour les détails structurels de code
- **Audit technique** : Graphify pour relier les audits précédents aux zones de code, GitNexus pour vérifier les impacts
- **Onboarding approfondi** : Graphify pour la carte mentale globale, GitNexus pour les flux d'exécution précis
- **Planification de refactoring** : Graphify pour identifier les communautés et god nodes concernés, GitNexus pour le blast radius exact

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
- compléter avec GitNexus sur les handlers et flows critiques repérés

### 2. Audit / review transverse

Quand : relier findings, docs, diagrammes, zones de code.

Corpus conseillé :

- `audits`
- `docs`
- la slice de code concernée (`src/Api`, `src/Mcp`, `src/Front`)

Utilisation :

- `/graphify audits`
- `python -m graphify query "what connects PAT auth to MCP tools?" --graph .\graphify-out\graph.json`
- compléter avec GitNexus pour confirmer impact et blast radius

### 3. MCP / IA tooling

Quand : comprendre les tools MCP, leur documentation, leurs prompts, et leur rattachement au produit.

Corpus conseillé :

- `docs/architecture/mcp-integration.md`
- `src/Mcp`
- `src/Api/InfraFlowSculptor.Application`

Utilisation :

- Graphify pour doc ↔ tool ↔ concept métier
- GitNexus pour le flux précis `Tool -> Handler -> Service -> Repository`

### 4. Génération Bicep / pipeline

Quand : questions de design, rationale, dette d'architecture, lecture transversale.

Corpus conseillé :

- `docs/architecture/bicep-generation.md`
- `docs/architecture/pipeline-generation.md`
- `src/Api/InfraFlowSculptor.BicepGeneration`
- `src/Api/InfraFlowSculptor.PipelineGeneration`

Utilisation :

- Graphify pour les communautés, god nodes, liens entre docs et moteurs
- GitNexus pour `BicepGenerationEngine`, `BicepAssembler`, `AppPipelineGenerationEngine`, `MonoRepoPipelineAssembler`

### 5. Frontend / UX / design system

Quand : relier les docs UX, conventions DS, écrans et composants.

Corpus conseillé :

- `src/Front/src`
- `.github/memory/14-frontend-design-system.md`
- `docs`

Utilisation :

- Graphify pour les patterns transverses, la cohérence de vocabulaire, les connexions entre écrans et composants
- GitNexus uniquement si une question structurelle code TS devient nécessaire

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
  3. Compléter avec GitNexus pour la structure de code exacte

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
  - Compléter avec GitNexus pour l'impact analysis avant de produire des recommandations

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

- **Ne pas** utiliser Graphify pour l'analyse d'impact avant modification de code → utiliser GitNexus `impact()`
- **Ne pas** utiliser Graphify pour le rename de symboles → utiliser GitNexus `rename()`
- **Ne pas** utiliser Graphify pour valider que tes changements sont propres → utiliser GitNexus `detect_changes()`
- **Ne pas** forcer tous les agents à lire `GRAPH_REPORT.md` systématiquement → seulement quand le skill s'applique
- **Ne pas** remplacer la mémoire projet par le rapport Graphify → la mémoire est normative et curée, le rapport est descriptif et auto-généré
