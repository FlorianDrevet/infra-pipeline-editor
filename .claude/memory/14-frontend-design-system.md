# Frontend Design System

## Vague DS migration intégrale W1→W8 [2026-05-27]

Exécution complète du plan audit-design-system-2026-05-27 en une seule session (sur demande explicite utilisateur). Toutes les vagues sauf W2 livrées.

### Nouveaux primitives DS (7)

- **`app-ds-spinner`** (W1) — SVG circle stroke-dasharray, sizes `sm|md|lg|xl` (14/18/24/40px), `inline` mode, `currentColor`, `prefers-reduced-motion` honored. Tokens `--ifs-*` exclusivement.
- **`app-ds-progress-bar`** (W1) — modes `determinate|indeterminate`, tones `brand|success|danger`, value 0-100. Indeterminate via keyframes whitelistées (cf shimmer skeleton).
- **`app-ds-tag-input`** (W1) — Inputs typés `DsTagInputItem[]`, validators, addOnComma/Enter/Blur, Backspace removes last. Pour usages mono-valeur (pas key-value).
- **`app-ds-menu`** + **`DsMenuDirective`** (W1) — CDK Overlay, ArrowUp/Down navigation, Esc fermeture, items typés `DsMenuItem { id, label, icon?, iconTrailing?, tone?, disabled?, divider? }`. Utilisé en W8 pour role-assignments (4 menus).
- **`app-ds-card-mat`** (W7, N3) — 3 slots nommés `[ds-card-header]`, `[ds-card-content]`, `[ds-card-actions]`, inputs `title?/subtitle?/tone`, tone `neutral|brand|success|warning|danger`. Remplace les chains `mat-card-header/title/subtitle/content/actions` (6 usages migrés : multi-repo-push-dialog×2, config-detail-git-section×2, networking-tab×2).
- **`app-ds-key-value-input`** (dette W2) — ControlValueAccessor `DsKeyValueItem { key, value }[]`. Dual text-fields + add button + chip row. Validators (duplicate key, required). Migré 3 fichiers (config-detail-tags, project-detail-tags, add-project-environment-dialog). Naming-templates exclus (cursor-placement UIs).
- **`app-ds-accordion`** (dette N7) — expand/collapse, ARIA `aria-expanded`, icon, tone `neutral|brand|info`, model `expanded`. Migré DNS tutorial custom-domains. 0 `mat-expansion-panel` restant.

### Patterns découverts / décisions clés

- **Query params `?tab=`** : constantes typées `*_TAB_IDS` (cf `CONFIG_DETAIL_TAB_IDS`, `PROJECT_DETAIL_TAB_IDS`) avec mappers id↔query pour deep-links.
- **`app-ds-tabs` stretch** : `[stretch]="true"` pour largeur pleine. Envelopper tabs + contenu dans wrapper commun pour partager le panneau visuel.
- **DsTabs label** : pré-traduire dans `computed()` lisant `languageService.currentLanguage()`.
- **`<app-ds-button>` n'accepte pas `(click)`** : toujours `(clicked)`.
- **API `icon`/`iconPosition`** : ne pas projeter `<mat-icon>`, utiliser `icon="X" iconPosition="start"`.
- **Segmented control** : 5 usages (password scope, sensitive mode, deployment mode, ACR auth, app-config-key mode).

### SCSS hardening

- Tokens fantômes purgés : `--text-primary|secondary|tertiary`, `--border`, `--surface`, `--code-bg`, `--primary|error|success`, tous → `--ifs-*`.
- Hex hardcodés purgés : 10 fichiers (settings, home, project-members, custom-domains, pipeline-options, generation-board, layout-repositories, split-generation-switcher, create-pat-dialog, networking-tab).
- Overrides `::ng-deep .mat-mdc-tab-*` / `--mat-tab-*` : supprimés (0 hit `features/**/*.scss`).


## `app-ds-ip-input` — saisie IP/CIDR segmentée (style date) [2026-06-02]

Primitive DS unique pour IPv4 + CIDR, `mode: 'ipv4' | 'cidr'`. **Remplace** les anciens `app-ds-cidr-input` + `app-ds-ipv4-input` (supprimés). Champs segmentés type « saisie de date » : un input par octet (+ champ préfixe en `cidr`), séparateurs `.`/`/` rendus comme masque permanent (`aria-hidden`), frappe chiffres seulement, focus qui **avance tout seul** (octet plein à 3 chiffres OU `valeur*10 > max`), Backspace/Flèches reviennent au segment précédent, collage qui distribue, CVA qui émet le contrat inchangé `a.b.c.d` / `a.b.c.d/p`.

