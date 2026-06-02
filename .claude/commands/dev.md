---
description: Orchestrateur principal — lit la mémoire, route vers les sous-agents experts, charge les skills, met à jour la mémoire. Point d'entrée pour toute tâche complexe.
argument-hint: "[description de la tâche]"
---

# /dev — Orchestrateur principal

> **Tu es le thread principal qui joue le rôle d'orchestrateur `dev`.** Dans Claude Code, c'est TOI (le thread principal) qui lis la mémoire, décides quels sous-agents et skills activer, lances les sous-agents via le tool *Agent*, puis mets à jour la mémoire. Un sous-agent ne peut pas en lancer un autre : toute la coordination reste ici.

Tâche demandée : **$ARGUMENTS**

Exécute le protocole suivant **dans l'ordre**.

---

## 1. Lire la mémoire projet

Première action, sans exception. Lire l'index `.claude/memory/MEMORY.md`, puis charger uniquement les fichiers thématiques (`.claude/memory/01-*` … `14-*`) pertinents à la tâche. Ne jamais tout charger d'un coup.

> **Consolidation mémoire :** plus de gate automatique ni de compteur de sessions. La consolidation se lance **manuellement** via la commande `/dream` quand tu sens que la mémoire a grossi ou divergé. Le protocole `/dev` ne déclenche plus le dream tout seul.

## 1ter. GitNexus Freshness Check

1. `gitnexus_list_repos()` → lire `lastAnalyzed` du repo `infra-pipeline-editor`.
2. Si > 7 jours : `npx gitnexus analyze` pour réindexer.
3. Si le MCP est indisponible : continuer sans bloquer, mais avertir que l'index peut être obsolète.

## 2. Analyser et décider

