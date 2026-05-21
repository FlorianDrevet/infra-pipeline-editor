# Authentication & Authorization

- **Provider:** Azure AD (Entra ID) JWT Bearer.
- **Config section:** `AzureAd` in appsettings.
- **Fallback policy:** authenticated users only (`RequireAuthenticatedUser`).
- **Admin policy:** `IsAdmin`.
- **Current user:** `ICurrentUser` → `CurrentUser`.

## PAT Authentication (MCP) [2026-04-28]

- **Scheme:** `PersonalAccessToken` with custom `AuthenticationHandler`.
- **Token format:** `ifs_` prefix + 32 random bytes base64url encoded; SHA-256 hash stored in DB.
- **Flow:** HTTP `Authorization: Bearer ifs_...` → hash lookup → revoked/expiry checks → `HttpContext.Items["ProvisionedUserId"]` → transparent `ICurrentUser` resolution.
- **API endpoints:** `GET /personal-access-tokens`, `POST /personal-access-tokens`, `DELETE /personal-access-tokens/{id}`.
- **Workspace entrypoint:** `.vscode/mcp.json` uses the HTTP MCP server at `http://127.0.0.1:5258/mcp` with a PAT bearer header.
- **Defaults:** `McpOptions` resolve to `http://127.0.0.1:5258` + `/mcp`; override via `Mcp:ListenUrl`, `MCP__LISTENURL`, and `Mcp:Route`.
- **Primary doc:** `docs/architecture/mcp-integration.md`.
- **Usage persistence throttling [2026-05-13]:** `PersonalAccessTokenAuthenticationHandler` no longer persists `LastUsedAt` on every authenticated request. It writes only when the elapsed interval exceeds `PersonalAccessTokenAuthenticationDefaults.UsagePersistenceInterval`, which is the current write-amplification guard for PAT auth.
- **Scopes model [2026-05-17]:** `PersonalAccessToken` now owns a `Scopes` collection persisted in `PersonalAccessTokenScopes`, with `Read`, `Write`, and `Generate` values; token creation defaults to `Read` when the caller omits scopes.
- **Enforcement [2026-05-20]:** `PersonalAccessTokenAuthenticationHandler` now emits one `ifs_pat_scope` claim per granted scope, `CurrentUser.HasPersonalAccessTokenScopeAsync(...)` resolves those claims, and `PersonalAccessTokenScopeBehavior` enforces scopes centrally across MediatR requests: `IQuery<T>` requires `Read` (with `Write` also satisfying read), `ICommand<T>` requires `Write`, and `IGenerateCommand<T>` requires `Generate`.

## API User Provisioning [2026-05-13]

- `UserProvisioningMiddleware` no longer depends directly on `ProjectDbContext`; it now calls `IUserProvisioningService` from Application and still stores the resolved `UserId` in `HttpContext.Items["ProvisionedUserId"]` for `ICurrentUser`.
- `UserProvisioningService` is implemented in Infrastructure and uses a PostgreSQL upsert (`ON CONFLICT ("EntraId") DO NOTHING`) on the `User` table to avoid the old check-then-insert race.
- This slice is the current reference for removing API-layer persistence coupling without changing the downstream `CurrentUser` contract.

## Build & Run Commands

```powershell
dotnet build .\InfraFlowSculptor.slnx
dotnet test .\InfraFlowSculptor.slnx
aspire run
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj
```

- Frontend from `src/Front`: `npm install; npm run start; npm run build; npm run typecheck`.
- On Windows, stop running `InfraFlowSculptor.Api`, `InfraFlowSculptor.Mcp`, `InfraFlowSculptor.AppHost`, and stale PowerShell reflection/debug shells before rebuilding, or MSBuild can fail on locked `bin\Debug\net10.0` assemblies such as `InfraFlowSculptor.GenerationCore.dll`.
- `src/Api/InfraFlowSculptor.Api/Dockerfile` must build from a repo-root context (`WORKDIR /repo`) and copy `.editorconfig`; otherwise central props outside `src/Api` and local warning-severity overrides are missing inside containerized API/PR builds [2026-05-21].
- `*.csproj.lscache` files are local language-service artifacts and must stay ignored at the repo level; they should never be committed in PRs because they massively inflate diffs without affecting runtime or tests.

## Tests

- Active test projects under `tests/`: `Api`, `Application`, `BicepGeneration`, `Contracts`, `Domain`, `GenerationCore`, `Infrastructure`, `Mcp`, and `PipelineGeneration`.
- `tests/InfraFlowSculptor.GenerationParity.Tests/` is only a placeholder folder; do not put regular unit tests there.
- Shared coverage collection is now enabled for all test projects via `tests/Directory.Build.props` + `coverlet.collector`; use `dotnet test .\InfraFlowSculptor.slnx --collect:"XPlat Code Coverage"` or `.\scripts\test-coverage.ps1`.
- `tests/InfraFlowSculptor.Contracts.Tests/Responses/ContractsResponseShapeSnapshotTests.*` is the approval gate for public Contracts response DTO shape. When adding, removing, or splitting response types, update the verified snapshot in strict alphabetical order and keep the `Count` value aligned with the approved list.

