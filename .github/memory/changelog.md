# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-28 | dev | **Sonar hardening follow-up:** project combined ZIP downloads now enforce CRC + size/count/path guards with localized errors, and frontend Roboto/Material Icons stylesheets moved from Google Fonts links to local npm packages bundled by Angular. |
| 2026-04-28 | dev | **Sonar quick wins:** backend regex timeouts bounded (250 ms) with new Contracts/Domain test projects, frontend local regex/bypass/archive-path hotspots hardened, Copilot setup actions pinned to full SHAs, and frontend Docker image tightened (explicit COPY, ignore-scripts, unprivileged nginx). |
| 2026-04-27 | dev | **Pipeline refactor Vague 1 COMPLETE (1.0-1.3).** 3 engines decomposed into staged facades. Net -335 LOC dead code. 91/91 tests green (44 golden + 47 stage). Golden parity sentinels for 4 engines (83 golden files). Domain decoupled from PipelineGeneration. Plan at `docs/architecture/pipeline-refactoring/00-PLAN.md`. |
| 2026-04-27 | dev | **Bicep V2 migration COMPLETE.** All 18 generators migrated legacy const string to Builder+IR (BicepModuleSpec). Phase 0 IR infra + Phases 1-5 generators by tier + Phase 6 legacy cleanup. IrOutputPruningStage + OutputUsageTracker replaced regex-based pruner. 842 tests total. |
| 2026-04-27 | dev | **Tooling wave:** bicep-v2-migration skill + migration tracker. tdd-workflow skill (RED-GREEN-REFACTOR-VERIFY mandatory). review-expert + review-remediator agents + review-main prompt. Bicep onboarding guide rewritten. License changed to PolyForm Noncommercial 1.0.0. xunit-unit-testing skill added. GenerationParity.Tests csproj removed. |
| 2026-04-27 | dev | **Bug fixes:** MainBicepAssembler aliases colliding exported type names. Bootstrap az pipelines create on Windows PS 5.1 guarded against NativeCommandError. |
| 2026-04-26 | dev | **Frontend polish:** DockerfilePicker visibility fixed (brand-palette colors). Wizard footer sticky. Split download ZIPs repo-scoped. Bootstrap preview split infra/code. POST /projects/with-setup route restored. Pipeline pool emitted in wrappers. KV app settings ViaBicepparam classified as secrets. Config-detail Generate/Push gated to true MultiRepo. |
| 2026-04-25 | dev | **Create-project wizard V1:** 4/5-step Material stepper, atomic POST /projects/with-setup, sessionStorage draft, Quick Start, resume-draft banner. Old create-project-dialog deleted. |
| 2026-04-25 | dev | **SplitInfraCode generation:** Bootstrap split (BootstrapMode FullOwner/ApplicationOnly), dedicated infra/code push actions, MultiRepoPushDialog (infra/code/both), panel collapse/expand, BootstrapSetupGuide, DsPanelActionButton, generation panel header redesign. |
| 2026-04-25 | dev | **Bicep module uniformity:** Generic userAssignedIdentityId param (AVM-style). Same-typed resources sharing modules. Project-level Git push re-sliced by root folder. Dream exclusive lock. Auth-expiry UX hardened. Local DB snapshot refreshed. |
| 2026-04-24 | dev | Angular 21 + DS rollout. SplitInfraCode Common/ restored for project Bicep. Global Material override. 216 form fields migrated to DS controls. |
| 2026-04-23 | dev/copilot | Multi-repo wave: backend audit fixes (path traversal, auth, security headers, rate limiting), layout-driven topology (LayoutPreset), legacy Git config removed, SplitInfraCode dual-repo generation/push, bootstrap ADO, Bicep/pipeline correctness fixes, frontend multi-repo UX. |
| 2026-04-22 | copilot | Custom domains E2E, bootstrap ADO hardening, mono-repo combined push, existing-resource verification, Container App health probes, grouped Bicep params, CAE LAW security, DNS name availability, resource abbreviation overrides, SQL Server fixes, snapshot seeding. |
| 2026-04-21 | copilot | Bicep hardening: secure params, pipeline override wiring, output symbol validation, LAW fallback, artifact path sanitization, module reference disambiguation. |
| 2026-04-20 | copilot | Infra release artifact flow standardized (drop + artifact-relative paths). Aspire MCP detection mismatch documented. |
| 2026-04-17 | copilot | .bicepparam filenames switched to environment short names. |
| 2026-04-16 | copilot | Sealed enum VOs, string DTO ids, .ProducesProblem(401), tag limits, shared role-assignment service, camelCase Bicep, ErrorOr alignment, Aspire decoupling. |
| 2026-04-15 | copilot | Merged main into DDD branch, sealed AzureResource aggregates, audit automation refreshed. |
| 2026-04-14 | copilot | Audit scripts: Windows PS 5.1 compat, UTF-8, no PS7 requirement. |
| 2026-04-13 | copilot | Draw.io tooling added, architecture diagram refreshed. |
| 2026-04-04 | copilot | Generation/pipeline stabilization, unified app pipeline endpoint, AgentPoolName to Project, BicepAssembler refactored, FK cascade fixes. |
| 2026-04-03 | copilot | App pipeline generation, GitNexus integration, Container App runtime/pipeline fixes. |
| 2026-04-02 | copilot | Assigned user-managed identity refactored across domain/CQRS/Bicep/frontend/diagnostics. |
| 2026-04-01 | copilot | Windows-first conventions reinforced, general-config i18n/runtime cleanup. |
| 2026-03-31 | copilot | Event Hub namespace support, configuration keys UX redesign. |
| 2026-03-30 | copilot | ACR pull role assignments, pipeline variable groups, Unit of Work, domain quality conventions. |
| 2026-03-29 | copilot | Azure DevOps pipeline YAML generation (AzureResourceManagerConnection). |
| 2026-03-28 | copilot | Unified generation UX, mono-repo pipeline, secret/static app settings, architect agent. |
| 2026-03-27 | copilot | UAI grouping, Storage Account CORS/lifecycle editing. |
| 2026-03-26 | copilot | Storage Account CORS UX, queue/table Bicep generation. |
| 2026-03-24 | copilot | Log Analytics Workspace + Application Insights delivered E2E. |