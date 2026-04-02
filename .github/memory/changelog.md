# Changelog

> Entries older than 30 days should be pruned during dream consolidation.

| Date | Author | Change |
|------|--------|--------|
| 2026-04-02 | copilot | Remove FunctionsWorkerRuntime from FunctionApp per-env (full stack) — derived automatically in Bicep from runtimeStack+runtimeVersion. Migration: `RemoveFunctionsWorkerRuntimeFromFunctionAppEnvSettings`. |
| 2026-04-02 | copilot | Fix FunctionApp creation modal — remove runtimeStack/runtimeVersion from per-env tab (now general config only). `add-resource-dialog`: env form group + builder + HTML template. |
| 2026-04-02 | copilot | Fix WebApp creation modal — remove runtimeStack/runtimeVersion from per-env tab (now general config only). `add-resource-dialog`: env form group + builder + HTML template. |
| 2026-04-02 | copilot | dev.agent.md — 3 Coordinator Mode upgrades: (1) step 4bis Plan→Execute→Verify loop, (2) Task Budget IN/OUT OF SCOPE dans template scratchpad, (3) section "Discipline de prompt Static vs Dynamic". Checklist de fin de tâche mise à jour. |
| 2026-04-02 | copilot | dev.agent.md — Coordinator Mode improvements: Research phase (@Explore, step 2bis), Session Scratchpad (/memories/session/, step 2ter), Precise Delegation rule (no vague prompts), @Explore added to routing table, end-of-task checklist updated. |
| 2026-04-02 | copilot | Fix ACR re-check reactivity for ContainerApp and WebApp after role removal. Added `isAcrEnabled` computed as single source of truth. |
| 2026-04-02 | copilot | KV missing role warning for AppConfiguration Config Keys tab — full stack. |
| 2026-04-02 | copilot | ACR banner UX improvements — no-UAI message, yellow card, why-UAI accordion. |
| 2026-04-02 | copilot | KV missing role — UAI chip when single UAI. |
| 2026-04-02 | copilot | DockerImageName at resource level for ContainerApp — full stack. Migration: `AddDockerImageNameToContainerApp`. |
| 2026-04-02 | copilot | Tab warning badges on resource-edit tabs (General, Environments, App Settings). |
| 2026-04-02 | copilot | Missing environment settings section in generation diagnostics modal. |
| 2026-04-02 | copilot | Skip impact dialog when no impact on role assignment delete. |
| 2026-04-02 | copilot | Configuration Diagnostics — visual indicators for missing RBAC roles (full stack). |
| 2026-04-01 | copilot | Remove LastRoleToTarget warning from impact dialog. |
| 2026-04-01 | copilot | Windows environment + build-only convention added to all agents. |
| 2026-04-01 | copilot | Fix role-assignment-impact-dialog: i18n + contrast + title icon. |
| 2026-04-01 | copilot | RuntimeStack/RuntimeVersion moved to general-config only + version dropdown. |
| 2026-03-31 | copilot | Added EventHubNamespace aggregate — full-stack CQRS. |
| 2026-03-31 | copilot | Configuration Keys tab visual redesign. |
| 2026-03-30 | copilot | UAI-based ACR role assignment flow. PVG refactor. Unit of Work pattern. Domain XML docs. Pipeline Variable Groups full stack. |
| 2026-03-29 | copilot | Pipeline Generation Engine — real YAML output. AzureResourceManagerConnection end-to-end. Remove TenantId. |
| 2026-03-28 | copilot | Unified generation UX. Mono-repo Pipeline Generation. Secret static app settings. Static app settings per environment. architect agent creation. Identity injection mixed mode. |
| 2026-03-27 | copilot | UAI grouping in Identity & Access tab. ValueObject nullable operators fix. StorageAccount CORS + lifecycle to Storage Services tab. EF Core PendingModelChangesWarning fix. |
| 2026-03-26 | copilot | StorageAccount CORS editor redesign. Storage queue/table Bicep generation. Module file renaming. |
| 2026-03-24 | copilot | LogAnalyticsWorkspace + ApplicationInsights aggregates — full stack (51 new files). |
