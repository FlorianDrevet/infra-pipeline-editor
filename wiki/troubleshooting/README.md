# Troubleshooting

> Common issues and their solutions.

## Table of Contents

1. [Common Issues](#common-issues)
2. [Build Errors](#build-errors)
3. [Runtime Errors](#runtime-errors)
4. [Frontend Issues](#frontend-issues)
5. [Generation Issues](#generation-issues)

---

## Common Issues

### Aspire won't start

**Symptom:** `aspire run` fails or resources stay in "Starting" state.

**Solutions:**
1. Check Docker is running: `docker info`
2. Check port conflicts: `netstat -ano | findstr :5432`
3. Kill existing instances: stop Docker containers
4. Clean data volumes: `docker volume prune`
5. Rebuild: `dotnet build .\src\Aspire\InfraFlowSculptor.AppHost`

---

### Database migration fails

**Symptom:** Error on startup: "Failed to apply migrations"

**Solutions:**
1. Check connection string is correct
2. Check PostgreSQL is running and accessible
3. Try manual migration: 
   ```powershell
   cd src\Api\InfraFlowSculptor.Infrastructure
   dotnet ef database update --startup-project ..\InfraFlowSculptor.Api
   ```
4. If schema is corrupted: drop and recreate
   ```powershell
   dotnet ef database drop --force --startup-project ..\InfraFlowSculptor.Api
   dotnet ef database update --startup-project ..\InfraFlowSculptor.Api
   ```

---

### EF Core LINQ error: "could not be translated"

**Symptom:** `InvalidOperationException: The LINQ expression could not be translated`

**Common causes:**
- Using `.Value` on typed IDs: `x.Id.Value == id.Value` ❌
- Fix: `x.Id == id` ✅
- Using pattern matching in Mapster expressions: `x is not null` ❌
- Fix: `x != null` ✅

---

## Build Errors

### "SDK not found" or wrong version

**Symptom:** `dotnet build` fails with SDK version error

**Solution:** Install .NET SDK 10.0.100 (from `global.json`):
```powershell
winget install Microsoft.DotNet.SDK.10
```

---

### "Package version conflict"

**Symptom:** NuGet restore fails with version conflicts

**Solution:** This project uses Central Package Management (Directory.Packages.props):
```powershell
# Clean and restore
dotnet nuget locals all --clear
dotnet restore .\InfraFlowSculptor.slnx
```

---

### Angular build fails

**Symptom:** `npm run build` fails with TypeScript errors

**Solutions:**
1. Check TypeScript version matches: `npx tsc --version`
2. Clean install: `rm -r node_modules; npm ci`
3. Check for type errors: `npm run typecheck`

---

## Runtime Errors

### 401 Unauthorized on all requests

**Causes:**
- Token expired → Re-login or refresh token
- Wrong audience in JWT → Check `AzureAd:Audience` config
- PAT revoked → Create new PAT
- Missing `Authorization` header → Check Axios interceptor

---

### 403 Forbidden

**Causes:**
- User not member of project → Add via project members API
- PAT scope insufficient → Create PAT with correct scope (Read/Write/Generate)
- Trying to write with Read-only PAT → Create Write-scoped PAT

---

### 429 Too Many Requests

**Cause:** Rate limit exceeded

**Solution:** Wait for `Retry-After` header value, then retry. For automation, implement exponential backoff.

---

## Frontend Issues

### Blank page after login

**Causes:**
- MSAL redirect URI mismatch → Check `environment.redirectUri`
- CORS blocked → Verify API CORS config includes frontend URL
- JavaScript error → Check browser console

---

### "Cannot read properties of null"

**Common cause:** Signal accessed before initialization

**Solution:** Initialize signals with defaults:
```typescript
readonly data = signal<MyType[]>([]);  // Not signal<MyType[] | null>(null)
```

---

## Generation Issues

### "Generation produced empty output"

**Causes:**
- No resources in configuration → Add at least one resource
- No environments defined → Add at least one environment
- Resource missing required properties → Fill all required fields

---

### "Invalid Bicep: unresolved reference"

**Cause:** Cross-config reference to deleted resource

**Solution:** 
1. Check resource references in the config
2. Remove stale references to deleted resources
3. Re-generate

---

### Push to Git fails

**Causes:**
- Repository PAT expired → Update in Key Vault / settings
- Repository URL incorrect → Verify DevOps organization and project
- Branch protection → Check branch policies in Azure DevOps
