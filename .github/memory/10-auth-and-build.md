# Authentication & Authorization

- **Provider:** Azure AD (Entra ID) JWT Bearer
- **Config section:** `"AzureAd"` in appsettings
- **Fallback policy:** Authenticated users only (`RequireAuthenticatedUser`)
- **Admin policy:** `"IsAdmin"` policy
- **Current user:** `ICurrentUser` → `CurrentUser` service

## Build & Run Commands

```powershell
# Build full solution
dotnet build .\InfraFlowSculptor.slnx

# Run full stack (Aspire)
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj

# Frontend (from src/Front)
npm install; npm run start; npm run build; npm run typecheck
```

## Windows PowerShell Notes

- Audit PowerShell scripts under `scripts/` must read their `.sh` source files with `Get-Content -Encoding UTF8` to avoid mojibake such as `SÃ©vÃ©ritÃ©` when launched from `cmd` via `powershell.exe` 5.1.
- Keep console progress messages ASCII-only when possible; `cmd` cannot have its parent code page changed reliably from inside the script.
- Bootstrap Azure DevOps generation must emit `powershell` steps, not `script`/Bash syntax. Self-hosted Windows agents execute `script` via `cmd.exe`, which breaks constructs like `|| true`, backslash continuations, `$(...)`, and `if [ ... ]`. Bootstrap runtime auth relies on `$(System.AccessToken)` exposed to scripts, not on a PAT inside the generated YAML.

## Infrastructure Services

- **GitHub Git Provider**: uses Refit (`IGitHubTreeApi`) instead of raw `HttpClient` for GitHub API calls [2026-04-16]. Registered in `Infrastructure/DependencyInjection.cs`.
- **Azure DevOps integration boundary [2026-04-22]**: `AzureDevOpsGitProviderService` currently covers Git repository operations only (test connection, list branches, push files). The codebase does **not** yet provision Azure DevOps pipelines or variable groups via REST. For future production-grade Azure DevOps automation, prefer Microsoft Entra OAuth for interactive web apps and service principals / managed identities for background services; reserve PATs for personal or ad hoc scripts.
- **DNS Name Availability**: `DnsNameAvailabilityChecker` [2026-04-22] — resolves `{name}.{azureSuffix}` via DNS, no Azure auth required. Replaced previous ARM-based `AzureNameAvailabilityChecker`.
- **Diagnostic Rules**: `IDiagnosticRule.EvaluateAsync()` [2026-04-22] — async interface with `Task.WhenAll` parallelization. Rules: `AcrPullDiagnosticRule`, `KeyVaultAccessDiagnosticRule`, `NameAvailabilityDiagnosticRule`.

## Sonar Quality Rules
- **S1192** — Duplicate strings in migrations (accepted)
- **new_duplicated_lines_density** — Quality gate threshold **3%**
