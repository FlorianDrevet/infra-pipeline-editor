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

## Shared Components
- `DeploymentConfigComponent` [2026-04-02] — extracted container/code deployment mode toggle + ACR selector + UAI flow
- `ConfirmDialogComponent` — reusable confirm dialog with i18n
- `EditAbbreviationDialogComponent` [2026-04-22] — reusable abbreviation override editor (config-detail + project-detail naming tabs)
- `ToggleSectionCardComponent` [2026-04-22] — generic reusable toggle card (icon + title + subtitle + slide toggle + content projection). Visual identity: blue accent border, rounded card, shadow, fade animation. Used for health probe config in ContainerApp (add-resource-dialog + resource-edit). Reusable for any section needing enable/disable toggle with collapsible content.

## Name Availability UX [2026-04-22]
- `NameAvailabilityService.check$()` + debounced 500 ms `switchMap` on `name` field in `resource-edit` and `add-resource-dialog`.
- 10 resource types supported via `NAME_AVAILABILITY_TYPES` Set (ContainerRegistry, StorageAccount, KeyVault, RedisCache, AppConfiguration, ServiceBusNamespace, EventHubNamespace, WebApp, FunctionApp, SqlServer).
- Save/submit blocking (`isSaveBlockedByNameAvailability`) with bypass override button ("It's my resource"). `"current"` status shows blue check icon for already-deployed names.
- `add-resource-dialog`: inline results panel with per-env status icons, submit/next blocked when names unavailable.

## Existing Resource Diagnostics [2026-04-22]
- `config-detail` and `project-detail` generation preflight checks must skip existing resources when building “missing environment configuration” warnings.
- These guards depend on `resource.isExisting` from `/resource-group/{id}/resources`; if the backend mapping omits that flag, both the warning badge in lists and the generation verification dialog become incorrect.

## SqlServer Resource Edit [2026-04-22]
- Full CRUD support in `resource-edit.component`: version/administratorLogin general fields, per-env TLS dropdown.

## Project-Detail Abbreviation Filter [2026-04-22]
- Abbreviation list shows only resource types actually used in the project (`usedResourceTypes` from backend `GetDistinctResourceTypesByProjectIdAsync`).

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
