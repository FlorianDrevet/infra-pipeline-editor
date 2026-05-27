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

- **Query params `?tab=` après migration `mat-tab-group` → `app-ds-tabs`** : extraire des constantes typées `*_TAB_IDS` (cf `CONFIG_DETAIL_TAB_IDS`, `PROJECT_DETAIL_TAB_IDS` dans `shared/enums/detail-route-tabs.ts`) avec mappers id↔query. Indispensable pour préserver les deep-links.
- **`app-ds-tabs` sur pages de détail** : quand les onglets doivent occuper toute la largeur de la barre, utiliser `[stretch]="true"`. Ne pas styliser uniquement le composant `app-ds-tabs` si le contenu doit partager le même panneau visuel ; envelopper tabs + contenu dans un wrapper commun (`.config-tabs`, `.project-tabs`, `.resource-tabs`) pour éviter une barre bleue isolée au-dessus d'un contenu flottant.
- **DsTabs label pré-traduit** : DsTabsComponent reçoit le label en string brute (pas de pipe `translate`). Pré-traduire dans `computed()` lisant `languageService.currentLanguage()` pour la réactivité i18n.
- **`<app-ds-button>` n'accepte pas `(click)`** : toujours `(clicked)`. Piège récurrent.
- **API `icon`/`iconPosition` sur ds-button** : ne JAMAIS projeter `<mat-icon>X</mat-icon> Label` dans le slot ; utiliser `<app-ds-button icon="X" iconPosition="start">Label</app-ds-button>`.
- **DsIconButton variant `danger`** : déjà existant (W1 extension annulée). Tone alias `'neutral' | 'primary' | 'accent' | 'danger'`.
- **Segmented control à la place de mat-button-toggle-group** : utilisé 5 fois en migration (password scope, sensitive mode, deployment mode, ACR auth mode, app-config-key mode). Pattern à réutiliser.
- **N2 `ds-dialog-shell` SKIP** : ROI faible, l'override global de `mat-dialog-content/actions` dans `styles.scss` suffit en pratique.
- **N7 `ds-accordion` SKIP** : single-usage (DNS tutorial), `mat-expansion-panel` conservé.
- **W2 deferred** : `tag-input` créé mais 5 cibles audit étaient en réalité des inputs key-value (env vars, naming tokens) ou des action-chips. Créer primitive `ds-key-value-input` séparé avant re-rollout.

### SCSS hardening

- **Tokens fantômes purgés** : `var(--text-primary|--text-secondary|--text-tertiary|--border|--surface|--surface-alt|--code-bg|--primary|--error|--success|--ds-color-primary|--ds-color-warn)` tous remplacés par `var(--ifs-*)`. Notamment `networking-tab.scss` refonte intégrale (174 lignes) qui rendait silencieusement le composant en light hardcodé sur shell dark.
- **Hex hardcodés purgés** : 10 fichiers (settings, home, project-members, custom-domains, pipeline-options, generation-board, layout-repositories, split-generation-switcher, create-pat-dialog, networking-tab). Tokens utilisés : `--ifs-text-on-brand`, `--ifs-warning`/`-bg`, `--ifs-success`/`-bg`, `--ifs-brand-400/500`, `--ifs-accent-400/500`, `--ifs-danger`.
- **Résidu intentionnel** : palette `--bicep-syntax-*` dans `settings.component.scss` (duplicate de `bicep-file-panel`). Dette P3 : extraire en partial `@use 'shared/bicep-syntax-palette'`.
- **Overrides `::ng-deep .mat-mdc-tab-*` / `--mat-tab-*`** : tous supprimés (grep final 0 hit dans `features/**/*.scss`).

### Métriques finales

- **0 occurrence** `<mat-button|mat-stroked|mat-flat|mat-raised|mat-icon-button|mat-tab-group|mat-card|mat-card-*|mat-checkbox|mat-radio-group|mat-button-toggle|mat-progress-bar|mat-menu>` dans `src/Front/src/app/features/**/*.html` et `src/Front/src/app/shared/components/**/*.html` (hors zones intentionnelles).
- **2 résidus légitimes** : `<mat-spinner>` interne à `ds-autocomplete.component.html` (wrapper interne au DS).
- **0 token fantôme** non préfixé `--ifs-*` dans `src/Front/src/app/**/*.scss`.
- `npm run typecheck` : vert (0 erreur).
- `npm run build` : vert (warnings préexistants bundle budget + OpenTelemetry CommonJS, non liés).

