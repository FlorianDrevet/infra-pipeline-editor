# ADR-005: .NET Aspire for Local Orchestration

## Status
Accepted

## Context

Local development requires running multiple services: API, MCP server, PostgreSQL, frontend, blob storage emulator. Developers need a consistent, one-command way to start the full stack.

Options considered:
1. **Docker Compose** — Standard multi-container orchestration
2. **.NET Aspire** — .NET-native distributed app orchestration
3. **Manual scripts** — PowerShell scripts to start each service
4. **Tye** — Microsoft's previous orchestrator (deprecated)

## Decision

Use **.NET Aspire** as the local orchestration framework.

## Consequences

### Positive
- One command (`aspire run`) starts everything
- Built-in observability dashboard (traces, logs, metrics)
- Service discovery between projects
- Health checks and restart policies
- Native .NET integration (project references, not just containers)
- Database data volumes persist across restarts

### Negative
- Aspire is relatively new (less community knowledge)
- Requires .NET SDK on dev machine
- Frontend (Node.js) runs as npm app, less integrated
- Some features are preview-only

### Risks
- Aspire evolving rapidly (mitigated: pinned to SDK version via global.json)
- Workload installation confusion (mitigated: workload is obsolete, SDK-only now)
