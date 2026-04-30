# Skill : gitnexus-workflow — Utilisation de GitNexus dans InfraFlowSculptor

> **Charger ce skill avec `read_file` pour toute tâche nécessitant exploration de code,
> analyse d'impact, ou validation de changements via le knowledge graph GitNexus.**

---

## Pré-requis

1. Le repo doit être indexé : `npx gitnexus analyze` (vérifié via `mcp_gitnexus_list_repos`)
2. Le nom du repo indexé est : **`infra-pipeline-editor`**
3. Si l'index est stale (> 24h ou après gros commit), re-indexer avant d'utiliser les outils

---

## Quand utiliser GitNexus vs exploration classique

| Besoin | Outil GitNexus | Alternative classique |
|--------|---------------|----------------------|
| "Qui appelle ce handler / cette interface ?" | `context(name)` | `grep_search` (moins fiable, matches textuels) |
| "Quel est le flux complet d'une requête HTTP ?" | `query("concept")` | `@Explore` + lecture manuelle (lent, incomplet) |
| "Si je change X, qu'est-ce qui casse ?" | `impact(target, "upstream")` | Aucun — impossible sans graphe |
| "Est-ce que mes changements touchent plus que prévu ?" | `detect_changes()` | `git diff` (ne montre pas les impacts transitifs) |
| "Je veux renommer un symbole proprement" | `rename(old, new, dry_run: true)` | Find-and-replace (risqué, ignore le graphe) |
| "Quels flux d'exécution existent dans le projet ?" | `gitnexus://repo/infra-pipeline-editor/processes` | Lecture manuelle de MEMORY.md (incomplet) |

**Règle :** Utiliser GitNexus en premier pour la compréhension structurelle, puis `@Explore` / `read_file` pour le contenu exact des fichiers identifiés.

---

## GitNexus vs Graphify — Séparation des responsabilités

Ce dépôt utilise **deux graphes de connaissance** complémentaires. Ne pas confondre leurs rôles :

