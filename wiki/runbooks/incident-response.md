# Incident Response Runbook

## API Errors (5xx)

### Symptoms
- API returning HTTP 500 responses
- Frontend showing "An unexpected error occurred"
- Application Insights showing increased failure rate

### Diagnosis Steps

1. **Check health endpoint:**
   ```bash
   curl http://localhost:5102/health
   ```

2. **Check Aspire Dashboard:**
   - Open `http://localhost:15888`
   - Navigate to Structured Logs
   - Filter by severity: Error
   - Look for stack traces

3. **Check database connectivity:**
   ```powershell
   # In Aspire Dashboard, check postgres resource status
   # Or directly:
   psql -h localhost -p 5432 -U postgres -d infraDb -c "SELECT 1"
   ```

4. **Check recent changes:**
   - Any new migrations applied?
   - Any configuration changes?
   - Any NuGet package updates?

### Resolution

| Root Cause | Fix |
|-----------|-----|
| Database connection refused | Restart PostgreSQL container |
| Migration failed | Check migration logs, fix and re-apply |
| Null reference in handler | Check recent code changes, add null checks |
| EF Core model mismatch | Verify model matches DB schema |

---

## Database Connectivity

### Symptoms
- Health check returning "Unhealthy"
- "Npgsql.NpgsqlException" in logs
- Connection pool exhaustion warnings

### Diagnosis Steps

1. **Check PostgreSQL is running:**
   ```powershell
   # Aspire Dashboard → Resources → postgres → Status
   docker ps | Select-String postgres
   ```

2. **Check connection pool:**
   ```sql
   SELECT count(*) FROM pg_stat_activity WHERE datname = 'infraDb';
   ```

3. **Check for blocking queries:**
   ```sql
   SELECT pid, query, state, wait_event 
   FROM pg_stat_activity 
   WHERE datname = 'infraDb' AND state != 'idle';
   ```

### Resolution

| Root Cause | Fix |
|-----------|-----|
| PostgreSQL not running | `docker start <container>` or restart Aspire |
| Too many connections | Check for connection leaks, restart API |
| Long-running query | Kill blocking PID: `SELECT pg_terminate_backend(<pid>)` |
| Disk full | Check Docker volume, clean up |

---

## Frontend Not Loading

### Symptoms
- Blank page on `http://localhost:4200`
- Console errors in browser DevTools
- CORS errors in network tab

### Diagnosis Steps

1. **Check Angular dev server:**
   ```powershell
   cd src\Front
   npm run start
   ```

2. **Check CORS configuration:**
   - Verify `ApiCorsOptions` in API configuration
   - Check that `http://localhost:4200` is in allowed origins

3. **Check API accessibility:**
   ```bash
   curl http://localhost:5102/health
   ```

### Resolution

| Root Cause | Fix |
|-----------|-----|
| Angular compile error | Check terminal output, fix TypeScript errors |
| CORS blocked | Add origin to `ApiCorsOptions` |
| API unreachable | Start Aspire or API manually |
| Auth redirect loop | Clear session storage, re-login |

---

## Generation Failures

### Symptoms
- "Generation failed" error in UI
- 500 response from `/generate/bicep`
- Partial file output

### Diagnosis Steps

1. **Check generation logs:**
   - Aspire Dashboard → Traces → Find generation request
   - Check which stage failed (100-900)

2. **Validate input data:**
   - Does the config have at least one resource group?
   - Does it have at least one environment?
   - Are all required resource properties filled?

3. **Run generation in isolation:**
   ```bash
   curl -X POST http://localhost:5102/infrastructure-configs/{configId}/generate/preview \
     -H "Authorization: Bearer <token>"
   ```

### Resolution

| Root Cause | Fix |
|-----------|-----|
| Missing required property | Fill in resource properties in UI |
| Cross-reference to deleted resource | Remove stale references |
| Generator bug | Check specific generator for the failing resource type |
| Cancellation | Retry (may have been timeout) |