- Logique pure isolée et testée dans `ds-ip-input.util.ts` : `getIpSegmentDefs / sanitizeSegment / shouldAdvanceSegment / joinSegments / splitToSegments` (+ `ds-ip-input.types.ts`). Ne jamais réimplémenter le découpage octet à la main.
- **Branchement** : passe par `app-ds-list-input` `inputType="cidr|ipv4"` (son `@switch` rend `app-ds-ip-input mode=...`). Donc add-resource modal ET resource-edit en héritent **sans changer le template des écrans**. Couche données (`addressSpacesInput`/`dnsServersInput` string[]) et validateurs `vnet-tag-input.helpers.ts` intacts.
- i18n aria : `DS.IP_INPUT.OCTET_ARIA` (`{{index}}`) + `DS.IP_INPUT.PREFIX_ARIA` (en/fr).
- Tests : util spec + component spec, 31/31 Karma vert.

### Métriques finales

- **0 occurrence** `<mat-button|mat-stroked|mat-flat|mat-raised|mat-icon-button|mat-tab-group|mat-card|mat-card-*|mat-checkbox|mat-radio-group|mat-button-toggle|mat-progress-bar|mat-menu>` dans `src/Front/src/app/features/**/*.html` et `src/Front/src/app/shared/components/**/*.html` (hors zones intentionnelles).
- **2 résidus légitimes** : `<mat-spinner>` interne à `ds-autocomplete.component.html` (wrapper interne au DS).
- **0 token fantôme** non préfixé `--ifs-*` dans `src/Front/src/app/**/*.scss`.
- `npm run typecheck` : vert (0 erreur).
- `npm run build` : vert (warnings préexistants bundle budget + OpenTelemetry CommonJS, non liés).

### Dette résiduelle (tracée dans `.claude/test-debt.md`)

- **P2** — Tests Karma des 4 nouveaux primitives DS à activer.
- **P2** — 4 specs Karma cassés sur sélecteurs CSS legacy supprimés (resource-edit identity-access) — à re-cibler via DS harness.
- **P3** — `ds-key-value-input` primitive à scoper avant rollout W2 réel.
- **P3** — Purge finale SCSS dead-classes (`.delete-btn`, `.add-btn`, etc.) après stabilisation visuelle multi-vagues.
- **P3** — Palette Bicep à extraire en partial dans `settings.component.scss`.
- **P3** — N7 `ds-accordion` si futur 2ème usage.

## Audit DS coverage — Résolu [2026-05-27]

- **Source** : [audits/audit-design-system-2026-05-27.md](../../audits/audit-design-system-2026-05-27.md) ; tracker [docs/features/ds-migration-2026-05-tracker.md](../../docs/features/ds-migration-2026-05-tracker.md).
- **Résultat** : audit exécuté et migration W1→W8 livrée dans la même session. Tous les gaps identifiés (N1 spinner, N4 tag-input, N5 progress-bar, N6 menu, N3 card-mat) sont créés et déployés. N2 `ds-dialog-shell` et N7 `ds-accordion` skippés (ROI faible). Networking-tab tokens fantômes corrigés en W8.
- **Zones intentionnellement hors DS** : `features/login/**`, `shared/components/bicep-file-panel/**`, `features/project-detail/bootstrap-setup-guide/**`.
- **Risque résiduel** : `app-ds-button` ne forward pas `form="<id>"` — garder des `<button>` natifs pour les detached submit triggers.

## UI Refresh 2026-05 — Key Decisions [2026-05-11]

- **DS autocomplete** (`app-ds-autocomplete`) is the only allowed autocomplete wrapper; raw `MatAutocomplete` forbidden in feature code. Uses DS text-field shell + branded loading/empty states.
- **Dockerfile picker** uses `app-ds-autocomplete` for branch selection (not `app-ds-select`), avoids nested CDK overlays.
- **Blue-presence theme [2026-05-12]**: dark global warmed toward saturated blue. Surfaces: `#08111d/0f1928/142033/1b2a41`. Brand: `#4e86f4`. Accent: `#46b5ff`. New `--ifs-app-backdrop` with radial halos.
- **Vague 6 [2026-05-11]**: 5 primitives (empty-state, skeleton, tooltip, banner, status-dot). Skeleton shimmer whitelisted. Purged anti-patterns across ~9 SCSS files (no gradients/backdrop-filter/translateY in feature code).
- **Vague 7 [2026-05-11]**: Compat layer purge — 168 deprecated SCSS token occurrences migrated in 15 files, 106 deprecated lines removed from `_tokens.scss`, 5 deprecated mixins removed from `_mixins.scss`.
- **Dark-mode contrast fix [2026-05-11]**: ~420 hardcoded occurrences replaced in ~35 SCSS files. Border-radii modernized to `var(--ifs-radius-md/lg)`. Semantic colors migrated to tokens.
- **Config-detail conventions**: naming-preview as compact chip (11px mono), resource actions use local semantic tones (accent add, brand edit, danger delete), 5-column grids for variable-group tables with `overflow-x: auto`.
- **DS toggle off/on**: thumb must be clearly visible at OFF state (lighter than track + ring + shadow), ON state tinted with accent, honors `prefers-reduced-motion`.
- **Sonar cleanup**: `app-ds-button` uses `(clicked)` output only. `ds-table`/`ds-tree-view` avoid simulated ARIA roles on custom elements. Component inputs typed with non-deprecated aliases.