### Dette résiduelle (tracée dans `.github/test-debt.md`)

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

## UI Refresh 2026-05 — Vagues complètes [2026-05-11]

- **DS autocomplete rollout [2026-05-18]** : `shared/components/ds/ds-autocomplete/` is now the standard search/autocomplete primitive for frontend dialogs that previously exposed raw `MatAutocomplete` panels. The component keeps the DS text-field shell, injects a branded loading state plus a compact empty state inside the suggestion panel, and is the only remaining place where `MatAutocompleteModule` is allowed in `src/Front/src/**`. The former raw usages in `add-project-member-dialog`, `push-to-git-dialog`, and `multi-repo-push-dialog` were migrated to `app-ds-autocomplete`; no feature template should render `<mat-autocomplete>` directly anymore.
- **Dockerfile picker branch selector [2026-05-20]** : `DockerfilePickerComponent` sits inside its own `cdkConnectedOverlay`; do not embed `app-ds-select` there for branch selection because it creates a nested CDK select overlay/backdrop that regresses the picker UX. Reuse `app-ds-autocomplete` for repository branch selection, as in Git push dialogs, and keep file rows on DS text tokens (`var(--ifs-text-primary)`) rather than hardcoded dark blues.
- **DS autocomplete anchor/layout guard [2026-05-18]** : `app-ds-autocomplete` must anchor the Material trigger on a full-width input, not on the reduced text box between prefix/suffix affordances. Keep the prefix icon, spinner, and clear button absolutely positioned over the control shell, otherwise the suggestion panel starts after the icon and renders narrower than the field. Do not force `panelWidth="auto"` on the underlying `mat-autocomplete`; let Material size the overlay from the trigger, then keep the DS panel at `width/min-width: 100%`. The loading/empty-state row should also use vertical centering (`align-items: center`) so the icon shell and copy stay visually aligned, and the copy column must stay non-growing (`flex: none`) so the icon+label group remains visually centered.

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
- **Config-detail resource action semantics (2026-05-16)** : dans `config-detail`, la liste dense des ressources doit garder des variantes d'action lisibles à droite sans changer le markup. Réutiliser les classes existantes `.add-rg-btn`, `.add-resource-btn`, `.add-child-resource-btn`, `.resource-action-btn--edit`, `.resource-action-btn--delete`, et `.delete-btn`, avec des tons sémantiques locaux pilotés par tokens : ajout en bleu accent plus clair, édition en bleu brand plus profond, suppression en rouge clairement destructif. Garder ces traitements sobres, tokenisés, et limités au scope `config-detail` plutôt que d'introduire un nouveau pattern DS global.
- **Naming abbreviations icon alignment (2026-05-16)** : dans les tables d'abréviations de `config-detail` et `project-detail`, la colonne `abbreviation-list__col--type` doit être un row flex aligné au centre avec `min-width: 0`, et le libellé doit vivre dans un wrapper `.abbreviation-row__label` séparé. Sans ce duo, le `mat-icon` reste calé sur la baseline du texte au lieu d'être centré verticalement, et l'ellipsis du nom peut se perdre.
- **Project-detail agent pool settings (2026-05-16)** : la carte `Agent Pool` de `project-detail` doit utiliser `app-ds-toggle` avec `labelPosition="before"` pour garder le libellé à gauche et le switch à droite, et l'action `Enregistrer` doit être un `app-ds-button` rendu uniquement quand la valeur effective diffère réellement de `project.agentPoolName`. Garder la normalisation `trim`/`null`/whitespace dans `project-detail-agent-pool.helper.ts` pour éviter les faux états dirty et faire disparaître le bouton immédiatement après une sauvegarde réussie. Le cycle OFF puis ON du toggle ne doit jamais effacer le draft `agentPoolName` tant que l'utilisateur n'a ni enregistré ni rechargé le projet ; la transition de toggle se centralise donc dans un helper pur qui préserve la valeur locale non sauvegardée.
- **DS toggle off/on polish (2026-05-16)** : `app-ds-toggle` doit garder un thumb clairement visible à l'état OFF, plus clair que le track (`color-mix` blanc/surface sombre + ring interne fin + ombre courte), sinon le contrôle se lit comme une simple pilule. À l'état ON, le thumb peut se teinter légèrement avec l'accent plutôt que rester neutre, avec une transition sobre de glissement + couleur + ombre et un micro `scale(1.03)` ; conserver `prefers-reduced-motion` pour neutraliser ces transitions si demandé.
- **Project-detail config cards (2026-05-11/16)** : le header de `config-card` doit rester sur une seule ligne (`align-items: center`, `justify-content: space-between`) et le `h3` doit explicitement resetter ses marges par défaut. Sans ça, le nom reste au-dessus tandis que les actions passent sur une seconde ligne malgré la carte en layout horizontal. La flèche de navigation doit rester collée au libellé dans un cluster `config-card__title`, tandis que l'action destructive reste le dernier élément de la ligne, tout à droite.
- **Pipeline variable groups table grid (2026-05-16)** : les tables de groupes de variables de `project-detail` et `config-detail` exposent 5 colonnes logiques (`variable`, `app setting`, `resource`, `resource type`, `config`). Le SCSS doit déclarer une vraie grille 5 colonnes avec minima explicites et `overflow-x: auto`; ne jamais retomber à une grille 3 colonnes, sinon les deux dernières cellules passent automatiquement sur une seconde ligne et le tableau devient illisible.
- **Resource-edit app settings inset (2026-05-16)** : `sections/app-settings/resource-edit-app-settings-section.component.*` doit utiliser un wrapper interne paddé pour que le tip, la rangée d'actions et les sections de variables ne lisent plus comme collés au bord du panneau. Les deux actions du haut doivent être des `app-ds-button` (`primary` pour l'ajout, `secondary` pour l'import), et le libellé d'import doit rester générique multi-format (`Importer depuis un fichier` / `Import from file`) au lieu de mentionner JSON.
- **Sonar PR 327 frontend cleanup (2026-05-11)** : `app-ds-button` doit utiliser son output `clicked` dans les templates DS-aware au lieu d'accrocher un `(click)` sur l'élément custom. `ds-table` et `ds-tree-view` ne doivent plus simuler des rôles ARIA `table`/`tree` sur des `div`/composants custom ; préférer des sémantiques natives simples avec support clavier explicite (`tabindex`, `Enter`, `Space`). `DsCardComponent`, `DsChipComponent` et `DsIconButtonComponent` doivent typer leurs inputs avec des alias `*Input*` non dépréciés pour garder les alias publics historiques comme surface de compatibilité seulement.
- **Build vague 6** : typecheck OK, `ng build` OK (initial bundle 953.40 kB inchangé, resource-edit chunk 520 kB inchangé). Aucune nouvelle référence à `linear-gradient`/`backdrop-filter`/`translateY(-`/`transition: all` introduite (sauf shimmer skeleton whitelisté).