## GitHub to Azure DevOps Mirror [2026-05-16]

- `.github/workflows/mirror-to-azure-devops.yml` mirrors all GitHub branches and tags into Azure DevOps on `push`, `create`, `delete`, and manual `workflow_dispatch`, guarded by a dedicated concurrency group plus repository settings `AZURE_DEVOPS_MIRROR_URL` and `AZURE_DEVOPS_MIRROR_PAT` (`Code (Read & Write)`).
- The workflow force-updates rewritten refs, prunes deleted refs, enumerates source branches from `git ls-remote --heads origin`, and aborts on any native Git branch-push failure before prune can run.

## MCP Runtime Hardening [2026-05-13]

- `src/Mcp/InfraFlowSculptor.Mcp/Program.cs` now reuses API rate limiting through `AddRateLimiting()` and the shared security headers middleware through `UseMcpHttpPipeline()`.
- The MCP HTTP pipeline applies security headers, `UseHsts()` outside Development, `UseRateLimiter()`, and PAT auth/authorization in the same ordering constraints as the API.
- `MapMcp(mcpOptions.Route)` now requires both authorization and the `RateLimitingPolicyNames.Expensive` policy.
- MCP health endpoints are now mapped through `MapMcpHealthChecks()` and both `/health` and `/alive` require the dedicated `RateLimitingPolicyNames.HealthChecks` policy while remaining anonymous.
- Source-controlled MCP rate-limiting defaults live in `src/Mcp/InfraFlowSculptor.Mcp/appsettings.json`.
- `ProjectDraftService` now enforces `ProjectDraftStorageOptions.MaxDraftCount` and the tool layer returns a structured limit error instead of allowing unbounded in-memory draft growth.
- `UseMcpHttpPipeline()` logs a warning when `McpOptions.ListenUrl` uses plain HTTP outside Development; keep that guard on the shared pipeline rather than duplicating it in `Program.cs`.

## API Runtime Hardening [2026-04-23]

- Security headers: `X-Frame-Options=DENY`, `X-Content-Type-Options=nosniff`, `Referrer-Policy=strict-origin-when-cross-origin`, restrictive `Permissions-Policy`, and `UseHsts()` outside Development.
- Rate limiting binds typed options from `RateLimiting`, applies a global fixed-window limiter, keeps an `Expensive` policy for heavy generation/download/push routes, partitions authenticated traffic by stable user claims before remote IP, and emits `Retry-After` on `429`.
- API health endpoints now have their own `HealthChecks` rate-limiting policy with a dedicated typed options bucket; keep health throttling separate from the broader `Expensive` generation routes.
- Focused coverage lives in `tests/InfraFlowSculptor.Api.Tests/RateLimiting/RateLimitingTests.cs`.
- `Program.cs` now binds request-body limits through `AddApiRequestLimits(builder.Configuration)`; default max body size is `52_428_800` bytes (50 MB).
- Outside Development, `InfraFlowSculptor.Api` now adds Azure Key Vault as an extra configuration source when `KeyVault:VaultUri` is set, using `DefaultAzureCredential`; keep this additive and optional for local/dev hosts.
- Output caching is opt-in: the base policy is `NoCache()`, and the `ShortLived` policy is `5s` with `Authorization` vary-by-header. Current reference usage is selective Project GET endpoints, not blanket caching for every GET route.

## API Security Perimeter [2026-05-12]

- CORS now binds through `AddApiCors(builder.Configuration)` and `ApiCorsOptions`; fallback origin remains `http://localhost:4200` with credentials enabled.
- Shared response headers now also add `Cross-Origin-Opener-Policy=same-origin`, `Cross-Origin-Resource-Policy=same-site`, and a route-aware CSP.
- Default CSP stays strict for API and OpenAPI JSON responses; `/scalar` gets the narrower relaxed CSP required for the embedded Development UI.
- Contract-layer tag limits are centralized in `TagRequestConstraints` (`512` / `256` / `15`) and generated-file endpoints must validate `/{*filePath}` through `SafeRelativePath.TryNormalize(...)`.

## Handler Authorization Coverage [2026-05-12]

