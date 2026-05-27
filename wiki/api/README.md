# API Documentation

> REST API reference: endpoints, contracts, authentication, error handling.

## Table of Contents

1. [Endpoints Reference](endpoints.md) — Full API endpoint catalog
2. [Authentication](authentication.md) — Auth flows for API consumers
3. [Contracts](contracts.md) — Request/response DTO conventions
4. [Error Handling](error-handling.md) — Error codes and problem details
5. [Rate Limiting](rate-limiting.md) — Throttling policies

---

## Base URL

| Environment | URL |
|-------------|-----|
| Local (Aspire) | `http://localhost:5102` |
| Production | `https://api.infraflowsculptor.com` (planned) |

---

## API Style

- **Minimal APIs** — No controllers, direct endpoint mapping
- **OpenAPI** — Full spec available at `/scalar/v1`
- **Versioning** — Not yet implemented (single version)
- **Content-Type** — `application/json` exclusively
- **Auth** — All endpoints require authentication (401 on missing/invalid token)

---

## Quick Start

```bash
# Get a token (human user via MSAL or PAT)
TOKEN="ifs_your_personal_access_token"

# List projects
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5102/projects

# Get a specific resource
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5102/infrastructure-configs/{configId}/container-apps/{id}
```