## Récap waves UI Refresh 2026-05

- Vague 1 — tokens dark-only, Inter Variable self-host, _typography/tailwind/styles.scss alignés, suppression gradient app.
- Vague 2 — primitives DS V2 : button, card, text-field, textarea, select, chip, alert, icon-button, toggle, checkbox, radio-group.
- Vague 3 — shell V3 : sidebar fixe 240px, top-bar 48px, footer status-bar 28px ; ds-tabs + ds-segmented-control.
- Vague 4 — pages denses project-detail/config-detail refondues, ds-table + ds-tree-view + PageContextService (breadcrumb signal-driven).
- Vague 5 — resource-edit refondu (54 kB CSS, 270+ classes préservées, HTML inchangé), 9 dialogs purgés, ds-option-card refondu.
- Vague 6 — polish : 5 primitives finales + purge ciblée des composants feature/shared restants.

- Direction artistique + plan de refonte phasé en 6 vagues mergeables : `docs/design/ui-refresh-2026-05.md` (audit, manifeste, palette/typo/spacing/radius/shadows cible, inventaire DS, anti-patterns, annexe `_tokens.scss` prêt à coller).
- Décisions structurantes prises par `@architect` : (1) suppression du gradient global de fond + glass/blur applicatif (gradients limités à login + bouton primary) ; (2) palette pivot — 1 famille bleu désaturée `#4f74b3` brand-500 + 1 accent cyan unique `#3aa3c9` pour actions critiques et focus, dark mode par défaut, light opt-in via `[data-theme="light"]` ; (3) typo pivot — Inter 6 niveaux 12/13/14/15/18/22/28 px, poids max 600, tabular numerals.
- Vagues : (1) tokens & fondations, (2) primitives DS critiques, (3) layout/nav/sidebar enterprise + ds-tabs/ds-segmented-control, (4) pages denses project-detail/config-detail + ds-table/ds-tree-view, (5) resource-edit & dialogs denses, (6) polish + ds-empty-state/ds-skeleton/ds-tooltip/ds-banner/ds-status-dot + a11y final.
- Points en attente de validation utilisateur avant vague 1 : Inter CDN puis self-host vs self-host immédiat ; sidebar permanente vs nav top renforcée ; double thème dark/light vs dark only.

## Platform Baseline [2026-04-24]

- Angular frontend is on **v21** with standalone components and zoneless `provideZonelessChangeDetection`.
- Material/CDK v21 sass migrations were applied; `styles.scss` now carries the global override layer for remaining Material primitives.
- SSR remains intentionally out of scope because the app is authenticated, browser-centric, and MSAL-heavy.
- SPA container baseline: Node 22 build stage, nginx 1.29 runtime, BuildKit npm cache, gzip, immutable hashed assets, and a health check.

## Token System

- Shared SCSS sources live under `src/Front/src/scss/`: tokens, mixins, animations, typography, and the forwarded `main` entry point.
- Core visual primitives: brand/cta/login/nav/app gradients, blurred glass surfaces, premium shadows, and brand-blue/cyan focus states.
- `angular.json` exposes `stylePreprocessorOptions.includePaths: ["src"]`, so components can use `@use "scss/main" as *;`.
- `tailwind.config.js` mirrors the same `ifs-*` colors, radii, shadows, and gradient utilities.

## Component Suite

- Layout and CTA primitives: `app-ds-button`, `app-ds-card`, `app-ds-alert`, `app-ds-section-header`, `app-ds-page-header`.
- CVA form controls: `app-ds-text-field`, `app-ds-textarea`, `app-ds-autocomplete`, `app-ds-select`, `app-ds-toggle`, `app-ds-checkbox`, `app-ds-radio-group`.
- Support controls: `app-ds-chip`, `app-ds-icon-button`, `app-ds-panel-action-button`, `app-ds-date-picker`.
- `DsSelectComponent` uses `cdkConnectedOverlay`; panel must use `width: 100%` and `min-width: 100%` to match trigger width.
- Scrollable picker dialogs (e.g. `add-resource-dialog`) need `scrollbar-gutter: stable` + right padding for scroll lane.
- Full-row button rows in overlays must use `box-sizing: border-box` to prevent horizontal overflow.
- `app-ds-toggle` is the only production toggle allowed; all `MatSlideToggleModule` in feature code is migration drift [2026-05-16].
- `DsButtonComponent`: enterprise baseline (no large glow/hero gradients). Use `icon`/`iconPosition` API, never project raw `<mat-icon>` in label content.
- `DsSectionHeaderComponent`/`DsPageHeaderComponent`: keep `__title-row` with icon+title on same flex row, subtitle below.
- `DsPanelActionButtonComponent`: compact panel-header actions with tone/surface/pressed APIs. `project-detail` SplitInfraCode overrides to restrained slate/ink enterprise feel [2026-05-11].

