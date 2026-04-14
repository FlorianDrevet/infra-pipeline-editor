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

## Sonar Quality Rules
- **S1192** — Duplicate strings in migrations (accepted)
- **new_duplicated_lines_density** — Quality gate threshold **3%**
