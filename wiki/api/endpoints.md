# API Endpoints Reference

## Overview

The API exposes **36 controllers** organized by feature. All endpoints require authentication.

---

## Projects

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/projects` | List user's projects |
| `POST` | `/projects` | Create a project |
| `GET` | `/projects/{projectId}` | Get project details |
| `PUT` | `/projects/{projectId}` | Update project |
| `DELETE` | `/projects/{projectId}` | Delete project |
| `GET` | `/projects/{projectId}/resources` | List all resources in project |

## Project Members

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/projects/{projectId}/members` | List members |
| `POST` | `/projects/{projectId}/members` | Add member |
| `PUT` | `/projects/{projectId}/members/{memberId}` | Update role |
| `DELETE` | `/projects/{projectId}/members/{memberId}` | Remove member |

## Infrastructure Configs

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/projects/{projectId}/infrastructure-configs` | List configs |
| `POST` | `/projects/{projectId}/infrastructure-configs` | Create config |
| `GET` | `/infrastructure-configs/{configId}` | Get config |
| `PUT` | `/infrastructure-configs/{configId}` | Update config |
| `DELETE` | `/infrastructure-configs/{configId}` | Delete config |

## Resource Groups

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/infrastructure-configs/{configId}/resource-groups` | List RGs |
| `POST` | `/infrastructure-configs/{configId}/resource-groups` | Create RG |
| `PUT` | `/resource-groups/{rgId}` | Update RG |
| `DELETE` | `/resource-groups/{rgId}` | Delete RG |

## Environments

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/infrastructure-configs/{configId}/environments` | List environments |
| `POST` | `/infrastructure-configs/{configId}/environments` | Create environment |
| `PUT` | `/environments/{envId}` | Update environment |
| `DELETE` | `/environments/{envId}` | Delete environment |
| `PUT` | `/environments/{envId}/values` | Set environment values |

## Azure Resources (Per-Type)

Each of the 22 resource types follows this pattern:

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/infrastructure-configs/{configId}/{resource-type}s` | List resources |
| `POST` | `/infrastructure-configs/{configId}/{resource-type}s` | Create resource |
| `GET` | `/infrastructure-configs/{configId}/{resource-type}s/{id}` | Get resource |
| `PUT` | `/infrastructure-configs/{configId}/{resource-type}s/{id}` | Update resource |
| `DELETE` | `/infrastructure-configs/{configId}/{resource-type}s/{id}` | Delete resource |

### Resource Type URL Slugs

| Type | Slug |
|------|------|
| Container App | `container-apps` |
| Container App Environment | `container-app-environments` |
| Container Registry | `container-registries` |
| Key Vault | `key-vaults` |
| SQL Server | `sql-servers` |
| SQL Database | `sql-databases` |
| Storage Account | `storage-accounts` |
| Application Insights | `application-insights` |
| Log Analytics Workspace | `log-analytics-workspaces` |
| Service Bus | `service-buses` |
| Redis Cache | `redis-caches` |
| App Service Plan | `app-service-plans` |
| App Service | `app-services` |
| Static Web App | `static-web-apps` |
| Virtual Network | `virtual-networks` |
| Subnet | `subnets` |
| Network Security Group | `network-security-groups` |
| Private Endpoint | `private-endpoints` |
| Private DNS Zone | `private-dns-zones` |
| User Assigned Identity | `user-assigned-identities` |
| Cosmos DB Account | `cosmos-db-accounts` |
| Signal R | `signal-rs` |

## Generation

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/infrastructure-configs/{configId}/generate/bicep` | Generate Bicep |
| `POST` | `/infrastructure-configs/{configId}/generate/pipeline` | Generate Pipeline |
| `POST` | `/infrastructure-configs/{configId}/generate/preview` | Preview generation |
| `GET` | `/infrastructure-configs/{configId}/generated-files` | List generated files |
| `GET` | `/infrastructure-configs/{configId}/generated-files/{path}` | Get file content |
| `POST` | `/infrastructure-configs/{configId}/push-to-git` | Push to DevOps |

## Personal Access Tokens

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/projects/{projectId}/pats` | List PATs |
| `POST` | `/projects/{projectId}/pats` | Create PAT |
| `DELETE` | `/projects/{projectId}/pats/{patId}` | Revoke PAT |

## Layout Presets

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/infrastructure-configs/{configId}/layout-preset` | Get layout |
| `PUT` | `/infrastructure-configs/{configId}/layout-preset` | Set layout |

## Import

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/infrastructure-configs/{configId}/import/arm` | Import ARM template |

## Health

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/health` | Full health check |
| `GET` | `/alive` | Liveness probe |

---

## Response Conventions

### Success

```json
// Single entity
{ "id": "fb8699ea-...", "name": "My Project", ... }

// List
[{ "id": "...", "name": "..." }, ...]

// Creation
"fb8699ea-f568-4afb-864b-e82d2efd0905"  // Returns new ID as string
```

### Error

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "The container app was not found.",
  "errors": {}
}
```
