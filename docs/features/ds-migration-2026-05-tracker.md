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
| **W1** | Fondations DS — 4 primitives créés (ds-spinner, ds-progress-bar, ds-tag-input, ds-menu) | **Done** | @angular-front | 2026-05-27 | — (icon-button danger déjà existant) |
| **W2** | Tags input rollout (5 écrans) | **Deferred** | @angular-front | 2026-05-27 | Bloquant : les 5 cibles audit sont en fait key-value ou action-chips, pas de simples tag-inputs. Nécessite primitive `ds-key-value-input` séparé (à scoper). Dette P3. |
| **W3** | Spinner rollout (35 fichiers, 69 occurrences) | **Done** | @angular-front | 2026-05-27 | — |
| **W4.1** | Boutons hors DS — `config-detail/**` (11 fichiers) | **Done** | @angular-front | 2026-05-27 | — |
| **W4.2** | Boutons hors DS — `project-detail/**` (~40 boutons, 10 fichiers) | **Done** | @angular-front | 2026-05-27 | — (purge SCSS dead-classes à compléter après stabilisation) |
| **W4.3** | Boutons hors DS — `resource-edit/sections/**` + dialogs (11 fichiers) | **Done** | @angular-front | 2026-05-27 | Dette P2 : 4 specs cassés sur classes legacy (`.uai-used-row__unlink`, etc.) à re-cibler via harness |
| **W4.4** | Boutons hors DS — `resource-edit.component.html` monolithe (~2080 lignes) | **Done** | @angular-front | 2026-05-27 | 6 batches : header, password, save-bar, storage, CORS, lifecycle. Segmented control intégré pour password scope. |
| **W4.5** | Boutons hors DS — `shared/components/**` (3 fichiers principaux) | **Done** | @angular-front | 2026-05-27 | deployment-config, generation-diagnostics-dialog complets ; 2 segmented controls + 7 icon-buttons + 5 ds-buttons |
| **W5** | Form controls Material résiduels — 8 fichiers (radio, checkbox, progress-bar) | **Done** | @angular-front | 2026-05-27 | 0 mat-radio/checkbox/button-toggle/progress-bar résiduel hors networking-tab (déjà clean en W7) |
| **W6** | Tabs migration — 10 instances mat-tab-group dans 7 fichiers, jusqu'à 4 niveaux nested | **Done** | @angular-front | 2026-05-27 | Query params `?tab=` préservés via mappers `CONFIG_DETAIL_TAB_IDS` / `PROJECT_DETAIL_TAB_IDS`. ARIA i18n ajoutée. |
| **W7** | Cards Material — N3 `app-ds-card-mat` créé + 6 cards migrées (3 features) | **Done** | @angular-front | 2026-05-27 | N2 `ds-dialog-shell` SKIP (ROI faible). 0 mat-card* résiduel. |
| **W8** | Menus (N6) + hex purge (10 fichiers) + ghost tokens purge + networking-tab.scss refonte | **Done** | @angular-front | 2026-05-27 | 4 mat-menu → ds-menu. N7 accordion SKIP (1 usage, dette P3). Palette Bicep settings.scss dette P3 (extraction partial). |

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

### 2026-05-27 — Exécution intégrale W1→W8 en une session (sur demande utilisateur)