| Question | Outil correct | Outil incorrect |
|----------|---------------|-----------------|
| "Qui appelle ce handler ?" | **GitNexus** `context()` | ~~Graphify~~ (trop imprécis sur les appels) |
| "Quel est le blast radius si je modifie X ?" | **GitNexus** `impact()` | ~~Graphify~~ (pas d'impact analysis) |
| "Comment la doc architecture se relie à la génération Bicep ?" | **Graphify** `query` / `path` | ~~GitNexus~~ (ne voit pas les docs) |
| "Quels sont les god nodes du projet (concepts les plus connectants) ?" | **Graphify** `GRAPH_REPORT.md` | ~~GitNexus~~ (pas de community detection cross-corpus) |
| "Je veux renommer un symbole proprement" | **GitNexus** `rename()` | ~~Graphify~~ (pas de mutation) |
| "Quels audits parlent de cette zone du code ?" | **Graphify** `query` / `path` | ~~GitNexus~~ (ne voit pas les audits) |
| "Quels fichiers et flux sont impactés par mes changements ?" | **GitNexus** `detect_changes()` | ~~Graphify~~ (pas de détection de changements) |
| "Vue d'ensemble architecture pour un nouvel arrivant ?" | **Graphify** `GRAPH_REPORT.md` + communautés | ~~GitNexus~~ (trop granulaire pour l'onboarding) |

**Règle absolue :** Ne jamais utiliser Graphify pour l'impact analysis, le rename, ou la détection de changements. Ne jamais utiliser GitNexus pour la traçabilité doc-to-code ou l'analyse de communautés cross-corpus.

Pour charger le skill Graphify : `.github/skills/graphify-corpus/SKILL.md`

---

## Commandes par phase de travail

### Phase 1 — Exploration / Compréhension

Quand un agent doit comprendre un concept, un flux, ou un symbole avant d'agir :

```
1. query("concept en langage naturel")
   → Retourne les flux d'exécution et symboles liés
   → Exemples : query("bicep generation"), query("member access validation")

2. context("NomDuSymbole")
   → Vue 360° : qui l'appelle (incoming), ce qu'il appelle (outgoing), dans quels process
   → Exemples : context("GenerateBicepCommandHandler"), context("IInfraConfigAccessService")

3. READ gitnexus://repo/infra-pipeline-editor/processes
   → Liste tous les flux d'exécution tracés (300 flows disponibles)

4. READ gitnexus://repo/infra-pipeline-editor/clusters
   → Toutes les aires fonctionnelles avec scores de cohésion
```

### Phase 2 — Analyse d'impact (AVANT toute modification)

**Obligatoire avant de modifier un symbole partagé** (interface, service, base class, handler utilisé par plusieurs endpoints) :

```
1. impact(target: "NomDuSymbole", direction: "upstream")
   → Blast radius : d=1 (WILL BREAK), d=2 (LIKELY AFFECTED), d=3 (MAY NEED TESTING)

2. Interpréter les résultats :
   - d=1 → MUST update ces fichiers dans la même tâche
   - d=2 → SHOULD tester ces chemins
   - d=3 → Test if critical path

3. Si risque HIGH ou CRITICAL → ALERTER l'utilisateur avant de modifier
```

**Exemples pour ce projet :**
- Avant de modifier `AzureResource` (base TPT) → `impact("AzureResource", "upstream")` — d=1 touche les 21 agrégats enfants
- Avant de modifier `IInfraConfigAccessService` → `impact("IInfraConfigAccessService", "upstream")` — d=1 touche tous les handlers Resource
- Avant de modifier `BicepGenerationEngine` → `impact("BicepGenerationEngine", "upstream")` — d=1 touche `GenerateBicepCommandHandler` et `GenerateProjectBicepCommandHandler`

### Phase 3 — Validation post-changement

**Après implémentation, avant commit :**

```
1. detect_changes()
   → Compare les changements git avec le graphe
   → Vérifie que seuls les fichiers/flux attendus sont impactés

2. Si des flux inattendus sont touchés :
   → Investiguer avec context() sur les symboles concernés
   → Corriger ou documenter l'écart
```

### Phase 4 — Refactoring sûr

Pour les renommages et restructurations :

```
1. rename(symbol_name: "OldName", new_name: "NewName", dry_run: true)
   → Preview : graph_edits (safe) + text_search_edits (review needed)

2. Reviewer le preview — vérifier que chaque edit est correct

3. rename(symbol_name: "OldName", new_name: "NewName", dry_run: false)
   → Application réelle
```

---

## Requêtes Cypher avancées

Pour des questions spécifiques que les outils standard ne couvrent pas :

```cypher
-- Trouver tous les handlers qui appellent un repository donné
MATCH (h)-[:CodeRelation {type: 'CALLS'}]->(r {name: "IKeyVaultRepository"})
RETURN h.name, h.filePath

-- Trouver les classes orphelines (pas d'appelant)
MATCH (c:Class)
WHERE NOT EXISTS { MATCH ()-[:CodeRelation]->(c) }
RETURN c.name, c.filePath

-- Trouver tous les agrégats enfants d'AzureResource
MATCH (c:Class)-[:CodeRelation {type: 'EXTENDS'}]->(p {name: "AzureResource"})
RETURN c.name, c.filePath
```

Le schéma complet est consultable via `READ gitnexus://repo/infra-pipeline-editor/schema`.

---

## Conventions de nommage pour les requêtes

Les symboles dans ce projet suivent ces patterns — les utiliser pour formuler des requêtes précises :

| Type | Pattern de nommage | Exemple |
|------|-------------------|---------|
| Aggregate root | `{ResourceName}` | `KeyVault`, `StorageAccount` |
| Command handler | `{Action}{Resource}CommandHandler` | `CreateKeyVaultCommandHandler` |
| Query handler | `{Action}{Resource}QueryHandler` | `GetInfrastructureConfigQueryHandler` |
| Repository interface | `I{Resource}Repository` | `IKeyVaultRepository` |
| Repository impl | `{Resource}Repository` | `KeyVaultRepository` |
| EF Config | `{Resource}Configuration` | `KeyVaultConfiguration` |
| Validator | `{Action}{Resource}CommandValidator` | `CreateKeyVaultCommandValidator` |
| API Controller | `{Resource}Controller` | `KeyVaultController` |
| Bicep generator | `{Resource}TypeBicepGenerator` | `KeyVaultTypeBicepGenerator` |

---

## Intégration avec la mémoire projet

- Les résultats d'exploration GitNexus (clusters importants, flows critiques, symboles à haut risque) sont persistés dans `.github/memory/13-code-graph.md` par `@dream`
- Les agents lisent d'abord la mémoire (connaissance pré-cachée) puis GitNexus (vérification dynamique)
- Si un résultat GitNexus contredit la mémoire → la mémoire est obsolète et doit être mise à jour

---

## Re-indexation

Lancer après chaque série de commits significative :

```powershell
npx gitnexus analyze
```

Si des embeddings existaient auparavant (vérifier `.gitnexus/meta.json`) :

```powershell
npx gitnexus analyze --embeddings
```
