# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-29 | dev | **PR #324 vibe-coding review remediation.** Fixed 7 findings: BLOCKER-001 (deduplicated ResourceCommandFactory → single shared class in Application), BLOCKER-002 (removed reflection from orchestrator), HIGH-003 (renamed `TokenPrefix_` → `TokenPrefix`), HIGH-004 (persist `RecordUsage` via `SaveChangesAsync`), HIGH-005 (TTL eviction on singleton stores + `InMemoryCleanupService` background cleanup), HIGH-006 (explicit `.RequireAuthorization()` on MCP route), MEDIUM-008 (rethrow `OperationCanceledException`). Drafts now cleaned up after project creation. Tests updated for typed MediatR mocks. All 937+ tests pass. |
| 2026-04-29 | dev | **Magic strings eliminated in import layer.** Added `IacSourceFormat`, `ImportPreviewGapCategory`, `ImportDependencyType` public constants in Application. Deleted MCP's duplicate `IacSourceFormat.cs`. All 105 MCP tests green. |
| 2026-04-29 | dev | **ARM template parsing strongly typed.** Replaced `JsonDocument.Parse` + manual `TryGetProperty` with `JsonSerializer.Deserialize<ArmTemplateDocument>`. New models: `ArmTemplateDocument`, `ArmResource`, `ArmSku` in `Application/Imports/Common/Arm/`. 20/20 analyzer tests green. |
| 2026-04-29 | dev | **Agent anti-vibe coding ajouté.** New `.github/agents/vibe-coding-refractaire.agent.md` provides a harsh senior PR review pass focused on vibe-coding smells and low-signal generated code. Wired into `review-main`, `pr-manager`, `dev.agent.md`, `.github/copilot-instructions.md`, and `.github/memory/11-agents-skills.md` so technical review and PR creation now require this second-pass gate. |
| 2026-04-29 | dev | **MCP code quality refactoring.** One-class-per-file (29 new files), `McpJsonDefaults` (eliminates 7 dupes), `IacSourceFormat` constant, `LayoutPresetEnum` domain enum, typed `TopologyInfo`/`ResourceTypeInfo` records. 104 MCP tests, 940 total. |
| 2026-04-29 | dev | **MCP resource creation end-to-end.** `ResourceCommandFactory` (18 types, dependency ordering) + `ProjectSetupOrchestrator`. Golden ARM template tests. |
| 2026-04-29 | dev | **Import preview/apply extracted to Application layer.** `IImportPreviewAnalyzer`, `PreviewIacImportQuery`, `ApplyImportPreviewCommand` now in `Application/Imports/`. API endpoints `POST /imports/preview` + `POST /imports/apply`. MCP retains preview-ID storage. |
| 2026-04-29 | dev | **DS Date Picker + Settings/PAT DS migration.** `app-ds-date-picker` (CDK overlay, calendar, year-selection, CVA). PAT table: `app-ds-chip` + shared `@mixin ifs-data-table`. Nav icon fix. DS integration rule (pitfall #12). |
| 2026-04-28 | dev | **PAT authentication end-to-end.** `PersonalAccessToken` aggregate, CQRS commands, EF migration, custom auth handler, MCP secured with PAT, frontend `/settings` page. |
| 2026-04-28 | dev | **MCP V1 COMPLETE + HTTP migration.** 8 tools, 2 resources, 1 prompt (ModelContextProtocol SDK v1.2.0). Migrated stdio to ASP.NET Core HTTP `/mcp` under Aspire with PAT auth. MCP skill created + expanded. |
| 2026-04-28 | dev | **Sonar wave.** S1192 semantic constants (3 generators), secret blocker fix, backend quick-wins (regex timeouts, hotspots), duplicate code reduction (6 shared helpers, CPD exclusions), ZIP guards, local font bundling, Docker tightening. |
| 2026-04-27 | dev | **Pipeline refactor Vague 1 COMPLETE.** 3 engines decomposed into staged facades. -335 LOC. 91 tests (44 golden + 47 stage). |
| 2026-04-27 | dev | **Bicep V2 migration COMPLETE.** 18 generators migrated to Builder+IR. IrOutputPruningStage + OutputUsageTracker. 842 tests. |
| 2026-04-27 | dev | **Tooling wave.** bicep-v2-migration, tdd-workflow, xunit-unit-testing skills. review-expert + review-remediator agents. License PolyForm NC 1.0. |
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
