# ADR-012: PAT Authentication for Automation

## Status
Accepted

## Context

AI agents (MCP), CI/CD pipelines, and automation scripts need to authenticate with the API without going through the interactive Entra ID login flow. We need a non-interactive authentication mechanism that:
- Works without browser interaction
- Supports least-privilege (scoped access)
- Can be revoked without affecting human users
- Is auditable

## Decision

Implement **Personal Access Tokens (PAT)** as a secondary authentication mechanism:
- Format: `ifs_` prefix + 32 random bytes (base64url)
- Storage: SHA-256 hash in database (plaintext shown only once)
- Scopes: Read, Write, Generate
- Lifecycle: Optional expiration, explicit revocation

## Consequences

### Positive
- Non-interactive auth for automation
- Scoped permissions (least privilege)
- Revocable per-token (no impact on other tokens)
- Auditable (creation, usage, revocation tracked)
- Simple to use (`Authorization: Bearer ifs_...`)
- Compatible with existing Bearer token infrastructure

### Negative
- Security responsibility on user (must store token safely)
- Token leakage risk (mitigated: scope limits blast radius)
- Additional auth path to maintain and test
- Hash lookup on every request (mitigated: indexed, fast)

### Risks
- Token sprawl (mitigated: expiration dates, PAT list UI)
- Overprivileged tokens (mitigated: Write doesn't include Generate)
