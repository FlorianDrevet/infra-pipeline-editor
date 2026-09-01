---
name: codegraph-workflow
description: "Use when: code exploration via knowledge graph, execution-flow discovery, impact/blast-radius analysis before modifying a shared symbol, post-change validation, or safe refactoring with Codegraph MCP."
---

# Skill : codegraph-workflow — Utilisation de Codegraph dans InfraFlowSculptor

> **Charger ce skill pour toute tâche nécessitant exploration de code,
> analyse d'impact, ou validation de changements via le knowledge graph Codegraph.**

---

## Pré-requis

Codegraph est un outil **built-in de Codex**. Il maintient un graphe SQLite de tous les symboles, arêtes et fichiers du workspace via un file watcher. L'index est quasiment toujours à jour (lag < 1 seconde). Pas de ré-indexation manuelle nécessaire.

Vérifier l'état si besoin : `codegraph_status()` — si le graphe signale une anomalie, attendre quelques secondes.

Le dossier `.codegraph/` est présent à la racine du repo. Les fichiers de base de données sont gitignorés (`.codegraph/.gitignore`) ; chaque dev reconstruit l'index automatiquement au premier usage de Codex sur le repo.

---

## Outil principal : `codegraph_explore`

**`codegraph_explore` est l'outil PRIMAIRE.** La plupart des questions se répondent en un seul appel.

Il accepte une question en langage naturel ou un ensemble de noms de symboles/fichiers et retourne le code source verbatim des symboles pertinents groupés par fichier. C'est l'équivalent d'un `Read` ciblé intelligent.

```
codegraph_explore("bicep generation flow")
codegraph_explore("GenerateBicepCommandHandler BicepAssembler")
codegraph_explore("member access validation IInfraConfigAccessService")
codegraph_explore("how does PAT authentication work")
```

N'utiliser `Read`/`Grep` que pour confirmer un détail que `codegraph_explore` n'a pas couvert.

---

## Quand utiliser Codegraph vs exploration classique

| Besoin | Outil Codegraph | Alternative classique |
|--------|-----------------|----------------------|
| "Comment fonctionne X ?" / architecture / où est X ? | `codegraph_explore("X")` | `@Explore` + lecture manuelle (lent) |
| "Qui appelle ce handler / cette interface ?" | `codegraph_callers("Symbol")` | `grep_search` (moins fiable) |
| "Quel est le flux complet d'une requête HTTP ?" | `codegraph_explore("flux concept")` | Lecture manuelle (lent, incomplet) |
| "Si je change X, qu'est-ce qui casse ?" | `codegraph_impact("X")` | Aucun — impossible sans graphe |
| "Où est défini le symbole X ?" | `codegraph_search("X")` | `grep_search` |
| "Code complet d'un symbole spécifique" | `codegraph_node("Symbol")` | `Read` (si chemin connu) |
| "Quels fichiers correspondent à ce pattern ?" | `codegraph_files("pattern")` | `Glob` |

---

## Codegraph vs Graphify — Séparation des responsabilités

Ce dépôt utilise **deux graphes de connaissance** complémentaires. Ne pas confondre leurs rôles :

