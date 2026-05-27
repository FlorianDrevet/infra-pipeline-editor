# Cloud Architecture

## Target Deployment

```mermaid
flowchart TB
    subgraph "Azure"
        subgraph "Frontend"
            SWA[Static Web App]
        end
        
        subgraph "Backend"
            CA_API[Container App - API]
            CA_MCP[Container App - MCP]
        end
        
        subgraph "Data"
            PG[(Azure Database for PostgreSQL)]
            BLOB[Azure Blob Storage]
            KV[Azure Key Vault]
        end
        
        subgraph "Identity"
            ENTRA[Entra ID]
        end
        
        subgraph "Monitoring"
            AI[Application Insights]
            LAW[Log Analytics]
        end
    end
    
    SWA --> CA_API
    CA_API --> PG
    CA_API --> BLOB
    CA_API --> KV
    CA_MCP --> PG
    CA_MCP --> BLOB
    ENTRA --> CA_API
    ENTRA --> CA_MCP
    CA_API --> AI
    CA_MCP --> AI
```

---

## Resource Mapping

| Component | Azure Resource | SKU |
|-----------|---------------|-----|
| API | Container App | Consumption |
| MCP Server | Container App | Consumption |
| Frontend | Static Web App | Standard |
| Database | Azure Database for PostgreSQL Flexible | Burstable B1ms |
| File Storage | Storage Account (Blob) | Standard LRS |
| Secrets | Key Vault | Standard |
| Monitoring | Application Insights | — |
| Logs | Log Analytics Workspace | Pay-per-GB |
| Auth | Entra ID | Standard (tenant-level) |

---

## Networking

| Flow | Protocol | Auth |
|------|----------|------|
| Browser → Frontend | HTTPS | — |
| Frontend → API | HTTPS | JWT Bearer |
| MCP Client → MCP | HTTPS | JWT / PAT |
| API → PostgreSQL | TCP 5432 | Managed Identity |
| API → Blob Storage | HTTPS | Managed Identity |
| API → Key Vault | HTTPS | Managed Identity |
| API → Azure DevOps | HTTPS | PAT |

---

## Environments

| Environment | Purpose | Database | Isolation |
|-------------|---------|----------|-----------|
| Development | Local Aspire | Docker PostgreSQL | Full |
| Staging | Pre-prod validation | Shared Flexible | Resource Group |
| Production | Live users | Dedicated Flexible | Subscription |
