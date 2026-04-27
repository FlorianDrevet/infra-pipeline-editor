# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-27 | copilot | Removed the existing `InfraFlowSculptor.GenerationParity.Tests` `.csproj`, detached it from `InfraFlowSculptor.slnx`, kept the folder, and updated test instructions/memory to reflect a zero-project .NET test baseline. |
| 2026-04-27 | copilot | Added the `xunit-unit-testing` skill, routed xUnit work through `dev` and `dotnet-dev`, and documented the repo-wide unit-test placement and execution conventions under `tests/`. |
| 2026-04-27 | dev | Added architecture audit docs under `docs/architecture/bicep-refactoring/` to capture the current Bicep-generation pain points and the proposed refactoring paths (Pipeline, Builder + IR, Visitor). |
| 2026-04-26 | dev | Closed the post-wizard / split-generation polish wave: restored `POST /projects/with-setup`, added explicit root-level `pool` blocks to infra wrapper pipelines, made bootstrap Key Vault variable-group provisioning secret-aware with clearer ADO diagnostics, aligned SplitInfraCode preview/download/push paths and scopes (including code-repo app bootstrap), introduced code-repo Dockerfile browsing endpoints plus `DockerfilePickerComponent`, and gated config-level generate/push actions to true `MultiRepo` projects. |
| 2026-04-25 | dev | Completed the main SplitInfraCode delivery: shared same-type module reuse, split-aware bootstrap/application artifacts, project creation wizard V1, scoped generated-artifact cleanup, split push/download UX, generation-panel controls, bootstrap guide extraction, multi-repo Git UX alignment, dream concurrency locking, auth-expiry redirect hardening, and local snapshot refresh. |
| 2026-04-24 | dev | Upgraded the frontend to Angular 21, rolled out the Design System broadly, and restored `Common/` plus `.azuredevops/Common/...` shared layouts for project-level split generation. |
| 2026-04-23 | dev/copilot | Landed the large multi-repo + generation hardening wave: security headers/rate limiting/path validation, layout-driven repository topology, legacy Git-topology removal, SplitInfraCode generation/push, bootstrap ADO fixes, Bicep/pipeline correctness fixes, frontend multi-repo UX, DB repair tooling, and ACR RBAC prerequisite guidance. |
| 2026-04-22 | copilot | Delivered custom domains end to end, bootstrap ADO hardening, mono-repo combined push, Container App health probes and grouped Bicep params, CAE Log Analytics security fixes, DNS name availability, resource abbreviation overrides, SQL Server fixes, favicon refresh, and snapshot seeding. |
| 2026-04-21 | copilot | Added secure parameters, pipeline override wiring, output-symbol validation, LAW fallback, artifact path sanitization, and module reference disambiguation. |
| 2026-04-20 | copilot | Standardized infra release artifacts and documented the Aspire MCP detection mismatch. |
| 2026-04-17 | copilot | Switched `.bicepparam` filenames to environment short names while keeping full environment names inside the payload. |
| 2026-04-16 | copilot | Aligned platform conventions around sealed enum value objects, string response IDs, `.ProducesProblem(401)`, tag limits, shared role-assignment services, camelCase Bicep identifiers, and standalone API/Aspire wiring. |
| 2026-04-15 | copilot | Merged `main`, sealed concrete `AzureResource` aggregates, and refreshed audit automation plus GitHub audit issues. |
| 2026-04-14 | copilot | Made audit automation Windows-first with UTF-8-safe PowerShell entry points and no PowerShell 7 requirement. |
| 2026-04-13 | copilot | Added draw.io architecture tooling and refreshed the main architecture diagram. |
| 2026-04-04 | copilot | Stabilized generation/pipelines with Azure DevOps release fixes, unified app pipeline generation, a BicepAssembler refactor, and FK cascade corrections. |
| 2026-04-03 | copilot | Added app pipeline generation, GitNexus integration, and Container App runtime/pipeline fixes. |
| 2026-04-02 | copilot | Refactored the assigned user-managed identity flow across domain, CQRS, Bicep, frontend, and diagnostics. |
| 2026-04-01 | copilot | Reinforced Windows-first conventions and cleaned up general-config i18n/runtime issues. |
| 2026-03-31 | copilot | Delivered Event Hub namespace support and the configuration-keys UX redesign. |
| 2026-03-30 | copilot | Added ACR pull role assignments, pipeline variable groups, Unit of Work behavior, and domain quality conventions. |
| 2026-03-29 | copilot | Added Azure DevOps pipeline YAML generation around `AzureResourceManagerConnection`. |
| 2026-03-28 | copilot | Introduced unified generation UX, mono-repo pipelines, secret/static app settings, and the architect agent. |
| 2026-03-27 | copilot | Improved UAI grouping plus Storage Account CORS/lifecycle editing. |
| 2026-03-26 | copilot | Redesigned Storage Account CORS UX and queue/table Bicep generation. |
| 2026-03-24 | copilot | Delivered Log Analytics Workspace and Application Insights end to end. |
