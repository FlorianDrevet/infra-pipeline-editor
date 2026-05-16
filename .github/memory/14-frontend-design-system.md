# Frontend Design System

## UI Refresh 2026-05 — Vagues complètes [2026-05-11]

- **Blue-presence theme refresh [2026-05-12]** : la base dark globale a été réchauffée vers un bleu plus assumé sans revenir au glassmorphism applicatif. `src/Front/src/scss/_tokens.scss` pousse désormais une famille brand plus vive (`--ifs-brand-500: #4e86f4`), un accent plus lumineux (`--ifs-accent-500: #46b5ff`), des surfaces dark bleutées (`--ifs-bg/#08111d`, `--ifs-surface-1/#0f1928`, `--ifs-surface-2/#142033`, `--ifs-surface-3/#1b2a41`), des gradients login/CTA plus clairs, et un nouveau token `--ifs-app-backdrop` basé sur des halos radiaux bleus + un voile sombre. `src/Front/src/styles.scss` applique ce backdrop globalement sur `body` et `.mat-app-background`, ce qui redonne de la présence colorée au shell, à la navigation et aux écrans de détail sans retouche écran par écran. Validation : `npm run typecheck`, `npm run build`.

- **Vague 6 (polish, livrée 2026-05-11)** : 5 nouvelles primitives DS créées et exportées via `ds/index.ts` :
  - `app-ds-empty-state` — slot icon/title/description + `[actions]` slot, container neutre 64×64 icon wrap, no border/shadow.
  - `app-ds-skeleton` — variants `box`/`line`/`circle`, shimmer linear-gradient *whitelisté* (commentaire `// V2 whitelisted: skeleton shimmer is functional, not decorative`), `prefers-reduced-motion` honored, props width/height/count/gap, rendu décoratif `aria-hidden=true`.
  - `app-ds-tooltip` — directive `[appDsTooltip]` + composant overlay via CDK Overlay, delay configurable (default 350ms), positions top/bottom/left/right, hide on blur/Esc, `aria-describedby` câblé sur le trigger, `prefers-reduced-motion` honored.
  - `app-ds-banner` — full-width strip, indicator 3px à gauche selon variant info/success/warning/danger, optional `[actions]` slot, dismissible via `app-ds-icon-button`, role=alert sur danger sinon role=status.
  - `app-ds-status-dot` — variants success/warning/danger/info/idle, sizes sm (8px) / md (10px), pulse animation opacity 1↔0.45 sur 2s, ariaLabel optionnel pour usage standalone.
