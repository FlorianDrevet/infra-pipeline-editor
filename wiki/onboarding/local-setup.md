# Local Setup

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0.100 | Backend compilation and runtime |
| Node.js | 22.x LTS | Frontend build toolchain |
| npm | 10.x+ | Package management |
| Docker Desktop | Latest | PostgreSQL, Blob Storage emulator |
| Git | 2.40+ | Version control |
| Visual Studio Code | Latest | Recommended IDE |
| PowerShell | 5.1+ or 7.x | Scripts and automation |

### Recommended VS Code Extensions

| Extension | Purpose |
|-----------|---------|
| C# Dev Kit | .NET development |
| Angular Language Service | Angular IntelliSense |
| Bicep | Bicep file support |
| Docker | Container management |
| GitLens | Git history visualization |
| REST Client | API testing |
| Aspire | Local orchestration tools |

---

## Step-by-Step Installation

### 1. Clone the Repository

```powershell
git clone https://github.com/FlorianDrevet/infra-pipeline-editor.git
cd infra-pipeline-editor
```

### 2. Verify .NET SDK

```powershell
dotnet --version
# Expected: 10.0.100
```

If the version doesn't match, the `global.json` file pins the required SDK version. Install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download).

### 3. Restore NuGet Packages

```powershell
dotnet restore .\InfraFlowSculptor.slnx
```

### 4. Build the Solution

```powershell
dotnet build .\InfraFlowSculptor.slnx
```

### 5. Install Frontend Dependencies

```powershell
cd src\Front
npm install
cd ..\..
```

### 6. Start Docker (Required for PostgreSQL)

Ensure Docker Desktop is running. The Aspire AppHost will start a PostgreSQL container automatically.

### 7. Run with Aspire (Recommended)

```powershell
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj
```

Or using the Aspire CLI:

```powershell
aspire run
```

This starts:
- **PostgreSQL** container (port 5432)
- **DbGate** database management UI
- **InfraFlowSculptor API** (.NET backend)
- **InfraFlowSculptor MCP** (Model Context Protocol server)
- **Angular Frontend** (port 4200)
- **Azure Blob Storage emulator**
- **Key Vault emulator**
- **Aspire Dashboard** (traces, logs, metrics)

### 8. Access the Application

| Service | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| API (Swagger/Scalar) | http://localhost:5000/scalar |
| Aspire Dashboard | http://localhost:15888 |
| MCP Endpoint | http://localhost:5258/mcp |
| DbGate | http://localhost:3000 |

---

## Running Without Aspire (Standalone)

If you prefer running components individually:

### Backend API

```powershell
dotnet run --project .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj
```

Requires:
- A running PostgreSQL instance
- Connection string in `appsettings.Development.json` (key: `infraDb`)

### Frontend

```powershell
cd src\Front
npm run start
```

Runs on `http://localhost:4200` with the `development` configuration.

---

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ConnectionStrings__infraDb` | Yes | From Aspire | PostgreSQL connection string |
| `AzureAd__TenantId` | Yes | From config | Azure AD tenant |
| `AzureAd__ClientId` | Yes | From config | Azure AD app registration |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | — | OpenTelemetry collector endpoint |
| `KeyVault__VaultUri` | No | — | Azure Key Vault for secrets (prod) |
| `MCP__LISTENURL` | No | `http://127.0.0.1:5258` | MCP server listen address |

---

## Database Management

### Reset Database (Full Wipe)

```powershell
# Find the PostgreSQL container
docker ps | Select-String postgres

# Connect and reset
docker exec <container-id> sh -lc "psql -U postgres -c 'DROP DATABASE IF EXISTS \"infraDb\"; CREATE DATABASE \"infraDb\";'"
```

### Apply Migrations

Migrations run automatically at startup. To apply manually:

```powershell
dotnet ef database update --project .\src\Api\InfraFlowSculptor.Infrastructure --startup-project .\src\Api\InfraFlowSculptor.Api
```

### Seed Data

```powershell
.\scripts\seed-project-snapshot.ps1 -ApiBaseUrl "http://localhost:5000" -BearerToken "<your-token>"
```

---

## Running Tests

```powershell
# Run all tests
dotnet test .\InfraFlowSculptor.slnx

# Run a specific test project
dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj

# Run with coverage
.\scripts\test-coverage.ps1
```

---

## MCP (Model Context Protocol) Setup

The MCP server allows AI agents (GitHub Copilot) to interact with InfraFlowSculptor:

1. Start the stack with Aspire (MCP starts on port 5258)
2. Create a Personal Access Token via the API:
   ```http
   POST /personal-access-tokens
   Authorization: Bearer <azure-ad-token>
   ```
3. Configure `.vscode/mcp.json`:
   ```json
   {
     "servers": {
       "infraflowsculptor": {
         "type": "http",
         "url": "http://127.0.0.1:5258/mcp",
         "headers": {
           "Authorization": "Bearer ifs_<your-pat>"
         }
       }
     }
   }
   ```

---

## Troubleshooting First Setup

| Problem | Solution |
|---------|----------|
| `dotnet build` fails with locked assemblies | Stop all running instances (API, MCP, AppHost) |
| PostgreSQL won't start | Ensure Docker Desktop is running |
| Frontend can't reach API | Check proxy config in `src/Front/proxy.conf.js` |
| Auth redirect loop | Verify `AzureAd` config in appsettings |
| MCP "No AppHost running" | Use `aspire run` instead of `dotnet run` |
| EF migration error | Reset the database (see above) |
| npm install fails | Delete `node_modules` and `package-lock.json`, retry |
