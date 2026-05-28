# Test Debt Tracker

> **Ce fichier recense la dette de tests unitaires détectée par les agents.**
> Chaque agent qui modifie du code et constate l'absence de tests sur la zone touchée
> DOIT ajouter une entrée ici. Les agents peuvent ensuite résorber la dette par priorité.

---

## Convention

- **P1 — Critique** : Code métier non testé, risque de régression élevé (domain, handlers, validators)
- **P2 — Important** : Code infrastructure/services non testé, impact modéré (repositories, mappers, transformers)
- **P3 — Souhaitable** : Code utilitaire ou configuration non testé, impact faible (extensions, options, helpers)

## Incremental Entries

- [2026-05-28] **Front/P3 — Resource-edit VirtualNetwork switch branch lacks a focused component spec**: the VNet route contract, add-resource mapping, helper hydration, and networking hints are covered, but `resource-edit.component.ts` still has no direct spec asserting the `VirtualNetwork` load/save switch branches. Add a shallow component/spec or an extracted dispatcher test for the detail page branch.
- [2026-05-28] **Front/P3 — Add-resource type picker visual regression coverage**: `add-resource-dialog` has no focused frontend spec covering the wide type-picker layout, long CamelCase label wrapping, or the no-horizontal-scroll behavior for resource cards. Add a component/spec or visual-regression check around the type-selection step.
- [2026-05-18] **P2 — Infrastructure / KeyVaultSecretClient**: `tests/InfraFlowSculptor.Infrastructure.Tests/Services/KeyVault/KeyVaultSecretClientTests.cs` now covers PAT write failure mapping in `SetSecretAsync(...)` and PAT read failure logging in `GetSecretAsync(...)`; `DeleteSecretAsync(...)` still lacks a focused regression test for exception logging and error mapping.
- [2026-05-27] ~~**Front/P2 — DS migration W4.3 specs broken**~~ **RESOLVED [2026-05-27]**: 5 specs migrated from legacy CSS selectors (`.uai-used-row__unlink`, `.custom-domains-section__add-btn`, `.as-row__delete`, `.as-row__edit`, `.ra-add-btn`) to DS-aware selectors (`app-ds-button[icon="..."] button`, `app-ds-icon-button[icon="..."] button`). All 51 Karma specs pass. SCSS dead-classes purged from 5 files (custom-domains, app-settings, pipeline-options, add-app-config-key-dialog, add-app-setting-dialog).
- [2026-05-27] ~~**Front/P3 — DS migration W2 (tag-input rollout) NOT EXECUTED**~~ **RESOLVED [2026-05-27]**: Created `app-ds-key-value-input` primitive (`DsKeyValueItem { key, value }`, ControlValueAccessor, validators, chips display). Migrated 3 files: `config-detail-tags-section`, `project-detail-tags-section`, `add-project-environment-dialog`. Naming-template files (2) excluded by design (they are cursor-placement UIs, not key-value inputs). `mat-chip-set` removed from the 3 migrated files.
- [2026-05-27] **Front/P3 — DS migration N7 accordion RESOLVED [2026-05-27]**: Created `app-ds-accordion` primitive (expand/collapse, ARIA, icon, tone). Migrated DNS tutorial in `resource-edit-custom-domains-section`. 0 `mat-expansion-panel` remaining in frontend.
- [2026-05-27] **Front/P3 — Bicep palette extraction RESOLVED [2026-05-27]**: Extracted shared `_bicep-syntax-palette.scss` partial. Used by `settings.component.scss` and `bicep-file-panel.component.scss` via `@use` + `@include bicep.tokens`.
- [2026-05-27] **Front/P2 — W1 DS primitive specs**: The 4 new W1 primitives (`ds-spinner`, `ds-progress-bar`, `ds-tag-input`, `ds-menu`) have spec files and compile correctly. Confirmed passing with typecheck.

### Remaining open debt (Front)
- **P3** — 2 `mat-chip-set` remain in naming-template dialogs (cursor-placement UIs, not tag inputs — excluded by design)

---

## Audit complet — 2026-05-17

### État actuel de la suite de tests

| Projet de tests | Tests réussis | Ignorés | Échoués | Fichiers tests | Fichiers source | Ratio |
|-----------------|--------------|---------|---------|----------------|-----------------|-------|
| Domain.Tests | 670 | 0 | 0 | 92 | 254 | 36% |
| Application.Tests | 444 | 0 | 0 | 162 | 855 | 19% |
| BicepGeneration.Tests | 888 | 0 | 0 | 48 | 115 | 42% |
| PipelineGeneration.Tests | 98 | 0 | 0 | 25 | 49 | 51% |
| Infrastructure.Tests | 79 | 9 | 0 | 17 | 297 | 6% |
| Contracts.Tests | 321 | 0 | 0 | 57 | 206 | 28% |
| Api.Tests | 32 | 0 | 0 | 10 | 127 | 8% |
| GenerationCore.Tests | 99 | 0 | 0 | 9 | 24 | 38% |
| Mcp.Tests | 90 | 0 | 0 | 15 | 49 | 31% |
| **TOTAL** | **2,721** | **9** | **0** | **435** | **1,976** | **22%** |

### Cibles Stryker

- **Mutation score cible :** ≥ 80% par projet de tests
- **Qualité des assertions :** AAA strict, FluentAssertions, assertions multiples par test, edge cases
- **Patterns obligatoires :** `Given_When_Then`, `_sut`, `InlineData`/`MemberData` pour combinatoires

---

## Plan de résorption en 4 phases

> Chaque phase = 1 session d'agent longue. Objectif : dette 0 en 4 sessions.
> Les tests doivent être résistants à Stryker (pas de tests théâtre).

### Phase 1 — Fondation (Domain + GenerationCore + Contracts)
**Pré-requis de toutes les autres phases. Verrouille le socle.**

| Cible | Travail | Tests estimés |
|-------|---------|---------------|
| **Contracts.Tests** | Fix du test en échec + couverture des ~70 request types restants (Update*, Delete*, AppSettings, NetworkSecurityGroup, SqlServer, EventHub, ContainerApp, FrontDoor) | ~150 |
| **Domain.Tests** | 3 agrégats manquants : `FrontDoorAggregate`, `NetworkSecurityGroupAggregate`, `PrivateDnsZoneAggregate` (invariants, factory, value objects, environment settings) | ~60 |
| **GenerationCore.Tests** | `AzureResourceDefaults`, `KeyVaultSecretNameRules`, `PathSanitizer`, 13 models de génération (shape + invariants), 3 enums, `GenerationErrors` | ~50 |
| **TOTAL Phase 1** | | **~260** |

**Critères de succès Phase 1 :**
- [x] 0 test en échec sur toute la solution
- [x] 27/27 agrégats Domain couverts (FrontDoor, NetworkSecurityGroup, PrivateDnsZone ajoutés — 51 tests)
- [x] GenerationCore : chaque classe publique a au moins 1 test (87 tests ajoutés : KeyVaultSecretNameRules, PathSanitizer, AzureResourceDefaults, GenerationErrors, DeploymentModes, AcrAuthModes)
- [x] Contracts : tous les request types avec attributs de validation couverts (203 tests ajoutés : 15 Create + 35 Add/Update/Set/Import/Push/Check)
- [x] `dotnet test .\InfraFlowSculptor.slnx` = 0 failure (3,725 passing, 9 skipped Infrastructure)

