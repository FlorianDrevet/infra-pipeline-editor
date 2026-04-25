# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-25 | dev | Dream orchestration now uses an exclusive temp-directory lock and skips already-closed same-day dream gates to prevent concurrent memory rewrites; frontend auth-expiry UX was also hardened so `AxiosService` redirects both backend `401` responses and pre-request token acquisition failures to `/login` without flashing component-level errors. Validation: targeted instruction diff review, `npm run typecheck`, `npm run build`, `gitnexus_detect_changes` (risk `low`, no affected processes). |
| 2026-04-24 | dev | Frontend moved to Angular 21 and completed the main Design System rollout: global Material theming, DS buttons/toggles/form controls, `cdkConnectedOverlay` select panels, ACA health-probe card polish, warning cleanup, and broad DS migrations across dialogs, wizards, `resource-edit`, `config-detail`, and `project-detail`. Project-level SplitInfraCode generation was then realigned to restore `Common/` for Bicep and `.azuredevops/Common/...` for pipelines, with matching bootstrap and frontend tree updates. Validation covered `dotnet build` on Application, parity `dotnet test`, `npm run typecheck`, and `npm run build` (same bundle/style budget warnings). |
| 2026-04-23 | dev/copilot | Major multi-repo + generation wave: backend audit fixes (path traversal guard, auth on generation/download, security headers, HSTS, rate limiting, validator sweep, `ValidationBehavior` cleanup, unique role-assignment index, KeyVault FK `SetNull`), layout-driven repository topology (`LayoutPreset`, per-config `LayoutMode`/`InfraConfigRepository`), removal of legacy `GitRepositoryConfiguration` / `RepositoryMode` / `CommonsStrategy` / `RepositoryBinding`, SplitInfraCode dual-repo generation/push and flat Bicep layout, bootstrap ADO pipelines/environments/permissions guidance, many Bicep/pipeline correctness fixes (RBAC grouping and service category, constants union, KV secret ordering, mono-repo PR `Common` checkout, app pipeline split/promote flows, path normalization), frontend multi-repo switcher/push dialog/i18n/storage/SQL/ACR/custom-domain refinements, the local DB repair script, and the ACR RBAC deployment prerequisite note. |
| 2026-04-22 | copilot | Custom domains were implemented end to end; bootstrap Azure DevOps was hardened and documented; mono-repo combined push, existing-resource generation verification, pipeline variable usage fixes, Container App health probes and grouped Bicep parameters, CAE Log Analytics security hardening, DNS-based name availability, resource abbreviation overrides, SQL Server fixes, favicon refresh, snapshot seeding, and Aspire updates all landed. |
| 2026-04-21 | copilot | Bicep generation hardening added secure parameter support, pipeline override wiring, output symbol validation, LAW fallback logic, artifact path sanitization, and module reference disambiguation. |
| 2026-04-20 | copilot | Infra release artifact flow was standardized around `drop` and artifact-relative paths, and the Aspire MCP detection mismatch was documented. |
| 2026-04-17 | copilot | `.bicepparam` files switched to environment short names for filenames while keeping full environment names inside the file payload. |
| 2026-04-16 | copilot | Cross-cutting platform alignment landed: sealed enum value objects, string DTO ids, `.ProducesProblem(401)`, tag limits, shared role-assignment domain service, repository naming cleanup, camelCase Bicep identifiers, ErrorOr handling alignment, and Aspire decoupling of API DbContext/OTel/health wiring. |
| 2026-04-15 | copilot | `main` was merged into the DDD branch, concrete `AzureResource` aggregates were sealed, and audit automation plus GitHub audit issues were refreshed. |
| 2026-04-14 | copilot | Audit automation gained Windows-friendly PowerShell entry points, UTF-8 reads, and no longer requires PowerShell 7. |
| 2026-04-13 | copilot | Draw.io architecture tooling was added and the main architecture diagram was refreshed. |
| 2026-04-04 | copilot | Generation/pipeline stabilization fixed Azure DevOps release issues, unified app pipeline generation into `POST /generate-pipeline`, moved `AgentPoolName` to `Project`, refactored `BicepAssembler`, and corrected FK cascade behavior. |
| 2026-04-03 | copilot | App pipeline generation, GitNexus integration, and several Container App runtime/pipeline fixes landed. |
| 2026-04-02 | copilot | The assigned user-managed identity flow was refactored across domain, CQRS, Bicep, frontend, and diagnostics. |
| 2026-04-01 | copilot | Windows-first conventions were reinforced and general-config i18n/runtime issues were cleaned up. |
| 2026-03-31 | copilot | Event Hub namespace support and the configuration keys UX redesign landed. |
| 2026-03-30 | copilot | ACR pull role assignments, pipeline variable groups, Unit of Work behavior, and domain quality conventions were introduced. |
| 2026-03-29 | copilot | Azure DevOps pipeline YAML generation support was added around `AzureResourceManagerConnection`. |
| 2026-03-28 | copilot | Unified generation UX, mono-repo pipeline support, secret/static app settings, and the architect agent were introduced. |
| 2026-03-27 | copilot | UAI grouping and Storage Account CORS/lifecycle editing were improved. |
| 2026-03-26 | copilot | Storage Account CORS UX and queue/table Bicep generation were redesigned. |
| 2026-03-24 | copilot | Log Analytics Workspace and Application Insights were delivered end to end. |
