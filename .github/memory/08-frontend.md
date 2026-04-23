# Frontend (Angular)

## Stack & Conventions
- Angular 19 standalone, zoneless (`provideExperimentalZonelessChangeDetection`)
- Signals for state (`signal`, `computed`, `toSignal`), `inject()` for DI
- 3 separate files per component (`.ts`, `.html`, `.scss`)
- New control flow syntax (`@if`, `@for`, `@switch`)
- Material + Tailwind, Axios HTTP client

## Structure
- `core/` — layout/shell (navigation, footer)
- `shared/` — services, facades, guards, enums, interfaces
- `features/` — feature pages (lazy-loaded)
- `environments/` — API base URLs

## ACA Containerization [2026-04-23]
- `src/Front/Dockerfile` builds the Angular app with `node:20-alpine`, injects the production `API_URL` into `src/environments/environment.ts` at image build time, then serves the compiled SPA from `nginx:1.27-alpine`.
- `src/Front/nginx.conf` listens on port `8080` and uses `try_files $uri $uri/ /index.html` for Angular client-side routing.
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
- The favicon follows the login page visual DNA: deep blue to cyan gradient + four-tile infra grid motif.

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

## Custom Domains UX [2026-04-23]
- `resource-edit` now manages custom domains inside each environment panel instead of through a dedicated tab.
- `customDomainsForEnv(envName)` replaced grouped state, and the add-domain dialog preselects the active environment.
- The page includes an inline 5-step Azure DNS tutorial inside a `mat-expansion-panel` under the custom-domain section.

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

## SqlServer Resource Edit [2026-04-22]
- Full CRUD support in `resource-edit.component`: version/administratorLogin general fields, per-env TLS dropdown.

## Project-Detail Abbreviation Filter [2026-04-22]
- Abbreviation list shows only resource types actually used in the project (`usedResourceTypes` from backend `GetDistinctResourceTypesByProjectIdAsync`).

## Mono-repo Git Push UX [2026-04-22]
- `project-detail` now exposes a single mono-repo push CTA instead of separate Bicep/Pipeline/Bootstrap push buttons.
- The CTA is enabled only when all three project-level generation results exist.
- `PushToGitDialogComponent` supports `isCombinedProjectPush` and now calls the dedicated backend endpoint `POST /projects/{projectId}/push-generated-artifacts-to-git`.
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
