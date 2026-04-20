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

## Infrastructure Services

- **GitHub Git Provider**: uses Refit (`IGitHubTreeApi`) instead of raw `HttpClient` for GitHub API calls [2026-04-16]. Registered in `Infrastructure/DependencyInjection.cs`.

## Sonar Quality Rules
- **S1192** — Duplicate strings in migrations (accepted)
- **new_duplicated_lines_density** — Quality gate threshold **3%**