## Récap waves UI Refresh 2026-05

- Vague 1 — tokens dark-only, Inter Variable self-host, _typography/tailwind/styles.scss alignés, suppression gradient app.
- Vague 2 — primitives DS V2 : button, card, text-field, textarea, select, chip, alert, icon-button, toggle, checkbox, radio-group.
- Vague 3 — shell V3 : sidebar fixe 240px, top-bar 48px, footer status-bar 28px ; ds-tabs + ds-segmented-control.
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
- CVA form controls: `app-ds-text-field`, `app-ds-textarea`, `app-ds-autocomplete`, `app-ds-select`, `app-ds-toggle`, `app-ds-checkbox`, `app-ds-radio-group`.
- Support controls: `app-ds-chip`, `app-ds-icon-button`, `app-ds-panel-action-button`, `app-ds-date-picker`.
- `DsSelectComponent` uses `cdkConnectedOverlay` so dropdowns escape scrollable/tabbed containers instead of creating nested scrollbars.
- `DsSelectComponent` panel must explicitly use `width: 100%` and `min-width: 100%` so the rendered dropdown matches the trigger width instead of collapsing to its intrinsic menu width. [2026-05-11]
- Scrollable picker dialogs with dense option grids, especially `add-resource-dialog`, should keep `scrollbar-gutter: stable` and extra right-side padding on the scroll container so the scrollbar sits in its own lane instead of reading as glued to the last column of cards. [2026-05-16]
- Full-row button rows inside overlays/dialogs (for example the topbar search popup) must not combine `width: 100%` with horizontal padding in the default content-box model. Keep the reset styles in scoped SCSS and use `box-sizing: border-box`, otherwise the row width overshoots the container and creates a fake horizontal scrollbar. [2026-05-16]
- `DsToggleComponent` exposes `ariaLabel` for icon-only or label-less usages and is reused by `ToggleSectionCardComponent`.
- `app-ds-toggle` is now the only production toggle primitive allowed in Angular screens and dialogs. The last raw `mat-slide-toggle` usages were retired from the project environment dialog, the config naming inheritance banner, and the project agent-pool settings on 2026-05-16; any new toggle must start from `app-ds-toggle`, and any future `MatSlideToggleModule` import in feature code should be treated as migration drift.
- `DsButtonComponent` keeps a shared enterprise baseline: `primary` and `success` stay slightly layered for hierarchy, but avoid large glow, aggressive hover lift, or flashy hero gradients because the component fans out across dialogs, wizards, and detail screens. [2026-05-11]
- `DsButtonComponent` icon+label alignment depends on its dedicated `icon` / `iconPosition` API. In dialogs and action rows, do not project a raw `<mat-icon>` inside the button label content when the intent is a standard leading/trailing button icon, otherwise the projected icon bypasses the DS button's aligned layout. [2026-05-11]
- `DsSectionHeaderComponent` and `DsPageHeaderComponent` must keep a dedicated `__title-row` that contains the icon and the title on the same flex row, with the subtitle rendered below that row. This keeps header icons visually centered against the title line instead of drifting when a subtitle is present. [2026-05-16]
- `DsPanelActionButtonComponent` targets compact panel-header actions with `tone` (`neutral | accent | danger`), `surface` (`light | dark`), optional `pressed`/`ariaExpanded`, and a premium glassy soft-square visual. It currently powers the generation-panel collapse/close cluster in `config-detail` and `project-detail`.
- `project-detail` SplitInfraCode generation now deliberately overrides that default glassy feel: the outer tabs and `SplitGenerationSwitcherComponent` use a restrained slate/ink surface, smaller quieter counters, thin underlines, and command-bar style CTA rows so the screen reads as enterprise product UI rather than neon/glass hero chrome. [2026-05-11]
- `BicepFilePanelComponent` now renders generated artifacts as a workspace/editor surface instead of a faux terminal: calm dark chrome, file-oriented headers, restrained badges, and readable code highlighting replace prompt/cursor metaphors and neon terminal accents. [2026-05-11]

