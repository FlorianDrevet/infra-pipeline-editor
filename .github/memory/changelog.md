# Changelog

> Entries older than 30 days should be pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-03 | copilot | Memory audit via GitNexus: Fixed aggregate count (21→22), AzureResource children count (21→18), resource type identifiers (17→18), removed ghost entity `ResourceEnvironmentConfig`, added missing base entities (AppSetting, AppSettingEnvironmentValue, InputOutputLink, RoleAssignment) to 03-domain-model, added ResourceNamingTemplate/CrossConfigResourceReference to InfrastructureConfig entities, completely rewrote 12-api-endpoints with all 27 controllers, enriched 13-code-graph with verified counts, fixed incorrect `/infra-config/generate-bicep` route. |
| 2026-04-03 | copilot | GitNexus integration: new `gitnexus-workflow` skill, modified architect/dotnet-dev/angular-front/dev/dream agents with impact analysis + detect_changes, new `13-code-graph.md` memory file, updated copilot-instructions + 11-agents-skills registry. |
| 2026-04-03 | copilot | ContainerApp: remove `containerImage` from per-env (now resource-level `DockerImageName`). Fix Bicep generation to emit all typed per-env params. Fix ACR UAI card + ACR pull banner reactivity for WebApp/FunctionApp/ContainerApp. Fix assigned UAI visibility in role assignments list. Assign UAI: revert dialog approach — now converts SAI→UAI with dedup. |
| 2026-04-02 | copilot | Full Assign Identity refactor: `AssignedUserAssignedIdentityId` FK on AzureResource, CQRS commands, API endpoints, Bicep engine, frontend. Auto-expand identity group. Auto-select UAI in role dialog. Remove FunctionsWorkerRuntime from FunctionApp per-env. Fix WebApp/FunctionApp creation modals. `DockerImageName` at resource level for ContainerApp. Tab warning badges. Config diagnostics modal + missing env settings section. Skip impact dialog when no impact. ACR banner UX improvements. KV missing role warnings. dev.agent.md Coordinator Mode v2 (scratchpad, @Explore, delegation rules). |
| 2026-04-01 | copilot | Windows env + build-only convention in agents. Fix impact dialog i18n/contrast. RuntimeStack/RuntimeVersion to general-config only. Remove LastRoleToTarget warning. |
| 2026-03-31 | copilot | EventHubNamespace aggregate full-stack. Configuration Keys tab visual redesign. |
| 2026-03-30 | copilot | UAI-based ACR role assignment flow. PVG refactor. Unit of Work pattern. Domain XML docs. Pipeline Variable Groups full stack. |
| 2026-03-29 | copilot | Pipeline Generation Engine — real YAML output. AzureResourceManagerConnection end-to-end. Remove TenantId. |
| 2026-03-28 | copilot | Unified generation UX. Mono-repo Pipeline Generation. Secret/static app settings per environment. architect agent. Identity injection mixed mode. |
| 2026-03-27 | copilot | UAI grouping in Identity & Access tab. ValueObject nullable operators fix. StorageAccount CORS + lifecycle to Storage Services tab. EF Core PendingModelChangesWarning fix. |
| 2026-03-26 | copilot | StorageAccount CORS editor redesign. Storage queue/table Bicep generation. Module file renaming. |
| 2026-03-24 | copilot | LogAnalyticsWorkspace + ApplicationInsights aggregates — full stack (51 new files). |