- Generation/download handlers now enforce the same access services as the rest of the app: `GeneratePipeline` uses `VerifyWriteAccessAsync(...)`, `DownloadPipeline` uses `VerifyReadAccessAsync(...)`, and Bicep handlers were already protected.
- `CreateProjectCommandHandler` and `CreateProjectWithSetupCommandHandler` keep self-service creation but now translate missing current-user resolution into `Error.Unauthorized(...)` instead of leaking `500`.
- Current branch status: APP-002 is effectively closed without adding an extra admin-only gate.

## Package Vulnerability Note [2026-05-12]

- `Directory.Packages.props` pins `Microsoft.AspNetCore.DataProtection` to `10.0.7`.
- `InfraFlowSculptor.Infrastructure` keeps an explicit `PackageReference` so restore no longer falls back to the vulnerable `10.0.0` transitive path.

## Project Pipeline Layout

- Project-level pipeline generation returns repo-relative `.azuredevops/...` paths for both combined and split infra/code outputs.
- Blob storage still uses `pipeline/project/{id}/{ts}/{infra|app}/...`, but file-content handlers resolve both displayed repo-relative paths and bucketed blob paths.
- Mono-repo infra wrappers reference `../Common/...`; split app wrappers reference `../../../Common/...`.
- Bootstrap pipeline definitions follow the same repo layout under `.azuredevops/{config}` and `.azuredevops/{config}/apps/{appName}`.

## App Pipeline Rules

- Shared YAML templates live under `.azuredevops/{pipelines,jobs,steps}/`; per-resource wrappers live under `apps/{appName}/...`.
- Azure DevOps resolves `template:` relative to the template file, not the wrapper; keep helper path generation aligned with `.azuredevops/pipelines/`.
- CI/release split remains build-once then promote.
- `AppPipelineRequestFactory` must propagate `ContainerApp.SourceCodePath` into `AppPipelineGenerationRequest.SourceCodePath`; otherwise generated Container App CI/PR/release wrappers silently fall back to `buildContext: '.'` even when the resource stores a custom build context.
- CI/PR app-wrapper trigger paths must also honor `AppPipelineGenerationRequest.SourceCodePath`: `AppTriggerPathHelper` normalizes the configured relative source path for YAML path filters and falls back to the legacy `{config}/{resource}` folder only when no source path is provided.
- `Create*AppRequest` / `Update*AppRequest` DTOs for Container Apps, Web Apps, and Function Apps now reject unsafe `DockerfilePath` / `SourceCodePath` values via `SafeRelativePathValidation`; the API file-reading helper delegates to `Contracts.Common.SafeRelativePath` so request validation and generated-artifact endpoints share the same traversal guard.
- App pipeline generation now emits CI, PR, and release wrappers for every generated app (`ci.app-pipeline.yml`, `pr.app-pipeline.yml`, `release.app-pipeline.yml`). Bootstrap app definitions must provision all three; app PR templates validate code or Docker builds without ACR login/push.
- Container delivery uses immutable tags and optional Trivy/Syft scans.
- Shared app step templates now emit Windows-compatible `powershell` steps and AzureCLI `scriptType: ps` for inline scripts (`app-compute-release-tag`, `app-acr-login`, `app-docker-buildx-push`, `app-docker-buildx-validate`, `app-trivy-scan`, `app-syft-sbom`, `app-load-metadata`, `app-build-code`, `app-acr-promote`, `app-deploy-container`). Trivy and Syft install their Windows zip assets directly from GitHub releases instead of piping shell installers through Bash.
- In shared app step templates, the body of every `powershell: |` literal block must remain indented under the pipe token. `DockerBuildxPushStep`, `DockerBuildxValidateStep`, `SyftSbomStep`, and the Node/Python branches in `BuildCodeStep` are the current reference fixes for Azure DevOps YAML parse failures such as `While scanning a simple key, could not find expected ':'`.
- Container App ACR Docker service connections are per-environment settings on `ContainerAppEnvironmentSettings.ContainerRegistryServiceConnection`, not Common variables or `AppPipelineStepOptions`. The generated app CI wrapper passes `containerRegistryServiceConnection` explicitly to the shared pipeline/job/ACR-login templates, and the shared templates keep `$(containerRegistryServiceConnection)` as the compatibility fallback.
- App pipeline templates that use Azure DevOps `extends:` must not place `pool:` at the root; put the pool on the generated stage/job level.
- Do not embed Azure DevOps compile-time directives such as `${{ if }}` inside multiline script strings. Use YAML-level directives for complete nodes or runtime script conditionals.
- In `SplitInfraCode`, the app/code blob bucket must include any Common variable files referenced by app wrappers/templates, because the target code repo receives only the app bucket.
- `PipelineGenerationEngine` must map only known validation-style `InvalidOperationException` prefixes to `ErrorOr` validation errors (currently the variable-group-name guard). Unexpected `InvalidOperationException` instances must bubble so handlers/global error handling treat them as internal failures instead of `Generation.InvalidInfrastructurePipelineConfiguration` user errors.