## Global Material Override

- `src/Front/src/styles.scss` restyles the remaining Material surfaces globally: form fields, tabs, dialogs, menus, snack bars, buttons, checkbox/toggle, tooltips, selection, and related MDC shells.
- Dialog footers that expose the common two-action pattern `button[mat-stroked-button]` then `app-ds-button` now inherit a shared global `mat-dialog-actions` layout from `src/Front/src/styles.scss`: equal-width secondary/primary actions, `0.875rem` gap on desktop, and stacked full-width actions under `640px`. The selector is intentionally narrow (`:has()` + direct-child pair) so dialogs with extra tertiary actions or wrapper nodes stay untouched. [2026-05-16]
- Keep raw Material where the DS layer still depends on framework behaviors that are not reimplemented yet.

## Migration Status [2026-05-27]

- **0** `mat-expansion-panel` remaining — replaced by `app-ds-accordion`.
- **0** `mat-chip-set` in feature code except 2 naming-template dialogs (cursor-placement UIs, excluded by design).
- **0** Material buttons/icons-buttons in feature templates (only DS wrappers internally in `ds-autocomplete`).
- Shared SCSS partial `_bicep-syntax-palette.scss` extracts Bicep syntax tokens used by `settings.component.scss` and `bicep-file-panel.component.scss`.
- All legacy dead-classes purged from 5 SCSS files (custom-domains, app-settings, pipeline-options, add-app-config-key-dialog, add-app-setting-dialog).
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
- The `/settings` PAT creation success state in `features/settings/create-pat-dialog` should keep the one-time token reveal visually split between content and actions: the key marker sits on the same row as the token value inside the reveal card and should stay vertically centered against it, while the footer actions place the copy CTA on the left and the final confirmation CTA on the right. Keep the copy action on a regular-size `app-ds-button` using variant `subtle` until the token is copied, then switch to `success`; avoid `secondary` on that light token-reveal surface because the transparent dark-theme treatment reads as disabled or washed out.

