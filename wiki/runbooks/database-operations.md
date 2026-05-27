# Database Operations Runbook

## Local Reset

### Full database reset (delete all data)

```powershell
# Stop Aspire
# Then:
cd .\src\Api\InfraFlowSculptor.Infrastructure

# Drop and recreate
dotnet ef database drop --startup-project ..\InfraFlowSculptor.Api --force
dotnet ef database update --startup-project ..\InfraFlowSculptor.Api
```

### Reset with seed data

```powershell
# After database update:
.\scripts\seed-project-snapshot.ps1
```

---

## Seeding

### Apply the reference seed

```powershell
.\scripts\seed-project-snapshot.ps1
```

This creates the reference project (`fb8699ea-f568-4afb-864b-e82d2efd0905`) with all resource types populated.

### Verify seed

```sql
SELECT COUNT(*) FROM "Projects";                    -- Expected: 1
SELECT COUNT(*) FROM "AzureResources";              -- Expected: 20+
SELECT COUNT(*) FROM "InfrastructureConfigs";       -- Expected: 1+
```

---

## Migrations

### Create a new migration

```powershell
cd .\src\Api\InfraFlowSculptor.Infrastructure
dotnet ef migrations add 20250601_DescriptiveName `
    --startup-project ..\InfraFlowSculptor.Api
```

### Apply pending migrations

```powershell
cd .\src\Api\InfraFlowSculptor.Infrastructure
dotnet ef database update --startup-project ..\InfraFlowSculptor.Api
```

### Revert last migration

```powershell
cd .\src\Api\InfraFlowSculptor.Infrastructure
dotnet ef migrations remove --startup-project ..\InfraFlowSculptor.Api
```

### List applied migrations

```powershell
cd .\src\Api\InfraFlowSculptor.Infrastructure
dotnet ef migrations list --startup-project ..\InfraFlowSculptor.Api
```

---

## Backup (Production)

### Manual backup

```bash
pg_dump -h <host> -U <user> -d infraDb > backup_$(date +%Y%m%d).sql
```

### Point-in-time restore

Azure Database for PostgreSQL supports PITR:
1. Azure Portal → PostgreSQL Flexible Server
2. Overview → Restore
3. Select point-in-time
4. Confirm restore to new server

---

## Connection String

### Local (from Aspire)

Aspire auto-injects the connection string. Manual override:
```
Host=localhost;Port=5432;Database=infraDb;Username=postgres;Password=<from-aspire-logs>
```

### Find Aspire-assigned password

Check the Aspire Dashboard → postgres resource → Environment variables.
