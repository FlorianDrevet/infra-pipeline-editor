# Persistence (EF Core)

## DbContext
- `ProjectDbContext` lives in `src/Api/InfraFlowSculptor.Infrastructure/Persistence/ProjectDbContext.cs`.
- PostgreSQL target with `ApplyConfigurationsFromAssembly()`.
- `ProjectDbContext.SaveChangesAsync(...)` is now the minimal in-process domain-event dispatch boundary: collect `IHasDomainEvents` aggregates from the change tracker, save first, clear their events, then dispatch them through the optional `IDomainEventDispatcher`. Keep this seam in-process only; outbox/audit/event-sourcing remain separate concerns.

## Configuration Pattern
- One sealed `IEntityTypeConfiguration<T>` per aggregate/entity: table/key mapping, typed converters, indexes, and navigation configuration.
- Optional enum-backed and strongly typed ID columns must use `NullableEnumValueConverter<,>` and `NullableIdValueConverter<>`, not their non-nullable counterparts.

## Canonical String Lengths [2026-05-12]
- `Project.Name` = `80`; `Project.DefaultNamingTemplate` = `500`.
- `ProjectEnvironmentDefinition.Name` = `100`; `ShortName` = `20`; `Prefix` / `Suffix` = `50`.
- `InfrastructureConfig.Name` = `100`; `InfrastructureConfig.DefaultNamingTemplate` = `500`.
- `ResourceGroup.Name` = `90`; `AzureResource.Name` / `CustomNameOverride` = `260`.
- `ParameterDefinition.Name` = `100`; `Type` = `20`; `DefaultValue` = `500`.
- `ProjectResourceNamingTemplate.Template` and `ResourceNamingTemplate.Template` = `500`.
- `BlobContainer.Name`, `StorageQueue.Name`, and `StorageTable.Name` = `63`; `RoleAssignment.RoleDefinitionId` = `36`.
- `FunctionApp.RuntimeVersion` / `WebApp.RuntimeVersion` = `20`; `FunctionApp.DockerImageName` / `WebApp.DockerImageName` = `512`.
- `AppServicePlanEnvironmentSettings.EnvironmentName`, `FunctionAppEnvironmentSettings.EnvironmentName`, `WebAppEnvironmentSettings.EnvironmentName`, `SqlServerEnvironmentSettings.EnvironmentName`, and `SqlDatabaseEnvironmentSettings.EnvironmentName` = `100`.
- `FunctionAppEnvironmentSettings.DockerImageTag` and `WebAppEnvironmentSettings.DockerImageTag` = `128`.
- When a persistence cap is introduced, align the request contract and validator in the same change set. The current reference slices are `CreateProject`, `CreateInfrastructureConfig`, and `CreateResourceGroup`.
- `CoreStringLengthConfigurationTests` is now the DB-001 guardrail for every persisted `string` column in the EF model. It intentionally excludes keyless views and model-side `string` properties converted to non-string provider columns (for example `InputOutputLink` persisted as integers) [2026-05-13].

## Model Conventions
- For index coverage verification, use a relational provider (`Npgsql`) rather than the InMemory provider; `IndexCoverageConfigurationTests` is the reference test.
- Verified DB-002-obsolete coverage: explicit indexes on `InfrastructureConfig.ProjectId` and `AzureResource.ResourceType`, convention/FK indexes on the main resource hierarchy FKs, and the unique composite index on `RoleAssignment(SourceResourceId, TargetResourceId, UserAssignedIdentityId, RoleDefinitionId)`.
- If an aggregate exposes filtered/computed projections over the same persisted backing field, map only the persisted navigation and `Ignore(...)` the computed projections.
- After `OwnsMany(...)` on an `IReadOnlyCollection`, always add `Navigation(...).HasField(...).UsePropertyAccessMode(Field)`.

## Shared Converters
- `IdValueConverter<TId>` and `NullableIdValueConverter<TId>` map typed IDs to `Guid` / nullable `Guid`.
- `SingleValueConverter<TValueObject, TPrimitive>` maps single-value objects.
- `EnumValueConverter<TEnumValueObject, TEnum>` and `NullableEnumValueConverter<TEnumValueObject, TEnum>` map enum value objects.
- DB-015 closure rule [2026-05-13]: reuse `NullableIdValueConverter<TId>` for nullable strongly typed identifiers instead of cloning local `Guid?` converters. The current reference usages are `ContainerAppConfiguration`, `ContainerAppEnvironmentConfiguration`, `FunctionAppConfiguration`, and `WebAppConfiguration`.

