# Frontend (Angular)

## Stack & Conventions
- Angular **21** standalone, zoneless (`provideZonelessChangeDetection` — stable API since v20; renamed from `provideExperimentalZonelessChangeDetection` during the v19→v20 migration)
- Signals for state (`signal`, `computed`, `toSignal`), `inject()` for DI
- 3 separate files per component (`.ts`, `.html`, `.scss`)
- New control flow syntax (`@if`, `@for`, `@switch`) — `CommonModule` imports auto-removed by the Angular control-flow migration in v21
- Material 21 + Tailwind, Axios HTTP client
- TypeScript `lib: es2022` (set by Angular CLI v21 migration)


## Structure
- `core/` — layout/shell (navigation, footer)
- `shared/` — services, facades, guards, enums, interfaces
- `features/` — feature pages (lazy-loaded)
- `environments/` — API base URLs

## ACA Containerization [2026-04-23, updated 2026-04-24 for Angular 21]
- `src/Front/Dockerfile` builds the Angular app with `node:22-alpine` (Angular 21 requires Node ≥20.19, bumped to LTS 22), uses BuildKit cache mount on `/root/.npm` for faster CI rebuilds, injects the production `API_URL` into `src/environments/environment.ts` at image build time, then serves the compiled SPA from `nginx:1.29-alpine`. Multi-stage build pinned to `--configuration=production`. `HEALTHCHECK` curls `/` every 30s.
- `src/Front/nginx.conf` listens on port `8080`, gzip-compresses JS/CSS/JSON/SVG/XML, serves hashed assets with 1-year `immutable` cache and uses `try_files $uri $uri/ /index.html` (with `Cache-Control: no-cache`) for Angular client-side routing.
- `src/Front/.dockerignore` excludes `node_modules`, `dist`, and `.angular` so local Windows artifacts do not leak into the Linux image build context.

## Build Budgets [2026-03-21]
- `anyComponentStyle`: warning 10 kB / error 20 kB
- `initial` bundle: warning 500 kB / error 1 MB

## i18n (FR/EN) [2026-03-21]
- `@ngx-translate/core` + `@ngx-translate/http-loader` v17
- JSON files: `public/i18n/fr.json` + `en.json`
- `LanguageService`: signal-based, localStorage persistence, fallback: persisted → navigator.language → 'fr'
- Every component imports `TranslateModule`, uses `| translate` pipe
- **PITFALL [2026-04-02]:** Dialog components under `resource-edit` use keys nested inside `RESOURCE_EDIT` — always use full path.
- **PITFALL [2026-04-23]:** `DeploymentConfigComponent` resolves all ACR labels through `RESOURCE_EDIT.FIELDS.*`; if the `RESOURCE_EDIT` namespace is missing the `ACR_AUTH_MODE*` keys in one language file, the shared ACR assignment/auth UI renders raw translation keys.
- **PITFALL [2026-04-23]:** `project-detail` multi-repo screens consume `PROJECT_DETAIL.LAYOUT.*`; if that namespace exists in `en.json` but not in `fr.json`, the entire Layout & Repositories section renders raw i18n keys in French.
- **PITFALL [2026-04-23]:** `GenerationBoardComponent` resolves its labels from `PROJECT_DETAIL.BOARD.*`, not `CONFIG_DETAIL.BOARD.*`; putting the texts under the wrong namespace leaves the project-detail board rendering raw keys even though the strings exist elsewhere in the file.
- **VALIDATION [2026-04-23]:** The PowerShell source-vs-dictionary i18n scan will still report `_`-suffixed pseudo-keys such as `HOME.RECENT.TYPE_`, `PROJECT_DETAIL.MEMBERS.ROLE_`, and `CONFIG_DETAIL.RESOURCES.FORM.CREATE_PLAN_*_`; these are dynamic prefixes concatenated at runtime, not real missing translation leaves.

## Auth (MSAL) [2026-03-17]
- `@azure/msal-browser@^5` (no `@azure/msal-angular`)
- `MsalAuthService`: lazy-init `PublicClientApplication`, `loginRedirect()`, deterministic account selection
- Auth loop fix [2026-03-21]: explicit active account from `handleRedirectPromise()`

## API Services
All `providedIn: 'root'`, use `AxiosService.request$<T>()`. Key services: `InfraConfigService`, `ResourceGroupService`, `KeyVaultService`, `RedisCacheService`, `StorageAccountService`, `RoleAssignmentService`, `BicepGeneratorService`, `ProjectService`, `ContainerRegistryService`.