- **Purge anti-patterns vague 6** : `split-generation-switcher.component.scss` (11→0 hits, gradients/box-shadows brand purgés, panel-action CSS vars remappés sur tokens neutres, mat-tab header sur surface-2), `home.component.scss` (5→0 hits, action-card/mini-card/overview-card hover lifts remplacés par shadow-md neutre + surface-2 hover, gradient action-card--create remplacé par surface-2), `projects.component.scss` (1→0 hits, project-card hover sans translateY), `bootstrap-setup-guide.component.scss` (3→0 hits, container sur surface-2, step-number permission/success en couleur semantic flat), `bicep-file-panel.component.scss` (5→0 hits, workspace/viewer flat surface-1/2 + accent-500 actif), `deployment-config.component.scss` (4→0 hits, deployment-content sur surface-2, `transition: all` remplacés par énumérations token-based), `toggle-section-card.component.scss` (2→0 hits, gradient blanc remplacé par surface-1, keyframe sans translateY), `compact-select.component.scss` (1→0), `dockerfile-picker.component.scss` (1→0).
- **Out-of-scope vague 6 — dette suivie** : ~~`ds-panel-action-button.scss` (22 hits)~~, ~~`ds-date-picker.scss` (4 hits)~~, ~~suppression du compat layer SCSS legacy~~ → **tous résolus en vague 7 (2026-05-11)**.
- **Vague 7 (compat layer purge, livrée 2026-05-11)** : refonte `ds-panel-action-button` (flat V2, 0 gradient/backdrop/translateY), refonte `ds-date-picker` (flat header, no translateY, selected → accent-500 flat), migration bulk 168 occurrences deprecated SCSS tokens → V2 CSS vars dans 15 fichiers, suppression compat layer (106 lignes deprecated supprimées de `_tokens.scss` + 5 mixins deprecated supprimées de `_mixins.scss`). Build vert.
- **Dark-mode contrast & modernization fix (2026-05-11)** : 7 passes de remplacement couvrant ~420 occurrences hardcodées dans ~35 fichiers SCSS. Tokens boosted : surfaces widened (`#0c0e12/#13161b/#1a1e25/#22272f`), borders fortified (`#2a303a/#3a4250`), text raised (`#f0f2f5/#b4bcc9/#7d8694`), accent brightened (`#3ab4d9`, new `--ifs-accent-400: #56c4e6`), CTA gradient modernized (135deg, brighter stops), shadows deepened. Border-radii modernisés : 1.3–1.6rem → `var(--ifs-radius-md)` / `var(--ifs-radius-lg)` (8/12px). Couleurs sémantiques migrées vers tokens : `#c62828` → `var(--ifs-danger)`, `#e65100/#f57f17` → `var(--ifs-warning)`, `#2e7d32` → `var(--ifs-success)`, gray-blues → `var(--ifs-text-*)`. Build vert.
- **Config-detail dense resource polish (2026-05-11)** : `naming-preview` est désormais un chip compact (11px mono, padding 2×6, icon 12px) pour éviter d'écraser les lignes de ressources. Les `storage-sub-item` étant des `button`, ils doivent resetter `text-align: left`, `font-family: inherit`, `color: inherit`, `width: 100%`, et ancrer explicitement `&__name` à gauche / `&__badge` à droite, sinon le nom du blob paraît centré dans la grille.
- **Naming abbreviations icon alignment (2026-05-16)** : dans les tables d'abréviations de `config-detail` et `project-detail`, la colonne `abbreviation-list__col--type` doit être un row flex aligné au centre avec `min-width: 0`, et le libellé doit vivre dans un wrapper `.abbreviation-row__label` séparé. Sans ce duo, le `mat-icon` reste calé sur la baseline du texte au lieu d'être centré verticalement, et l'ellipsis du nom peut se perdre.
- **Project-detail config cards (2026-05-11/16)** : le header de `config-card` doit rester sur une seule ligne (`align-items: center`, `justify-content: space-between`) et le `h3` doit explicitement resetter ses marges par défaut. Sans ça, le nom reste au-dessus tandis que les actions passent sur une seconde ligne malgré la carte en layout horizontal. La flèche de navigation doit rester collée au libellé dans un cluster `config-card__title`, tandis que l'action destructive reste le dernier élément de la ligne, tout à droite.
- **Sonar PR 327 frontend cleanup (2026-05-11)** : `app-ds-button` doit utiliser son output `clicked` dans les templates DS-aware au lieu d'accrocher un `(click)` sur l'élément custom. `ds-table` et `ds-tree-view` ne doivent plus simuler des rôles ARIA `table`/`tree` sur des `div`/composants custom ; préférer des sémantiques natives simples avec support clavier explicite (`tabindex`, `Enter`, `Space`). `DsCardComponent`, `DsChipComponent` et `DsIconButtonComponent` doivent typer leurs inputs avec des alias `*Input*` non dépréciés pour garder les alias publics historiques comme surface de compatibilité seulement.
- **Build vague 6** : typecheck OK, `ng build` OK (initial bundle 953.40 kB inchangé, resource-edit chunk 520 kB inchangé). Aucune nouvelle référence à `linear-gradient`/`backdrop-filter`/`translateY(-`/`transition: all` introduite (sauf shimmer skeleton whitelisté).

## Récap waves UI Refresh 2026-05

