# Changelog

> Entries older than 30 days should be pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-04 | copilot | Add configurable agent pool for pipeline generation: Domain (`AgentPoolName` on InfrastructureConfig), EF Core config, ReadModel, GenerationRequest + AppPipelineGenerationRequest, all 10 hardcoded pool refs replaced with `AppendPool()` helper, SetAgentPool CQRS command + API endpoint `PUT /infra-configs/{id}/agent-pool`, Contracts request/response, Mapster mapping. 18 files modified. |
| 2026-04-05 | copilot | ContainerAppEnvironment LAW selector: moved `logAnalyticsWorkspaceId` from per-env settings to aggregate root as optional FK (`AzureResourceId?`). Backend: Domain, Application (commands/handlers/result), Contracts, Mapster, EF Core config, ReadRepository. Frontend: dropdown selector in resource-edit general form + add-resource-dialog parent selector step (optional). 22 files modified total. |
| 2026-04-04 | copilot | Fix cross-config ACR dropdown: resource-edit and add-resource-dialog now load ContainerRegistries from all configs in the same project via getProjectResources API. Cross-config ACRs show with `(configName)` suffix. |
| 2026-04-04 | copilot | Add DELETE /resource-group/{id} endpoint + frontend delete button with confirm dialog on config-detail page. Backend command/handler already existed. |
| 2026-04-04 | copilot | Refactor BicepAssembler: monolith (~1680 lines) → thin orchestrator (~180 lines) + 14 specialized classes (Assemblers/, Helpers/, StorageAccount/, Models/). Public API unchanged. |
| 2026-04-04 | copilot | Fix project deletion FK cascade: RoleAssignment.TargetResourceId → Cascade, AppSettings/AppConfigKeys source/KV FKs → SetNull, ResourceLinks/Dependencies/ParameterUsages → Cascade. 3 EF migrations. |
| 2026-04-04 | copilot | App Pipeline Rework: merged into infra pipeline POST /generate-pipeline. Removed separate endpoint + GenerateAppPipeline CQRS. Added ApplicationName on compute resources, AppPipelineMode (Isolated/Combined) on InfraConfig. Frontend: moved pipeline fields to App Pipeline tab. |
| 2026-04-03 | copilot | App Pipeline Generation v1 (domain props, engine, 5 generators, CQRS, frontend tab). ContainerApp per-env fix. ACR/UAI reactivity fixes. GitNexus integration (skill, agents, 13-code-graph.md). Memory audit (corrected aggregate counts, rewrote 12-api-endpoints). |
| 2026-04-02 | copilot | Full Assign Identity refactor: `AssignedUserAssignedIdentityId` FK on AzureResource, CQRS commands, API endpoints, Bicep engine, frontend. Auto-expand identity group. Auto-select UAI in role dialog. Remove FunctionsWorkerRuntime from FunctionApp per-env. Fix WebApp/FunctionApp creation modals. `DockerImageName` at resource level for ContainerApp. Tab warning badges. Config diagnostics modal + missing env settings section. Skip impact dialog when no impact. ACR banner UX improvements. KV missing role warnings. dev.agent.md Coordinator Mode v2 (scratchpad, @Explore, delegation rules). |
| 2026-04-01 | copilot | Windows env + build-only convention in agents. Fix impact dialog i18n/contrast. RuntimeStack/RuntimeVersion to general-config only. Remove LastRoleToTarget warning. |
| 2026-03-31 | copilot | EventHubNamespace aggregate full-stack. Configuration Keys tab visual redesign. |
| 2026-03-30 | copilot | UAI-based ACR role assignment flow. PVG refactor. Unit of Work pattern. Domain XML docs. Pipeline Variable Groups full stack. |
| 2026-03-29 | copilot | Pipeline Generation Engine — real YAML output. AzureResourceManagerConnection end-to-end. Remove TenantId. |
| 2026-03-28 | copilot | Unified generation UX. Mono-repo Pipeline Generation. Secret/static app settings per environment. architect agent. Identity injection mixed mode. |
| 2026-03-27 | copilot | UAI grouping in Identity & Access tab. ValueObject nullable operators fix. StorageAccount CORS + lifecycle to Storage Services tab. EF Core PendingModelChangesWarning fix. |
| 2026-03-26 | copilot | StorageAccount CORS editor redesign. Storage queue/table Bicep generation. Module file renaming. |
