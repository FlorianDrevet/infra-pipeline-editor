# Aspire Integration

## Decoupling [2026-04-16]
- The API project (`InfraFlowSculptor.Api`) has **no Aspire NuGet packages** and **no ServiceDefaults reference**.
- DbContext is registered via standard `AddDbContext<ProjectDbContext>` with `UseNpgsql` in `Infrastructure.DependencyInjection.AddPersistence()`.
- OTel is configured in `Infrastructure.DependencyInjection.AddObservability()` (conditionally exports via OTLP if `OTEL_EXPORTER_OTLP_ENDPOINT` is set).
- Health checks (`/health`, `/alive`) are registered in `Infrastructure.DependencyInjection.AddDefaultHealthChecks()` and mapped in `Program.cs`.
- Connection string key is `infraDb` (matches Aspire resource name → Aspire injects `ConnectionStrings__infraDb` as env var; standalone reads from `appsettings.json`).
- The API can be deployed standalone (`dotnet run`) without the Aspire AppHost.

## AppHost Wiring
- PostgreSQL (`postgres` container, image `postgres:17.6`), DbGate, main API, Angular frontend
- Main API: `infraflowsculptor-api`, frontend: `angular-frontend`
- MCP HTTP service: `infraflowsculptor-mcp` on fixed port `5258`
- Azure Blob Storage emulator for generation output

## MCP Under Aspire [2026-04-28]
- `src/Mcp/InfraFlowSculptor.Mcp` is now an ASP.NET Core MCP host exposed over HTTP at `/mcp`; the previous workspace-local `stdio` mode was removed.
- `src/Aspire/InfraFlowSculptor.AppHost/AppHost.cs` starts the MCP service alongside the API, Postgres, blob storage, keyvault emulator, and frontend.
- The MCP host uses the same `AddApplication()` + `AddInfrastructure(...)` stack as the API, but calls `AddInfrastructure(..., includeAuthentication: false)` so shared infrastructure registration does not force Microsoft.Identity.Web config into the MCP process.
- `AddInfrastructure(...)` now also registers MCP OpenTelemetry sources/meters (`Experimental.ModelContextProtocol`, `ModelContextProtocol`, `ModelContextProtocol.Core`) so Aspire can surface MCP calls as traces beyond a generic `POST /mcp` request.
- Workspace clients now connect through `.vscode/mcp.json` with `type: "http"` and `url: "http://127.0.0.1:5258/mcp"`.

## Frontend in Aspire [2026-03-18]
```csharp
builder.AddJavaScriptApp("angular-frontend", "../../Front", "start:aspire")
    .WithNpm()
    .WithReference(infraApi)
    .WaitFor(infraApi)
    .WithHttpEndpoint(targetPort: 4200, env: "NG_PORT")
    .WithExternalHttpEndpoints();
```

## Proxy Configuration
`src/Front/proxy.conf.js` reads Aspire env vars:
- `/api-proxy/*` → API backend
- `/otlp/*` → Aspire Dashboard OTLP

## Angular Configurations
| Config | Env file | Usage |
|---|---|---|
| `development` | `environment.development.ts` | standalone dev |
| `aspire` | `environment.aspire.ts` | via Aspire |
| `production` | `environment.ts` | production build |

## OpenTelemetry [2026-03-18]
- `TelemetryService` via `APP_INITIALIZER`, only if `environment.otlpEnabled === true`
- API OTel v1.x: `resourceFromAttributes()`, `ATTR_SERVICE_NAME`

## PostgreSQL Reset Procedure [2026-03-30]
1. `docker ps` → find container ID
2. `docker exec <id> sh -lc "printenv POSTGRES_PASSWORD"`
3. `DROP DATABASE IF EXISTS "infraDb"; CREATE DATABASE "infraDb";`
4. Terminate connections first: `SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'infraDb' AND pid <> pg_backend_pid();`

## MCP Detection Trap [2026-04-20]
- Aspire MCP tooling in this environment explicitly reports that tools require `aspire run` from the AppHost project directory; a plain `dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj` can start the dashboard and AppHost process without becoming MCP-detectable.
- Observed locally with Aspire CLI `13.1.3` and repo Aspire packages `13.2.0`: `aspire run` starts `InfraFlowSculptor.AppHost` and the dashboard, but MCP `list_apphosts` / `list_resources` may still report `No Aspire AppHost is currently running`.
- When that happens, treat it as an environment/tooling compatibility issue first: verify the local Aspire CLI / VS Code Aspire MCP tooling version against the repo Aspire package version before changing application code.