- Vague 1 — tokens dark-only, Inter Variable self-host, _typography/tailwind/styles.scss alignés, suppression gradient app.
- Vague 2 — primitives DS V2 : button, card, text-field, textarea, select, chip, alert, icon-button, toggle, checkbox, radio-group.
- Vague 3 — shell V3 : sidebar permanente 240/56, top-bar 48px, footer status-bar 28px ; ds-tabs + ds-segmented-control.
- Vague 4 — pages denses project-detail/config-detail refondues, ds-table + ds-tree-view + PageContextService (breadcrumb signal-driven).
- Vague 5 — resource-edit refondu (54 kB CSS, 270+ classes préservées, HTML inchangé), 9 dialogs purgés, ds-option-card refondu.
- Vague 6 — polish : 5 primitives finales + purge ciblée des composants feature/shared restants.

## Enterprise UI Refresh — Direction artistique [2026-05-11]

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
- CVA form controls: `app-ds-text-field`, `app-ds-textarea`, `app-ds-select`, `app-ds-toggle`, `app-ds-checkbox`, `app-ds-radio-group`.
- Support controls: `app-ds-chip`, `app-ds-icon-button`, `app-ds-panel-action-button`, `app-ds-date-picker`.
- `DsSelectComponent` uses `cdkConnectedOverlay` so dropdowns escape scrollable/tabbed containers instead of creating nested scrollbars.
- `DsSelectComponent` panel must explicitly use `width: 100%` and `min-width: 100%` so the rendered dropdown matches the trigger width instead of collapsing to its intrinsic menu width. [2026-05-11]
- `DsToggleComponent` exposes `ariaLabel` for icon-only or label-less usages and is reused by `ToggleSectionCardComponent`.
- `DsButtonComponent` keeps a shared enterprise baseline: `primary` and `success` stay slightly layered for hierarchy, but avoid large glow, aggressive hover lift, or flashy hero gradients because the component fans out across dialogs, wizards, and detail screens. [2026-05-11]
- `DsButtonComponent` icon+label alignment depends on its dedicated `icon` / `iconPosition` API. In dialogs and action rows, do not project a raw `<mat-icon>` inside the button label content when the intent is a standard leading/trailing button icon, otherwise the projected icon bypasses the DS button's aligned layout. [2026-05-11]
- `DsPanelActionButtonComponent` targets compact panel-header actions with `tone` (`neutral | accent | danger`), `surface` (`light | dark`), optional `pressed`/`ariaExpanded`, and a premium glassy soft-square visual. It currently powers the generation-panel collapse/close cluster in `config-detail` and `project-detail`.
- `project-detail` SplitInfraCode generation now deliberately overrides that default glassy feel: the outer tabs and `SplitGenerationSwitcherComponent` use a restrained slate/ink surface, smaller quieter counters, thin underlines, and command-bar style CTA rows so the screen reads as enterprise product UI rather than neon/glass hero chrome. [2026-05-11]
- `BicepFilePanelComponent` now renders generated artifacts as a workspace/editor surface instead of a faux terminal: calm dark chrome, file-oriented headers, restrained badges, and readable code highlighting replace prompt/cursor metaphors and neon terminal accents. [2026-05-11]

## Global Material Override

- `src/Front/src/styles.scss` restyles the remaining Material surfaces globally: form fields, tabs, dialogs, menus, snack bars, buttons, checkbox/toggle, tooltips, selection, and related MDC shells.
- Keep raw Material where the DS layer still depends on framework behaviors that are not reimplemented yet.

## Migration Status [2026-04-24]

- Primary CTAs across `home`, `projects`, `project-detail`, `config-detail`, `resource-edit`, and shared dialogs were largely migrated to `app-ds-button`.
- About 216 former `<mat-form-field>` usages were migrated to DS form controls across 25+ files, including `add-resource-dialog` and `resource-edit`.
- `projects` toolbar and card affordances now stay on DS primitives: search uses `app-ds-text-field`, favorites uses `app-ds-button`, sorting uses `app-ds-select`, and project meta/favorite affordances use `app-ds-chip` plus `app-ds-icon-button`. The project card layout keeps members/environment chips in a dedicated bottom footer block even when a project has no description. [2026-05-11]
- `project-detail` top tabs plus the SplitInfraCode generation shell were visually rebalanced toward a thinner enterprise language: less glow, softer header chrome, quieter accent colors, and flatter CTA grouping around generation/push actions. [2026-05-11]
- Remaining raw Material inputs are intentional for:
  - `matAutocomplete` flows in `push-to-git-dialog` and `add-project-member-dialog`
  - naming-template dialogs that require `ElementRef` cursor manipulation
  - submit buttons that rely on native `form="<id>"` wiring