## Global Material Override

- `styles.scss` restyles remaining Material surfaces globally (form fields, tabs, dialogs, menus, snack bars, buttons, checkbox/toggle, tooltips, selection, MDC shells).
- Dialog footers: shared `mat-dialog-actions` layout (equal-width actions, stacked under 640px). Narrow `:has()` selector so extra-action dialogs stay untouched [2026-05-16].
- Keep raw Material only where DS layer depends on unreimplemented framework behaviors.

## Migration Status [2026-05-27] — COMPLETED

Vagues W1→W8 exécutées dans la même session. Tous les gaps identifiés (spinner, tag-input, progress-bar, menu, card-mat, key-value-input, accordion) livrés. Seuls 2 résidus `mat-chip-set` dans naming-templates (exclus par design — cursor-placement UIs). 0 `mat-*` primitive in feature/shared HTML (hors zones intentionnelles: login, bicep-file-panel, bootstrap-guide). Tokens fantômes purgés (networking-tab refonte). Tracker: `docs/features/ds-migration-2026-05-tracker.md`. Dette résiduelle tracée dans `.claude/test-debt.md` (P2 tests Karma + P3 SCSS dead-classes).

## Migration Status [2026-05-27]

- **0** `mat-expansion-panel`, `mat-chip-set` (except 2 naming-template cursor UIs), Material buttons/icon-buttons in feature templates.
- Shared SCSS `_bicep-syntax-palette.scss` extracts Bicep tokens. All legacy dead-classes purged from 5 SCSS files.
- `project-detail` top tabs + SplitInfraCode generation shell rebalanced toward enterprise language [2026-05-11].
- Remaining raw Material is intentional: `matAutocomplete` in push/member dialogs, naming-template `ElementRef` cursor dialogs, `form="<id>"` submit buttons.
- `footer`, `navigation`, `projects`, `home`, and main detail screens use tokenized DS baseline.

## Usage Constraints

- Prefer `formControlName` for DS forms. For isolated signal-based inputs, use standalone `ngModel`.
- Do not assume DS controls forward arbitrary native attributes; `app-ds-button` does not forward `form="<id>"`.
- `app-ds-panel-action-button` is the preferred control for compact panel/card header actions on mixed light/dark surfaces; do not overload `app-ds-icon-button` for toggled panel-state affordances.
- `TranslateService.instant()`-built `DsSelectOption[]` labels are not reactive on language switch; component recreation is required.
- DS controls require a parent `FormGroup` (or standalone `ngModel`); `DsTextField` migrations should rely on CVA binding, not ad hoc `value`/`valueChange`.

## UI Caveats

- Shared components (e.g. `DockerfilePickerComponent`) embedded in light DS forms must use brand-palette colors for triggers/borders, not white-on-white. Verify trigger contrast on the actual host form surface.
- `/settings` PAT creation token reveal: key marker + token value on same row (vertically centered), copy CTA left (variant `subtle` → `success` after copy), confirmation CTA right. Avoid `secondary` variant on the light reveal surface.

## Shared SCSS Mixins [2026-04-28]

- `@include ifs-data-table` (in `src/Front/src/scss/_tables.scss`) remains available for legacy flex-table surfaces, but new dense tabular UIs should prefer `app-ds-table`. The `/settings` PAT list is the reference migration: DS grid columns + typed cell templates + horizontal overflow wrapper, `density="compact"`, allow header wrapping for i18n, keep destructive row actions visually secondary with page-local sizing.
- The `/settings` Bicep-theme chooser reuses syntax-color CSS variables from `shared/components/bicep-file-panel/` for live previews.
- `DsDatePickerComponent` — fully custom CDK overlay calendar: day/month/year views, 42-day grid, min/max constraints, locale-aware via `Intl.DateTimeFormat` + `LanguageService`, fast year-selection (12-year grid), month-selection grid, clamping navigation when target falls outside bounds, CVA support, ARIA labels under `COMMON.DATE_PICKER.*`. Used in PAT creation dialog with dynamic `today` to `today + 1 year` bounds.

## DS Integration Rule [2026-04-28]

- **Mandatory**: Any new screen/dialog/form MUST use existing `app-ds-*` components. If a UI pattern has no DS component yet, create a reusable one in `shared/components/ds/` BEFORE using it.
- This rule is enforced in `copilot-instructions.md` (pitfall #12) and `angular-patterns/SKILL.md`.