## Repository Pattern
- Repository interfaces live in Application; implementations live in Infrastructure.
- `BaseRepository<T, TContext>` owns the common tracked and read-only key lookups, plus `AddAsync`, `UpdateAsync`, and `DeleteAsync`.
- `IRepository<T>.GetAllAsync(...)` now keeps the original includes-only signature and also exposes an additive token-aware overload `GetAllAsync(CancellationToken, params includes)`. `BaseRepository` routes the legacy overload to the token-aware path, applies `AsNoTracking()` before includes, and passes the token to `ToListAsync(cancellationToken)` so default list reads stay detached. Keep `IUserRepository` as the deliberate specialized exception with its own explicit token-aware signature [2026-05-13].
- APP-012 closure decision: keep eager-loading contracts explicit (`GetByIdWithXAsync(...)`, `GetByContainedXIdAsync(...)`, read-only variants) and do not widen `IRepository<>` with a generic includes callback API. The explicit repository surface is the documented convention for this codebase [2026-05-13].
- `UserProvisioningService` is the current reference when an HTTP/auth boundary needs an atomic persistence-side existence check: it lives in Infrastructure, implements an Application interface, and uses PostgreSQL `INSERT ... ON CONFLICT ("EntraId") DO NOTHING` against the `User` table before reusing the persisted `Id` [2026-05-13].
- In EF LINQ, compare whole value objects (`x.Id == id`), never `x.Id.Value == id.Value`.
- `StorageAccountRepository` is the DB-007 reference for duplicated eager-loading graphs: keep the shared include chain in a private `WithSubResources(...)` helper.
- DB-007 follow-up [2026-05-13]: resource repositories with repeated eager-loading graphs now keep them repository-local behind private helpers instead of duplicating inline `Include(...)` chains or widening `IRepository<>`. `RepositoryIncludeHelperConventionTests` guards the touched set (`WebApp`, `FunctionApp`, `AppServicePlan`, `ApplicationInsights`, `KeyVault`, `LogAnalyticsWorkspace`, `ContainerApp`, `ContainerAppEnvironment`, `ContainerRegistry`, `CosmosDb`, `RedisCache`, `EventHubNamespace`, `ServiceBusNamespace`, `SqlServer`, `SqlDatabase`, `AppConfiguration`).

## FK Cascade / Delete Pitfalls [2026-04-04]
- `Restrict` on cross-resource FKs is unsafe when parent deletes already cascade through `AzureResources`.
- Use `SetNull` for optional references that should survive the parent deletion (`AppSetting.SourceResourceId`, `AppSetting.KeyVaultResourceId`, `AppConfigurationKey.KeyVaultResourceId`).
- Use `Cascade` for mandatory child relationships (`ResourceLinks.SourceResourceId`, `AzureResourceDependencies.DependsOnId`, `ResourceParameterUsages.ParameterId`, `RoleAssignment.TargetResourceId`).
- If one path deletes rows and another path later tries to `SetNull` the same rows, both paths must become `Cascade`.

## Polymorphic Reads And SQL Views
- `ProjectDbContext` must expose `DbSet<AzureResource> AzureResources` for polymorphic TPT lookups used by base repositories.
- `vw_ResourceEnvironmentEntries` and `vw_ChildToParentLinks` are mapped as keyless read models and back `ResourceGroupRepository` read paths.
- `GetConfiguredEnvironmentsByResourceGroupAsync(...)` already uses the view; the old DB-004 N+1 finding is obsolete on the current codebase.
- `ListResourceGroupResourcesQueryHandler` keeps the complementary optimization for Storage Account children via three narrow batch queries over blobs/queues/tables.

## Read-Only Authorization Lookups [2026-05-12]
- Rule: read flows use dedicated `*ReadOnlyAsync(...)` repository methods; write and owner flows stay on tracked lookups.
- `ProjectAccessService` and `InfraConfigAccessService` are the reference split: `VerifyReadAccessAsync(...)` is detached, `VerifyWriteAccessAsync(...)` / `VerifyOwnerAccessAsync(...)` stay tracked.
- `ResourceGroupRepository.GetByIdReadOnlyAsync(...)` is the standard lookup when a query only needs group metadata or `InfraConfigId`; pure read handlers for resource groups, Key Vault, Redis, Storage, App Configuration, and compute resources follow this pattern.
- Generic `IRepository.GetByIdReadOnlyAsync(...)`, specialized `AzureResourceRepository<TEntity>.GetByIdReadOnlyAsync(...)`, and polymorphic `IAzureResourceRepository` read-only variants cover simple detail queries plus RBAC read paths.
- `ListCrossConfigReferencesQueryHandler` and `ListAppConfigurationKeysQueryHandler` are the reference follow-ups for detached infra-config/resource-group reads.
- The DB-003 closure slice extends the same detached-read rule beyond query handlers to pure read helper services: `AppPipelineRequestFactory` and `ApplicationFolderNameResolver` now use `GetByIdReadOnlyAsync(...)` because they only enrich generation/download flows and never mutate loaded aggregates.
- The remaining read-model follow-ups now also include `GetDependentResourcesQueryHandler`, `ListAppSettingsQueryHandler`, `CheckKeyVaultAccessQueryHandler`, `GetAvailableOutputsQueryHandler`, `CheckAcrPullAccessQueryHandler`, `ListCustomDomainsQueryHandler`, `GetSecureParameterMappingsQueryHandler`, `SearchCodeRepoFilesQueryHandler`, and `ListCodeRepoBranchesQueryHandler`; they are the current reference set for swapping tracked read calls to detached variants without touching write/owner paths.
- `StorageAccountAccessHelper` routes read flows through `GetByIdWithSubResourcesReadOnlyAsync(...)`.
- `PersonalAccessTokenRepository.GetByTokenHashAsync(...)` is the deliberate exception: it stays tracked because PAT auth updates `LastUsedAt`.

