# Audit Design System — Frontend Angular

**Date** : 2026-05-27
**Périmètre** : `src/Front/src/app/**` (Angular 21, standalone, zoneless)
**Auteur** : `@dev` (agent orchestrateur)
**Statut** : Phase 1 (audit) + Phase 2 (mapping) + Phase 3 (gap analysis) + Phase 4 (plan)
**Mode** : audit exhaustif, **aucune modification de code dans cette session** — livrable de cadrage pour vagues de refactor mergeables.

---

## 0. Résumé exécutif

Le design system InfraFlowSculptor (`app-ds-*`) compte **27 primitives** matures (voir [src/Front/src/app/shared/components/ds/index.ts](src/Front/src/app/shared/components/ds/index.ts)) et 6 vagues de migration UI ont déjà été livrées (cf. [.github/memory/14-frontend-design-system.md](.github/memory/14-frontend-design-system.md)). **La couverture DS est désormais majoritaire** sur les écrans principaux (`home`, `projects`, `project-detail`, `config-detail`, `resource-edit`), mais des poches significatives de dette persistent.

### Métriques globales

| Indicateur | Valeur | Source |
|---|---|---|
| Templates HTML feature/shared | **111** | [src/Front/src/app/**/*.component.html](src/Front/src/app/) |
| Templates DS (sous `shared/components/ds/`) | **27** | DS catalog |
| **Templates non-DS à auditer** | **~84** | différence |
| Occurrences `<mat-...` (toutes balises) hors DS | **≥ 300** | grep |
| Occurrences `mat-button` / `mat-stroked-button` / `mat-flat-button` / `mat-icon-button` | **66+** | grep |
| Fichiers SCSS avec hex hardcodés (hors DS, hors tokens) | **~30** | grep `#[0-9a-fA-F]{3,6}` |
| Boutons `<button class="...">` natifs custom (non DS, non Material) | **~120+** | grep |
| `<input ... class="..." [(ngModel)]>` natifs hors DS | **10** | grep |

### Verdict

**Le DS existe et est solide**, mais il **n'est pas appliqué de façon homogène**. Trois zones rouges concentrent ~70 % de la dette :

1. **`resource-edit/**`** — templates massifs (~2000 lignes pour le seul `resource-edit.component.html`), dialogs `add-app-setting-dialog`, `add-app-config-key-dialog`, `add-role-assignment-dialog`, ainsi que `components/networking-tab` (qui utilise encore des tokens CSS `--text-secondary`, `--border`, `--primary` **non DS**).
2. **`config-detail/**`** — `mat-tab-group`, `mat-card`/`mat-card-*` dans `sections/git/`, `mat-form-field` + `mat-chip-set` dans les naming-template dialogs, `mat-spinner` éparpillés.
3. **`project-detail/**`** — `mat-tab-group`, `mat-chip-set`, `mat-card` dans `multi-repo-push-dialog`, classes locales `.btn`, `.add-btn`, `.delete-btn`, `.repo-card__action-btn` qui doublonnent `app-ds-button`/`app-ds-icon-button`.

Et trois zones **hors DS totalement assumées** par mémoire projet, qui doivent rester telles quelles : `login`, `bicep-file-panel` (theming dédié), `bootstrap-setup-guide` (terminal stylisé).

---

## 1. Méthodologie

### 1.1 Critères de détection

Un usage est **non conforme** s'il satisfait l'un des critères suivants :

- `<mat-*>` ou `mat-*` directives en dehors des cas listés "intentionnels" en [.github/memory/14-frontend-design-system.md](.github/memory/14-frontend-design-system.md)
- `<button>` avec classes locales (`.btn`, `.add-btn`, `.delete-btn`, `.bicep-btn`, `.repo-card__action-btn`, `.template-action-btn`, `.ra-add-btn`, etc.) au lieu de `app-ds-button` / `app-ds-icon-button` / `app-ds-panel-action-button`
- `<input>` ou `<textarea>` natifs avec ngModel hors `app-ds-text-field` / `app-ds-textarea` / `app-ds-autocomplete`
- Hex de couleur hardcodés hors fichiers tokens (`src/Front/src/scss/_tokens.scss`) — viole le contrat de tokens documenté
- Classes utilitaires locales `card`, `chip`, `badge`, `tag` qui dupliquent un primitive DS
- Tokens CSS fantômes type `var(--primary, #...)`, `var(--text-secondary, #...)`, `var(--border, #...)` qui ne sont **pas** dans `_tokens.scss` (DS V2 utilise `--ifs-*`)

