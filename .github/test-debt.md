# Test Debt Tracker

> **Ce fichier recense la dette de tests unitaires détectée par les agents.**
> Chaque agent qui modifie du code et constate l'absence de tests sur la zone touchée
> DOIT ajouter une entrée ici. Les agents peuvent ensuite résorber la dette par priorité.

---

## Convention

- **P1 — Critique** : Code métier non testé, risque de régression élevé (domain, handlers, validators)
- **P2 — Important** : Code infrastructure/services non testé, impact modéré (repositories, mappers, transformers)
- **P3 — Souhaitable** : Code utilitaire ou configuration non testé, impact faible (extensions, options, helpers)

---

## Debt Register

| # | Assembly | Zone / Classe | Description | Priorité | Détecté le | Résolu le | Résolu par |
|---|----------|---------------|-------------|----------|------------|-----------|------------|
| 1 | `InfraFlowSculptor.Domain` | All aggregates | Comprehensive invariant test coverage added across 22 aggregates + value objects (468 tests) | P1 | 2026-04-27 | 2026-04-30 | dev |
| 2 | `InfraFlowSculptor.Application` | All handlers/validators | Initial coverage of Project / InfrastructureConfig / ResourceGroup + 12 AzureResource Create/Delete/Get handlers, plus focused validation coverage for `CreateProjectWithSetupCommandValidator` cross-rules (231 tests). Update / SetEnvironment / multi-aggregate orchestration handlers still uncovered. | P1 | 2026-04-27 | 2026-04-30 | dev (partial) |
| 3 | `InfraFlowSculptor.Infrastructure` | Repositories, services | Initial coverage of 6 critical repositories with EF Core InMemory (44 active + 9 skipped due to ComplexProperty/Owned-type InMemory limits). Auth services, Refit clients, Azure clients, BlobService still untested beyond pre-existing slice. | P2 | 2026-04-27 | 2026-04-30 | dev (partial) |
| 4 | `InfraFlowSculptor.Api` | Controllers, DI, error mapping | No unit tests for endpoint registration, Mapster configs, error conversion | P2 | 2026-04-27 | | |
| 5 | `InfraFlowSculptor.PipelineGeneration` | Pipeline generators | No dedicated test project (only parity tests) | P2 | 2026-04-27 | 2026-04-27 | dev |
| 6 | `InfraFlowSculptor.GenerationCore` | Shared generation abstractions | No dedicated test project | P3 | 2026-04-27 | | |
| 7 | `InfraFlowSculptor.PipelineGeneration` | Bootstrap and app pipeline generators | Dedicated test project now exists, but most generator paths are still uncovered beyond the bootstrap pipeline warning regression | P2 | 2026-04-27 | 2026-04-27 | dev |
| 8 | `InfraFlowSculptor.PipelineGeneration` | 4 engines (PipelineGenerationEngine, BootstrapPipelineGenerationEngine, AppPipelineGenerationEngine, MonoRepoPipelineAssembler) | Byte-for-byte parity sentinels via golden file tests covering all 4 engines + R2 frozen-set lock on AppPipeline shared templates (20 paths) — 22 tests + 83 captured goldens. Behavioral coverage of internal stages still missing (will be added during Vagues 1.1/1.2/1.3 stage decomposition via TDD). | P2 | 2026-04-27 | 2026-04-27 | dev |
| 9 | `InfraFlowSculptor.Contracts` | Request and validation DTOs | Coverage extended to all custom validation attributes (Enum/Guid/RedisVersion) + 16 representative request DTOs across major features (101 tests). Remaining: response DTOs, less-critical request DTOs lacking attributes (noted in observations). | P2 | 2026-04-28 | 2026-04-30 | dev (partial) |
| 10 | `Front (Angular)` | Local Sonar quick-win helpers | No focused frontend specs currently cover the local CORS validators, generated artifact archive path sanitization, alias normalization helpers, split-generation tree typing helpers, push-dialog dispatch helpers, app setting/config key name normalizers, or Bicep highlight pipe. Validation is currently limited to `npm run typecheck` and `npm run build`. | P2 | 2026-04-28 | | |
| 11 | `InfraFlowSculptor.Application` | Imports/PreviewIacImport analyzer + query | A dedicated `InfraFlowSculptor.Application.Tests` project now exists and covers the apply-import handler slice, but the ARM preview analyzer and `PreviewIacImportQueryHandler` are still exercised only through `InfraFlowSculptor.Mcp.Tests`. | P1 | 2026-04-28 | | |
| 16 | `InfraFlowSculptor.Application` | Update / SetEnvironment / SubResource / PushToGit / Generate handlers across all features | Batch 2 only covered Create/Delete/Get for 12 AzureResource features. Update*, SetEnvironmentSettings*, AddBlobContainer/AddSecret/AddRoleAssignment, PushToGit, Generate*, Download*, CreateProjectWithSetup orchestration handlers remain uncovered. Many require Refit/file-system/Bicep gen mocks; better as integration tests. | P2 | 2026-04-30 | | |
| 17 | `InfraFlowSculptor.Infrastructure` | UserRepository / ProjectRepository (queries with Members.User include) | EF Core InMemory provider cannot translate queries that materialize User due to `builder.ComplexProperty(user => user.Name)`. 9 tests skipped pending integration-level harness (Sqlite :memory: or Testcontainers PostgreSQL). | P2 | 2026-04-30 | | |
| 18 | `InfraFlowSculptor.Infrastructure` | Auth services, GitProviders Refit clients, AzureNameAvailability, BlobService (write paths), KeyVault clients | Service-layer coverage limited to existing Services/ tests (BlobService read path). External-dependency clients still untested. | P2 | 2026-04-30 | | |
| 19 | `InfraFlowSculptor.Api` | Mapster configs, controllers, error mapping | Still no `InfraFlowSculptor.Api.Tests` project. Mapster configurations and minimal-API endpoint registrations are uncovered. | P2 | 2026-04-30 | | |
| 20 | `InfraFlowSculptor.BicepGeneration.Tests` | Pre-existing test failure (BicepEmitterTests.Given_MinimalSpec_When_EmitModule_Then_ReturnsExpectedDocument) | Resolved by canonicalizing emitted text line endings to LF in `BicepEmitter.EnsureTrailingNewline()`. The prior FluentAssertions formatting error only masked an underlying CRLF vs LF mismatch on Windows. | P1 | 2026-04-30 | 2026-04-30 | dev |
| 21 | `InfraFlowSculptor.PipelineGeneration.Tests` | Pre-existing test failure (ConfigVarsStageTests.Given_StandardContext_When_Execute_Then_EmitsExpectedYaml) | Resolved by canonicalizing generated YAML line endings to LF in `ConfigVarsStage.GenerateConfigVarsFile()`. The failure was a Windows-only CRLF vs LF mismatch, not a business-logic defect. | P1 | 2026-04-30 | 2026-04-30 | dev |
| 12 | `Scripts (PowerShell)` | `fix-legacy-repository-topology.ps1` | One-off repository topology repair script has no automated regression coverage; current remediation relies on syntax validation and manual/local container execution only. | P3 | 2026-04-28 | | |
| 13 | `Front (Angular)` | `ConfigDetailComponent` template interactions + `src/index.html` Clarity bootstrap | This Sonar bug batch adds focused specs for `DsCardComponent`, naming-template dialogs, and resource-type sorting, but the large `config-detail.component.html` interaction surface and the inline Clarity bootstrap in `src/index.html` still have no practical isolated unit-test harness. Validation currently relies on targeted Karma specs plus `npm run typecheck` and `npm run build`. | P3 | 2026-04-30 | | |
| 14 | `Front (Angular)` | `ConfigDetailComponent` local Sonar refactor helpers | The Bicep tree shaping, resource grouping, and diagnostics aggregation paths in `config-detail.component.ts` were refactored locally to satisfy IDE Sonar rules, but this component still has no focused spec harness for those branches. Validation for this slice currently relies on IDE analysis plus `npm run typecheck` and `npm run build`. | P2 | 2026-04-30 | | |
| 15 | `Front (Angular)` | Resource creation and app setting/config key dialogs local refactor helpers | The local IDE-cleanup refactors in `add-resource-dialog.component.ts`, `add-app-setting-dialog.component.ts`, `add-app-config-key-dialog.component.ts`, `push-to-git-dialog.component.ts`, and `bicep-highlight.pipe.ts` still have no focused spec harness for their submit gating, request building, error parsing, environment-setting mapping, and highlighting paths. Validation for this slice currently relies on IDE analysis plus `npm run typecheck` and `npm run build`. | P2 | 2026-04-30 | | |
| 22 | Mixed (Backend + Frontend) | S3776 cognitive complexity backlog (39 methods/functions) | SonarCloud S3776 issues currently suppressed via `[SuppressMessage]` (24 C# methods), inline `// NOSONAR S3776` (8 TS functions), and per-file `sonar.issue.ignore.multicriteria` rules in `sonar-project.properties` (6 large feature components). Refactor each into focused helpers AFTER dedicated unit-test coverage is added (otherwise extraction risks silent regressions). **Suppressed C# methods**: `IdentityInjectionStage.Execute`, `ParentReferenceResolutionStage.Execute`, `MainBicepAssembler.Generate`, `BicepAssembler.Assemble`, `ParameterFileAssembler.ApplyEnvironmentOverrides` + `MergePropertyIntoObject`, `FunctionAppTypeBicepGenerator.GenerateSpec`, `WebAppTypeBicepGenerator.GenerateSpec`, `ProjectController.UseProjectController`, `StorageAccountMappingConfig.Register`, `CreateProjectWithSetupCommandHandler.Handle`, `CreateProjectWithSetupCommandValidator..ctor`, `PushProjectArtifactsToMultiRepoCommandHandler.Handle`, `AddAppConfigurationKeyCommandHandler.Handle`, `ListAppConfigurationKeysQueryHandler.Handle`, `AddAppSettingCommandHandler.Handle`, `ListAppSettingsQueryHandler.Handle`, `ListIncomingCrossConfigReferencesQueryHandler.Handle`, `CreateRedisCacheCommandHandler.Handle`, `UpdateRedisCacheCommandHandler.Handle`, `ListRoleAssignmentsByIdentityQueryHandler.Handle`, `Project.EnsureRepositoryAllowedByLayout`, `InfrastructureConfig.EnsureRepositoryAllowedByLayout`, `AzureDevOpsGitProviderService.PushScopedFilesAsync`, `InfrastructureConfigReadRepository.GetByIdWithResourcesAsync`. **Refactored (already done)**: `IdentityInjectionStage.Execute` (extracted `ApplyIdentity` + `ResolveIdentityKind`), `SecureParameterOverrideHelper.DeriveSecureParameterOverrides` (extracted `MergeCustomMappingIntoVariableGroups`), `CheckResourceNameAvailabilityQueryHandler.Handle` (extracted `ResolveEnvironmentAvailabilityAsync`). **Suppressed TS files** (per-file): `project-detail.component.ts`, `config-detail.component.ts`, `add-resource-dialog.component.ts`, `add-app-config-key-dialog.component.ts`, `add-app-setting-dialog.component.ts`, `resource-edit.component.ts`. **Suppressed TS inline `// NOSONAR S3776`**: `split-generation-switcher.component.ts` (2), `push-to-git-dialog.component.ts`. | P2 | 2026-04-30 | | |

---

## How to use this file

### Adding debt (any agent)
When modifying code and discovering the target zone has no tests:
1. Add a row with the next `#`, assembly, zone, description, priority, and today's date.
2. Leave `Résolu le` and `Résolu par` empty.

### Resolving debt (any agent, typically `dotnet-dev` + `xunit-unit-testing` skill)
1. Write the missing tests following the `xunit-unit-testing` skill.
2. Verify tests pass: `dotnet test .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj`.
3. Fill `Résolu le` with today's date and `Résolu par` with the agent name.
4. Do NOT delete the row — resolved rows serve as audit trail.

### Querying debt
- Filter by `Priorité` column to find highest-priority gaps.
- Filter by empty `Résolu le` to find open debt.
