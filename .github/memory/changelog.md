# Changelog

> Entries older than 60 days are pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-04 | copilot | Configurable agent pool: `AgentPoolName` on InfrastructureConfig, `PUT /infra-configs/{id}/agent-pool`, `PipelineGenerationEngine.AppendPool()`. EF migration `PoolPipeline`. |
| 2026-04-04 | copilot | ContainerAppEnvironment: moved `LogAnalyticsWorkspaceId` from per-env to aggregate root (optional FK to LAW AzureResourceId). EF migration `AceWithLaw`. |
| 2026-04-04 | copilot | ResourceGroup DELETE endpoint: `DELETE /resource-group/{id}` + frontend delete button with confirm dialog. |
| 2026-04-04 | copilot | BicepAssembler refactor: monolith (~1680 lines) -> thin orchestrator (~180 lines) + 14 specialized classes (Assemblers/, Helpers/, StorageAccount/, Models/). |
| 2026-04-04 | copilot | Fix ALL cross-resource FK cascade issues for project deletion. 3 migrations: `FixRoleAssignmentTargetCascadeDelete`, `FixAllRestrictCascadeDeleteForProjectDeletion`, `FixCrossResourceFKCascadeForProjectDeletion`. |
| 2026-04-04 | copilot | App Pipeline Rework: merged into infra pipeline (POST /generate-pipeline produces both). Removed standalone app pipeline endpoint. Added `ApplicationName` + `AppPipelineMode` (Isolated/Combined). |
| 2026-04-03 | copilot | Application Pipeline Generation: domain props + AppPipelineGenerationEngine + 5 generators + CQRS + frontend App Pipeline tab. |
| 2026-04-03 | copilot | Memory audit via GitNexus: fixed aggregate/resource counts, rewrote 12-api-endpoints (27 controllers), enriched 13-code-graph. |
| 2026-04-03 | copilot | GitNexus integration: `gitnexus-workflow` skill, agent modifications, `13-code-graph.md` memory file. |
| 2026-04-03 | copilot | ContainerApp: `DockerImageName` at resource level, fix Bicep per-env params, ACR/UAI reactivity fixes. |
| 2026-04-02 | copilot | Full Assign Identity refactor: FK on AzureResource, CQRS, Bicep, frontend. Config diagnostics. dev.agent Coordinator v2. |
| 2026-04-01 | copilot | Windows env + build-only convention. Fix impact dialog i18n. RuntimeStack/RuntimeVersion to general-config. |
| 2026-03-31 | copilot | EventHubNamespace aggregate full-stack. Configuration Keys tab redesign. |
| 2026-03-30 | copilot | UAI-based ACR role assignment. PVG refactor. Unit of Work. Domain XML docs. Pipeline Variable Groups. |
| 2026-03-29 | copilot | Pipeline Generation Engine -- real YAML output. AzureResourceManagerConnection. Remove TenantId. |
| 2026-03-28 | copilot | Unified generation UX. Mono-repo pipeline gen. Secret/static app settings. Architect agent. |
| 2026-03-27 | copilot | UAI grouping. ValueObject nullable fix. StorageAccount CORS+lifecycle to Storage tab. |
| 2026-03-26 | copilot | StorageAccount CORS redesign. Storage queue/table Bicep. Module renaming. |
| 2026-03-24 | copilot | LogAnalyticsWorkspace + ApplicationInsights -- full stack (51 new files). |