## Visual Baseline (validated 2026-03-21)
```scss
background: linear-gradient(135deg, #1a237e 0%, #0288d1 50%, #00bcd4 100%);
// Cards: rgba(255,255,255,0.08) + blur(10px)
// CTA: linear-gradient(135deg, #0288d1, #00bcd4)
```

## Branding Assets [2026-04-22]
- Browser tab icon now uses versioned assets in `src/index.html` to break Chrome favicon cache: `public/ifs-favicon.svg` + `public/ifs-favicon.png`, with `public/favicon.ico` regenerated as legacy fallback.
## Design System V2 [2026-04-24]
- **8 new form components** under `src/Front/src/app/shared/components/ds/` (all `ControlValueAccessor`, compatible with `formControlName` / `ngModel` / two-way `[(value)]`):
  - `DsTextFieldComponent` (`app-ds-text-field`) — native `<input>` (no mat-form-field), brand-blue label, cyan focus ring, prefix/suffix mat-icon, hint/error, clearable, types `text`/`email`/`password`/`number`/`tel`/`url`.
  - `DsTextareaComponent` (`app-ds-textarea`) — autoResize via `effect()` + `viewChild` on textarea, optional `maxLength` with character counter.
  - `DsSelectComponent` (`app-ds-select`) — custom dropdown (NOT `mat-select`), `DsSelectOption { value, label, icon?, disabled?, description? }`, searchable filter, click-outside + Escape close (HostListener), clearable, animated chevron.
  - `DsToggleComponent` (`app-ds-toggle`) — iOS-style switch, brand-gradient track when checked, label + description, labelPosition before/after.
  - `DsCheckboxComponent` (`app-ds-checkbox`) — square brand-blue when checked, indeterminate state.
  - `DsRadioGroupComponent` (`app-ds-radio-group`) — `DsRadioOption { value, label, description?, disabled? }`, vertical/horizontal layout.
  - `DsChipComponent` (`app-ds-chip`) — variants `neutral`/`primary`/`success`/`warning`/`error`/`cyan`, sizes `sm`/`md`, optional icon, removable.
  - `DsIconButtonComponent` (`app-ds-icon-button`) — circular icon button, variants `ghost`/`subtle`/`primary`/`danger`, sizes `sm`/`md`/`lg`, loading spinner, requires `ariaLabel` for a11y.
- **Global Material Theme Override** added in `src/Front/src/styles.scss` (~210 lines). Uses `$ifs-*` tokens to restyle EVERY existing Material usage instantly without touching components: `mat-mdc-form-field` (outline brand, label brand-blue on focus, error red, container-shape 12px), `mat-mdc-select-panel` (radius md + shadow-lg + border, selected option brand gradient), `mat-mdc-checkbox` + `mat-mdc-slide-toggle` (brand-blue selected), `mat-mdc-tab-group` (cyan indicator + brand-blue active label), `mat-mdc-raised-button.mat-primary` (gradient-cta + shadow-cta + lift hover, opt-out via `.no-ifs-override`), `mat-mdc-outlined-button.mat-primary` (info-border + brand-blue label), `mat-mdc-dialog-surface` (radius 2xl + shadow-xl), `mat-mdc-snack-bar-container`, `mat-mdc-progress-spinner`, `mat-mdc-menu-panel`, `mat-mdc-tooltip`, `mat-expansion-panel`, plus `::selection { background: rgba(2,136,209,0.25) }`.
- **Migrations explicites V2** :
  - `core/layouts/footer` — SCSS migré vers tokens (`$ifs-gradient-glass-footer`, `$ifs-ink-900/500`, `$ifs-brand-blue`).
  - `core/layouts/navigation` — SCSS migré vers tokens (`$ifs-gradient-nav`, `$ifs-shadow-nav`).
  - `features/projects` — header remplacé par `<app-ds-page-header variant="gradient" icon="folder_special">`, CTAs `Create` + `Try again` + empty-state remplacés par `<app-ds-button variant="primary|ghost">`.
  - `features/home` — `quick-actions` migré vers `<app-ds-card variant="outlined" accent="primary" [interactive]="true" (cardClick)>`. Greeting-bar volontairement non touchée (layout custom).
- **Showcase étendu** (`/design-system`) avec 8 nouvelles sections : Form Inputs (avec error, disabled, clearable), Textarea (avec maxLength), Select (avec searchable + clearable), Toggle, Checkbox, Radio Group (vertical + horizontal), Chips (tous variants + removable), Icon Buttons (toutes variants × sizes + loading + disabled). Toutes les sections form sont câblées via un `FormGroup` + `formControlName` pour démontrer l'intégration Reactive Forms.