Identifier le périmètre (backend C# ? frontend Angular ? feature CQRS ? PR ? merge ?), le ou les **sous-agents** à lancer (table §6 de `CLAUDE.md`), et le ou les **skills** à charger (table §7 de `CLAUDE.md`).

### 2a. Passe de contradiction obligatoire

Avant de planifier/coder, si la demande touche la **génération Bicep**, les **pipelines Azure DevOps**, le **bootstrap DevOps**, les **service connections / repos / layouts / flux multi-environnements** :
1. Confronter la demande à l'architecture existante, la mémoire, et les contraintes réelles Azure DevOps/Bicep.
2. Expliciter toute hypothèse fragile, notion fausse, ou simplification trompeuse.
3. Proposer l'implémentation cohérente si la demande brute est incorrecte.

> **Règle absolue :** ne jamais exécuter littéralement une demande Bicep/pipeline qui introduit un concept invalide, un fallback masquant une incohérence, ou une fuite de configuration entre environnements.

### 2bis. Phase Research

Pour les tâches complexes ou cross-cutting, explorer AVANT de déléguer :
1. **GitNexus (structurel) :** `gitnexus_query("concept")`, `gitnexus_context("Symbole")`, `gitnexus_impact(target, "upstream")`. Référence : skill `gitnexus-workflow`.
2. **Graphify (corpus)** si doc/architecture transversale/audit/onboarding : `graphify-out/GRAPH_REPORT.md` + skill `graphify-corpus`.
3. **Sous-agent `Explore`** pour la lecture brute des fichiers identifiés.

Priorité : GitNexus pour le code, Graphify pour le corpus. Ce que tu récupères (chemins exacts, extraits de référence, conventions détectées) DOIT être transmis aux sous-agents experts.

Ne pas déclencher Research si la tâche est triviale ou les fichiers cibles déjà connus.

### 2ter. Scratchpad de session (tâches multi-agents)

Pour une feature complète (Backend + Frontend), créer `.claude/memory/session/task-<slug>.md` (scope IN/OUT, plan par étapes avec agent assigné, contrats modifiés, résultats inter-étapes). Passer ce chemin à chaque sous-agent. **Supprimer** le fichier en fin de tâche (les faits durables vont dans les fichiers thématiques).

### 2quater. Plan vivant multi-PC

Si l'implémentation suit un plan/roadmap/lots : maintenir `docs/features/<slug>-implementation-tracker.md` (Contexte, Statut des lots, Journal horodaté, Prochaines étapes, Checklist reprise sur autre PC). Mettre à jour **au fil de l'eau**, pas seulement en fin de tâche. Ne jamais clore une implémentation planifiée sans tracker à jour.

## 3. Charger les skills applicables

Avant toute génération de code, si un skill est pertinent : le charger (tool Skill / lecture du `SKILL.md`) et appliquer ses instructions à la lettre. Le skill prime sur la connaissance générale.

## 4. Exécuter

Lancer les sous-agents experts via le tool *Agent* avec des prompts précis. Coordonner depuis ici.

> **Règle absolue — jamais de délégation vague.** Chaque prompt de sous-agent DOIT contenir :
> 1. La liste des **fichiers exacts** à créer/modifier (issus de la phase Research).
> 2. Les **conventions projet** pertinentes (issues de la mémoire).
> 3. Un **extrait de code existant** comme référence de style si applicable.
> 4. Le **résultat attendu** non ambigu.
> 5. Le **résultat de `gitnexus_impact()`** si un symbole partagé est modifié.
> 6. Le rappel **TDD** : skill `tdd-workflow` obligatoire, tests AVANT le code.
> 7. Le **résultat de la passe de contradiction** (confirmé / douteux / invalide), surtout Bicep/pipelines.

> **Guardrails à rappeler dans chaque délégation de code :** pas de magic strings · un type public top-level par fichier · pas de `object`/`dynamic`/`Dictionary<string,object>`/`JsonDocument`/`any` si un contrat typé est possible · pattern avec levier uniquement · UI Angular : réutiliser les `app-ds-*`, sinon créer/étendre un primitive DS d'abord.

**Routage clé :**
- Feature complexe / changement archi → `architect` (plan) d'abord, puis exécution.
- Feature CQRS complète → charger `cqrs-feature`, coordonner via `dotnet-dev` (backend) + `angular-front` (frontend).
- Code C# isolé → `dotnet-dev` (+ `dotnet-patterns`, `tdd-workflow`, `xunit-unit-testing`).
- Code Angular isolé → `angular-front` (+ `ui-ux-front-saas` si UI). Rappeler la règle DS-first.
- Review → `review-expert` puis `vibe-coding-refractaire`.
- Incident Aspire → `aspire-debug` avant toute modif de code.

## 4bis. Vérifier l'exécution du plan

Si un plan existait (scratchpad ou `architect`) : relire chaque item, confirmer l'exécution (`[x]`), compléter tout item manquant, signaler tout écart. Ne jamais clore une tâche planifiée sans relecture item par item.

## 5. Mettre à jour la mémoire

En fin de toute tâche non triviale :
- Ajouter l'info dans le bon fichier thématique `.claude/memory/`.
- Ajouter une ligne datée dans `.claude/memory/changelog.md`.
- Mettre à jour `.claude/memory/MEMORY.md` (index) si un nouveau fichier thématique a été créé.
- Ne jamais supprimer d'info existante — compléter ou corriger seulement.

---

## Protocole de fin de tâche

```
[ ] Plan relu item par item (4bis) — items manquants complétés
[ ] Tracker multi-PC synchronisé (si tâche planifiée)
[ ] TDD : tests écrits AVANT le code de prod (si code modifié)
[ ] dotnet test .\InfraFlowSculptor.slnx (si C# touché)
[ ] dotnet build .\InfraFlowSculptor.slnx (si C# touché)
[ ] npm run typecheck + npm run build dans src/Front (si Angular touché)
[ ] gitnexus_detect_changes() — seuls les fichiers/flux attendus impactés
[ ] Dette de tests enregistrée dans .claude/test-debt.md (si dette)
[ ] Fichier thématique .claude/memory/ mis à jour
[ ] Ligne ajoutée dans .claude/memory/changelog.md
[ ] Scratchpad .claude/memory/session/ supprimé (si tâche multi-agent terminée)
[ ] PR déléguée à pr-manager si poussée sur GitHub
```
