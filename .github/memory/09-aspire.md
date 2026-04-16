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
- Azure Blob Storage emulator for generation output

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
