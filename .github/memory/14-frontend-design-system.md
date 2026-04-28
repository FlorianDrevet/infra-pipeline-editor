# Frontend Design System

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
- Support controls: `app-ds-chip`, `app-ds-icon-button`, `app-ds-panel-action-button`.
- `DsSelectComponent` uses `cdkConnectedOverlay` so dropdowns escape scrollable/tabbed containers instead of creating nested scrollbars.
- `DsToggleComponent` exposes `ariaLabel` for icon-only or label-less usages and is reused by `ToggleSectionCardComponent`.
- `DsPanelActionButtonComponent` targets compact panel-header actions with `tone` (`neutral | accent | danger`), `surface` (`light | dark`), optional `pressed`/`ariaExpanded`, and a premium glassy soft-square visual. It currently powers the generation-panel collapse/close cluster in `config-detail` and `project-detail`.

## Global Material Override

- `src/Front/src/styles.scss` restyles the remaining Material surfaces globally: form fields, tabs, dialogs, menus, snack bars, buttons, checkbox/toggle, tooltips, selection, and related MDC shells.
- Keep raw Material where the DS layer still depends on framework behaviors that are not reimplemented yet.

## Migration Status [2026-04-24]

- Primary CTAs across `home`, `projects`, `project-detail`, `config-detail`, `resource-edit`, and shared dialogs were largely migrated to `app-ds-button`.
- About 216 former `<mat-form-field>` usages were migrated to DS form controls across 25+ files, including `add-resource-dialog` and `resource-edit`.
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

## DS Integration Rule [2026-04-28]

- **Mandatory**: Any new screen/dialog/form MUST use existing `app-ds-*` components. If a UI pattern has no DS component yet, create a reusable one in `shared/components/ds/` BEFORE using it.
- This rule is enforced in `copilot-instructions.md` (pitfall #12) and `angular-patterns/SKILL.md`.