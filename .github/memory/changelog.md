# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-05-11 | dev | Fixed the mono-repo project-detail generation UX so Generate All keeps each tab in loading state until the full batch settles, then reveals file trees/errors/download actions together; added a focused frontend helper spec for the reveal gate. |
| 2026-05-11 | dev | Fixed the frontend generated-file explorer ordering bug by sorting hierarchical Bicep/Pipeline entries before flattening them, and added a regression spec for interleaved Common/modules paths. |
| 2026-05-11 | dev | Added a focused BicepGeneration regression test that locks the Container Registry role-assignment module path to `Common/modules/ContainerRegistry/containerregistry.roleassignments.module.bicep` during project-level generation. |
| 2026-05-11 | dev | **BicepGeneration parameter-model + strict generator-literal hardening.** Legacy `Generate(...)` paths now use typed fixed-schema parameter models with `BicepParameterModelConverter`; `BicepFormattingHelper` and `ParameterFileAssembler` honor `JsonPropertyName` during serialization and environment-override merges; all 18 `*TypeBicepGenerator` files were swept so semantic Bicep literals in builder/generator code now live in constants; generator-specific literals stay file-local, while the small shared set with a stable cross-generator contract now lives in `Generators/Constants/BicepGeneratorSharedConstants.cs` (`name`, `location`, `properties`, `kind`, `id`, `./types.bicep`, `true`, `false`); `.github/instructions/code-quality-guardrails.instructions.md` now codifies both the strict no-inline rule and the narrow shared-constants exception. |
| 2026-04-30 | dev | **Coverage, Sonar, README, and Windows output consolidation.** Expanded Application validator/handler coverage, normalized LF output in `BicepEmitter` and `ConfigVarsStage`, completed the Sonar remediation wave and local zero-diagnostic sweep, rewrote the GitHub landing page + README MCP onboarding, restored missing ARM import models, fixed the WebApp legacy generator build, and documented the parallel git-worktree workflow. |
| 2026-04-29 | dev | **MCP/import/runtime hardening wave + Graphify/agent alignment.** Consolidated the MCP HTTP runtime/docs/PAT setup, optional-subscription draft behavior, Mapster and blob-host fixes, missing direct dependency injection, enum-safe default normalization, typed dispatch/coordinator/import-model refactors, Graphify corpus integration, stricter code-quality guardrails and new agents, plus DS date-picker enhancements and typed ARM parsing/property extraction. |
| 2026-04-28 | dev | **PAT + MCP HTTP + Sonar wave.** Landed end-to-end PAT authentication, completed MCP V1 over HTTP under Aspire, and shipped the related Sonar/security quick wins across generators, ZIP guards, fonts, and Docker assets. |
| 2026-04-27 | dev | **Generation and tooling milestone.** Completed the staged pipeline refactor, the Bicep V2 Builder+IR migration, and the associated review/TDD/xUnit tooling wave. |
| 2026-04-26 | dev | **Frontend polish.** DockerfilePicker brand colors, wizard footer sticky, split download ZIPs repo-scoped, bootstrap preview split, config-detail Generate/Push gated to MultiRepo. |
| 2026-04-25 | dev | **Create-project wizard V1 + SplitInfraCode generation.** 4/5-step stepper, bootstrap split, MultiRepoPushDialog, DsPanelActionButton, generic UAI param, dream lock. |
| 2026-04-24 | dev | Angular 21 + DS rollout. 216 form fields migrated. Global Material override. SplitInfraCode Common/ restored. |
| 2026-04-23 | dev | Multi-repo wave: audit fixes, LayoutPreset, SplitInfraCode dual-repo gen/push, bootstrap ADO, frontend multi-repo UX. |
| 2026-04-22 | copilot | Custom domains E2E, bootstrap ADO hardening, health probes, grouped Bicep params, CAE LAW security, SQL Server fixes. |
| 2026-04-21 | copilot | Bicep hardening: secure params, output validation, LAW fallback, artifact path sanitization. |
| 2026-04-20 | copilot | Infra release artifact flow standardized. |
| 2026-04-17 | copilot | .bicepparam filenames switched to environment short names. |
| 2026-04-16 | copilot | Sealed enum VOs, string DTO ids, .ProducesProblem(401), camelCase Bicep, Aspire decoupling. |
| 2026-04-15 | copilot | Merged main into DDD branch, sealed AzureResource aggregates. |
| 2026-04-14 | copilot | Audit scripts: Windows PS 5.1 compat. |
| 2026-04-13 | copilot | Draw.io tooling added. |
| 2026-04-04 | copilot | Generation/pipeline stabilization, BicepAssembler refactored, FK cascade fixes. |
| 2026-04-03 | copilot | App pipeline generation, GitNexus integration. |
| 2026-04-02 | copilot | User-managed identity refactored across all layers. |
| 2026-04-01 | copilot | Windows-first conventions, general-config cleanup. |
| 2026-03-31 | copilot | Event Hub namespace, configuration keys UX. |
| 2026-03-30 | copilot | ACR pull roles, variable groups, Unit of Work, domain quality. |
| 2026-03-29 | copilot | Azure DevOps pipeline YAML generation. |
| 2026-03-28 | copilot | Unified generation UX, mono-repo pipeline, architect agent. |
| 2026-03-27 | copilot | UAI grouping, Storage Account CORS/lifecycle. |
| 2026-03-26 | copilot | Storage Account CORS UX, queue/table Bicep. |
| 2026-03-24 | copilot | Log Analytics Workspace + Application Insights E2E. |