### 1.2 Cas intentionnellement hors DS (à ne pas migrer)

D'après [.github/memory/14-frontend-design-system.md](.github/memory/14-frontend-design-system.md) et [.github/memory/08-frontend.md](.github/memory/08-frontend.md) :

| Composant | Raison de l'exception |
|---|---|
| `features/login/**` | Surface non authentifiée, theming dédié, brand glassmorphism delibéré |
| `shared/components/bicep-file-panel/**` | Theming code-editor (Bicep) avec data-theme switch utilisateur ; couleurs hardcodées = palette syntaxe |
| `features/project-detail/bootstrap-setup-guide/**` | Visuel terminal volontaire (#7ce8ff / #ffbd2e / #50fa7b = traffic-lights mac) |
| `add-config-dialog` submit button | Native `<button form="...">` car `app-ds-button` ne forward pas `form=` |
| `add-naming-template-dialog`, `add-project-naming-template-dialog` | `mat-form-field` + `ElementRef` pour manipulation curseur sur tokens |
| Container `mat-autocomplete` interne à `app-ds-autocomplete` | Le primitive DS *wrappe* Material — légitime |
| Container `mat-spinner` interne à un DS primitive (autocomplete, date-picker) | Wrapper interne |
| `mat-dialog-content` / `mat-dialog-actions` | Pas de primitive DS dialog-shell ; convention globale appliquée via `src/Front/src/styles.scss` |

---

## 2. Inventaire — DS existant vs gaps

### 2.1 Primitives DS disponibles (27)

`ds-button`, `ds-card`, `ds-alert`, `ds-banner`, `ds-section-header`, `ds-page-header`, `ds-text-field`, `ds-textarea`, `ds-autocomplete`, `ds-select`, `ds-toggle`, `ds-checkbox`, `ds-radio-group`, `ds-chip`, `ds-icon-button`, `ds-panel-action-button`, `ds-option-card`, `ds-date-picker`, `ds-tabs`, `ds-segmented-control`, `ds-table`, `ds-tree-view`, `ds-empty-state`, `ds-skeleton`, `ds-tooltip` (+ directive), `ds-status-dot`.

### 2.2 Gaps DS identifiés — **6 nouveaux composants à créer**

| # | Composant proposé | Justification (volume de duplication) | Remplace |
|---|---|---|---|
| **N1** | `app-ds-spinner` | `mat-spinner` apparaît **30+ fois** dans 18 fichiers avec `diameter` non standardisé (14/16/18/20/22/24/32/36/40) | `<mat-spinner diameter="...">` |
| **N2** | `app-ds-dialog-shell` (composant container) | `mat-dialog-content` + `mat-dialog-actions` répétés dans 30+ dialogs avec à chaque fois du SCSS custom pour scroll, header sticky, actions équilibrées | Pattern HTML répété, déjà partiellement absorbé par override `styles.scss` |
| **N3** | `app-ds-card-mat` (refonte du legacy `mat-card`/`mat-card-header`/`mat-card-content`/`mat-card-actions`) | Utilisé dans `config-detail/sections/git`, `multi-repo-push-dialog`, `networking-tab` — `app-ds-card` n'expose pas slots header/title/subtitle/actions formels | `<mat-card>` chains |
| **N4** | `app-ds-tag-input` (chip-input pattern) | `add-project-environment-dialog`, `add-naming-template-dialog`, `add-project-naming-template-dialog`, `tags-section`, `project-detail-tags-section` répliquent un input + bouton add + `mat-chip-set` avec remove — 5 implémentations distinctes | `mat-chip-set` + custom `<button matChipRemove>` |
| **N5** | `app-ds-progress-bar` | `mat-progress-bar` utilisé dans `import-app-settings-dialog`, et au moins un autre flux d'upload | `<mat-progress-bar>` |
| **N6** | `app-ds-menu` (overlay popup avec items) | `mat-menu` + `matMenuTrigger` dans `resource-edit-role-assignments-section` (5 instances) ; pas de primitive DS équivalente | `mat-menu` + `matMenuTrigger` |

### 2.3 Compléments à des DS existants (extension d'API)

| Composant | Évolution requise |
|---|---|
| `app-ds-tabs` | Vérifier qu'il supporte les onglets icon+label+count (cf `tab-icon` + `tab-count` partout) et accepte un `animationDuration`. Sinon, étendre. |
| `app-ds-empty-state` | Garde la pédagogie OK ; aucun changement attendu |
| `app-ds-button` | Pas de forward `form="<id>"` (limitation connue). Si volonté de purger les natifs `<button form="add-config-form">`, ajouter cette capacité. |
| `app-ds-icon-button` | Variant `danger` est utile (présent ?) — sinon ajouter pour remplacer les `mat-icon-button color="warn"` |

---

## 3. Findings — par fichier (zones rouges)

### 3.1 BLOCKER — `resource-edit.component.html` (~2080 lignes)

Fichier monstre, **densité de violations** très élevée :

| Pattern | Occurrences | Action attendue |
|---|---|---|
| `mat-tab-group` / `mat-tab` (3 niveaux imbriqués) | 14+ | Migrer vers `app-ds-tabs` |
| `mat-radio-group` / `mat-radio-button` | 1 group (password mode) | `app-ds-radio-group` |
| `mat-button-toggle-group` | 1 (scope project/configuration) | `app-ds-segmented-control` |
| `mat-stroked-button` / `mat-flat-button` | 5+ | `app-ds-button` |
| `mat-spinner` | 2+ | `app-ds-spinner` (N1) |
| `<button class="storage-item__remove">`, `.ghost-button`, `.cors-rule-card__remove`, `.cors-field__chip-remove`, `.cors-field__add-btn`, `.cors-field__suggestion`, `.cors-presets__option`, `.save-bar__override`, `.save-bar__discard`, `.delete-btn` | 30+ | Mix `app-ds-button` / `app-ds-icon-button` / `app-ds-chip` (chip remove) |
| `<input class="cors-field__input">` natif | 6 | `app-ds-text-field` (sauf si `#viewChild` cursor manipulation requis — auditer cas par cas) |

> Recommandation : ce fichier doit être **éclaté** avant migration (déjà partiellement fait via `sections/**`) — voir mémoire `Resource-edit performance [2026-05-22]`. La migration doit se faire **section par section** dans des PR distincts.

### 3.2 BLOCKER — `resource-edit/add-app-setting-dialog`, `add-app-config-key-dialog`, `add-role-assignment-dialog`

| Pattern | Action |
|---|---|
| `mat-stroked-button` (~14 instances `goBack()`) | `app-ds-button variant="secondary"` |
| `mat-radio-button` / `mat-radio-group` (`SystemAssigned` / `UserAssigned`) | `app-ds-radio-group` |
| `mat-checkbox` (sensitive flag) | `app-ds-checkbox` |
| `mat-spinner` (~10) | `app-ds-spinner` (N1) |
| `mat-dialog-content` / `mat-dialog-actions` | `app-ds-dialog-shell` (N2) — sinon laisser tel quel |
| `<span class="btn-content"><mat-icon>shield</mat-icon> ...` projetés dans `<app-ds-button>` | Utiliser l'API `icon`/`iconPosition` de `app-ds-button` (rappel pitfall connu) |

### 3.3 BLOCKER — `resource-edit/components/networking-tab`

**Anomalie critique** : ce composant utilise des tokens CSS **non DS** : `var(--text-secondary, #6b7280)`, `var(--border, #d1d5db)`, `var(--surface, #fff)`, `var(--text-primary, #111827)`, `var(--primary, #6366f1)`, `var(--text-tertiary, #9ca3af)`, `var(--error, #ef4444)`, `var(--success, #16a34a)`, `var(--code-bg, #f3f4f6)`, `var(--surface-alt, #f3f4f6)`. **Ces tokens n'existent pas dans `_tokens.scss`** → tous les fallbacks hex prennent le dessus, faisant basculer la carte en mode light hardcodé sur un shell dark.

| Pattern | Action |
|---|---|
| `<mat-card>` / `<mat-card-content>` | `app-ds-card-mat` (N3) ou `app-ds-card` si pas besoin slots |
| `<input class="add-form-input" [(ngModel)]="...">` × 4 | `app-ds-text-field` |
| `<button mat-stroked-button>` / `<button mat-flat-button color="primary">` / `<button mat-icon-button color="warn">` | `app-ds-button` / `app-ds-icon-button variant="danger"` |
| Tokens CSS fantômes (`--text-secondary`, `--border`, etc.) | Remplacer par `--ifs-text-secondary`, `--ifs-border-subtle`, `--ifs-surface-1`, `--ifs-brand-500`, `--ifs-danger`, `--ifs-success`, etc. |
| `linear-gradient` éventuels | Audit anti-pattern (mémoire vague 7) |

### 3.4 MAJOR — `config-detail/config-detail.component.html` + `project-detail/project-detail.component.html`

| Pattern | Occurrences | Action |
|---|---|---|
| `<mat-tab-group>` racine | 1 chacun | `app-ds-tabs` |
| `<mat-tab>` (icon + count) | 5-7 par fichier | items de `app-ds-tabs` |
| `mat-spinner diameter="32"` (loading) | 2 | `app-ds-spinner` (N1) |
| `<button class="bicep-btn">`, `.add-btn`, `.delete-btn`, `.bicep-cta`, `.bicep-cta--git` | 5+ | `app-ds-button` (variants `primary`, `danger`, `success`) |

### 3.5 MAJOR — `config-detail/sections/git/config-detail-git-section.component.html`

Carde **fully Material** : `<mat-card>` / `<mat-card-header>` / `<mat-card-title>` / `<mat-card-subtitle>` / `<mat-card-content>` / `<mat-chip>` / `<mat-card-actions>` / `<button mat-stroked-button>` ×4. À migrer en bloc via `app-ds-card-mat` (N3) + `app-ds-chip` + `app-ds-button`.

### 3.6 MAJOR — `config-detail/sections/generation/config-detail-generation-section.component.html`

`mat-tab-group` (inner gen tabs), `mat-spinner` ×3, `bicep-retry-btn`/`bicep-download-btn` custom buttons. À migrer via `app-ds-tabs` + `app-ds-button` + `app-ds-spinner`.

### 3.7 MAJOR — `project-detail/split-generation-switcher.component.html` (367 lignes HTML)

| Pattern | Action |
|---|---|
| `mat-tab-group` × 4 niveaux (outer Infra/Code + inner Bicep/Pipeline/Bootstrap) | `app-ds-tabs` × 4, avec config `mode="enterprise"` (cf mémoire) |
| `mat-tab` × 14 | items |
| `mat-spinner diameter="22"` × 6 | `app-ds-spinner` (N1) |
| `.split-switcher__retry` ×4, `.bicep-download-btn` × N | `app-ds-button` |
| Tokens `--mat-tab-active-*` overrides (#ffffff) | Garder ou centraliser dans tokens DS |

### 3.8 MAJOR — `project-detail/multi-repo-push-dialog.component.html`

| Pattern | Action |
|---|---|
| `mat-dialog-content` / `mat-dialog-actions` | N2 |
| `<mat-card>` × 2 | N3 |
| `mat-spinner diameter="32"` × 2 | N1 |
| `<button mat-stroked-button>` × 5 | `app-ds-button variant="secondary"` |

### 3.9 MAJOR — `project-detail/layout-repositories.component.html` + `repository-dialog.component.html`

| Pattern | Action |
|---|---|
| `<button class="repo-card__action-btn">`, `.repo-card__action-btn--danger`, `.slot-empty` (button) × 8 | `app-ds-button` / `app-ds-icon-button` |
| `mat-spinner diameter="32"` | N1 |
| `<mat-checkbox [formControlName]="i">` (repository-dialog) | `app-ds-checkbox` |

### 3.10 MAJOR — `shared/components/generation-diagnostics-dialog.component.html`

| Pattern | Action |
|---|---|
| `<button mat-icon-button [matTooltip]>` × 5 (go-to-resource arrows) | `app-ds-icon-button` + `appDsTooltip` directive |
| `<mat-dialog-content>` / `<mat-dialog-actions>` | N2 |

### 3.11 MAJOR — `shared/components/deployment-config.component.html`

| Pattern | Action |
|---|---|
| `.deployment-mode__segment` × 2 (mode segmented) | `app-ds-segmented-control` |
| `.acr-identity-card__create-link`, `.acr-identity-card__action-btn`, `.acr-identity-card__why-toggle` × 4 | `app-ds-button` |
| `mat-spinner diameter="14/16"` × 3 | N1 |
| `mat-icon-button` × 2 | `app-ds-icon-button` |

### 3.12 MAJOR — `shared/components/edit-abbreviation-dialog`, `confirm-dialog`, `cascade-delete-dialog`

Trois dialogs partagés : `<mat-dialog-content>` + `<mat-dialog-actions>`. Migration alignée sur N2 + `app-ds-button` pour actions.

### 3.13 MAJOR — `resource-edit/sections/custom-domains` + `sections/identity-access/resource-edit-role-assignments-section`

| Pattern | Action |
|---|---|
| `mat-icon-button` × 3-5 (validate/delete/dns) | `app-ds-icon-button` (variants `primary` / `danger`) |
| `mat-menu` + `matMenuTrigger` × 5 (assign UAI menu, switch identity menu) | **N6 `app-ds-menu`** (créer) |
| `mat-expansion-panel` (DNS tutorial) | À auditer : pas de primitive DS d'accordion → potentiellement **N7 `app-ds-accordion`** (low prio, 1 usage) |
| `mat-button-toggle-group` (KV missing role card) | `app-ds-segmented-control` |

### 3.14 MAJOR — `resource-edit/import-app-settings-dialog`

| Pattern | Action |
|---|---|
| `<mat-progress-bar>` | **N5 `app-ds-progress-bar`** |
| `mat-checkbox` × 2 (select-all + per-row) | `app-ds-checkbox` |
| `mat-spinner` × 3 | N1 |

### 3.15 MINOR — `projects/create-project-wizard/**`

| Pattern | Action |
|---|---|
| `<mat-stepper>` | Pas de primitive DS stepper → laisser en Material, ou créer **N8 `app-ds-stepper`** (low prio si vague de polish wizard) |
| Material par étape | Auditer chaque step pour résidus button/input natifs |

### 3.16 MINOR — Tags input pattern (5 implémentations)

Tags input répliqué dans :

- `add-project-environment-dialog`
- `project-detail/tags-section`
- `config-detail/sections/tags`
- `add-naming-template-dialog`
- `add-project-naming-template-dialog`

Forme commune : `<input>` + `<button mat-icon-button>(add)` + `<mat-chip-set>` + `<mat-chip (removed)>` + `<button matChipRemove>`. **Candidat fort pour N4 `app-ds-tag-input`** (gain DRY × 5).

### 3.17 SCSS — Couleurs hex hardcodées hors DS

Hors zones intentionnelles (login, bicep-file-panel, bootstrap-setup-guide, ds-* internes) :

- `networking-tab` (cf 3.3) — **critique**
- `split-generation-switcher.component.scss` — `#7e99bd`, `#6f9da1`, `#ffffff` overrides Material tab
- `settings.component.scss` — palette Bicep dupliquée (devrait `@use` celle de `bicep-file-panel`)
- `home.component.scss` — `#fff`, `#f59e0b` (favoris star)
- `project-members.component.scss` — `#fff`
- `resource-edit/sections/custom-domains.scss` — `#bf360c`, `#1b5e20` (state colors → `--ifs-danger-deep` / `--ifs-success-deep` à créer si manquant)
- `resource-edit/components/pipeline-options.scss` — `#ffffff`
- `add-resource-dialog.scss` — `#7c4dff` (devrait être `--ifs-accent-500`)
- `generation-board.component.scss` — `var(--ds-color-warn, #ef5350)` (token fantôme `--ds-color-warn`)
- `layout-repositories.component.scss` — `#22c55e`

→ **6 à 10 fichiers** à migrer vers tokens `--ifs-*`. Cf rappel mémoire "Dark-mode contrast fix [2026-05-11]".

### 3.18 Tokens CSS fantômes (variables sans définition)

| Variable utilisée | Origine | Vrai token DS à utiliser |
|---|---|---|
| `--text-primary`, `--text-secondary`, `--text-tertiary` | `networking-tab.scss` | `--ifs-text-primary`, `--ifs-text-secondary`, `--ifs-text-tertiary` |
| `--border` | `networking-tab.scss` | `--ifs-border-subtle` |
| `--surface`, `--surface-alt`, `--code-bg` | `networking-tab.scss` | `--ifs-surface-1`, `--ifs-surface-2` |
| `--primary`, `--error`, `--success` | `networking-tab.scss` | `--ifs-brand-500`, `--ifs-danger`, `--ifs-success` |
| `--ds-color-primary`, `--ds-color-warn` | `create-pat-dialog`, `generation-board` | `--ifs-brand-500`, `--ifs-danger` |

---

## 4. Plan de migration — vagues mergeables

> Principe : **chaque vague est mergeable indépendamment**, livre une valeur visible, et n'introduit pas de régression. Pas de mass-rename. Pas de PR géant.

### W1 — Fondations DS (créer les manques) [HIGH prio]

**Livrables :**
- `app-ds-spinner` (N1) + skeleton de test, tailles `sm` (14) / `md` (18) / `lg` (24) / `xl` (40)
- `app-ds-progress-bar` (N5) determinate + indeterminate
- `app-ds-tag-input` (N4) avec API typée `Tag[]`, support label, validation, slot `(add)` / `(remove)`
- Étendre `app-ds-icon-button` avec variant `danger` si absent
- TDD obligatoire (skill `tdd-workflow` + `xunit-unit-testing` côté backend N/A — ici tests Karma)
- Export depuis `ds/index.ts` + ajout à la page showcase `features/design-system`

**Validation :** `npm run typecheck`, `npm run build`, screenshots design-system.

### W2 — Tags input rollout [HIGH prio]

Migrer les **5** implémentations vers `app-ds-tag-input` :

1. `add-project-environment-dialog`
2. `project-detail/tags-section`
3. `config-detail/sections/tags`
4. `add-naming-template-dialog` *(attention : ElementRef sur token chips → vérifier compat)*
5. `add-project-naming-template-dialog` *(idem)*

**Validation :** créer/éditer/supprimer un tag dans chaque écran, i18n FR/EN.

### W3 — Spinner rollout [MEDIUM prio]

Remplacement mécanique de **30+ occurrences** `<mat-spinner diameter="X">` par `<app-ds-spinner size="...">` dans 18 fichiers (sauf wrappers internes `ds-autocomplete`, `ds-date-picker`). Pas de regression visuelle attendue. **Vague à automatiser**.

### W4 — Boutons hors DS (purge) [HIGH prio, plus gros volume]

Cible : éradication des classes `.btn`, `.add-btn`, `.delete-btn`, `.bicep-btn`, `.bicep-cta`, `.repo-card__action-btn`, `.ra-add-btn`, `.template-action-btn`, `.split-switcher__retry`, `.bicep-retry-btn`, `.bicep-download-btn`, `.save-bar__override`, `.save-bar__discard`, `.storage-item__remove`, `.cors-*-btn`, `.acr-identity-card__*-btn`, `.deployment-mode__segment`, etc. au profit de :

- `app-ds-button` (`primary` / `secondary` / `subtle` / `success` / `danger`)
- `app-ds-icon-button` (`neutral` / `primary` / `danger`)
- `app-ds-segmented-control` pour `.deployment-mode__segment`

**Sous-vagues** (pour rester mergeable) :

- **W4.1** — `config-detail/**` + `config-detail/sections/**`
- **W4.2** — `project-detail/**` + `project-detail/sections/**` + `multi-repo-push-dialog`
- **W4.3** — `resource-edit/sections/**` + dialogs `resource-edit/add-*` + `resource-edit/role-assignment-impact-dialog`
- **W4.4** — `resource-edit.component.html` (gros fichier, à découper en plusieurs PR par tab)
- **W4.5** — `shared/components/deployment-config`, `generation-diagnostics-dialog`, `cascade-delete-dialog`, `edit-abbreviation-dialog`, `confirm-dialog`

### W5 — Form controls Material résiduels [MEDIUM]

- `<mat-radio-group>` × ~3 → `app-ds-radio-group`
- `<mat-checkbox>` × ~6 → `app-ds-checkbox`
- `<mat-button-toggle-group>` × 2 → `app-ds-segmented-control`
- `<input class="...">` natifs hors viewChild → `app-ds-text-field`

### W6 — Tabs migration [MEDIUM, plus délicat]

Cible : `mat-tab-group` × ~10 → `app-ds-tabs`. Fichiers : `project-detail.component.html`, `config-detail.component.html`, `config-detail/sections/generation`, `resource-edit.component.html` (× 3 niveaux nested), `split-generation-switcher`, `generation-board`, `add-resource-dialog` (env tabs).

⚠️ Le composant `app-ds-tabs` doit d'abord être vérifié pour parité fonctionnelle (animation, icon+label+count slots, programmatic selectedIndex, keyboard a11y). Si manquant, étendre l'API **avant** la migration. **Tester chaque écran après bascule** (deep links, query params `?tab=`).

### W7 — Cards Material + dialog shell [MEDIUM]

- Créer N3 `app-ds-card-mat` (header/title/subtitle/content/actions slots)
- Optionnellement N2 `app-ds-dialog-shell`
- Migrer : `config-detail/sections/git` (cards repo), `multi-repo-push-dialog`, `networking-tab`

### W8 — Menus + accordion + finitions [LOW]

- N6 `app-ds-menu` ; migrer `resource-edit-role-assignments-section`
- (Optionnel) N7 `app-ds-accordion` pour DNS tutorial expansion panel
- Purge des hex hardcodés (cf 3.17) et tokens fantômes (cf 3.18) — **anti-pattern Sonar**
- Refactor `networking-tab.scss` complet (perd ses pseudo-tokens)

---

## 5. Critères de succès (Definition of Done)

À l'issue de l'ensemble des vagues, **must-have** :

- [ ] 0 occurrence de `<mat-button|mat-stroked-button|mat-flat-button|mat-icon-button>` hors zones intentionnelles
- [ ] 0 occurrence de `<mat-tab-group>` hors zones intentionnelles
- [ ] 0 occurrence de `<mat-card*>`, `<mat-spinner>`, `<mat-checkbox>`, `<mat-radio*>`, `<mat-button-toggle*>`, `<mat-progress-bar>` hors wrappers DS
- [ ] 0 fichier SCSS feature avec hex hardcodé hors palette syntaxe Bicep et zones intentionnelles
- [ ] 0 token CSS fantôme `var(--text-primary)`, `var(--border)`, etc. — uniquement `--ifs-*`
- [ ] `npm run typecheck` + `npm run build` verts
- [ ] Screenshots avant/après par vague archivés sous `docs/design/ui-refresh-2026-05/`
- [ ] Mise à jour de `.github/memory/14-frontend-design-system.md` à la fin de chaque vague
- [ ] Création/MAJ d'un skill `ds-migration-rollout` documentant les patterns de bascule

---

## 6. Risques et points d'attention

| Risque | Impact | Mitigation |
|---|---|---|
| Régression visuelle sur les `mat-tab-group` nested (`resource-edit`, `split-generation-switcher`) à cause d'override CSS `::ng-deep .mdc-tab--active` | HAUT | Tests visuels manuels obligatoires par écran après W6 |
| Form controls migrés perdent leur binding (CVA vs `[(ngModel)]` vs `formControlName`) | MOYEN | Pitfall #4 mémoire — toujours préférer `formControlName` |
| `app-ds-button` ne forward pas `form="<id>"` | MOYEN | Auditer chaque dialog avec submit détaché ; soit étendre l'API, soit garder natif |
| `mat-autocomplete` interne à `app-ds-autocomplete` doit rester en wrapper | FAIBLE | Documenté en mémoire, ne pas casser |
| Migration aveugle des hex hardcodés sur `bicep-file-panel` casserait le theming user | HAUT | **Exclure explicitement** ce fichier de toute purge automatique |
| `networking-tab.scss` migration peut révéler un visuel cassé invisible jusqu'ici (les fallbacks light cachaient le bug dark) | MOYEN | Tester en dark + light après migration tokens |
| Dossier `resource-edit.component.html` (2080 lignes) → 1 PR géant ingérable | HAUT | Découper par tab principal (général / env / pipeline / identity / networking / storage) |

---

## 7. Conventions à respecter pendant la migration

Rappels obligatoires (issus de [.github/memory/](file:.github/memory/)) :

1. **Pitfall #13** (DS obligatoire) : tout pattern UI sans DS component doit créer un primitive réutilisable dans `shared/components/ds/`.
2. **Pitfall #14** (une classe par fichier) : pas de fichiers fourre-tout (`components.ts`, `dialogs.ts`).
3. **Pitfall #4** (i18n) : clés sous `RESOURCE_EDIT.DIALOG_NAME.*`, jamais à plat. Préserver les clés existantes lors d'un wrapper change.
4. **TDD** : skill `tdd-workflow` chargé avant toute création (W1). Tests Karma pour les nouveaux primitives.
5. **GitNexus** : lancer `gitnexus_impact(target, "upstream")` avant de modifier un primitive partagé (`app-ds-button`, `app-ds-icon-button`).
6. **Pattern DsButton icon** : utiliser l'API `icon` / `iconPosition` plutôt que projeter `<mat-icon>` dans le slot label.
7. **Pas de `<click>` sur `<app-ds-button>`** : utiliser l'output `clicked` (rappel Sonar PR 327).
8. **Branche par vague** (W1, W2, …) avec PR titrée `feat(ds-migration): W<n> — <topic>`.

---

## 8. Métriques de suivi — tracker

Créer en parallèle de cette migration : [docs/features/ds-migration-2026-05-tracker.md](docs/features/ds-migration-2026-05-tracker.md) avec, par vague :

- Lots / statut (`Not started` / `In progress` / `Blocked` / `Done`)
- Owner (`@angular-front`)
- Commits / PR liés
- Décisions prises (par ex. validation de l'API de `app-ds-spinner`)
- Capture avant/après

---

## Annexe A — Liste exhaustive des templates à auditer

(Liste tronquée — voir `file_search` `src/Front/src/app/**/*.component.html` → 111 résultats)

**Templates feature à migrer (non-intentionnels) — 84 fichiers** : voir résultats du `file_search` ci-dessus, en excluant :

- `shared/components/ds/**` (27 fichiers DS)
- `features/login/**`
- `features/design-system/**` (showcase)
- `shared/components/bicep-file-panel/**`
- `features/project-detail/bootstrap-setup-guide/**`

## Annexe B — Inventaire DS final (cible après W1)

`ds-button`, `ds-card`, `ds-card-mat` *(N3, optionnel)*, `ds-alert`, `ds-banner`, `ds-section-header`, `ds-page-header`, `ds-text-field`, `ds-textarea`, `ds-autocomplete`, `ds-select`, `ds-toggle`, `ds-checkbox`, `ds-radio-group`, `ds-chip`, `ds-icon-button`, `ds-panel-action-button`, `ds-option-card`, `ds-date-picker`, `ds-tabs`, `ds-segmented-control`, `ds-table`, `ds-tree-view`, `ds-empty-state`, `ds-skeleton`, `ds-tooltip`, `ds-status-dot`, **`ds-spinner` (N1)**, **`ds-progress-bar` (N5)**, **`ds-tag-input` (N4)**, **`ds-menu` (N6)**, *(optionnels : `ds-dialog-shell` N2, `ds-accordion` N7, `ds-stepper` N8)*.

**Total cible : 31 primitives (vs 27 actuels), +4 obligatoires, +3 optionnels.**

---

*Fin du rapport. Prochain step recommandé : revue avec `@architect` pour valider le plan W1→W8, puis ouverture du tracker `docs/features/ds-migration-2026-05-tracker.md` et démarrage de W1 sous `@angular-front`.*
