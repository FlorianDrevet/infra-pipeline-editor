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

## MCP Mapping Pitfall [2026-04-29]
- `create_project_from_draft` can still traverse Application handlers that require `MapsterMapper.IMapper` after the initial project aggregate is created, notably `CreateInfrastructureConfigCommandHandler` via `ProjectSetupOrchestrator.CreateInfrastructureAsync(...)`.
- If the MCP host only registers `AddApplication()` + `AddInfrastructure(...)` + `AddPatAuthentication()`, Aspire logs show `Unable to resolve service for type 'MapsterMapper.IMapper'` while the MCP resource itself remains `Running` / `Healthy`.
- `src/Mcp/InfraFlowSculptor.Mcp/Program.cs` now fixes this by calling `AddMcpMappings()` before `AddApplication()`. `AddMcpMappings()` lives in `src/Mcp/InfraFlowSculptor.Mcp/DependencyInjection/McpMappingServiceCollectionExtensions.cs` and reuses API `AddMapping()` registration from `InfraFlowSculptor.Api.Common.Mapping`.
- Regression test: `tests/InfraFlowSculptor.Mcp.Tests/DependencyInjection/McpMappingServiceCollectionExtensionsTests.cs`.

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

## VS Code Agent Config And Dev Certs [2026-04-29]
- Aspire doctor now expects the VS Code `aspire` server entry in `.vscode/mcp.json` to use `aspire agent mcp` instead of the deprecated `aspire mcp start` command.
- Adding `--non-interactive` to that server args list is compatible with this environment and avoids interactive assumptions when the stdio server is spawned by tooling.
- On this machine, `aspire agent init --non-interactive` still crashed while prompting for the workspace root, so the CLI migration path suggested by doctor was not usable and the config had to be patched manually.
- `aspire certs trust --non-interactive` successfully promoted the local HTTPS development certificate to full trust and cleared the Aspire doctor certificate warning.

## MCP Blob Artifact Config Pitfall [2026-04-29]
- `GenerateProjectBicepCommandHandler` resolves `IBlobService` even when the MCP host is running outside the API process, so it can hit Infrastructure blob settings that are present in API `appsettings*.json` but absent from `src/Mcp/InfraFlowSculptor.Mcp` configuration.
- Before the fix, live MCP generation for project `479602ce-4b7d-4fa3-b4b7-2f37a690f78c` (`mariage-edwige-henri`) crashed with `System.NullReferenceException` in `Azure.Storage.Blobs.BlobServiceClient.GetBlobContainerClient(...)` from `InfraFlowSculptor.Infrastructure.Services.BlobService.BlobService` because `BlobSettings.ContainerName` was null.
- `BlobSettings` now provides the default container name `bicep-output`, and `BlobService` resolves the configured container name defensively through `BlobSettings.ResolveContainerName(...)` instead of trusting configuration to be present in every host.
- Validation: focused Infrastructure tests for BlobService container resolution are green, and a live MCP call to `generate_project_bicep` now returns `status: generated` with 8 emitted files for `mariage-edwige-henri`.
