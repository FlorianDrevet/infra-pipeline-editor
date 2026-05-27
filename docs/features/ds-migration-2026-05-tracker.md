# DS Migration — Implementation Tracker

**Feature** : Refactor exhaustif vers le design system InfraFlowSculptor
**Branche** : (à créer par vague, ex. `feat/ds-migration-w1-foundations`)
**Audit source** : [audits/audit-design-system-2026-05-27.md](../../audits/audit-design-system-2026-05-27.md)
**Owner** : `@angular-front` (orchestré par `@dev`)
**Date d'ouverture** : 2026-05-27

---

## Contexte

L'audit du 2026-05-27 a identifié ~84 templates non conformes au DS, 30+ usages de `mat-spinner`, 66+ boutons Material résiduels, et 6 gaps de primitives DS. Migration planifiée en 8 vagues mergeables (W1→W8) pour ne pas casser le produit.

---

## Statut des lots

| Lot | Sujet | Statut | Owner | Dernière MAJ | Reste à faire |
|---|---|---|---|---|---|
| **W1** | Fondations DS — créer N1 `ds-spinner`, N5 `ds-progress-bar`, N4 `ds-tag-input`, variants manquants | Not started | @angular-front | 2026-05-27 | Spec API + TDD + impl + showcase |
| **W2** | Tags input rollout (5 écrans) | Not started | @angular-front | 2026-05-27 | Dépend de W1 |
| **W3** | Spinner rollout (30+ occurrences, 18 fichiers) | Not started | @angular-front | 2026-05-27 | Dépend de W1 |
| **W4.1** | Boutons hors DS — `config-detail/**` | Not started | @angular-front | 2026-05-27 | grep `<button class=` → migrer |
| **W4.2** | Boutons hors DS — `project-detail/**` + `multi-repo-push-dialog` | Not started | @angular-front | 2026-05-27 | idem |
| **W4.3** | Boutons hors DS — `resource-edit/sections/**` + dialogs | Not started | @angular-front | 2026-05-27 | idem |
| **W4.4** | Boutons hors DS — `resource-edit.component.html` (par tab) | Not started | @angular-front | 2026-05-27 | À découper en 6 PR (general / env / pipeline / identity / networking / storage) |
| **W4.5** | Boutons hors DS — `shared/components/**` | Not started | @angular-front | 2026-05-27 | idem |
| **W5** | Form controls Material résiduels (radio, checkbox, button-toggle, inputs) | Not started | @angular-front | 2026-05-27 | ~12 fichiers |
| **W6** | Tabs migration (mat-tab-group → app-ds-tabs) | Not started | @angular-front | 2026-05-27 | ⚠️ vérifier parité API ds-tabs d'abord |
| **W7** | Cards Material + dialog shell (N3, N2 opt) | Not started | @angular-front | 2026-05-27 | `config-detail/sections/git`, `multi-repo-push-dialog`, `networking-tab` |
| **W8** | Menus (N6), accordion (N7 opt), hex purge, tokens fantômes | Not started | @angular-front | 2026-05-27 | `networking-tab.scss` complet, split-switcher, settings, home, project-members, custom-domains, pipeline-options, add-resource-dialog, generation-board, layout-repositories |

---

## Journal d'implémentation

### 2026-05-27 — Cadrage initial

- **@dev** : audit exhaustif livré sous `audits/audit-design-system-2026-05-27.md`.
- **@dev** : tracker créé.
- Décisions :
  - Migration en 8 vagues, jamais en un seul PR.
  - 4 primitives DS nouvelles obligatoires (N1 spinner, N4 tag-input, N5 progress-bar, N6 menu) ; 3 optionnelles (N2 dialog-shell, N3 card-mat, N7 accordion, N8 stepper).
  - Pas de migration de zones intentionnelles : `login`, `bicep-file-panel`, `bootstrap-setup-guide`.
  - W1 doit être terminée et mergée avant W2/W3.
- Aucun fichier de production modifié dans cette session — pure phase de cadrage.

---

## Prochaines étapes

1. **@architect** : revue du plan W1→W8 et challenge des estimations (gain/risque).
2. **@angular-front** : ouvrir branche `feat/ds-migration-w1-foundations` et démarrer W1 (TDD-first via `tdd-workflow` skill).
3. À chaque PR mergée : mettre à jour ce tracker + ajouter ligne dans `.github/memory/changelog.md` + section vague dans `.github/memory/14-frontend-design-system.md`.

---

## Checklist reprise sur un autre PC

```
[ ] git fetch && git checkout feat/ds-migration-w<n>-<sujet>   # ou main si nouvelle vague
[ ] cd src/Front && npm install
[ ] Lire audits/audit-design-system-2026-05-27.md (source de vérité)
[ ] Lire ce tracker pour identifier le lot In progress
[ ] Charger skills .github/skills/{tdd-workflow,angular-patterns,ui-ux-front-saas}/SKILL.md
[ ] Avant tout changement de symbole DS : gitnexus_impact target=<nom> direction=upstream
[ ] Tests : cd src/Front && npm run typecheck && npm run build
[ ] Commit + PR titrée: feat(ds-migration): W<n> — <topic>
[ ] Update tracker (statut + journal) + .github/memory/changelog.md
```

---

## Contrats modifiés (à remplir au fil de l'eau)

(W1 ouvrira ici les signatures des nouveaux primitives `app-ds-spinner`, `app-ds-progress-bar`, `app-ds-tag-input`.)

## Résultats inter-agents

(À remplir lors de l'exécution.)
