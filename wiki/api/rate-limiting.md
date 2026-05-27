# API Rate Limiting

## Policies

| Policy | Window | Limit | Applied To |
|--------|--------|-------|-----------|
| **Global** | Fixed window (1 min) | Per-user | All endpoints |
| **Expensive** | Fixed window (1 min) | Lower limit | Generation, download, push |
| **HealthChecks** | Fixed window (1 min) | Separate | `/health`, `/alive` |

---

## Partitioning Strategy

Rate limits are partitioned by identity:

1. **Authenticated users:** Partitioned by stable user claim (`oid` for JWT, token hash for PAT)
2. **Fallback:** Remote IP address (if no auth claim available)

```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
{
    var userId = context.User.FindFirst("oid")?.Value
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "anonymous";

    return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new()
    {
        PermitLimit = 100,
        Window = TimeSpan.FromMinutes(1),
    });
});
```

---

## Response Headers

When rate limited, the API returns:

| Header | Value |
|--------|-------|
| `Retry-After` | Seconds until window resets |
| HTTP Status | `429 Too Many Requests` |

---

## Expensive Operations

Generation endpoints have a lower limit to prevent abuse:

```csharp
app.MapPost("/generate/bicep", handler)
    .RequireRateLimiting("Expensive");
```

| Operation | Why expensive |
|-----------|-------------|
| Bicep generation | 10-stage pipeline, CPU intensive |
| Pipeline generation | Template assembly |
| Push to Git | External API call (Azure DevOps) |
| Download ZIP | Large file assembly |