| Question | Outil correct | Outil incorrect |
|----------|---------------|-----------------|
| "Qui appelle ce handler ?" | **Codegraph** `callers()` | ~~Graphify~~ (trop imprécis sur les appels) |
| "Quel est le blast radius si je modifie X ?" | **Codegraph** `impact()` | ~~Graphify~~ (pas d'impact analysis) |
| "Comment la doc architecture se relie à la génération Bicep ?" | **Graphify** `query` / `path` | ~~Codegraph~~ (ne voit pas les docs) |
| "Quels sont les god nodes du projet ?" | **Graphify** `GRAPH_REPORT.md` | ~~Codegraph~~ (pas de community detection cross-corpus) |
| "Quels audits parlent de cette zone du code ?" | **Graphify** `query` / `path` | ~~Codegraph~~ (ne voit pas les audits) |
| "Vue d'ensemble architecture pour onboarding ?" | **Graphify** `GRAPH_REPORT.md` + communautés | ~~Codegraph~~ (trop granulaire) |

**Règle absolue :** Codegraph pour le code, Graphify pour le corpus (docs+diagrammes+audits). Jamais l'inverse.

Pour charger le skill Graphify : `.Codex/skills/graphify-corpus/SKILL.md`

---

## Commandes par phase de travail

### Phase 1 — Exploration / Compréhension

```
1. codegraph_explore("concept ou symboles liés")
   → Retourne le source verbatim des symboles pertinents groupés par fichier
   → C'est le ONE CALL à faire en premier — couvre la grande majorité des besoins

2. codegraph_search("NomExact")
   → Trouve la localisation exacte d'un symbole nommé
   → À utiliser quand on cherche OÙ est défini un symbole précis

3. codegraph_node("NomExact")
   → Code complet d'un symbole spécifique
   → À utiliser quand explore() a tronqué un corps volumineux ou nom surchargé
```

### Phase 2 — Analyse d'impact (AVANT toute modification)

**Obligatoire avant de modifier un symbole partagé** (interface, service, base class, handler utilisé par plusieurs endpoints) :

```
1. codegraph_impact("NomDuSymbole")
   → Blast radius : retourne les symboles impactés par une modification
   → Interpréter :
     - Dépendants directs → MUST update dans la même tâche
     - Dépendants indirects → SHOULD tester ces chemins
     - Risque élevé → ALERTER l'utilisateur avant de modifier

2. codegraph_callers("NomDuSymbole")
   → Liste exhaustive de ce qui appelle ce symbole directement
   → Complémentaire de impact() pour voir les appelants directs
```

**Exemples pour ce projet :**
- Avant de modifier `AzureResource` → `codegraph_impact("AzureResource")` — 22 agrégats enfants
- Avant de modifier `IInfraConfigAccessService` → `codegraph_impact("IInfraConfigAccessService")` — tous les handlers Resource
- Avant de modifier `BicepGenerationEngine` → `codegraph_impact("BicepGenerationEngine")` — handlers de génération
- Avant de modifier `BlobDownloadHelper` → `codegraph_impact("BlobDownloadHelper")` — 33 dépendants directs, risque CRITICAL

### Phase 3 — Exploration des dépendances sortantes

```
codegraph_callees("NomDuSymbole")
  → Ce que ce symbole appelle (dépendances sortantes)
  → Utile pour tracer la chaîne d'appel descendante

codegraph_files("pattern")
  → Liste les fichiers correspondant à un pattern
  → Utile pour trouver tous les fichiers d'une feature ou d'un type
```

---

## Conventions de nommage pour les recherches

| Type | Pattern | Exemple |
|------|---------|---------|
| Aggregate root | `{ResourceName}` | `KeyVault`, `StorageAccount` |
| Command handler | `{Action}{Resource}CommandHandler` | `CreateKeyVaultCommandHandler` |
| Query handler | `{Action}{Resource}QueryHandler` | `GetInfrastructureConfigQueryHandler` |
| Repository interface | `I{Resource}Repository` | `IKeyVaultRepository` |
| Repository impl | `{Resource}Repository` | `KeyVaultRepository` |
| EF Config | `{Resource}Configuration` | `KeyVaultConfiguration` |
| Validator | `{Action}{Resource}CommandValidator` | `CreateKeyVaultCommandValidator` |
| Bicep generator | `{Resource}TypeBicepGenerator` | `KeyVaultTypeBicepGenerator` |

---

## Intégration avec la mémoire projet

- Les résultats d'exploration Codegraph (symboles à haut risque, flows critiques) sont persistés dans `.Codex/memory/13-code-graph.md` par `@dream`
- Les agents lisent d'abord la mémoire (connaissance pré-cachée) puis Codegraph (vérification dynamique)
- Si un résultat Codegraph contredit la mémoire → la mémoire est obsolète et doit être mise à jour

Symboles à haut risque pré-cachés : voir `.Codex/memory/13-code-graph.md`