- `footer`, `navigation`, `projects`, `home`, and the main detail screens already use the tokenized design language as the visual baseline for future UI work.

## Usage Constraints

- Prefer `formControlName` for DS forms. For isolated signal-based inputs, use standalone `ngModel`.
- Do not assume DS controls forward arbitrary native attributes; `app-ds-button` does not forward `form="<id>"`.
- `app-ds-panel-action-button` is the preferred control for compact panel/card header actions on mixed light/dark surfaces; do not overload `app-ds-icon-button` for toggled panel-state affordances.
- `TranslateService.instant()`-built `DsSelectOption[]` labels are not reactive on language switch; component recreation is required.
- Reactive-forms demos and showcases must keep DS controls inside a local `FormGroup` container or Angular throws runtime binding errors.
- `DsTextField` migrations should rely on CVA binding rather than ad hoc `value`/`valueChange` assumptions.

## UI Caveats

- Standalone shared components (e.g. `DockerfilePickerComponent`) embedded in light DS forms must use brand-palette colors (`#0d65c0` family) for triggers/borders, not white-on-white styling. The resource-edit form surfaces are light translucent (`rgba(255,255,255,0.84)`); white triggers become invisible. Verify trigger contrast on the actual host form before shipping a new shared icon control.

## Shared SCSS Mixins [2026-04-28]

- `@include ifs-data-table` (in `src/Front/src/scss/_tables.scss`) provides reusable flex-based data table styling with `.ifs-table__header`, `.ifs-table__row`, `.ifs-table__col`, `.ifs-table__muted`, `.ifs-table__mono`. Used in settings PAT table. Consumer adds local `*-col--*` flex rules.
- `DsTextFieldComponent` now supports `type="date"` and a `min` input for date constraints.
- `DsDatePickerComponent` provides a fully custom calendar date picker (CDK overlay, brand gradient header, 42-day grid, min/max constraints, locale-aware formatting via `Intl.DateTimeFormat`, CVA support). Used in PAT creation dialog.
- `DsDatePickerComponent` now includes a fast year-selection mode: clicking the header label switches to a 12-year grid with previous/next range navigation, selected/current year highlighting, and disabled years when fully outside `min`/`max`.
- `DsDatePickerComponent` now also exposes direct year stepping inside the day-view header, next to the current year chip, and that navigation is disabled whenever the target month would fall outside `min`/`max`. The PAT creation dialog binds dynamic bounds from `today` to `today + 1 year` through the shared `min`/`max` API instead of hardcoded template dates.
- The day-view header of `DsDatePickerComponent` now renders as two aligned rows on the same three-column grid: top row for year previous/current/next, second row for month previous/current/next. This removes the offset between month and year arrows and keeps both navigations visually stacked.
- `DsDatePickerComponent` now has a third `months` view: clicking the month label opens a 12-month grid for the visible year, with impossible months disabled from `min` / `max`. Year navigation no longer hard-blocks when only the current month is invalid in the target year; it clamps to the nearest allowed month in that year (for example May -> next year under an April max lands on April). Internal hints/ARIA labels now live under `COMMON.DATE_PICKER.*`, and all Intl formatting uses the app language from `LanguageService` rather than `navigator.language`.

## DS Integration Rule [2026-04-28]

- **Mandatory**: Any new screen/dialog/form MUST use existing `app-ds-*` components. If a UI pattern has no DS component yet, create a reusable one in `shared/components/ds/` BEFORE using it.
- This rule is enforced in `copilot-instructions.md` (pitfall #12) and `angular-patterns/SKILL.md`.