## Large Read-Model Mapping Contexts [2026-05-12]
- When a private mapper starts needing many preloaded collections, group them into a dedicated local context object instead of widening the method signature.
- `InfrastructureConfigReadRepository.ResourceMappingContext` is the current reference pattern.
- `InfrastructureConfigReadRepository.MapResource(...)` must emit canonical `AzureResourceTypes.ArmTypes.*` values for read-model `ResourceType` fields. Do not reintroduce raw `Microsoft.*` ARM type strings in that mapper; keep the API read models aligned with `GenerationCore.AzureResourceTypes` [2026-05-13].
- INFRA-003 closure rule: prefer targeted summary methods, read repositories, and local read models over a generic projections/DTO layer added to every repository [2026-05-13].

## Repository Naming And Layout Persistence
- Use `GetByContainedResourceIdAsync`-style names for parent-by-child lookups; avoid ambiguous `GetByResourceIdAsync`.
- `ProjectRepositories` and `InfraConfigRepositories` persist `RepositoryContentKinds` via `RepositoryContentKindsConverter`; valid flags are now `Infrastructure` and `ApplicationCode`.
- `InfrastructureConfigs.LayoutMode` is nullable and uses `NullableEnumValueConverter<ConfigLayoutMode, ConfigLayoutModeEnum>()`.
- `InfraConfigRepositories` owns the current config-level repo topology with cascade delete and unique `(InfrastructureConfigId, Alias)`.

## Legacy Repair And Snapshot Invariants
- `scripts/fix-legacy-repository-topology.ps1` is the one-off repair for legacy `Pipelines` content-kind rows and pre-layout-driven repository topologies.
- For managed-identity ACR auth in demo snapshots, `AzureResource.AssignedUserAssignedIdentityId` must reference the same UAI that receives the `AcrPull` role.
- The API JWT secret app setting key is `JwtSettings__Secret`; `JWT_SECRET` is the Key Vault secret name, not the ASP.NET configuration key.

## Audit Architecture Study (Proposal) [2026-05-12]
- The natural capture point for a future audit trail is the EF Core boundary (`SaveChangesAsync` / interceptor), not MediatR alone.
- The recommended first storage model is PostgreSQL audit tables plus JSONB deltas, kept separate from technical observability data.
- A dedicated request-scoped audit context is needed because `ICurrentUser` is not available early enough for every write path.
- PAT `LastUsedAt` and similar noisy security updates should not share the same compliance trail as business mutations.
- Event sourcing and a separate NoSQL audit store are intentionally out of scope for the first implementation.

## Migrations
- Schema changes still require a new EF migration under `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`.
- Current DB-001 / delete-behavior reference migrations: `20260512091902_AddCoreStringLengthConstraints`, `20260512095600_AddResourceGroupNameLengthConstraint`, `20260512121558_SetNullOnAppSettingSourceResource`, and `20260512140453_AddParameterDefinitionLengthConstraints`.
- PostgreSQL view dependency pitfall [2026-05-14]: if a migration alters the type/length of a column projected by `vw_ResourceEnvironmentEntries`, PostgreSQL rejects the `ALTER COLUMN` until the view is dropped. The current reference fix is `20260514104001_SyncPendingModelChanges`, which drops and recreates `vw_ResourceEnvironmentEntries` inside both `Up` and `Down` around the affected `EnvironmentName` column alterations.
- Do not squash a sub-range in the middle of the active EF Core migration chain. The DB-008 closure decision is now explicit: the only safe squash is a full baseline reset on an empty database, coordinated as release engineering, not a partial rewrite inside a feature branch with later migrations already layered on top.
