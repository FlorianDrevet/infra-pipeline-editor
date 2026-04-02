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

## PITFALL — Creation Modal sync [2026-04-02]
When a resource's parameters are moved between general config and per-environment config (or removed), the `add-resource-dialog` MUST be updated in 3 places:
1. `createEnvFormGroup(type)` — add/remove form controls
2. `buildXxxEnvironmentSettings()` — add/remove fields in the mapper
3. HTML `@case (ResourceTypeEnum.Xxx)` in the environments step — add/remove form fields
Failing to update all 3 causes phantom fields shown in the creation modal that don't match the actual resource schema.
