# Scripts d'audit — Création des issues GitHub

Ces scripts créent automatiquement les issues GitHub correspondant à l'audit de code du 14 avril 2026 (`docs/AUDIT-2026-04-14.md`).

## Prérequis

- [GitHub CLI (`gh`)](https://cli.github.com/) installé et authentifié (`gh auth login`)
- Droits de création d'issues et de labels sur le repository

## Utilisation

### Étape 1 — Créer les labels

```bash
chmod +x scripts/create-audit-labels.sh
./scripts/create-audit-labels.sh
```

Crée 25 labels organisés par :
- **Sévérité** : `severity: critical`, `severity: high`, `severity: medium`, `severity: low`
- **Domaine** : `area: security`, `area: database`, `area: domain`, `area: application`, `area: api`, `area: generation`, `area: infrastructure`, `area: testing`
- **Phase** : `phase: 0-foundation` à `phase: 7-quality`
- **Type** : `type: bug`, `type: refactor`, `type: performance`, `type: tech-debt`
- **Référence audit** : `audit: 2026-04`

### Étape 2 — Créer les issues

```bash
chmod +x scripts/create-audit-issues.sh
./scripts/create-audit-issues.sh
```

Crée **55 issues** avec :
- Titre formaté `[ID] Description` (ex: `[SEC-001] Mettre en place l'infrastructure de tests`)
- Labels de sévérité, domaine, phase et type
- Corps complet avec : contexte, constat, travail à réaliser, critères d'acceptation, estimation

### Résumé des issues créées

| Sévérité | Count | Exemples |
|----------|-------|----------|
| 🔴 Critique | 13 | SEC-001 à SEC-004, DB-001/002, APP-001/002/003, DDD-001/002, GEN-001/002 |
| 🟠 Haute | 16 | SEC-005 à SEC-007, DB-003 à DB-008, APP-004 à APP-007, DDD-003 à DDD-005, GEN-003/004 |
| 🟡 Moyenne | 16 | DDD-006 à DDD-011, APP-008 à APP-012, API-001 à API-005, GEN-005 à GEN-008, INFRA-001/002 |
| 🟢 Basse | 10 | DDD-012 à DDD-014, API-006/008, APP-013/014, INFRA-003/004 |

## Planification recommandée

Utiliser les labels `phase: X` pour créer un GitHub Project Board par phase :

1. **Phase 0** — Fondations (tests) → Avant toute refacto
2. **Phase 1** — Sécurité critique → Sprint 1
3. **Phase 2** — Base de données → Sprint 1-2
4. **Phase 3** — Domain model → Sprint 2
5. **Phase 4** — Application layer → Sprint 2-3
6. **Phase 5** — API & Contracts → Sprint 3
7. **Phase 6** — Bicep/Pipeline Generation → Sprint 3-4
8. **Phase 7** — Qualité globale → Sprint 4+