## Shared SCSS Mixins [2026-04-28]

- `@include ifs-data-table` (in `src/Front/src/scss/_tables.scss`) remains available for legacy flex-table surfaces with `.ifs-table__header`, `.ifs-table__row`, `.ifs-table__col`, `.ifs-table__muted`, `.ifs-table__mono`, but new dense tabular UIs should prefer `app-ds-table`. The `/settings` PAT list is now the reference migration: DS grid columns + typed cell templates + horizontal overflow wrapper instead of hand-rolled `ifs-table` markup.
- The `/settings` PAT list is also the reference for compact admin-action tables: use `app-ds-table` with `density="compact"`, widen long date-time columns enough for full localized labels, allow header wrapping instead of clipping when FR copy is longer, and keep destructive row actions visually secondary through page-local sizing instead of changing the shared DS button scale.
- In that same PAT table, compact rows still need a small body-cell vertical inset (`padding-block`) so the local `Révoquer` button does not visually kiss the horizontal separators. Keep that spacing local to the PAT table instead of loosening the shared DS table density globally.
- The `/settings` Bicep-theme chooser is also a DS-aligned reference for theme previews: keep the sample snippet local to the page, but reuse the same syntax-color CSS variables as `shared/components/bicep-file-panel/` so the chooser matches the real generated-file viewer.
- `DsTextFieldComponent` now supports `type="date"` and a `min` input for date constraints.
- `DsDatePickerComponent` provides a fully custom calendar date picker (CDK overlay, brand gradient header, 42-day grid, min/max constraints, locale-aware formatting via `Intl.DateTimeFormat`, CVA support). Used in PAT creation dialog.
- `DsDatePickerComponent` now includes a fast year-selection mode: clicking the header label switches to a 12-year grid with previous/next range navigation, selected/current year highlighting, and disabled years when fully outside `min`/`max`.
- `DsDatePickerComponent` now also exposes direct year stepping inside the day-view header, next to the current year chip, and that navigation is disabled whenever the target month would fall outside `min`/`max`. The PAT creation dialog binds dynamic bounds from `today` to `today + 1 year` through the shared `min`/`max` API instead of hardcoded template dates.
- The day-view header of `DsDatePickerComponent` now renders as two aligned rows on the same three-column grid: top row for year previous/current/next, second row for month previous/current/next. This removes the offset between month and year arrows and keeps both navigations visually stacked.
- `DsDatePickerComponent` now has a third `months` view: clicking the month label opens a 12-month grid for the visible year, with impossible months disabled from `min` / `max`. Year navigation no longer hard-blocks when only the current month is invalid in the target year; it clamps to the nearest allowed month in that year (for example May -> next year under an April max lands on April). Internal hints/ARIA labels now live under `COMMON.DATE_PICKER.*`, and all Intl formatting uses the app language from `LanguageService` rather than `navigator.language`.

## DS Integration Rule [2026-04-28]

- **Mandatory**: Any new screen/dialog/form MUST use existing `app-ds-*` components. If a UI pattern has no DS component yet, create a reusable one in `shared/components/ds/` BEFORE using it.
- This rule is enforced in `copilot-instructions.md` (pitfall #12) and `angular-patterns/SKILL.md`.