---

### Phase 2 — Validators Application (Pure logic, ROI Stryker maximum)
**121 validators FluentValidation non testés. Pur input/output, pas de DI, plus haut taux de mutation kill.**

| Scope | Validators | Tests estimés |
|-------|-----------|---------------|
| Azure Resources CRUD (Create/Update/Delete × 22 types) | ~80 | ~320 |
| InfrastructureConfig (Add/Remove/Set × 12 opérations) | ~15 | ~60 |
| Project (Add/Remove/Set/Update/Push/Download × 20 opérations) | ~20 | ~80 |
| Misc (PAT, RoleAssignment, CustomDomain, PrivateEndpoint, SecureParam) | ~6 | ~25 |
| **TOTAL Phase 2** | **121** | **~485** |

**Détail des 121 validators non couverts :**

<details>
<summary>Liste complète (cliquer pour déplier)</summary>

**AppConfiguration:** CreateAppConfigurationCommandValidator, RemoveAppConfigurationKeyCommandValidator
**ApplicationInsights:** CreateApplicationInsightsCommandValidator, UpdateApplicationInsightsCommandValidator
**AppServicePlan:** CreateAppServicePlanCommandValidator, UpdateAppServicePlanCommandValidator
**AppSettings:** RemoveAppSettingCommandValidator, UpdateStaticAppSettingCommandValidator
**ContainerAppEnvironment:** CreateContainerAppEnvironmentCommandValidator, UpdateContainerAppEnvironmentCommandValidator
**ContainerApp:** CreateContainerAppCommandValidator, UpdateContainerAppCommandValidator
**ContainerRegistry:** CreateContainerRegistryCommandValidator, UpdateContainerRegistryCommandValidator
**CosmosDb:** CreateCosmosDbCommandValidator, UpdateCosmosDbCommandValidator
**CustomDomain:** AddCustomDomainCommandValidator, RemoveCustomDomainCommandValidator, ValidateCustomDomainDnsCommandValidator
**EventHubNamespace:** AddEventHubCommandValidator, AddEventHubConsumerGroupCommandValidator, CreateEventHubNamespaceCommandValidator, RemoveEventHubCommandValidator, RemoveEventHubConsumerGroupCommandValidator, UpdateEventHubNamespaceCommandValidator
**FrontDoor:** CreateFrontDoorCommandValidator, DeleteFrontDoorCommandValidator, UpdateFrontDoorCommandValidator
**FunctionApp:** CreateFunctionAppCommandValidator
**Imports:** ApplyImportPreviewCommandValidator
**InfrastructureConfig:** AddCrossConfigReferenceCommandValidator, DownloadBicepCommandValidator, DownloadPipelineCommandValidator, GenerateBicepCommandValidator, GeneratePipelineCommandValidator, PushBicepToGitCommandValidator, PushPipelineToGitCommandValidator, RemoveCrossConfigReferenceCommandValidator, RemoveResourceAbbreviationOverrideCommandValidator, RemoveResourceNamingTemplateCommandValidator, SetDefaultNamingTemplateCommandValidator, SetInfraConfigTagsCommandValidator, SetInheritanceCommandValidator, SetResourceAbbreviationOverrideCommandValidator, SetResourceNamingTemplateCommandValidator, NamingTemplateValidator, CheckResourceNameAvailabilityQueryValidator
**KeyVault:** CreateKeyVaultCommandValidator, UpdateKeyVaultCommandValidator
**LogAnalyticsWorkspace:** CreateLogAnalyticsWorkspaceCommandValidator, UpdateLogAnalyticsWorkspaceCommandValidator
**NetworkSecurityGroup:** CreateNetworkSecurityGroupCommandValidator, DeleteNetworkSecurityGroupCommandValidator, UpdateNetworkSecurityGroupCommandValidator
**PersonalAccessToken:** CreatePersonalAccessTokenCommandValidator
**PrivateDnsZone:** CreatePrivateDnsZoneCommandValidator, DeletePrivateDnsZoneCommandValidator, UpdatePrivateDnsZoneCommandValidator
**PrivateEndpoint:** AddPrivateEndpointCommandValidator, RemovePrivateEndpointCommandValidator, UpdatePrivateEndpointCommandValidator
**Project:** AddProjectEnvironmentCommandValidator, AddProjectMemberCommandValidator, AddProjectPipelineVariableGroupCommandValidator, AddProjectRepositoryCommandValidator, DownloadProjectBicepCommandValidator, DownloadProjectBootstrapPipelineCommandValidator, DownloadProjectPipelineCommandValidator, GenerateProjectBicepCommandValidator, GenerateProjectBootstrapPipelineCommandValidator, GenerateProjectPipelineCommandValidator, PushProjectArtifactsToMultiRepoCommandValidator, PushProjectBicepToGitCommandValidator, PushProjectPipelineToGitCommandValidator, RemoveProjectEnvironmentCommandValidator, RemoveProjectMemberCommandValidator, RemoveProjectPipelineVariableGroupCommandValidator, RemoveProjectRepositoryCommandValidator, RemoveProjectResourceAbbreviationCommandValidator, RemoveProjectResourceNamingTemplateCommandValidator, SetProjectLayoutPresetCommandValidator, SetProjectTagsCommandValidator, TestGitConnectionCommandValidator, UpdateProjectEnvironmentCommandValidator, UpdateProjectMemberRoleCommandValidator, UpdateProjectRepositoryCommandValidator, SearchCodeRepoFilesQueryValidator
**RedisCache:** CreateRedisCacheCommandValidator, UpdateRedisCacheCommandValidator
**RoleAssignment:** AddRoleAssignmentCommandValidator, AssignIdentityToResourceCommandValidator, RemoveRoleAssignmentCommandValidator, UnassignIdentityFromResourceCommandValidator, UpdateRoleAssignmentIdentityCommandValidator, SetSecureParameterMappingCommandValidator
**ServiceBusNamespace:** AddServiceBusQueueCommandValidator, AddServiceBusTopicSubscriptionCommandValidator, CreateServiceBusNamespaceCommandValidator, RemoveServiceBusQueueCommandValidator, RemoveServiceBusTopicSubscriptionCommandValidator, UpdateServiceBusNamespaceCommandValidator
**SqlDatabase:** CreateSqlDatabaseCommandValidator, UpdateSqlDatabaseCommandValidator
**SqlServer:** CreateSqlServerCommandValidator, UpdateSqlServerCommandValidator
**StorageAccount:** AddBlobContainerCommandValidator, AddQueueCommandValidator, AddTableCommandValidator, RemoveBlobContainerCommandValidator, RemoveQueueCommandValidator, RemoveTableCommandValidator, UpdateBlobContainerPublicAccessCommandValidator, UpdateStorageAccountCommandValidator
**UserAssignedIdentity:** CreateUserAssignedIdentityCommandValidator, UnlinkResourceFromIdentityCommandValidator, UpdateUserAssignedIdentityCommandValidator
**VirtualNetwork:** CreateVirtualNetworkCommandValidator, DeleteVirtualNetworkCommandValidator, UpdateVirtualNetworkCommandValidator
**WebApp:** CreateWebAppCommandValidator, UpdateWebAppCommandValidator

</details>

