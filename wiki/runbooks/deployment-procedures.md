# Deployment Procedures Runbook

## Local Development

### Start full stack

```powershell
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj
```

### Start API only

```powershell
dotnet run --project .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj
```

### Start frontend only

```powershell
cd src\Front
npm run start
```

---

## Pre-Deployment Checklist

- [ ] All tests pass: `dotnet test .\InfraFlowSculptor.slnx`
- [ ] Frontend builds: `cd src\Front; npm run build`
- [ ] Type check passes: `cd src\Front; npm run typecheck`
- [ ] No new SonarQube issues
- [ ] Migrations reviewed and tested locally
- [ ] Health checks pass after migration
- [ ] Feature flags configured (if applicable)

---

## Production Deployment

### 1. Build containers

```powershell
# API
docker build -t infraflowsculptor-api:latest -f src/Api/InfraFlowSculptor.Api/Dockerfile .

# MCP
docker build -t infraflowsculptor-mcp:latest -f src/Mcp/InfraFlowSculptor.Mcp/Dockerfile .
```

### 2. Push to registry

```powershell
az acr login --name <registry>
docker tag infraflowsculptor-api:latest <registry>.azurecr.io/infraflowsculptor-api:latest
docker push <registry>.azurecr.io/infraflowsculptor-api:latest
```

### 3. Deploy

```powershell
az containerapp update --name api --resource-group <rg> `
    --image <registry>.azurecr.io/infraflowsculptor-api:latest
```

### 4. Verify

```bash
curl https://api.infraflowsculptor.com/health
curl https://api.infraflowsculptor.com/alive
```

### 5. Monitor

- Check Application Insights for errors in the first 15 minutes
- Verify key user flows work (create project, generate, download)

---

## Rollback

### Container App revision rollback

```powershell
# List revisions
az containerapp revision list --name api --resource-group <rg>

# Activate previous revision
az containerapp revision activate --name api --resource-group <rg> --revision <previous>

# Deactivate current
az containerapp revision deactivate --name api --resource-group <rg> --revision <current>
```

### Database rollback (last resort)

Only if migration caused data corruption:
1. Restore from point-in-time backup
2. Deploy previous code version
3. Investigate root cause before re-deploying