## Design System V1 [2026-04-23]
- **SCSS tokens** under `src/Front/src/scss/`:
  - `_tokens.scss` — single source of truth: brand (`$ifs-brand-*`), ink/surface palettes, semantic state colors, signature gradients (`$ifs-gradient-brand`, `$ifs-gradient-cta`, `$ifs-gradient-login`, `$ifs-gradient-nav`, `$ifs-gradient-app-bg`), borders, radius (`sm`/`md`/`lg`/`xl`/`2xl`/`3xl`/`pill`), shadows (`xs`→`xl` + `cta`/`cta-hover`/`hero`/`nav`), spacing (`0`→`16`), z-index, transitions (`$ifs-ease-out`, `$ifs-duration-fast/base/slow`), focus ring.
  - `_mixins.scss` — `ifs-gradient-brand`, `ifs-gradient-cta`, `ifs-glass($opacity, $blur)`, `ifs-card-surface($radius, $shadow)`, `ifs-hover-lift`, `ifs-form-grid($min, $row-gap, $col-gap)`, `ifs-focus-visible`, `ifs-truncate`, `ifs-scrollbar-soft`.
  - `_animations.scss` — `ifs-fade-in`, `ifs-slide-in-up/down`, `ifs-spin`, `ifs-pulse-soft`.
  - `_typography.scss` — `$ifs-text-xs`→`$ifs-text-4xl` maps + `@mixin ifs-text($scale)` + utility classes `.ifs-h1`..`.ifs-h4`, `.ifs-body`, `.ifs-body-sm`, `.ifs-caption`.
- `_main.scss` `@forward`s tokens + vars + typography + colors + mixins + animations.
- `angular.json` has `stylePreprocessorOptions.includePaths: ["src"]` — any component can do `@use "scss/main" as *;`.
- `tailwind.config.js` extends with `ifs-*` colors, radius, shadows, and gradient backgrounds (`bg-ifs-brand`, `bg-ifs-cta`, `bg-ifs-cyan`).
- **DS components** under `src/Front/src/app/shared/components/ds/` (barrel `index.ts` re-exports all 5):
  - `DsButtonComponent` (`app-ds-button`) — variants `primary`/`secondary`/`ghost`/`danger`, sizes `sm`/`md`/`lg`, `disabled`, `loading` (CSS spinner), `icon` (mat-icon), `iconPosition` `leading`/`trailing`, `type`, `fullWidth`, `clicked` output.
  - `DsCardComponent` (`app-ds-card`) — variants `elevated`/`outlined`/`glass`, `padding` `none`/`sm`/`md`/`lg`, `accent` `none`/`primary`/`success`/`warning`/`error` (left border 3px), `interactive` (cursor + hover-lift), projections `[ds-card-header]` and `[ds-card-footer]`, `cardClick` output.
  - `DsAlertComponent` (`app-ds-alert`) — `severity` `info`/`success`/`warning`/`error`, optional `title`, `dismissible` with internal `visible` signal, `dismissed` output, default mat-icons per severity.
  - `DsSectionHeaderComponent` (`app-ds-section-header`) — `icon`, `title` (required), `subtitle`, `level` 1/2/3, projection `[ds-section-action]`.
  - `DsPageHeaderComponent` (`app-ds-page-header`) — `title` (required), `subtitle`, `icon`, `variant` `gradient`/`plain`, projection `[ds-page-actions]`. Gradient variant uses `$ifs-gradient-brand` + `$ifs-shadow-hero`.

## Shared Components
- `DeploymentConfigComponent` [2026-04-02] — extracted container/code deployment mode toggle + ACR selector + UAI flow
- `ConfirmDialogComponent` — reusable confirm dialog with i18n
- `EditAbbreviationDialogComponent` [2026-04-22] — reusable abbreviation override editor (config-detail + project-detail naming tabs)
- `ToggleSectionCardComponent` [2026-04-22] — generic reusable toggle card (icon + title + subtitle + slide toggle + content projection). Visual identity: blue accent border, rounded card, shadow, fade animation. Used for health probe config in ContainerApp (add-resource-dialog + resource-edit). Reusable for any section needing enable/disable toggle with collapsible content.

## ACR Auth Mode UX [2026-04-23]
- `DeploymentConfigComponent` now owns the shared ACR auth-mode selector with `ManagedIdentity` and `AdminCredentials`, defaulting to managed identity whenever an ACR is selected without a stored mode.
- `resource-edit` and `add-resource-dialog` both persist `acrAuthMode` for `ContainerApp`, `WebApp`, and `FunctionApp` payloads and clear it when the selected ACR is cleared.
- The managed-identity path keeps the existing UAI/AcrPull diagnostics UX; the admin-credentials path hides that flow and shows an informational banner about pipeline or secure-variable injection instead.
- `ContainerRegistryService.checkAcrPullAccess(...)` now forwards optional `acrAuthMode` so the backend can bypass UAI diagnostics for admin credentials.