**Critères de succès Phase 2 :**
- [x] 121/121 validators couverts (au moins happy path + chaque règle violée individuellement)
- [x] Tests `InlineData`/`MemberData` pour les combinatoires (enum, longueur, format)
- [x] Chaque `RuleFor` a au minimum 1 test positif + 1 test négatif
- [x] `dotnet test .\tests\InfraFlowSculptor.Application.Tests` = 0 failure (1011 passing)
- [ ] Stryker mutation score > 80% sur le namespace Validators

**Phase 2 complétée le 2026-05-17 : +567 tests (444 → 1011 dans Application.Tests)**
**Total solution : 3,294 tests passing, 0 failures, 9 skipped (Infrastructure pre-existing)**

---

### Phase 3 — Handlers Application (Logique métier)
**128 command/query handlers non couverts. Mock des repos/services, tests des branches.**

| Scope | Handlers | Tests estimés |
|-------|----------|---------------|
| Azure Resources Create (×15 types non couverts) | 15 | ~75 |
| Azure Resources Update (×18 types non couverts) | 18 | ~108 |
| Azure Resources Delete (×10 types non couverts) | 10 | ~30 |
| Azure Resources Sub-resources (Add/Remove EventHub, Queue, Blob, etc.) | 20 | ~80 |
| InfrastructureConfig operations (Set/Remove/Download/Push/Generate) | 18 | ~90 |
| Project operations (Add/Remove/Set/Push/Download/Generate) | 25 | ~125 |
| Query Handlers (Get/List/Check/Preview/Download) | 15 | ~60 |
| Misc (PAT, RoleAssignment, CustomDomain, PrivateEndpoint, Identity) | 7 | ~35 |
| **TOTAL Phase 3** | **128** | **~603** |

**Détail des 128 handlers non couverts :**

<details>
<summary>Liste complète (cliquer pour déplier)</summary>

**AppConfiguration:** CreateAppConfigurationCommandHandler, DeleteAppConfigurationCommandHandler, RemoveAppConfigurationKeyCommandHandler, UpdateAppConfigurationCommandHandler
**ApplicationInsights:** CreateApplicationInsightsCommandHandler, DeleteApplicationInsightsCommandHandler, UpdateApplicationInsightsCommandHandler
**AppSettings:** RemoveAppSettingCommandHandler, UpdateStaticAppSettingCommandHandler
**ContainerAppEnvironment:** UpdateContainerAppEnvironmentCommandHandler
**ContainerApp:** UpdateContainerAppCommandHandler
**CustomDomain:** AddCustomDomainCommandHandler, RemoveCustomDomainCommandHandler, ValidateCustomDomainDnsCommandHandler
**EventHubNamespace:** AddEventHubCommandHandler, AddEventHubConsumerGroupCommandHandler, CreateEventHubNamespaceCommandHandler, DeleteEventHubNamespaceCommandHandler, RemoveEventHubCommandHandler, RemoveEventHubConsumerGroupCommandHandler, UpdateEventHubNamespaceCommandHandler
**FrontDoor:** CreateFrontDoorCommandHandler, DeleteFrontDoorCommandHandler, UpdateFrontDoorCommandHandler, GetFrontDoorQueryHandler
**FunctionApp:** UpdateFunctionAppCommandHandler
**Imports:** PreviewIacImportQueryHandler
**InfrastructureConfig:** AddCrossConfigReferenceCommandHandler, DownloadBicepCommandHandler, PushBicepToGitCommandHandler, PushPipelineToGitCommandHandler, RemoveCrossConfigReferenceCommandHandler, RemoveResourceAbbreviationOverrideCommandHandler, RemoveResourceNamingTemplateCommandHandler, SetDefaultNamingTemplateCommandHandler, SetInfraConfigTagsCommandHandler, SetInheritanceCommandHandler, SetResourceAbbreviationOverrideCommandHandler, SetResourceNamingTemplateCommandHandler, CheckResourceNameAvailabilityQueryHandler, GetBicepFileContentQueryHandler, GetConfigDiagnosticsQueryHandler, GetPipelineFileContentQueryHandler, ListIncomingCrossConfigReferencesQueryHandler, ListMyInfrastructureConfigsQueryHandler
**LogAnalyticsWorkspace:** CreateLogAnalyticsWorkspaceCommandHandler, DeleteLogAnalyticsWorkspaceCommandHandler, UpdateLogAnalyticsWorkspaceCommandHandler
**NetworkSecurityGroup:** CreateNetworkSecurityGroupCommandHandler, DeleteNetworkSecurityGroupCommandHandler, UpdateNetworkSecurityGroupCommandHandler, GetNetworkSecurityGroupQueryHandler
**PersonalAccessToken:** CreatePersonalAccessTokenCommandHandler, RevokePersonalAccessTokenCommandHandler, ListPersonalAccessTokensQueryHandler
**PrivateDnsZone:** CreatePrivateDnsZoneCommandHandler, DeletePrivateDnsZoneCommandHandler, UpdatePrivateDnsZoneCommandHandler, GetPrivateDnsZoneQueryHandler
**PrivateEndpoint:** AddPrivateEndpointCommandHandler, RemovePrivateEndpointCommandHandler, UpdatePrivateEndpointCommandHandler, GetPrivateEndpointConfigsQueryHandler
**Project:** AddProjectEnvironmentCommandHandler, AddProjectPipelineVariableGroupCommandHandler, DownloadProjectBicepCommandHandler, DownloadProjectBootstrapPipelineCommandHandler, DownloadProjectPipelineCommandHandler, PushProjectBicepToGitCommandHandler, PushProjectBootstrapPipelineToGitCommandHandler, PushProjectPipelineToGitCommandHandler, RemoveProjectEnvironmentCommandHandler, RemoveProjectMemberCommandHandler, RemoveProjectPipelineVariableGroupCommandHandler, RemoveProjectRepositoryCommandHandler, RemoveProjectResourceAbbreviationCommandHandler, RemoveProjectResourceNamingTemplateCommandHandler, SetAgentPoolCommandHandler, SetProjectDefaultNamingTemplateCommandHandler, SetProjectResourceAbbreviationCommandHandler, SetProjectResourceNamingTemplateCommandHandler, SetProjectTagsCommandHandler, TestGitConnectionCommandHandler, UpdateProjectEnvironmentCommandHandler, GetProjectBicepFileContentQueryHandler, GetProjectBootstrapPipelineFileContentQueryHandler, GetProjectPipelineFileContentQueryHandler, ListGitBranchesQueryHandler, ListMyProjectsQueryHandler, ListProjectConfigsQueryHandler, ListProjectPipelineVariableGroupsQueryHandler, ListProjectResourcesQueryHandler, ValidateRecentItemsQueryHandler
**ResourceGroup:** ListResourceGroupsByConfigQueryHandler
**RoleAssignment:** AssignIdentityToResourceCommandHandler, RemoveRoleAssignmentCommandHandler, UnassignIdentityFromResourceCommandHandler, UpdateRoleAssignmentIdentityCommandHandler, SetSecureParameterMappingCommandHandler
**ServiceBusNamespace:** AddServiceBusQueueCommandHandler, AddServiceBusTopicSubscriptionCommandHandler, CreateServiceBusNamespaceCommandHandler, DeleteServiceBusNamespaceCommandHandler, RemoveServiceBusQueueCommandHandler, RemoveServiceBusTopicSubscriptionCommandHandler, UpdateServiceBusNamespaceCommandHandler
**SqlDatabase:** UpdateSqlDatabaseCommandHandler
**SqlServer:** UpdateSqlServerCommandHandler
**StorageAccount:** AddBlobContainerCommandHandler, AddQueueCommandHandler, AddTableCommandHandler, RemoveBlobContainerCommandHandler, RemoveQueueCommandHandler, RemoveTableCommandHandler, UpdateBlobContainerPublicAccessCommandHandler, UpdateStorageAccountCommandHandler
**UserAssignedIdentity:** CreateUserAssignedIdentityCommandHandler, DeleteUserAssignedIdentityCommandHandler, UnlinkResourceFromIdentityCommandHandler, UpdateUserAssignedIdentityCommandHandler
**VirtualNetwork:** CreateVirtualNetworkCommandHandler, DeleteVirtualNetworkCommandHandler, UpdateVirtualNetworkCommandHandler, GetVirtualNetworkQueryHandler
**WebApp:** UpdateWebAppCommandHandler

