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
- Bootstrap Azure DevOps generation must emit `powershell` steps, not `script`/Bash syntax. Self-hosted Windows agents execute `script` via `cmd.exe`, which breaks constructs like `|| true`, backslash continuations, `$(...)`, and `if [ ... ]`. Bootstrap runtime auth relies on `$(System.AccessToken)` exposed to scripts, not on a PAT inside the generated YAML. `az devops configure` itself must not receive `--detect false`.
- Bootstrap Azure DevOps must decode URL-encoded Azure DevOps URL segments (`%20`, etc.) before injecting organization/project/repository names into CLI defaults. Otherwise `az devops configure` can fail with interpolation/parsing errors on project names containing spaces. The generated YAML now also declares explicit stage/job names for clearer Azure DevOps run UI.
- Bootstrap Azure DevOps pipeline definitions should keep generated pipeline names ASCII-safe (` - ` instead of Unicode dashes) and check existence with `az pipelines list` rather than `az pipelines show`. On Windows self-hosted agents, `show` turns the normal "not found yet" case into a fatal PowerShell-native error instead of an idempotent empty result.
- Bootstrap Azure DevOps pipeline creation via `$(System.AccessToken)` also depends on Azure DevOps folder security: the project Build Service identity needs the `Create build pipeline` permission on the target path (for example `\Core`). The generated bootstrap YAML now fails fast if `az pipelines create` returns a non-zero exit code, instead of logging a false `Created pipeline` success message.

## Infrastructure Services

- **GitHub Git Provider**: uses Refit (`IGitHubTreeApi`) instead of raw `HttpClient` for GitHub API calls [2026-04-16]. Registered in `Infrastructure/DependencyInjection.cs`.
- **Azure DevOps integration boundary [2026-04-22]**: `AzureDevOpsGitProviderService` currently covers Git repository operations only (test connection, list branches, push files). The codebase does **not** yet provision Azure DevOps pipelines or variable groups via REST. For future production-grade Azure DevOps automation, prefer Microsoft Entra OAuth for interactive web apps and service principals / managed identities for background services; reserve PATs for personal or ad hoc scripts.
- **DNS Name Availability**: `DnsNameAvailabilityChecker` [2026-04-22] — resolves `{name}.{azureSuffix}` via DNS, no Azure auth required. Replaced previous ARM-based `AzureNameAvailabilityChecker`.
- **Diagnostic Rules**: `IDiagnosticRule.EvaluateAsync()` [2026-04-22] — async interface with `Task.WhenAll` parallelization. Rules: `AcrPullDiagnosticRule`, `KeyVaultAccessDiagnosticRule`, `NameAvailabilityDiagnosticRule`.

## Sonar Quality Rules
- **S1192** — Duplicate strings in migrations (accepted)
- **new_duplicated_lines_density** — Quality gate threshold **3%**
