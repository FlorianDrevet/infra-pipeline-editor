# Changelog

> Entries older than 60 days are pruned during dream consolidation.
> Entries from the same day are consolidated into a single durable summary.

- [2026-05-15] `dev`, `audit-expert`, `aspire-debug`, `dream` — Delivered the privatization/networking wave end-to-end (`VirtualNetwork`, `NetworkSecurityGroup`, `PrivateDnsZone`, `FrontDoor`, private endpoints, 5 Bicep generators, Angular networking UI), published `audits/audit-15-05-2026.md`, closed Phases 0-3 of that audit plus the last PR `#395` Sonar items (`AppPipelineStepOptionsData`, typed pipeline-step mapper, Subnet regex timeout), fixed shared API/MCP startup with EF migration `20260515102033_AddPipelineStepOptionsOwnedEntity`, repaired the follow-up Angular build regression (`CreateWebAppRequest.isExisting`, variable-groups controller brace balance, stale networking-tab DS import), fixed the authenticated API crash in `UserProvisioningService` caused by backslash-escaped identifiers inside a raw PostgreSQL SQL string, validated build/tests/Aspire, and refreshed the endpoint/code-graph memory cache.
- [2026-05-15] `dev`, `angular-front` — Fixed contextual sidebar routing in the Angular shell: project/config entries now deep-link their internal tabs through shared `tab` query-param constants, `project-detail` and `config-detail` bind `mat-tab-group` selection to the URL, and focused sidebar specs plus frontend `typecheck`/`build` are green.
- [2026-05-15] `dev`, `angular-front` — Restored the project generation board explorer on `/projects/:id/generate`: the page now reuses `ProjectDetailGenerationWorkflowService` to generate and browse artifacts again (mono-repo via `app-bicep-file-panel`, SplitInfraCode via `SplitGenerationSwitcherComponent`), and the target repositories cards keep their repo metadata while dropping the bottom per-config list.
- [2026-05-15] `dev`, `angular-front` — Reworked the `project-detail` environments tab: the old compact `env-card` strip was replaced with a dedicated standalone section component that renders each environment as a full-width structured panel (`Overview`, `Naming`, `Governance`, `Tags`) while preserving the existing add/edit/delete flows; focused Karma spec, frontend `typecheck`, and `build` are green.
- [2026-05-15] `dev`, `angular-front` — Improved the shared generated-file reader UX in `app-bicep-file-panel`: opening a file now scrolls directly to the code viewer, the viewer exposes a localized return action to jump back to the active file entry in the tree, and focused Karma coverage plus frontend `typecheck`/`build` remain green.
- [2026-05-14] `dev`, `dream` — Delivered pipeline step options end-to-end, fixed the shared `vw_ResourceEnvironmentEntries` migration trap in `20260514104001_SyncPendingModelChanges`, revalidated full build/tests/Aspire, and consolidated project memory.
- [2026-05-13] `dev`, `angular-front` — Audit 2026-05 execution was consolidated into the durable state: `audits/audit-13-05-2026.md` recorded `66` findings / `65` GitHub issues, `audits/correction-plan-13-05-2026.md` drove the P0–P4 remediation waves, and `audits/triage-2026-05-13.md` is now the source of truth with `13` remaining open issues. Durable outcomes captured in thematic memory include API/MCP hardening (`traceId`, PAT usage throttling, coverage collector, MCP headers/rate limiting/HTTP warning, additive cancellation propagation), domain/persistence/CQRS guardrails (`AzureResource` encapsulation, `IUserProvisioningService`, domain events, detached read defaults, EF length/include guardrails, validator and DTO snapshot tests), generation fixes (multi-ACR existing-resource resolution, existing-resource API-version coverage, representative Bicep CI validation, narrower pipeline exception remapping), and frontend/UI refactors (enterprise shell/sidebar/footer refresh, generation board page, shared resource metadata, large-component decomposition, build-budget fix).
- [2026-05-12] `dev` — Large PR #328 remediation wave: validators and command guardrails (`APP-001`), read-only repository splits and EF constraints (`DB-001` / `DB-003` / `DB-006`), Bicep/pipeline hardening (`GEN-*`), route-name constants, request limits/CORS/rate limiting/tag/path security, triage resync, and blue-presence frontend theme refresh; end-of-day full solution build and tests were green.
- [2026-05-11] `dev`, `angular-front`, `architect` — Frontend enterprise UI refresh waves 4-7, DS polish, project/config-detail UX refinements, import-app-settings multi-format, PR #327 Sonar/review cleanup, and Bicep/RBAC/Key Vault fixes; targeted frontend/backend validations were green.
- [2026-04-30] `dev` — Coverage/Sonar/README consolidation: expanded tests, LF output normalization, GitHub landing-page rewrite, Windows-first output cleanup, and refreshed parallel-worktree guidance.
- [2026-04-29] `dev` — MCP/import/runtime hardening plus Graphify/agent alignment: HTTP MCP under Aspire, typed import flows, dependency expansion/default normalization, PAT docs, and stricter code-quality guardrails.
- [2026-04-28] `dev` — PAT authentication and MCP V1 over HTTP delivered, plus Sonar/security quick wins across generators, ZIP handling, fonts, and Docker assets.
- [2026-04-27] `dev` — Bicep staged pipeline refactor, Builder+IR migration, and TDD/xUnit tooling milestone completed.
- [2026-04-26] `dev` — Frontend polish: Dockerfile picker brand fix, sticky wizard footer, repo-scoped split ZIP downloads, split bootstrap preview, and MultiRepo gating in config-detail.
- [2026-04-25] `dev` — Create-project wizard V1, SplitInfraCode generation/push, bootstrap split, DS panel actions, generic UAI parameter, and Dream lock workflow.
- [2026-04-24] `dev` — Angular 21 plus design-system rollout, global Material overrides, and `Common/` restore for SplitInfraCode generation.
- [2026-04-23] `dev` — Multi-repo/layout-driven repository wave with audit fixes, SplitInfraCode dual-repo generation/push, bootstrap ADO, and frontend multi-repo UX.
- [2026-04-22] `copilot` — Custom domains end-to-end, bootstrap ADO hardening, Container App health probes and grouped params, CAE LAW security, and SQL Server fixes.
- [2026-04-21] `copilot` — Bicep hardening: secure params, output validation, LAW fallback, and artifact path sanitization.
- [2026-04-20] `copilot` — Infra release artifact flow standardized.
- [2026-04-17] `copilot` — `.bicepparam` filenames switched to environment short names.
- [2026-04-16] `copilot` — Sealed enum value objects, string response IDs, `.ProducesProblem(401)` coverage, camelCase Bicep normalization, and API/Aspire decoupling.
- [2026-04-15] `copilot` — `main` merged into the DDD branch and AzureResource aggregates sealed.
- [2026-04-14] `copilot` — Audit scripts made Windows PowerShell 5.1 compatible.
- [2026-04-13] `copilot` — draw.io tooling added.
- [2026-04-04] `copilot` — Generation/pipeline stabilization, BicepAssembler refactor, and FK cascade fixes.
- [2026-04-03] `copilot` — App pipeline generation and GitNexus integration.
- [2026-04-02] `copilot` — User-assigned identity refactor completed across layers.
- [2026-04-01] `copilot` — Windows-first conventions and general-config cleanup.
- [2026-03-31] `copilot` — Event Hub namespace and configuration-keys UX delivered.
- [2026-03-30] `copilot` — ACR pull roles, variable groups, Unit of Work, and domain quality rules added.
- [2026-03-29] `copilot` — Azure DevOps pipeline YAML generation added.
- [2026-03-28] `copilot` — Unified generation UX, mono-repo pipeline, and architect agent introduced.
- [2026-03-27] `copilot` — UAI grouping and Storage Account CORS/lifecycle work delivered.
- [2026-03-26] `copilot` — Storage Account CORS UX plus queue/table Bicep support added.
- [2026-03-24] `copilot` — Log Analytics Workspace and Application Insights end-to-end delivered.