- **@dev** orchestre l'exécution séquentielle des 8 vagues via délégations `@angular-front`.
- **W1 Done** : 4 primitives DS créés (`ds-spinner`, `ds-progress-bar`, `ds-tag-input`, `ds-menu`) + directive `DsMenuDirective`. 9 exports ajoutés à `index.ts`. i18n `DS.SPINNER.LOADING` ajouté. icon-button `danger` variant déjà existant — extension annulée. Dette tests Karma P2.
- **W2 Deferred** : analyse révèle que les 5 cibles audit sont en fait des inputs key-value (pas de simples tags) ou des action-chips (naming-template). Migration sans extension API conduirait à régression fonctionnelle. Dette P3 : créer `ds-key-value-input` séparé puis re-scoper W2.
- **W3 Done** : 35 fichiers, 69 occurrences `<mat-spinner>` → `<app-ds-spinner>` avec mapping diameter→size. `MatProgressSpinnerModule` retiré partout sauf 2 zones internes DS (autocomplete) — légitimes.
- **W4.1 Done** : 11 fichiers config-detail migrés, 8 fichiers SCSS purgés des classes mortes (`.delete-btn`, `.bicep-cta`, `.template-action-btn`, etc.). 5 exceptions documentées (anchors, toggles structuraux).
- **W4.2 Done** : 10 fichiers project-detail, ~40 boutons → DS. Bug template corrigé dans split-generation-switcher (régression `@if`/`@else` Bootstrap). i18n FR/EN ajoutée pour environments+tags (MOVE_LEFT, MOVE_RIGHT, ADD_TAG, etc.).
- **W4.3 Done** : 11 fichiers resource-edit/sections + dialogs. 2 segmented controls (password scope, sensitive mode) + 6 ds-buttons + plusieurs icon-buttons. Dette P2 : 4 specs Karma cassés sur sélecteurs CSS legacy.
- **W4.4 Done** : `resource-edit.component.html` monolithe 2080 lignes purgé. 6 batches : header delete, password segmented, save-password CTA, storage/CORS/lifecycle items, save-bar. 0 mat-button résiduel.
- **W4.5 Done** : deployment-config + generation-diagnostics-dialog + 5 mat-icon-button → app-ds-icon-button + appDsTooltip. 2 nouveaux segmented controls (deployment mode, ACR auth mode). Animation slider remplacée par DS transitions natives.
- **W5 Done** : 8 fichiers, 0 mat-radio/checkbox/button-toggle/progress-bar résiduel hors `networking-tab`. Specs `repository-dialog` mises à jour. `import-app-settings-dialog` utilise désormais `app-ds-progress-bar`.
- **W6 Done** : 10 instances `mat-tab-group` migrées dans 7 fichiers, jusqu'à 4 niveaux nested (split-generation-switcher). Constantes `CONFIG_DETAIL_TAB_IDS` / `PROJECT_DETAIL_TAB_IDS` extraites pour préserver `?tab=` deep-links. Tous overrides `::ng-deep .mat-mdc-tab-*` purgés (0 résiduel vérifié grep).
- **W7 Done** : N3 `app-ds-card-mat` créé (slots header/content/actions, tone `neutral|brand|success|warning|danger`). 6 cards migrées : multi-repo-push-dialog (×2), config-detail-git-section (×2), networking-tab (×2). N2 `ds-dialog-shell` SKIP (ROI faible).
- **W8 Done** : 4 mat-menu → ds-menu (role-assignments). 10 fichiers SCSS purgés de hex hardcodés. 0 ghost token résiduel (`var(--text-*)`, `var(--surface)`, `var(--ds-color-*)`, etc. tous remplacés par `--ifs-*`). `networking-tab.scss` refonte intégrale (174 lignes). N7 accordion SKIP (1 usage). Palette Bicep settings.scss dette P3.

**Validation finale** :
- `npm run typecheck` : ✅ 0 erreur
- `npm run build` : ✅ vert (warnings préexistants : bundle budget +22kB, OpenTelemetry CommonJS — non liés)
- Grep résiduel `<mat-(button|tab-group|card|spinner|checkbox|radio-group|button-toggle|menu)` dans templates feature : **0 hits** (2 hits restants exclusivement dans `ds-autocomplete` interne — légitime)

**Dette résiduelle ouverte** :
- W2 — `ds-key-value-input` primitive à scoper avant re-rollout (P3)
- W4.3 — 4 specs Karma à re-cibler vers DS harness (P2)
- W4.2 / W4.3 — purge SCSS dead-classes à compléter après stabilisation visuelle (P3)
- W8 — palette Bicep settings.scss à extraire en `@use 'shared/bicep-syntax-palette'` (P3)
- W8 — N7 accordion (DNS tutorial single-usage) (P3)
- W1 — tests Karma des 4 nouveaux primitives (P2)

---

## Prochaines étapes

1. **@architect** : revue post-implémentation et validation des décisions de scope (W2 deferred, N2/N7 skip).
2. **@review-expert** + **@vibe-coding-refractaire** : passe pré-merge sur la branche courante.
3. **Tests visuels manuels** : split-generation-switcher (tooltip Code outer-tab repositionné), deployment-config (slider remplacé), networking-tab dark mode (tokens fantômes corrigés — anciens fallbacks light cachaient le bug).
4. **@angular-front** : sortir la dette P2 (specs Karma) en PR séparé avant merge production.

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