## Windows PowerShell & Bootstrap ADO Notes

- Generated YAML must use `powershell` steps, not Bash or `pwsh`, because self-hosted Windows agents may not have `pwsh.exe`.
- App pipeline shared-template stability is now guarded by `AppPipelineWindowsShellCompatibilityTests`, which must stay green whenever a step template introduces or changes inline script execution.
- `AppPipelineWindowsShellCompatibilityTests` also guards literal-block indentation for the generated PowerShell shared steps; update it alongside any future multiline script edits in the app templates.
- Bootstrap generation now injects a preflight PowerShell job that validates required ARM and ACR service connections before provisioning resources [2026-05-20].
- Bootstrap auth uses `$(System.AccessToken)`; do not bake PATs into YAML, and do not pass `--detect false` to `az devops configure`.
- Decode `%20`-style URL segments before feeding org/project/repo names to Azure DevOps CLI defaults.
- Pipeline creation on Windows PowerShell 5.1 must temporarily relax `$ErrorActionPreference`, capture `$LASTEXITCODE`, and use `--only-show-errors` around `az pipelines create`.
- Azure DevOps pipeline display names are centralized in `InfraFlowSculptor.PipelineGeneration.AzureDevOpsPipelineNameHelper`; it sanitizes config/resource segments and applies `[Infra]` to infrastructure CI/PR/release definitions and `[Code]` to application CI/release definitions.
- Bootstrap provisioning and downstream release YAML must share that helper contract. Do not rename only the bootstrap definitions in `ProjectBootstrapDefinitionBuilder`, because infra/app release pipelines resolve artifacts through matching `resources.pipelines[].source` names.
- Variable groups should be created with a temporary `PLACEHOLDER=bootstrap`, then cleaned once real variables exist.
- Generated variable-group names must go through `PipelineVariableGroupNameHelper`; only `{env}` is a supported placeholder and emitted `group:` values must stay single-quoted.
- Environment creation uses `shortName` for the technical Azure DevOps environment identifier and `name` only for display.
- Build Service permissions still need manual setup on pipeline folders, environments, libraries, and any referenced agent pool.
- App pipeline file paths returned by generators are relative to the app folder only; `AppPipelineGenerationEngine.GenerateAll()` owns the `apps/{appName}/...` prefix.

## Infrastructure Services

- ACR role assignments in ARM/Bicep require `Owner` or `User Access Administrator`; `Contributor` is insufficient.
- GitHub Git provider uses Refit (`IGitHubTreeApi`).
- GitHub Create Tree request/response payloads are strongly typed under `Infrastructure/Services/GitProviders/Models`; preserve the tests that assert create items omit `sha` and delete items serialize `sha: null`.
- GitHub/Azure DevOps push-provider request payloads should stay strongly typed model records, not anonymous `object` graphs or weak dictionaries; this is part of the repository-wide weak-object cleanup [2026-05-19].
- `KeyVaultSecretClient.SetSecretAsync(...)` and `GetSecretAsync(...)` now both log Azure Key Vault failures for PAT storage/retrieval. The frontend repository-PAT flows must treat the returned `GitRepository.SecretStorageFailed` / `GitRepository.SecretRetrievalFailed` codes as technical diagnostics: keep the detail in backend logs, but show only generic localized UI messages.
- Azure DevOps Git support covers Git operations only; pipeline/library security provisioning remains a manual prerequisite around the generated bootstrap YAML.
- `AppPipelineGenerationEngine` normalizes redundant `apps/{appName}/{resourceName}/...` paths down to `apps/{appName}/...`.
- Diagnostics rely on `IDiagnosticRule.EvaluateAsync()`; current rules cover ACR Pull, Key Vault access, and DNS name availability.

## Sonar Notes

- Accepted rule exceptions remain duplicate strings in migrations (`S1192`) and `new_duplicated_lines_density` `3%`; keep `sonar.cpd.exclusions` aligned with `tmp/**` and `**/tmp/**` so scratch PowerShell output stays out of duplication metrics.
- SonarCloud PR issue counts can lag local fixes; on PR `#395`, generator `S1192` findings persisted remotely until the next analysis even after local constant cleanup, so always re-check the actual branch contents before reopening the same slice.
- Current reference fixes from the recent Sonar waves are regex timeouts, ZIP extraction guards, pinned GitHub Actions SHAs, tightened Docker inputs, `AppPipelineStepOptionsData` replacing the 21-argument `Update(...)` mutator, and `VirtualNetworkAggregate.Entities.Subnet` as the regex-timeout example for hotspot `S6444`.
