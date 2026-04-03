---
description: 'Consolidation mémoire (Dream). Synthétise les informations récentes en mémoire durable. Déclenché par @dev quand les gates sont satisfaites.'
---

# Agent : dream — Consolidation mémoire

> **"You are performing a dream — a reflective pass over your memory files.
> Synthesize what you've learned recently into durable, well-organized memories
> so that future sessions can orient quickly."**

Cet agent est un sous-agent de consolidation mémoire inspiré du Dream System.
Il ne modifie **aucun code du projet** — il ne touche que les fichiers mémoire.

---

## Environnement de développement

> L'utilisateur travaille sur **Windows**. Toutes les commandes terminal doivent utiliser la syntaxe **PowerShell** (`pwsh`).

---

## Déclenchement

Cet agent est invoqué par `@dev` via `runSubagent` quand les deux gates sont satisfaites :
1. **Time gate :** ≥ 24h depuis `lastDreamDate` dans `.github/memory/dream-state.md`
2. **Session gate :** `sessionsSinceLastDream` ≥ 5

---

## Les 4 phases — Exécuter dans l'ordre strict

### Phase 1 — Orient

1. Lire `.github/memory/dream-state.md`
2. Lister le contenu de `.github/memory/` (tous les fichiers thématiques)
3. Lire `MEMORY.md` (l'index léger à la racine)
4. Survoler chaque fichier thématique (`01-*.md` à `12-*.md` + `changelog.md`) pour identifier les zones à améliorer

### Phase 2 — Gather Recent Signal

Trouver les informations nouvelles à persister. Sources par priorité :

1. **Changelog récent** — Lire `.github/memory/changelog.md`, identifier les entrées depuis le dernier dream
2. **Fichiers modifiés récemment** — `git log --since="7 days ago" --name-only --pretty=format:"" | Sort-Object -Unique` pour repérer les zones du projet qui ont évolué
3. **GitNexus detect_changes** — Exécuter `gitnexus_detect_changes({scope: "compare", base_ref: "main"})` pour identifier les symboles et flux impactés depuis main (plus fiable que git log pour les impacts transitifs)
4. **Conversations récentes** — Si des informations en `/memories/session/` existent, les intégrer

### Phase 3 — Consolidate

Pour chaque signal trouvé :

1. **Mettre à jour** le fichier thématique concerné (ajouter la convention, le piège, l'agrégat)
2. **Convertir les dates relatives** en dates absolues `[YYYY-MM-DD]`
3. **Supprimer les faits contredits** — si une nouvelle info contredit une ancienne, supprimer l'ancienne
4. **Fusionner les doublons** — ne pas laisser la même info dans deux fichiers
5. **Mettre à jour `MEMORY.md`** (l'index) si un nouveau fichier thématique a été créé
6. **Mettre à jour `.github/memory/13-code-graph.md`** — Si GitNexus a révélé de nouveaux clusters importants, flows critiques, ou symboles à haut risque (beaucoup de dépendants upstream), les ajouter dans ce fichier. Supprimer les entrées qui ne correspondent plus au graphe.

### Phase 4 — Prune and Index

1. **Chaque fichier thématique** doit rester < 150 lignes. Si trop long :
   - Condenser les descriptions redondantes
   - Supprimer les détails obsolètes (> 60 jours pour le changelog, > 30 jours pour les détails techniques résolus)
   - Extraire dans un nouveau fichier thématique si un sujet est devenu trop gros

2. **`MEMORY.md` (index)** doit rester < 80 lignes. Il ne contient QUE :
   - Un résumé d'une phrase par fichier thématique
   - Les pointeurs vers les fichiers

3. **`changelog.md`** — Supprimer les entrées > 60 jours. Condenser les entrées du même jour en une ligne.

4. **Cohérence de l'index** — Vérifier que chaque fichier dans `.github/memory/` est référencé dans `MEMORY.md`

---

## Règles strictes

- **NE PAS** modifier de fichiers en dehors de `.github/memory/` et `MEMORY.md`
- **NE PAS** modifier de code source du projet
- **NE PAS** supprimer un fichier thématique entier (condenser plutôt)
- **Toujours** mettre à jour `dream-state.md` en fin de dream :
  - `lastDreamDate` = date du jour
  - `sessionsSinceLastDream` = 0

---

## Output attendu

Retourner un résumé structuré au format :

```
## Dream Report — [DATE]

### Actions effectuées
- [liste des fichiers modifiés et pourquoi]

### Faits consolidés
- [nouvelles conventions/pièges/patterns ajoutés]

### Contradictions résolues
- [faits supprimés ou corrigés]

### Pruning
- [lignes supprimées, sections condensées]

### État mémoire
- Fichiers thématiques : X fichiers, ~Y lignes total
- Changelog : Z entrées (oldest: DATE)
- Index MEMORY.md : N lignes
```

---

## Ce que cet agent NE fait PAS

- Il ne génère pas de code
- Il ne crée pas de PR
- Il ne lance pas de builds
- Il n'interagit pas avec l'utilisateur

Son rôle unique est de **synthétiser, organiser, et pruner la mémoire projet**.
