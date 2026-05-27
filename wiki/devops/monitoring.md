# Production Monitoring

## Observability Stack

| Layer | Tool | Purpose |
|-------|------|---------|
| APM | Application Insights | Traces, metrics, exceptions |
| Logs | Log Analytics Workspace | KQL queries, alerts |
| Health | Health Check endpoints | Liveness/readiness probes |
| Infra | Azure Monitor | Resource-level metrics |
| Local | Aspire Dashboard | Development observability |

---

## Key Dashboards

### Application Health

| Metric | Query (KQL) | Alert |
|--------|-------------|-------|
| Error rate | `requests | where resultCode >= 500 | summarize count() by bin(timestamp, 5m)` | > 1% |
| Response time p95 | `requests | summarize percentile(duration, 95) by bin(timestamp, 5m)` | > 2000ms |
| Active requests | `requests | where success == true | summarize count()` | Trending |

### Database

| Metric | Source | Alert |
|--------|--------|-------|
| Connection pool usage | Azure Monitor | > 80% |
| CPU usage | Azure Monitor | > 90% sustained |
| Storage usage | Azure Monitor | > 80% |
| Slow queries | Application Insights dependencies | > 500ms |

### Container Apps

| Metric | Source | Alert |
|--------|--------|-------|
| Replica count | Azure Monitor | Min replicas < desired |
| CPU usage | Azure Monitor | > 80% |
| Memory usage | Azure Monitor | > 80% |
| HTTP 5xx | Ingress metrics | > 0 |

---

## Alerting Rules

| Severity | Condition | Action |
|----------|-----------|--------|
| Critical | API down (health check fails 3x) | Page on-call |
| Critical | Database unreachable | Page on-call |
| High | Error rate > 5% for 5 min | Notify team |
| Medium | p95 latency > 2s for 10 min | Notify team |
| Low | Disk > 80% | Create ticket |

---

## Log Queries (KQL)

### Recent Errors

```kql
traces
| where severityLevel >= 3
| where timestamp > ago(1h)
| project timestamp, message, customDimensions
| order by timestamp desc
| take 50
```

### Slow API Requests

```kql
requests
| where duration > 2000
| where timestamp > ago(24h)
| project timestamp, name, duration, resultCode
| order by duration desc
```

### Failed Generations

```kql
traces
| where message contains "generation" and severityLevel >= 3
| where timestamp > ago(7d)
| summarize count() by bin(timestamp, 1d)
```