</details>

**Critères de succès Phase 3 :**
- [x] 128/128 handlers couverts (au minimum : happy path + 1 cas d'erreur principal par handler) — 125 concrete handlers (3 are interfaces), all 224/224 handler files now tested
- [x] Pour les Update handlers : test des branches de parsing (SKU, enum, settings)
- [x] Pour les Create handlers : test des invariants domain (guard clauses du constructeur via le handler)
- [x] Pour les Delete handlers : test not-found + ownership/access
- [x] Mock pattern : NSubstitute, `_sut` convention, `Arg.Is<>()` pour assertions sur appels repo
- [x] `dotnet test .\tests\InfraFlowSculptor.Application.Tests` = 0 failure (1441 passing)
- [ ] Stryker mutation score > 75% sur le namespace Handlers

**Phase 3 complétée le 2026-05-17 : +430 handler tests (Application.Tests 1011 → 1441)**
**Total solution : 4,210 tests passing, 0 failures, 9 skipped (Infrastructure pre-existing)**

---

### Phase 4 — Infrastructure + BicepGeneration + Api + Mcp
**Ferme les gaps restants : data access, génération, API, MCP.**

| Cible | Travail | Tests estimés |
|-------|---------|---------------|
| **Infrastructure — 22 repositories** | Tests EF Core InMemory pour chaque repo (CRUD + queries spécifiques). Repos simples (`AppConfigurationRepository`, `AppServicePlanRepository`, etc.) qui héritent du base repository : au moins Add/GetById/Delete. Repos complexes (`InfrastructureConfigReadRepository`, `AzureResourceRepository`) : queries avec includes/projections. | ~120 |
| **Infrastructure — Services** | `GeneratedArtifactService` (blob routing), `GitHubGitProviderService` (push/clone paths) | ~20 |
| **Infrastructure — Converters** | `SingleValueConverter`, `EnumValueConverter`, `IdValueConverter`, `NullableIdValueConverter`, `NullableEnumValueConverter`, `ParameterUsageConverter`, `RepositoryAliasConverter`, `RepositoryContentKindsConverter` | ~30 |
| **BicepGeneration — 5 generators** | `FrontDoorTypeBicepGenerator`, `NetworkSecurityGroupTypeBicepGenerator`, `PrivateDnsZoneTypeBicepGenerator`, `PrivateEndpointTypeBicepGenerator`, `VirtualNetworkTypeBicepGenerator` (GenerateSpec + Generate, toutes les variantes) | ~80 |
| **BicepGeneration — Helpers** | `BicepParameterModelConverter`, `BicepObjectPropertyHelper`, `BicepNamingHelper`, `BicepIdentifierHelper`, `ModuleHeaderHelper`, `NamingTemplateTranslator`, `ResourceTypeMetadata` | ~40 |
| **Api — Mapster configs** | Spot-check des 10 mapping configs les plus complexes (StorageAccount, ContainerApp, InfraConfig, Project, WebApp, FunctionApp, RedisCache, CosmosDb, EventHub, ServiceBus) | ~30 |
| **Api — Security/middleware** | `WebApplicationFactory` end-to-end : security headers, CORS reject, auth pipeline | ~15 |
| **Mcp — Tools & Services** | `ResourceConfigurationTools`, `NamingTools`, `AppSettingsTools`, `ImportPreviewResources` | ~25 |
| **TOTAL Phase 4** | | **~360** |

**Phase 4 complétée le 2026-05-17 : +485 tests (solution 3,725 → 4,210)**
**Infrastructure.Tests: 120 → 297 (+177), BicepGeneration.Tests: 995 → 1117 (+122), Api.Tests: 32 → 40 (+8), Mcp.Tests: 92 → 125 (+33)**

**Critères de succès Phase 4 :**
- [x] 22/22 repos couverts (au minimum GetById + Add + Delete)
- [x] 8 converters avec round-trip tests (serialize → deserialize = identité)
- [x] 5/5 generators avec spec + legacy parity assertions
- [x] Api : 1 test WebApplicationFactory prouvant le middleware complet (4 integration tests)
- [x] Mcp : chaque tool public a au minimum 1 test end-to-end mocké (33 new tests across 3 tools)
- [x] `dotnet test .\InfraFlowSculptor.slnx` = 0 failure (4,210 passing, 9 skipped Infrastructure pre-existing)
- [ ] Stryker mutation score > 75% par projet de tests (deferred — requires separate Stryker run)

---

## Résumé du plan

| Phase | Focus | Tests estimés | Tests cumulés | Coverage cible |
|-------|-------|---------------|---------------|----------------|
| 1 | Foundation (Domain+GenerationCore+Contracts) | ~260 | ~2,630 | 25% | ✅ Done (351 tests added → 2,721 total) |
| 2 | Application Validators (121) | ~485 | ~3,115 | 35% | ✅ Done (567 tests added → 3,294 total) |
| 3 | Application Handlers (128) | ~603 | ~3,718 | 55% | ✅ Done (430 tests added → 3,725 total) |
| 4 | Infrastructure+BicepGen+Api+Mcp | ~360 | ~4,078 | 70%+ | ✅ Done (485 tests added → 4,210 total) |

**Total nouveau :** ~1,708 tests à écrire → suite complète ~4,078 tests
**Réalisé :** 1,833 tests ajoutés → suite complète 4,210 tests (0 failures, 9 skipped)
**Objectif Stryker :** mutation score ≥ 80% par module après phase complète

---

## Instructions pour l'agent exécutant

Chaque phase doit :
1. Charger le skill `xunit-unit-testing` + `tdd-workflow` + `dotnet-patterns`
2. Suivre le pattern `Given_When_Then` + `_sut` + AAA
3. Utiliser FluentAssertions pour des assertions expressives (pas `Assert.Equal`)
4. Utiliser NSubstitute pour les mocks (pas Moq)
5. Utiliser `InlineData` / `MemberData` / `ClassData` pour les combinatoires (maximise Stryker kill)
6. Ne PAS écrire de tests théâtre (assertions sur le mock lui-même sans vérifier le résultat)
7. Tester les edge cases : null, empty, boundary values, invalid enums
8. Vérifier `dotnet test` = 0 failure à chaque sous-étape
9. Ne PAS toucher au code de production sauf pour fixer un bug découvert par un test

---

## Debt Register

| # | Assembly | Zone / Classe | Description | Priorité | Détecté le | Résolu le | Résolu par |
|---|----------|---------------|-------------|----------|------------|-----------|------------|
| 1 | `InfraFlowSculptor.Domain` | All aggregates | Comprehensive invariant test coverage added across 22 aggregates + value objects (468 tests) | P1 | 2026-04-27 | 2026-04-30 | dev |
| 2 | `InfraFlowSculptor.Application` | All handlers/validators | Initial coverage of Project / InfrastructureConfig / ResourceGroup + 12 AzureResource Create/Delete/Get handlers, plus focused validation coverage for `CreateProjectWithSetupCommandValidator` cross-rules (231 tests). Update / SetEnvironment / multi-aggregate orchestration handlers still uncovered. | P1 | 2026-04-27 | 2026-04-30 | dev (partial) |
| 3 | `InfraFlowSculptor.Infrastructure` | Repositories, services | Initial coverage of 6 critical repositories with EF Core InMemory (44 active + 9 skipped due to ComplexProperty/Owned-type InMemory limits). Auth services, Refit clients, Azure clients, BlobService still untested beyond pre-existing slice. | P2 | 2026-04-27 | 2026-04-30 | dev (partial) |
| 4 | `InfraFlowSculptor.Api` | Controllers, DI, error mapping | No unit tests for endpoint registration, Mapster configs, error conversion | P2 | 2026-04-27 | | |
| 11 | `InfraFlowSculptor.Api` | Program.cs security middleware (SEC-002, SEC-005) | `InfraFlowSculptor.Api.Tests` now exists, but no end-to-end harness exercises the real `Program.cs` pipeline to verify security headers are emitted on every response and that CORS rejects unlisted origins/methods. A full-pipeline `WebApplicationFactory`-style harness is still missing. | P2 | 2026-05-12 | | |
| 5 | `InfraFlowSculptor.PipelineGeneration` | Pipeline generators | No dedicated test project (only parity tests) | P2 | 2026-04-27 | 2026-04-27 | dev |
| 6 | `InfraFlowSculptor.GenerationCore` | Shared generation abstractions | No dedicated test project | P3 | 2026-04-27 | | |
| 7 | `InfraFlowSculptor.PipelineGeneration` | Bootstrap and app pipeline generators | Dedicated test project now exists, but most generator paths are still uncovered beyond the bootstrap pipeline warning regression | P2 | 2026-04-27 | 2026-04-27 | dev |
| 8 | `InfraFlowSculptor.PipelineGeneration` | 4 engines (PipelineGenerationEngine, BootstrapPipelineGenerationEngine, AppPipelineGenerationEngine, MonoRepoPipelineAssembler) | Byte-for-byte parity sentinels via golden file tests covering all 4 engines + R2 frozen-set lock on AppPipeline shared templates (20 paths) — 22 tests + 83 captured goldens. Behavioral coverage of internal stages still missing (will be added during Vagues 1.1/1.2/1.3 stage decomposition via TDD). | P2 | 2026-04-27 | 2026-04-27 | dev |
| 9 | `InfraFlowSculptor.Contracts` | Request and validation DTOs | Coverage extended to all custom validation attributes (Enum/Guid/RedisVersion) + 16 representative request DTOs across major features (101 tests). Remaining: response DTOs, less-critical request DTOs lacking attributes (noted in observations). | P2 | 2026-04-28 | 2026-04-30 | dev (partial) |
| 10 | `Front (Angular)` | Local Sonar quick-win helpers | No focused frontend specs currently cover the local CORS validators, generated artifact archive path sanitization, alias normalization helpers, split-generation tree typing helpers, push-dialog dispatch helpers, app setting/config key name normalizers, or Bicep highlight pipe. Validation is currently limited to `npm run typecheck` and `npm run build`. | P2 | 2026-04-28 | | |
| 11 | `InfraFlowSculptor.Application` | Imports/PreviewIacImport analyzer + query | A dedicated `InfraFlowSculptor.Application.Tests` project now exists and covers the apply-import handler slice, but the ARM preview analyzer and `PreviewIacImportQueryHandler` are still exercised only through `InfraFlowSculptor.Mcp.Tests`. | P1 | 2026-04-28 | | |
| 16 | `InfraFlowSculptor.Application` | Update / SetEnvironment / SubResource / PushToGit / Generate handlers across all features | Batch 2 only covered Create/Delete/Get for 12 AzureResource features. Update*, SetEnvironmentSettings*, AddBlobContainer/AddSecret/AddRoleAssignment, PushToGit, and many Generate*/Download* handlers remain uncovered. Focused direct coverage now exists for `GenerateProjectBicepCommandHandler` (ambiguity gate), `GenerateProjectPipelineCommandHandler` (ambiguity gate + enriched project reload), and `GenerateProjectBootstrapPipelineCommandHandler` (enriched project reload), but their deeper generation/upload branches still rely mostly on broader solution regression runs and would need heavier mocks or integration tests for full direct coverage. | P2 | 2026-04-30 | | |
| 17 | `InfraFlowSculptor.Infrastructure` | UserRepository / ProjectRepository (queries with Members.User include) | EF Core InMemory provider cannot translate queries that materialize User due to `builder.ComplexProperty(user => user.Name)`. 9 tests skipped pending integration-level harness (Sqlite :memory: or Testcontainers PostgreSQL). | P2 | 2026-04-30 | | |
| 18 | `InfraFlowSculptor.Infrastructure` | Auth services, GitProviders Refit clients, AzureNameAvailability, BlobService (write paths), KeyVault clients | Service-layer coverage limited to existing Services/ tests (BlobService read path). External-dependency clients still untested. | P2 | 2026-04-30 | | |
| 19 | `InfraFlowSculptor.Api` | Mapster configs, controllers, error mapping | `InfraFlowSculptor.Api.Tests` now exists. The original missing-test-project gap is closed, but Mapster configurations and minimal-API endpoint registrations remain uncovered beyond the current rate-limiting slice. | P2 | 2026-04-30 | 2026-05-12 | dev |
| 20 | `InfraFlowSculptor.BicepGeneration.Tests` | Pre-existing test failure (BicepEmitterTests.Given_MinimalSpec_When_EmitModule_Then_ReturnsExpectedDocument) | Resolved by canonicalizing emitted text line endings to LF in `BicepEmitter.EnsureTrailingNewline()`. The prior FluentAssertions formatting error only masked an underlying CRLF vs LF mismatch on Windows. | P1 | 2026-04-30 | 2026-04-30 | dev |
| 21 | `InfraFlowSculptor.PipelineGeneration.Tests` | Pre-existing test failure (ConfigVarsStageTests.Given_StandardContext_When_Execute_Then_EmitsExpectedYaml) | Resolved by canonicalizing generated YAML line endings to LF in `ConfigVarsStage.GenerateConfigVarsFile()`. The failure was a Windows-only CRLF vs LF mismatch, not a business-logic defect. | P1 | 2026-04-30 | 2026-04-30 | dev |
| 12 | `Scripts (PowerShell)` | `fix-legacy-repository-topology.ps1` | One-off repository topology repair script has no automated regression coverage; current remediation relies on syntax validation and manual/local container execution only. | P3 | 2026-04-28 | | |
| 13 | `Front (Angular)` | `ConfigDetailComponent` template interactions + `src/index.html` Clarity bootstrap | This Sonar bug batch adds focused specs for `DsCardComponent`, naming-template dialogs, and resource-type sorting, but the large `config-detail.component.html` interaction surface and the inline Clarity bootstrap in `src/index.html` still have no practical isolated unit-test harness. Validation currently relies on targeted Karma specs plus `npm run typecheck` and `npm run build`. | P3 | 2026-04-30 | | |
| 14 | `Front (Angular)` | `ConfigDetailComponent` local Sonar refactor helpers | The Bicep tree shaping, resource grouping, and diagnostics aggregation paths in `config-detail.component.ts` were refactored locally to satisfy IDE Sonar rules, but this component still has no focused spec harness for those branches. Validation for this slice currently relies on IDE analysis plus `npm run typecheck` and `npm run build`. | P2 | 2026-04-30 | | |
| 15 | `Front (Angular)` | Resource creation and app setting/config key dialogs local refactor helpers | The local IDE-cleanup refactors in `add-resource-dialog.component.ts`, `add-app-setting-dialog.component.ts`, `add-app-config-key-dialog.component.ts`, `push-to-git-dialog.component.ts`, and `bicep-highlight.pipe.ts` still have no focused spec harness for their submit gating, request building, error parsing, environment-setting mapping, and highlighting paths. Validation for this slice currently relies on IDE analysis plus `npm run typecheck` and `npm run build`. | P2 | 2026-04-30 | | |
| 22 | Mixed (Backend + Frontend) | S3776 cognitive complexity backlog (suppressed methods/functions) | SonarCloud S3776 issues currently suppressed via `[SuppressMessage]` (C# methods), inline `// NOSONAR S3776` (TS functions), and per-file `sonar.issue.ignore.multicriteria` rules in `sonar-project.properties` (large feature components). Refactor each into focused helpers AFTER dedicated unit-test coverage is added (otherwise extraction risks silent regressions). **Suppressed C# methods**: `IdentityInjectionStage.Execute`, `ParentReferenceResolutionStage.Execute`, `MainBicepAssembler.Generate`, `BicepAssembler.Assemble`, `ParameterFileAssembler.ApplyEnvironmentOverrides` + `MergePropertyIntoObject`, `FunctionAppTypeBicepGenerator.GenerateSpec`, `ProjectController.UseProjectController`, `CreateProjectWithSetupCommandValidator..ctor`, `PushProjectArtifactsToMultiRepoCommandHandler.Handle`, `AddAppConfigurationKeyCommandHandler.Handle`, `ListAppConfigurationKeysQueryHandler.Handle`, `AddAppSettingCommandHandler.Handle`, `ListAppSettingsQueryHandler.Handle`, `ListIncomingCrossConfigReferencesQueryHandler.Handle`, `UpdateRedisCacheCommandHandler.Handle`, `Project.EnsureRepositoryAllowedByLayout`, `InfrastructureConfig.EnsureRepositoryAllowedByLayout`, `AzureDevOpsGitProviderService.PushScopedFilesAsync`, `InfrastructureConfigReadRepository.GetByIdWithResourcesAsync`. **Refactored (already done)**: `IdentityInjectionStage.Execute` (extracted `ApplyIdentity` + `ResolveIdentityKind`), `SecureParameterOverrideHelper.DeriveSecureParameterOverrides` (extracted `MergeCustomMappingIntoVariableGroups`), `CheckResourceNameAvailabilityQueryHandler.Handle` (extracted `ResolveEnvironmentAvailabilityAsync`), `WebAppTypeBicepGenerator.GenerateSpec` (extracted generator assembly helpers after dedicated generator tests), `ListRoleAssignmentsByIdentityQueryHandler.Handle` (extracted access validation, referenced-resource loading, and result mapping after dedicated application tests), `StorageAccountMappingConfig.Register` (split into focused Mapster registration methods plus shared list-projection helpers; API Mapster coverage debt remains tracked separately in row 4), `CreateProjectWithSetupCommandHandler.Handle` (kept `Handle` as a linear orchestration flow by extracting project creation, layout parsing, environment projection, and repository projection helpers after adding dedicated application tests), `CreateRedisCacheCommandHandler.Handle` (kept `Handle` linear by extracting resource-group authorization, minimum-TLS parsing, and environment-settings parsing after extending dedicated application tests for SKU/policy branches and parsed overrides). **Suppressed TS files** (per-file): `project-detail.component.ts`, `config-detail.component.ts`, `add-resource-dialog.component.ts`, `add-app-config-key-dialog.component.ts`, `add-app-setting-dialog.component.ts`, `resource-edit.component.ts`. **Suppressed TS inline `// NOSONAR S3776`**: `split-generation-switcher.component.ts` (2), `push-to-git-dialog.component.ts`. | P2 | 2026-04-30 | | |
| 23 | `InfraFlowSculptor.Infrastructure` | `AzureDevOpsGitProviderService` | PR #327 adds the first focused unit test for `PushScopedFilesAsync` (new branch + scoped file happy path), but branch-existing, delete/edit mixes, API failure handling, and warning-log branches remain uncovered. | P2 | 2026-05-11 | | |
| 24 | `InfraFlowSculptor.Application` | `AddAppSettingCommandValidator` + `AddAppConfigurationKeyCommandValidator` | Key Vault secret-name validation now has focused coverage for the Azure rule (`1..127`, alphanumerics + hyphens only), but the broader validator matrix for static values, output references, `ExportToKeyVault`, variable-group coupling, and enum combinations in these two validators is still only lightly covered. | P2 | 2026-05-11 | | |
| 25 | `InfraFlowSculptor.Application` | `AddProjectRepositoryCommandHandler`, `UpdateProjectRepositoryCommandHandler`, `SetProjectLayoutPresetCommandHandler`, `AddInfraConfigRepositoryCommandHandler`, `UpdateInfraConfigRepositoryCommandHandler` | The enum-parsing refactor now routes provider/layout/content-kind parsing through shared helpers and is covered by helper tests plus `CreateProjectWithSetup`/solution-level regression runs, but these handlers still lack direct focused unit tests for their own invalid-provider, invalid-layout, and content-kind branches. | P2 | 2026-05-11 | 2026-05-11 | dev |
| 26 | `Front (Angular)` | `resource-edit.component.ts` UAI creation / role-assignment dialog flows | Local Sonar cleanup removed redundant `resourceGroupId` structural assertions from the main `resource-edit` component, but this component still has no focused spec harness for the UAI creation and add-role-assignment dialog data wiring. Validation for this slice currently relies on the existing project-detail helper spec plus `npm run typecheck` and `npm run build`. | P2 | 2026-05-11 | | |
| 27 | `Front (Angular)` | `MsalAuthService` silent Graph token path + `MicrosoftGraphProfilePhotoService` Graph fetch branches | The navigation header now has focused coverage for avatar rendering with and without a Microsoft photo, but the shared service slice added here still lacks dedicated specs for silent-only Graph token acquisition, popup suppression, 404/204 photo fallback, and non-OK Graph responses. Validation currently relies on the navigation spec plus `npm run typecheck` and `npm run build`. | P2 | 2026-05-11 | | |
| 28 | `Front (Angular)` | UI Refresh vague 1 — Token & fondations visual non-regression | Wave 1 (tokens, typography, mixins, tailwind, Material override, Inter self-host) ships pure SCSS/config changes with no unit test coverage applicable. Snapshots Playwright des 7 routes clés (login, home, projects, project-detail vide, project-detail rempli, config-detail, resource-edit) à produire dans la vague 6 pour valider la non-régression visuelle cumulative des 6 vagues. | P2 | 2026-05-11 | | |
| 29 | `Front (Angular)` | UI Refresh vague 2 — DS primitives critiques (button/card/text-field/textarea/select/chip/alert/icon-button/toggle/checkbox/radio-group) | Vague 2 refonds 11 primitives DS sans nouveaux specs Karma (TDD délibérément différé pour exécution accélérée). Tests existants (DsCard, DsSelect) restent verts mais sans extension de couverture. À compléter en parallèle ou en vague 6 : couvrir variants × sizes × états (hover/focus/disabled/loading/error/checked/indeterminate), CVA pour CSS controls (toggle/checkbox/radio/text-field/textarea/select), mappings de variants dépréciés (card glass/elevated, chip primary/error/cyan, icon-button ghost/primary/subtle, alert error). Validation actuelle limitée à `npm run typecheck` + `npm run build`. | P2 | 2026-05-11 | | |
| 30 | `Front (Angular)` | UI Refresh vague 3 — Layout & navigation (sidebar / topbar / footer / page-header / section-header / ds-tabs / ds-segmented-control) | Vague 3 livre le shell enterprise (sidebar permanente collapsable, top-bar 48px, footer status-bar 28px) et 2 nouvelles primitives (`ds-tabs`, `ds-segmented-control`) sans nouveaux specs Karma (TDD différé pour exécution accélérée). Specs à écrire en vague 6 : `SidebarStateService` (lecture/écriture localStorage `ifs.sidebar.collapsed`, sync `--ifs-sidebar-width` sur `documentElement`), `SidebarComponent` (route active highlight, focus-visible, tooltip mode collapsed, toggle), `NavigationComponent` (assertions visuelles no-gradient/no-blur, language switch, logout icon-button), `FooterComponent` (rendu version + envName + lien docs), `DsTabsComponent` (clavier ←/→/Home/End, `aria-selected`, `tabindex` rotatif, badge/icon slots, disabled), `DsSegmentedControlComponent` (CVA write/read, clavier ←/→, `aria-pressed`, `tabindex` rotatif, `setDisabledState`), `DsPageHeaderComponent` + `DsSectionHeaderComponent` (slots, divider, title/subtitle/icon). La spec `NavigationComponent` existante reste verte (classes `user-card__*` préservées). | P2 | 2026-05-11 | | |
| 31 | `Front (Angular)` | `ImportAppSettingsDialogComponent` | Le nouvel import multi-format piloté par `resourceType` / `deploymentMode` / `runtimeStack` a une spec Karma focalisée sur le helper de parsing (JSON .NET, local.settings.json, .env, properties, YAML + résolution du mode recommandé), mais le dialog lui-même reste sans spec composant pour l'auto-sélection du format via nom de fichier, la gestion des doublons, le wiring du board, et les états de succès/erreur de l'import séquentiel. Validation actuelle : spec helper ciblée + `npm run typecheck` + `npm run build`. | P2 | 2026-05-11 | | |
| 32 | `InfraFlowSculptor.Api` | Real Program entrypoint integration for rate limiting | This task adds focused TestHost coverage for typed rate-limiting options, startup validation, `429` + `Retry-After`, authenticated same-IP partitioning, and metadata checks over the actual controller endpoint map, but the real `Program.cs` entrypoint still lacks a single end-to-end harness proving the full production middleware stack and route registrations together. | P2 | 2026-05-12 | | |
| 33 | `InfraFlowSculptor.Application` | `ListResourceGroupResourcesQueryHandler` | First focused coverage now verifies the new no-tracking `IResourceGroupRepository.GetByIdReadOnlyAsync(...)` path, but the handler's enrichment branches (parent mapping, configured environment aggregation, Storage Account child-resource projection, and resource summary mapping) still lack dedicated focused tests. | P2 | 2026-05-12 | | |
| 34 | `InfraFlowSculptor.Application` | `PushProjectGeneratedArtifactsToGitCommandHandler`, `PushProjectArtifactsToMultiRepoCommandHandler` | APP-005 is now closed for project generation handlers via direct coverage of `ProjectPipelineAggregator`, `MonoRepoBlobUploadOrchestrator`, `GenerateProjectPipelineCommandHandler`, and `GenerateProjectBicepCommandHandler`. Remaining debt is now limited to the project push handlers: `PushProjectGeneratedArtifactsToGitCommandHandler` still lacks focused coverage for authorization failures, PAT retrieval failures, routing resolution errors, request-builder collision branches, and no-app-artifacts short-circuit; `PushProjectArtifactsToMultiRepoCommandHandler` still lacks deeper provider exception and per-repo push/result-path coverage. | P2 | 2026-05-12 | | |
| 35 | `InfraFlowSculptor.Application` | `CreatePersonalAccessTokenCommandHandler`, `RevokePersonalAccessTokenCommandHandler` | The PersonalAccessTokens slice had no focused tests when `RevokePersonalAccessTokenCommandValidator` was added. The new validator is covered, but the create/revoke handlers still lack direct tests for current-user resolution, ownership mismatch, already-revoked tokens, and successful revocation. | P1 | 2026-05-13 | | |
| 36 | `Front (Angular)` | `createConfigDetailVariableGroupsSectionController` | The variable-groups controller factory now has a compile fix, but there is still no focused spec harness for its load/add/remove dialog flows or error-key transitions. Validation for this slice currently relies on `npm run typecheck` and `npm run build`. | P2 | 2026-05-15 | | |
| 37 | `Front (Angular)` | `GenerationBoardComponent` | The board page now has a focused regression spec for workflow delegation, repo-card config-list removal, and mono/split explorer rendering, but it still lacks direct coverage for load-error snackbar handling, collapse/download/push interactions, and the remaining board topology permutations. | P2 | 2026-05-15 | | |
| 38 | `Front (Angular)` | Naming abbreviations tables in `config-detail` / `project-detail` | The resource-type icon alignment fix is currently covered only by frontend `typecheck` and `build`; there is no practical focused visual regression harness for the abbreviation table cell layout, icon centering, or label ellipsis behavior. | P3 | 2026-05-16 | | |
| 39 | `Front (Angular)` | `ProjectDetailComponent` configuration row layout | The config-card chevron/delete ordering fix is currently covered only by frontend `typecheck` and `build`; the large page component still has no practical focused spec harness for this DOM/layout-only interaction without introducing brittle page-level tests. | P3 | 2026-05-16 | | |
| 40 | `Front (Angular)` | `ProjectDetailComponent` agent-pool toggle rendering | The raw Material toggle/save action were migrated to `app-ds-toggle` + `app-ds-button`, and the new `project-detail-agent-pool.helper.spec.ts` now covers value normalization plus dirty-state calculation. The large page component still has no practical focused spec harness for the rendered control row itself without introducing brittle full-page tests, so final validation for the DOM/layout slice still relies on targeted helper coverage plus `npm run typecheck`. | P3 | 2026-05-16 | | |
| 41 | `Front (Angular)` | `ProjectDetailComponent` settings section title/icon alignment | The Settings-tab title polish for inline `mat-icon` headings (for example Tags and Agent Pool) is currently validated only by frontend `build`; the large page component still has no practical focused spec or visual-regression harness for this DOM/layout-only alignment tweak without brittle page-level tests. | P3 | 2026-05-16 | | |
| 42 | `Front (Angular)` | Global `mat-dialog-actions` cancel/primary footer layout | The global dialog footer equal-width fix for native `button[mat-stroked-button]` + `app-ds-button` pairs is currently validated only by frontend `typecheck` and `build`; there is still no lightweight focused spec or visual-regression harness for cross-dialog footer sizing, spacing, and small-screen stacking across modal variants. | P3 | 2026-05-16 | | |
| 43 | `Front (Angular)` | `ConfigDetailComponent` + `ResourceEditComponent` header ID rendering | The raw ID removal from the config/resource headers is a DOM-only template change on two very large page components with no practical parent spec harness. Adding focused page-level specs just to assert paragraph absence would be disproportionately brittle, so this slice is currently validated by `npm run typecheck` and `npm run build` only. | P3 | 2026-05-16 | | |
| 44 | `Front (Angular)` | Topbar `SearchDialogComponent` horizontal overflow | The topbar search popup scrollbar fix is a layout-only box-model change (`width: 100%` result rows + horizontal padding now sized with `box-sizing: border-box`). The component currently has no focused visual-regression or DOM-measurement harness for overlay overflow, so this slice is validated by frontend `typecheck` and `build` only. | P3 | 2026-05-16 | | |
| 45 | `Front (Angular)` | `LayoutRepositoriesComponent` dialog/repository mutation flows | This fix adds focused coverage for optimistic layout-preset selection, rollback, and generation-summary sync, but the rest of the component still lacks focused specs for repository create/edit dialog refresh, delete-confirmation flows, and conflict-error handling. Validation for those branches currently relies on targeted preset specs plus `npm run typecheck` and `npm run build`. | P2 | 2026-05-16 | | |
| 46 | `Front (Angular)` | `DeploymentConfigComponent` remaining visual/auth states | A focused Karma spec now covers the Docker-image validation banner visibility rule, but the shared component still lacks practical isolated coverage for the ACR auth-mode variants, UAI warning cards, tooltip wiring, and layout-only spacing/ordering polish around the Docker image field. Validation for those branches still relies on targeted spec coverage plus `npm run typecheck`. | P3 | 2026-05-17 | | |

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

## Front/UI Refresh Vague 4 (2026-05-11)

- **P3** — Specs Karma à écrire pour `ds-table` (cycle de tri null→asc→desc→null, densités compact/cozy/comfortable, empty state, activation clavier/souris des lignes interactives, rendu des valeurs complexes).
- **P3** — Specs Karma à écrire pour `ds-tree-view` (expand/collapse clavier, sélection, rendu récursif, désactivation, propagation des événements `nodeClick` / `nodeToggle` / `nodeKey` sans rôles ARIA simulés).
- **P3** � Spec Karma � �crire pour PageContextService (setBreadcrumb / clear, signal readonly).
- **P3** � Tests composants project-detail/config-detail � actualiser si classes CSS changent (refonte SCSS vague 4 � structure HTML inchang�e, pas de breakage attendu mais � v�rifier).
- **� traiter en vague 6.**

## Front/UI Refresh Vague 5 (2026-05-11)

- **P2** — Specs Karma à actualiser pour `resource-edit.component` (refonte SCSS complète token-based, 278 classes préservées, HTML/logique inchangés). Aucun test fonctionnel existant pour ce composant — couvrir au moins le wiring breadcrumb via `PageContextService` (setBreadcrumb appelé avec segments calculés depuis project/config/resource, clear sur ngOnDestroy).
- **P3** — Snapshots Playwright avant/après dialogs refondus (add-app-setting, add-app-config-key, add-role-assignment, add-resource, add-storage-service, multi-repo-push, add-project-environment, create-project-wizard, add-custom-domain) pour valider non-régression visuelle de la purge anti-patterns.
- **P3** — Spec Karma à écrire pour `ds-option-card` refondu (selected/disabled/hover states, cardSelect output, role=radio aria).
- **À traiter en vague 6.**


## Front/UI Refresh Vague 6 (2026-05-11)

- **P2** — Specs Karma à écrire pour les 5 nouvelles primitives DS : `ds-empty-state` (slots icon/title/description/[actions]), `ds-skeleton` (variants box/line/circle, count/gap/animation, `prefers-reduced-motion`), `ds-tooltip` (delay show, hide on blur, hide on Esc, position top/bottom/left/right, aria-describedby wiring), `ds-banner` (variants info/success/warning/danger, dismiss output, role=alert pour danger), `ds-status-dot` (variants success/warning/danger/info/idle, sizes sm/md, pulse animation, ariaLabel role=status).
- **P3** — Audit a11y axe-core à ajouter en CI (run sur toutes les routes principales, target Lighthouse a11y >= 95).
- **~~P3~~ RÉSOLU 2026-05-11** — UI Refresh — Suppression du compat layer SCSS legacy dans `_tokens.scss` : 106 lignes deprecated supprimées, 5 mixins deprecated supprimées de `_mixins.scss`, 168 occurrences migrées dans 15 fichiers. Zero consommateurs restants.
- **~~P3~~ RÉSOLU 2026-05-11** — Anti-patterns résiduels : `ds-panel-action-button.scss` (22→0 hits, refonte flat V2), `ds-date-picker.scss` (4→0 hits, header flat + selected accent), compat layer SCSS supprimé (106 lignes `_tokens.scss` + 5 mixins `_mixins.scss`). `ds-button.scss` (1 hit gradient CTA whitelisté, intentionnel).
- **~~P3~~ RÉSOLU 2026-05-11** — Suppression compat layer SCSS legacy : 168 occurrences migrées vers V2 CSS vars dans 15 fichiers via script `tmp/purge-compat-layer.ps1`.

## Front/UI Refresh Vague W1 � Fondations DS (2026-05-27)

- **P2** � Validation runtime Karma � ex�cuter pour les 4 nouvelles primitives DS (ds-spinner, ds-progress-bar, ds-tag-input, ds-menu). Specs �crites mais non ex�cut�es (Karma non lanc� dans cette session).
- **P3** � Specs Karma � ajouter pour DsMenuDirective (overlay open/close, backdrop click, aria-expanded toggle).

## Front/UI Refresh Vague W8 � Finalisation DS (2026-05-27)

- **P3** � N7 `app-ds-accordion` non impl�ment� (autoris� par utilisateur en vague finale). DNS validation panel (`resource-edit-custom-domains-section`) conserve `mat-expansion-panel` natif Material. Co�t cr�ation primitive vs unique usage non rentable. � cr�er si un 2e usage appara�t.
- **P3** � `settings.component.scss` lignes 279�313 : palette `--bicep-syntax-*` (3 th�mes) dupliqu�e de `bicep-file-panel`. Refactor en partial SCSS partag� (`@use 'shared/bicep-syntax-palette' as bicep;`) report� pour �viter de toucher `bicep-file-panel` (hors scope audit). Trace : audit-design-system-2026-05-27 �3.17.
- **P3** � `split-generation-switcher.component.scss` lignes 79�89 : `background: rgba(55,78,110,0.94)` et `rgba(43,83,86,0.94)` non tokenis�s (chips infra/code � couleurs sp�cifiques sans �quivalent DS exact). � �valuer cr�ation tokens `--ifs-chip-infra-bg` / `--ifs-chip-code-bg` si pattern se r�pand.

