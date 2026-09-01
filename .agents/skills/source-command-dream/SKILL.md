---
name: "source-command-dream"
description: "Consolidation manuelle de la mémoire projet — synthétise, déduplique et prune .Codex/memory/. À lancer quand la mémoire a grossi ou divergé."
---

# source-command-dream

Use this skill when the user asks to run the migrated source command `dream`.

## Command Template

# /dream — Consolidation mémoire (manuelle)

Lance le sous-agent **`dream`** (via le tool *Agent*) pour consolider la mémoire projet.

> Déclenchement **manuel** : pas de gate, pas de verrou, pas de compteur de sessions. Tu lances `/dream` quand tu juges que la mémoire a besoin d'un nettoyage (après une grosse session, plusieurs features, ou si l'index a gonflé).

Le sous-agent `dream` exécute ses 4 phases (Orient → Gather → Consolidate → Prune) sur `.Codex/memory/` uniquement :
1. **Orient** — lit `MEMORY.md` + survole les fichiers thématiques.
2. **Gather** — récupère le signal récent (`changelog.md`, `git log --since`, `codegraph_status`).
3. **Consolidate** — met à jour les fichiers thématiques, convertit les dates relatives en absolues, supprime les faits contredits, fusionne les doublons.
4. **Prune & Index** — garde chaque fichier thématique < 150 lignes, l'index < 80 lignes, élague le changelog > 60 jours, vérifie la cohérence de l'index.

À la fin, le sous-agent met à jour `lastDreamDate` dans `.Codex/memory/dream-state.md` et renvoie un Dream Report.
