# Infrastructure Documentation

> Local stack, cloud architecture, database, blob storage, MCP deployment.

## Table of Contents

1. [Aspire Orchestration](aspire.md) — Local development with .NET Aspire
2. [Database](database.md) — PostgreSQL schema and management
3. [Cloud Architecture](cloud-architecture.md) — Azure deployment topology
4. [Blob Storage](blob-storage.md) — Generated artifact storage

---

## Stack Overview

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Orchestration | .NET Aspire 13.3 | Local service composition |
| Database | PostgreSQL 17.6 | Primary data store |
| Cache | In-memory (local) | API output caching |
| File storage | Azure Blob Storage | Generated Bicep/Pipeline files |
| Secret store | Azure Key Vault | Production secrets |
| Identity | Entra ID | Authentication provider |
| Frontend host | nginx | Static file serving |
