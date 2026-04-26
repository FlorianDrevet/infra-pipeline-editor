# Frontend (Angular)

## Stack & Conventions
- Angular **21** standalone, zoneless via `provideZonelessChangeDetection`
- Signals for state (`signal`, `computed`, `toSignal`), `inject()` for DI, and separate `.ts` / `.html` / `.scss` files per component
- New control flow syntax (`@if`, `@for`, `@switch`), Material 21 + Tailwind, Axios HTTP client
- TypeScript `lib: es2022`; typed Reactive Forms remain the default
- No SSR: the app is authenticated and browser/MSAL-heavy

## Structure
- `core/` — layout/shell (navigation, footer)
- `shared/` — services, facades, guards, enums, interfaces, design-system components
- `features/` — feature pages (lazy-loaded)
- `environments/` — API base URLs

## Containerization & Build
- `src/Front/Dockerfile` builds with `node:22-alpine`, injects `API_URL` into `environment.ts`, uses a BuildKit cache on `/root/.npm`, and serves the production build from `nginx:1.29-alpine` with a `/` healthcheck
- `src/Front/nginx.conf` listens on `8080`, gzips JS/CSS/JSON/SVG/XML, serves hashed assets with 1-year `immutable` cache, and falls back to `index.html` for Angular routing
- `src/Front/.dockerignore` excludes `node_modules`, `dist`, and `.angular`
- Build budgets: `anyComponentStyle` warning/error = `10 kB` / `20 kB`; `initial` warning/error = `500 kB` / `1 MB`
- Browser tab icons use versioned assets in `src/index.html`: `public/ifs-favicon.svg`, `public/ifs-favicon.png`, plus regenerated `public/favicon.ico`

## i18n (FR/EN)
- `@ngx-translate/core` + `@ngx-translate/http-loader` v17 with dictionaries in `public/i18n/fr.json` and `public/i18n/en.json`
- `LanguageService` is signal-based with localStorage persistence and fallback order `persisted -> navigator.language -> fr`
- `resource-edit` dialog keys stay under `RESOURCE_EDIT.*`; missing nested keys render raw labels
- `DeploymentConfigComponent` resolves ACR labels through `RESOURCE_EDIT.FIELDS.*`; missing `ACR_AUTH_MODE*` keys in one locale break the shared ACR UI
- Multi-repo project screens consume `PROJECT_DETAIL.LAYOUT.*`; `GenerationBoardComponent` reads labels from `PROJECT_DETAIL.BOARD.*`, not `CONFIG_DETAIL.BOARD.*`
- The PowerShell source-vs-dictionary scan still reports `_`-suffixed dynamic prefixes such as `HOME.RECENT.TYPE_`; those are not true missing leaves

## Auth & Frontend Services
- `@azure/msal-browser@^5` only; no `@azure/msal-angular`
- `MsalAuthService` lazy-inits `PublicClientApplication`, uses `loginRedirect()`, and sets the active account explicitly after `handleRedirectPromise()`
- `AxiosService` now routes both backend `401` responses and missing-token preflight failures through a single guarded redirect helper to `/login`; it avoids duplicate redirects and transient component-level error banners
- API services are `providedIn: 'root'` wrappers over `AxiosService.request$<T>()`

## Visual & Design System Baseline [2026-04-24]
- Signature look: app background `linear-gradient(135deg, #1a237e 0%, #0288d1 50%, #00bcd4 100%)`, glassy cards (`rgba(255,255,255,0.08)` + blur), and cyan CTA gradients
- SCSS tokens and mixins live under `src/Front/src/scss/`; `angular.json` exposes `src` as a style include path and Tailwind extends the same `ifs-*` palette, radii, shadows, and gradients
- The DS surface includes page/layout primitives (`app-ds-button`, `app-ds-card`, `app-ds-alert`, shared headers) plus CVA-based form controls (`app-ds-text-field`, `app-ds-textarea`, `app-ds-select`, `app-ds-toggle`, `app-ds-checkbox`, `app-ds-radio-group`, `app-ds-chip`, `app-ds-icon-button`)
- `src/Front/src/styles.scss` applies a global Material override so untouched Material components still inherit the project brand
- Most buttons, toggles, and form fields were migrated during the Angular 21 / DS waves; remaining raw `mat-form-field` cases are intentional where DS controls do not cover `matAutocomplete`, detached submit buttons, or `viewChild`-driven input editing
- `DsSelectComponent` renders via `cdkConnectedOverlay`; `DsToggleComponent` exposes `ariaLabel` and is the standard toggle used by `ToggleSectionCardComponent`

## Config Detail Git Tab Visibility [2026-04-25]
- In `config-detail`, the `Git` tab is visible only when `project.layoutPreset === 'MultiRepo'`.
- Config-level `Generate` / `Push to Git` actions are also visible only for true `MultiRepo` projects; `AllInOne` and `SplitInfraCode` reserve those actions to `project-detail`.
- The `Git` tab now mirrors the project-level layout/repository UX for configuration sub-modes: the chooser uses the same preset-card visual pattern as `project-detail`, and the repository bindings render with the same `repo-card` / `slot-empty` card language.
- Changing a configuration sub-mode (`AllInOne` / `SplitInfraCode`) updates the local signal optimistically before the refetch, so the click has immediate visible feedback instead of waiting for the round-trip.

