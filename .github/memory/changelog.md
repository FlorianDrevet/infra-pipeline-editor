# Changelog

> Entries older than 60 days are pruned during dream consolidation.
> Entries from the same day are consolidated into a single durable summary.

- [2026-05-13] `dev` — Plan d'intégration des options modulaires de pipeline applicatif livré dans `docs/features/pipeline-options-plan.md` : catalogue de 12 options (TU, couverture, Sonar, linting, security scans, cache, smoke tests), détection automatique de framework (.NET/Node/Python/Java), architecture domain→generation→frontend, exemples YAML, plan en 5 phases.
- [2026-05-13] `dev` — APP-005 closed with `IProjectPipelineAggregator` + `IMonoRepoBlobUploadOrchestrator`, DB-008 closed as a release-baseline decision (no mid-chain EF squash on this branch), GitHub issues `#183`, `#185`, `#196`, `#205`, `#213`, and `#214` closed, and triage dropped to 33 open issues / 12 real backlog items.
- [2026-05-13] `dev` — APP-001 and DB-003 closed on branch `refacto/audit-12-05`: remaining command validators completed with the structural guard `AllCommandsHaveValidatorsTests`, detached read-only swaps finalized across the last query handlers plus `AppPipelineRequestFactory` / `ApplicationFolderNameResolver`, GitHub issues `#168` and `#178` closed, triage resynced to 39 open issues / 14 real backlog items, and full solution build/tests stayed green.
- [2026-05-13] `dev` — UX/UI audit complet livré dans `audits/ux-audit-2026-05-13.md` : 20 findings (sidebar vide, footer sans mentions légales, génération mélangée avec configuration, droits dispersés, accessibilité), benchmarks SonarQube/GitHub/Azure Portal/Terraform Cloud, architecture cible avec sidebar contextuelle 3 modes + séparation Define/Generate/Manage, plan d'implémentation en 6 phases.
- [2026-05-13] `dev` — Project-level Bicep generation now infers missing cross-config Container Registry existing-resource references from compute `containerRegistryId` links before `GenerateMonoRepo`, closing the real ACR login-server gap behind project `fb8699ea-f568-4afb-864b-e82d2efd0905`; focused `Application.Tests` and related `BicepGeneration.Tests` regressions are green.
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