## Name Availability UX [2026-04-22]
- `NameAvailabilityService.check$()` + debounced 500 ms `switchMap` on `name` field in `resource-edit` and `add-resource-dialog`.
- 10 resource types supported via `NAME_AVAILABILITY_TYPES` Set (ContainerRegistry, StorageAccount, KeyVault, RedisCache, AppConfiguration, ServiceBusNamespace, EventHubNamespace, WebApp, FunctionApp, SqlServer).
- Save/submit blocking (`isSaveBlockedByNameAvailability`) with bypass override button ("It's my resource"). `"current"` status shows blue check icon for already-deployed names.
- `add-resource-dialog`: inline results panel with per-env status icons, submit/next blocked when names unavailable.

## Container App Environment UX [2026-04-23]
- In `resource-edit`, the Container App environment form is no longer a flat grid: it is split into two full-width categories before Health Probes.
- `Capacity and scaling` groups CPU, memory, and replica limits; `Ingress and network exposure` groups transport, target port, and ingress toggles.
- The final visual pattern reuses the same divider-based `env-extra-section` language as Health Probes (no boxed card background), while keeping the inner controls in small responsive grids.

## Environment Editor Categorization [2026-04-23]
- ALL 13 resource types in the Environments tab now use `env-extra-section` sections with icon, title, and description.
- Single-section resources (`env-extra-section--first` only): KeyVault, RedisCache, StorageAccount, AppServicePlan, WebApp, FunctionApp, LogAnalyticsWorkspace, SqlServer.
- Two-section resources (first `--first`, second normal): AppConfiguration (Plan & retention / Access & protection), ContainerAppEnvironment (Capacity / Network & resilience), ApplicationInsights (Collection & retention / Privacy & access), CosmosDb (API & consistency / Resilience & backup), ServiceBusNamespace (Capacity & plan / Security & resilience).
- ContainerApp (3 sections) was already done. ContainerRegistry and EventHubNamespace not categorized (not in architect plan scope).
- Fields are wrapped in `form-grid env-category-grid` inside each section for consistent sub-layout.
- 38 new i18n keys added under `RESOURCE_EDIT.FIELDS.*` in both `en.json` and `fr.json`.

## Existing Resource Diagnostics [2026-04-22]
- `config-detail` and `project-detail` generation preflight checks must skip existing resources when building “missing environment configuration” warnings.
- These guards depend on `resource.isExisting` from `/resource-group/{id}/resources`; if the backend mapping omits that flag, both the warning badge in lists and the generation verification dialog become incorrect.

## Config Detail Storage Sub-Resources [2026-04-23]
- `config-detail` no longer fire-and-forgets `GET /storage-accounts/{id}` during Resource Group expansion just to populate blob/queue/table children.
- The component seeds `storageAccountDetails` directly from the optional `resource.storageSubResources` payload returned by `/resource-group/{id}/resources`, so Storage Account children are visible on first display without a second interaction.
- Manual full `StorageAccountService.getById()` calls remain only for on-demand refreshes such as edit/remove flows.

## SqlServer Resource Edit [2026-04-22]
- Full CRUD support in `resource-edit.component`: version/administratorLogin general fields, per-env TLS dropdown.

## Project-Detail Abbreviation Filter [2026-04-22]
- Abbreviation list shows only resource types actually used in the project (`usedResourceTypes` from backend `GetDistinctResourceTypesByProjectIdAsync`).

## Mono-repo Git Push UX [2026-04-22]
- `project-detail` now exposes a single mono-repo push CTA instead of separate Bicep/Pipeline/Bootstrap push buttons.
- `PushToGitDialogComponent` supports `isCombinedProjectPush` and now calls the dedicated backend endpoint `POST /projects/{projectId}/push-generated-artifacts-to-git`.
- For projects with `LayoutPreset === 'SplitInfraCode'`, `GenerationBoardComponent.onGenerateAll()` opens `MultiRepoPushDialogComponent` instead (dual push: 2 cards Infra/Code with branch+commit forms, states `form|pushing|success|partial|error`, calls `POST /projects/{id}/push-multi-repo-artifacts-to-git`, HTTP always 200 — inspect `results[i].success`). Aliases auto-resolved from `project.repositories` filtered by `contentKinds.includes('Infrastructure'|'ApplicationCode')`. localStorage key `ifs-push-branch-multi-{projectId}-{alias}`.
- For `SplitInfraCode` projects, the project-detail generation tabs (`mat-tab-group.generation-tabs`) are replaced by `SplitGenerationSwitcherComponent` (outer 2 tabs Infra/Code with file-count chips, inner Bicep/Pipeline/Bootstrap for Infra and Pipeline-only for Code — Bootstrap stays infra-only). Reads new `pipelineResult.{infra,app}{Common,Config}FileUris` fields from `GenerateProjectPipelineResponse`.
- The mono-repo combined push now produces a single Git commit for Bicep + Pipeline + Bootstrap together; the frontend no longer orchestrates three separate project push calls.