## DS Constraints & Shared Components
- `app-ds-button` does **not** forward `form="<id>"`; keep native buttons for detached submit triggers such as `add-config-dialog`
- DS form controls are CVA-first: prefer `formControlName`; for ad hoc inputs use standalone `ngModel`
- `TranslateService.instant()`-built `DsSelectOption[]` labels are not reactive on language switch; showcase controls using `formControlName` must stay under a local `[formGroup]`
- `DeploymentConfigComponent` centralizes deployment mode, ACR selection, and UAI/ACR diagnostics UX
- `ToggleSectionCardComponent` wraps premium toggleable sections and is reused by ACA health-probe configuration
- `ConfirmDialogComponent` and `EditAbbreviationDialogComponent` are shared cross-feature dialogs

## Feature UX Conventions
- **Project creation wizard [2026-04-25]:** the layout-preset step renders preset icons through standard Angular Material `<mat-icon>` instead of raw `material-symbols-outlined` spans, because the latter can visibly fall back to literal icon names in this app shell. The wizard dialog opens at `960px` with `maxWidth: 96vw`, and `.ifs-wizard-dialog` adds scoped stepper-header spacing plus wrapped labels so the 4/5-step header remains readable.
- **ACR auth mode:** `DeploymentConfigComponent` owns the shared selector (`ManagedIdentity` / `AdminCredentials`). `resource-edit` and `add-resource-dialog` persist `acrAuthMode`, clear it when the ACR is cleared, and bypass UAI diagnostics for admin-credentials mode
- **Name availability:** `NameAvailabilityService.check$()` runs with a debounced 500 ms `switchMap` in `resource-edit` and `add-resource-dialog` for 10 Azure-name-sensitive resource types; submit/save is blocked unless the user explicitly bypasses it
- **Container App environments:** capacity/scaling, ingress/network, and Health Probes are split into sections; probe cards use a shared equal-width 3-column grid on desktop and collapse responsively
- **Environment editor categorization:** 13 resource editors use `env-extra-section` groups. ContainerRegistry and EventHubNamespace remain uncategorized; this added 38 `RESOURCE_EDIT.FIELDS.*` i18n keys in both locales
- **Existing-resource diagnostics:** generation preflight checks in `config-detail` and `project-detail` must ignore `resource.isExisting` items when computing missing-environment warnings
- **Generation result panels:** no visible "Generation Results" heading remains. `config-detail` exposes its compact top-right action cluster only for true `MultiRepo` projects, while `project-detail` in `SplitInfraCode` removes the close action entirely and renders a single collapse/expand button directly in the Infra/Code split header. Any fresh Bicep, Pipeline, or Bootstrap generation auto-expands the panel again so progress/output is immediately visible
- **Storage children in config-detail:** Storage Account blob/queue/table children seed directly from `resource.storageSubResources` returned by `/resource-group/{id}/resources`; full `getById()` calls remain only for explicit refresh flows
- **Project naming abbreviations:** project-detail shows overrides only for resource types actually used in the project
- **Mono-repo push UX:** combined project push uses `POST /projects/{id}/push-generated-artifacts-to-git`; `SplitInfraCode` projects hide that mono-repo CTA, surface dedicated Infra and Code push buttons from `SplitGenerationSwitcherComponent`, and open `MultiRepoPushDialogComponent` in `infra` / `code` / `both` modes based on generated artifact readiness, reading `pipelineResult.{infra,app}{Common,Config}FileUris` plus Bicep/bootstrap artifacts. The split push dialog uses a dedicated `ifs-multi-repo-push-dialog` panel class with mode-aware widths (`38rem` for single infra/code, `68rem` for dual-repo) and a deliberately simple layout: no top hero band, a neutral dialog surface, and the repo card plus validation/cancel actions as the primary visual structure.
- **Split bootstrap UX [2026-04-25]:** `SplitGenerationSwitcherComponent` now shows the bootstrap preview under both outer tabs in `SplitInfraCode`: infra reads `bootstrapResult.infraFileUris`, code reads `bootstrapResult.appFileUris`, and both viewers keep `infra/` / `app/` prefixes in their load URI so the bootstrap file-content endpoint resolves the correct blob bucket. The code tab adds its own setup/usage explainer and uses the dedicated `TAB_BOOTSTRAP_APP` label.
- **Bootstrap ADO UX:** project-detail shows one guide card with explicit Build Service permission steps and Azure DevOps menu paths
- **Secure parameter mapping:** `resource-edit` exposes SQL Server password mapping through `SecureParameterMappingService`, with either random generation or variable-group-backed injection
- **Custom domains:** supported only for ContainerApp, WebApp, and FunctionApp, inside per-environment panels with add-dialog preselection of the current environment
- **SQL Server editor:** `resource-edit` supports version and administrator login in general settings plus per-environment TLS

## Frontend Pitfalls
- `onContainerRegistryChange` must patch `generalForm.containerRegistryId`; `onDeploymentModeChange` must trigger `checkAcrPullAccess()` when switching back to container mode with an ACR already selected; `isAcrEnabled` should stay the single source of truth
- `groupedRoleAssignments` must seed the assigned-UAI group even when it has zero role assignments, and the empty state must treat `assignedUserAssignedIdentity` as meaningful state
- When a resource parameter moves between general and environment config, update all 3 creation-modal touchpoints: `createEnvFormGroup(type)`, `buildXxxEnvironmentSettings()`, and the HTML `@case (ResourceTypeEnum.Xxx)` block in the environments step
- The generation panel collapse state is shared at the panel level, not per tab. Reset it to `false` when a new generation starts or when the panel is fully closed, otherwise a user can launch generation and keep the loading/result content hidden accidentally
- Angular warning `NG8102` means a `??` fallback is redundant on a non-nullable expression; `TS-998113` means a standalone import remains in `imports` after the template stopped using it
