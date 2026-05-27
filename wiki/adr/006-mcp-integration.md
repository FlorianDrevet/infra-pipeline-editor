# ADR-006: MCP for AI Agent Integration

## Status
Accepted

## Context

We want AI agents (GitHub Copilot, VS Code agents, custom agents) to interact with InfraFlowSculptor programmatically — creating projects, configuring resources, and generating infrastructure code through natural language conversations.

Options considered:
1. **Custom REST API extensions** — Add AI-friendly endpoints to existing API
2. **OpenAI function calling** — Define functions for GPT models
3. **MCP (Model Context Protocol)** — Anthropic's standard protocol for tool integration
4. **LangChain tools** — Python-specific tool framework

## Decision

Implement a **Model Context Protocol (MCP) server** using the official .NET SDK (`ModelContextProtocol` package).

- Separate project (`src/Mcp/InfraFlowSculptor.Mcp`)
- Streamable HTTP transport on port 5258
- Same auth as API (Entra ID JWT + PAT)
- Reuses MediatR handlers (no logic duplication)
- Draft-based conversational flow for project creation

## Consequences

### Positive
- Standard protocol supported by multiple AI tools
- VS Code native integration via `.vscode/mcp.json`
- Conversational UX for complex operations
- Reuses existing business logic (MediatR)
- Independent deployment from main API
- OpenTelemetry traces for all tool invocations

### Negative
- Protocol is still evolving (SDK updates needed)
- Additional deployment target to maintain
- Draft state management adds complexity
- HTTP transport is less performant than stdio for local use

### Risks
- MCP standard may change significantly (mitigated: abstraction layer via MediatR)
- Security surface increases (mitigated: same auth model as API)
