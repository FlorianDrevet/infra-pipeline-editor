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
- On Windows, stop running `InfraFlowSculptor.Api`, `InfraFlowSculptor.Mcp`, or `InfraFlowSculptor.AppHost` processes before rebuilding or MSBuild can fail with locked `bin\Debug\net10.0` assemblies.
- Stale PowerShell shells can also lock `InfraFlowSculptor.GenerationCore.dll` after reflection/debug commands and leave `BicepGeneration` building against stale metadata.
- `*.csproj.lscache` files are local language-service artifacts and must stay ignored at the repo level; they should never be committed in PRs because they massively inflate diffs without affecting runtime or tests.

## Tests

- Active test projects under `tests/`: `Api`, `Application`, `BicepGeneration`, `Contracts`, `Domain`, `GenerationCore`, `Infrastructure`, `Mcp`, and `PipelineGeneration`.
- `tests/InfraFlowSculptor.GenerationParity.Tests/` is only a placeholder folder; do not put regular unit tests there.
- Shared coverage collection is now enabled for all test projects via `tests/Directory.Build.props` + `coverlet.collector`; use `dotnet test .\InfraFlowSculptor.slnx --collect:"XPlat Code Coverage"` or `.\scripts\test-coverage.ps1`.
- `tests/InfraFlowSculptor.Contracts.Tests/Responses/ContractsResponseShapeSnapshotTests.*` is the approval gate for public Contracts response DTO shape. When adding, removing, or splitting response types, update the verified snapshot in strict alphabetical order and keep the `Count` value aligned with the approved list.
- `tmp/test-output-mcp/` is not ignored by the root `.gitignore`; generated MCP artefacts there can pollute branch diffs.

## MCP Runtime Hardening [2026-05-13]

- `src/Mcp/InfraFlowSculptor.Mcp/Program.cs` now reuses API rate limiting through `AddRateLimiting()` and the shared security headers middleware through `UseMcpHttpPipeline()`.
- The MCP HTTP pipeline applies security headers, `UseHsts()` outside Development, `UseRateLimiter()`, and PAT auth/authorization in the same ordering constraints as the API.
- `MapMcp(mcpOptions.Route)` now requires both authorization and the `RateLimitingPolicyNames.Expensive` policy.
- Source-controlled MCP rate-limiting defaults live in `src/Mcp/InfraFlowSculptor.Mcp/appsettings.json`.
- `ProjectDraftService` now enforces `ProjectDraftStorageOptions.MaxDraftCount` and the tool layer returns a structured limit error instead of allowing unbounded in-memory draft growth.
- `UseMcpHttpPipeline()` logs a warning when `McpOptions.ListenUrl` uses plain HTTP outside Development; keep that guard on the shared pipeline rather than duplicating it in `Program.cs`.

## API Runtime Hardening [2026-04-23]

- Security headers: `X-Frame-Options=DENY`, `X-Content-Type-Options=nosniff`, `Referrer-Policy=strict-origin-when-cross-origin`, restrictive `Permissions-Policy`, and `UseHsts()` outside Development.
- Rate limiting binds typed options from `RateLimiting`, applies a global fixed-window limiter, keeps an `Expensive` policy for heavy generation/download/push routes, partitions authenticated traffic by stable user claims before remote IP, and emits `Retry-After` on `429`.
- API health endpoints now have their own `HealthChecks` rate-limiting policy with a dedicated typed options bucket; keep health throttling separate from the broader `Expensive` generation routes.
- Focused coverage lives in `tests/InfraFlowSculptor.Api.Tests/RateLimiting/RateLimitingTests.cs`.
- `Program.cs` now binds request-body limits through `AddApiRequestLimits(builder.Configuration)`; default max body size is `52_428_800` bytes (50 MB).

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
- Container delivery uses immutable tags and optional Trivy/Syft scans.
- `AppPipelineBuilderCommon` still contains removable dead inline YAML helpers; it is cleanup-only debt.
- `PipelineGenerationEngine` must map only known validation-style `InvalidOperationException` prefixes to `ErrorOr` validation errors (currently the variable-group-name guard). Unexpected `InvalidOperationException` instances must bubble so handlers/global error handling treat them as internal failures instead of `Generation.InvalidInfrastructurePipelineConfiguration` user errors.

## Windows PowerShell & Bootstrap ADO Notes

- Generated YAML must use `powershell` steps, not Bash or `pwsh`, because self-hosted Windows agents may not have `pwsh.exe`.
- Bootstrap auth uses `$(System.AccessToken)`; do not bake PATs into YAML, and do not pass `--detect false` to `az devops configure`.
- Decode `%20`-style URL segments before feeding org/project/repo names to Azure DevOps CLI defaults.
- Pipeline creation on Windows PowerShell 5.1 must temporarily relax `$ErrorActionPreference`, capture `$LASTEXITCODE`, and use `--only-show-errors` around `az pipelines create`.
- Pipeline display names must use `PathSanitizer.Sanitize(configName)`; release YAML resolves CI artifacts from that sanitized name.
- Variable groups should be created with a temporary `PLACEHOLDER=bootstrap`, then cleaned once real variables exist.
- Generated variable-group names must go through `PipelineVariableGroupNameHelper`; only `{env}` is a supported placeholder and emitted `group:` values must stay single-quoted.
- Environment creation uses `shortName` for the technical Azure DevOps environment identifier and `name` only for display.
- Build Service permissions still need manual setup on pipeline folders, environments, libraries, and any referenced agent pool.
- App pipeline file paths returned by generators are relative to the app folder only; `AppPipelineGenerationEngine.GenerateAll()` owns the `apps/{appName}/...` prefix.

## Infrastructure Services

- ACR role assignments in ARM/Bicep require `Owner` or `User Access Administrator`; `Contributor` is insufficient.
- GitHub Git provider uses Refit (`IGitHubTreeApi`).
- Azure DevOps Git support covers Git operations only; pipeline/library security provisioning remains a manual prerequisite around the generated bootstrap YAML.
- `AppPipelineGenerationEngine` normalizes redundant `apps/{appName}/{resourceName}/...` paths down to `apps/{appName}/...`.
- Diagnostics rely on `IDiagnosticRule.EvaluateAsync()`; current rules cover ACR Pull, Key Vault access, and DNS name availability.

## Sonar Notes

- Accepted rule exceptions: duplicate strings in migrations (`S1192`) and a `new_duplicated_lines_density` quality-gate threshold of `3%`.
- SonarCloud PR issue counts can lag behind the current workspace state; after a local fix wave, re-check the exact file contents before chasing the same finding again. On PR `#395`, the last `S1192` generator findings persisted remotely until the PR analysis reran, even though the raw literals were already reduced to single constant definitions locally.
- Keep `sonar.cpd.exclusions` explicitly aligned with `tmp/**` / `**/tmp/**`; the broader `sonar.exclusions` entry alone did not reliably prevent scratch PowerShell remediation scripts under `tmp/` from surfacing in Sonar duplication metrics.
- The 2026-04-28 remediation wave also standardized regex timeouts, hardened ZIP extraction guards, pinned GitHub Actions SHAs, and tightened Docker frontend build inputs.
- On PR `#395`, the last open `S107` on `AppPipelineStepOptions.Update(...)` was closed by introducing a dedicated `AppPipelineStepOptionsData` shape in Domain and a single Application-side mapper from `PipelineStepOptionsDto`, instead of keeping a 21-argument mutator signature.
- `VirtualNetworkAggregate.Entities.Subnet` now configures an explicit timeout on its compiled service-endpoint regex; this is the reference fix for Sonar hotspot `S6444` in Domain regex validators.