## Bootstrap ADO Onboarding UX [2026-04-22]
- In `project-detail`, the Bootstrap ADO tab displays a single unified guide card with 10 numbered steps organized in 4 sections: A — Prepare (steps 1-3), B — Run (steps 4-5), C — Permissions (steps 6-8), D — Finalize (steps 9-10).
- The guide appears below the generation terminal/file preview (not split around it).
- Permission steps explicitly name the Build Service identity: `{Project Name} Build Service ({Organization Name})` and explain the exact Azure DevOps menu paths (Pipelines > Pipelines > ⋯ > Manage security, Pipelines > Library > Security).
- Steps use color-coded number badges: blue for normal, amber for permissions, green for finalization.
- Orphan `CONFIG_DETAIL.BOOTSTRAP` keys were removed from fr.json (they were leftovers from the first implementation).

## Secure Parameter Mapping UX [2026-04-23]
- `resource-edit` loads secure parameter mappings through `SecureParameterMappingService` and currently exposes SQL Server password mapping for `administratorLoginPassword`.
- Password mode is either auto-generated (`random`) or variable-group-backed (`variableGroup`).
- In variable-group mode, the UI can reuse an existing project pipeline variable group or create a new one inline before saving the mapping.

## Custom Domains UX [2026-04-23]
- `resource-edit` supports custom domains for ContainerApp, WebApp, and FunctionApp only.
- The feature moved out of a dedicated tab: custom domains now live inside each environment panel, and the add dialog preselects the current environment.
- The UI groups domains per environment via `customDomainsForEnv(envName)` and includes a step-by-step Azure DNS tutorial in the editor flow.

## PITFALL — ACR reactivity [2026-04-03]
- `onContainerRegistryChange` must patch `generalForm.containerRegistryId` so `DeploymentConfigComponent` gets updated input.
- `onDeploymentModeChange` must trigger `checkAcrPullAccess()` when switching to Container mode with ACR already selected.
- `isAcrEnabled` should be a single computed source of truth for ACR state.

## PITFALL — Assigned UAI in role assignments list [2026-04-03]
`groupedRoleAssignments` must seed an assigned-UAI group first (even with 0 RAs). Empty-state condition must account for `assignedUserAssignedIdentity` being set. Related i18n keys: `UAI_NO_ASSIGNMENTS`, `ASSIGN_UAI_NO_SAI`.

## PITFALL — Creation Modal sync [2026-04-02]
When a resource's parameters are moved between general config and per-environment config (or removed), the `add-resource-dialog` MUST be updated in 3 places:
1. `createEnvFormGroup(type)` — add/remove form controls
2. `buildXxxEnvironmentSettings()` — add/remove fields in the mapper
3. HTML `@case (ResourceTypeEnum.Xxx)` in the environments step — add/remove form fields
Failing to update all 3 causes phantom fields shown in the creation modal that don't match the actual resource schema.
## PITFALL — Angular compiler warnings [2026-04-24]
- `NG8102` is emitted when a template uses `??` on an expression whose type is already non-nullable. Example: `resourceTypeIcons[item.resourceType] ?? 'category'` is invalid when `RESOURCE_TYPE_ICONS` is typed as a total map for the enum.
- `TS-998113` is emitted when a standalone component remains listed in a component's `imports` array but no longer appears in the template. After replacing a template fragment, remove the unused standalone import as well or Angular will keep warning during `ng serve`/`ng build`.

## PITFALL — Design System showcase forms [2026-04-24]
- In `features/design-system`, every demo control using `formControlName` must remain under a local `formGroup`/`[formGroup]` container. Leaving one of the DS form components (`app-ds-textarea`, `app-ds-select`, `app-ds-toggle`, `app-ds-checkbox`, `app-ds-radio-group`) outside the `showcaseForm` context causes a runtime Reactive Forms error and the page can render with placeholder white blocks instead of fully bound content.
