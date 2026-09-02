# CQRS Pattern

## Folder Structure

```
src/Api/InfraFlowSculptor.Application/
└── FeatureName/
    ├── Commands/
    │   └── DoSomethingCommand/
    │       ├── DoSomethingCommand.cs          record : ICommand<T>
    │       ├── DoSomethingCommandHandler.cs   ICommandHandler<Cmd, T>
    │       └── DoSomethingCommandValidator.cs AbstractValidator<DoSomethingCommand>
    ├── Queries/
    │   └── GetSomethingQuery/
    │       ├── GetSomethingQuery.cs           record : IQuery<T>
    │       └── GetSomethingQueryHandler.cs    IQueryHandler<Query, T>
    └── Common/
        └── SomethingResult.cs                 Application-layer result DTO
```

## Marker Interfaces [2026-03-30]

Commands and queries use project-owned marker interfaces:
```csharp
public interface ICommandBase;
public interface ICommand<TResult> : IRequest<ErrorOr<TResult>>, ICommandBase;
public interface IGenerateCommand<TResult> : ICommand<TResult>;
public interface IQuery<TResult> : IRequest<ErrorOr<TResult>>;
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, ErrorOr<TResult>>
    where TCommand : ICommand<TResult>;
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, ErrorOr<TResult>>
    where TQuery : IQuery<TResult>;
```

**Convention:** never use `IRequest<ErrorOr<T>>` or `IRequestHandler<,>` directly.
- `IGenerateCommand<TResult>` is the dedicated marker for artifact-generation commands that must require the PAT `Generate` scope instead of the broader `Write` scope.

## Typed Dynamic Dispatch [2026-04-29]

- For heterogeneous resource-creation flows (`Application/Imports/Common/Creation/ResourceCommandFactory`), keep the dynamic selection at the command-building boundary, but dispatch concrete creation commands through explicit `ICommand<TResult>` cases.
- Canonical shape: resource type + typed context in, typed switch on concrete `Create*Command`, generic helper constrained to `ICommand<TResult>`, and `ErrorOr<Guid>` out.
- Do not reintroduce `IMediator.Send((object)...)`, reflection over `ErrorOr<T>`, or `object`-based post-processing to recover `Id`/errors.

## Unit of Work [2026-03-30]

- `IUnitOfWork` / `UnitOfWork` wraps `ProjectDbContext.SaveChangesAsync`
- `ValidationBehavior` only applies to commands implementing `ICommandBase`; queries must bypass FluentValidation even if a validator exists for the query type.
- `PersonalAccessTokenScopeBehavior` enforces PAT scopes centrally: `IQuery<T>` requires `Read` (or `Write`), `ICommand<T>` requires `Write`, and `IGenerateCommand<T>` requires `Generate`.
- `UnitOfWorkBehavior` only applies to `ICommand<T>` (via `ICommandBase` constraint)
- Pipeline order: `ValidationBehavior` → `PersonalAccessTokenScopeBehavior` → `UnitOfWorkBehavior` → Handler
- **Critical:** Repositories MUST NOT call `SaveChangesAsync()`.

## Registration

- `DependencyInjection.cs` (Application) registers MediatR, ValidationBehavior, PersonalAccessTokenScopeBehavior, UnitOfWorkBehavior, validators by assembly scan.
- `DependencyInjection.cs` (Infrastructure) registers `IUnitOfWork`.

## Validator Cross-Rules [2026-04-30]

- `CreateProjectWithSetupCommandValidator` is the canonical example of a project-slice validator that mixes scalar rules with aggregate cross-rules: `LayoutPreset` drives the allowed project-level repository count (`AllInOne` = 1, `SplitInfraCode` = 2, `MultiRepo` = 0), and repository connection details (`ProviderType`, `RepositoryUrl`, `DefaultBranch`) must be provided all together or all omitted.
- Project-level Git push commands sharing the same input envelope (`ProjectId`, `BranchName`, `CommitMessage`) should stay aligned on the same guard rails: non-empty project id, branch name required with the existing Git-safe regex and `200`-character cap, and commit message required with a `500`-character cap. `PushProjectBootstrapPipelineToGitCommandValidator` and `PushProjectGeneratedArtifactsToGitCommandValidator` were added to align this slice with the pre-existing `PushProjectBicepToGitCommandValidator` and `PushProjectPipelineToGitCommandValidator`.
- Project-level optional settings commands should validate only the input contract they own and preserve the handler/domain clear semantics. `SetAgentPoolCommandValidator` now enforces a non-empty `ProjectId` and a `200`-character cap only when `AgentPoolName` is non-blank, while `SetProjectDefaultNamingTemplateCommandValidator` enforces a non-empty `ProjectId` and reuses `NamingTemplateValidationRules` only when `Template` is non-null, so `null` still means “clear the setting”.
- `AddInfraConfigRepositoryCommandValidator` is the current APP-001 reference for infra-config repository commands: require `ProjectId` and `ConfigId`, require the full connection tuple (`ProviderType`, `RepositoryUrl`, `DefaultBranch`), and reject empty `ContentKinds`. Repository aliases are obsolete; use repository ids and content-kind roles for routing/identity.
- `UpdateAppConfigurationCommandValidator` is the current APP-001 follow-up for typed resource update commands that still lacked FluentValidation: keep the rule set narrow and aligned with the handler-owned contract (`Id` and `Name` required), while leaving optional `EnvironmentSettings` semantics untouched.
- `RevokePersonalAccessTokenCommandValidator` is the current APP-001 follow-up for identity-only security commands: keep the rule set minimal (`Id` required) and leave ownership / already-revoked semantics to the handler, which still owns the authenticated-user check.
- `SetProjectResourceNamingTemplateCommandValidator` and `SetProjectResourceAbbreviationCommandValidator` are the current reference slice for catalog-backed string inputs: validate `ResourceType` against `AzureResourceTypes.All` at the validator boundary instead of introducing a cross-cutting `ResourceTypeName` value object into the Project aggregate flow [2026-05-13].
- `AllCommandsHaveValidatorsTests` is the durable APP-001 guardrail: every concrete `ICommand<T>` in the Application assembly must have a FluentValidation validator type discovered by assembly scan. When a new command is added, the structural test should fail until a validator exists.
- The APP-001 closure rule is now explicit: every command gets a validator, but validators stay input-only. Use them for required IDs, required top-level strings, max lengths, and parse-safe enum/catalog checks; leave aggregate existence, ownership, authorization, and business-state semantics in handlers/domain services.
- APP-012 closure rule [2026-05-13]: reduce validator duplication with narrow shared rule helpers such as `TagValidationRules` and `RepositoryConnectionValidationRules`, not with a generic cross-cutting `EntityCommandValidator<T>` base class.
- The allowed exception is a tight homogeneous family with a real shared contract. `CreateResourceCommandValidator<T>` + `ICreateResourceCommand` is the current reference: it centralizes only `ResourceGroupId`, `Name`, and `Location`, while concrete validators keep resource-specific rules explicit.
- Keep these orchestration-style checks in FluentValidation when they are pure input consistency checks and do not require repository access.

## Orchestration Handlers [2026-05-11]

- `CreateProjectWithSetupCommandHandler` is the reference shape for an application command that creates an aggregate, applies project defaults, parses enum-backed inputs, projects child collections, and persists once.
- Keep `Handle` linear and orchestration-only: create the aggregate, apply the layout preset, add environments, add repositories, then persist. Push enum/value-object parsing and per-item projection into private helpers once dedicated handler tests protect the slice.
- Project repository create/update now re-verifies configured repository details server-side before persisting. `PersonalAccessToken` is transient command input only: create requires it for configured repositories, update uses the new token when supplied or the stored repository-scoped secret otherwise. `VerifyProjectRepositoryConnectionCommand` is the stateless pre-save branch discovery flow used by the Angular modal.

## Enum-Backed Input Parsing [2026-05-11]

- `Application/Common/Helpers/EnumValueObjectParser.cs` is the shared helper for the narrow handler pattern `string -> Enum.TryParse(ignoreCase: true) -> EnumValueObject -> ErrorOr`.
- Use `Parse<TEnum, TValueObject>(...)` for required enum-backed inputs and `ParseOrNull<TEnum, TValueObject>(...)` only when `null` is the sole "missing" value. If a feature treats whitespace as "unset" (for example optional repository provider/layout inputs), keep that wrapper logic local in the handler and call `Parse(...)` only after the whitespace guard.
- `Application/Common/Helpers/RepositoryContentKindsParser.cs` centralizes the repeated handler-side parsing of `RepositoryContentKinds` flags from `IReadOnlyList<string>`.
- When extracting this kind of shared parser, keep at least one direct handler test per handler family for the handler-owned invalid branches (`invalid provider`, `invalid layout`, `invalid content kinds`) instead of relying only on helper tests or broader orchestration tests.
- Do not route FluentValidation rules, import fallbacks, fuzzy enum normalization, or post-parse business checks through these helpers. Those cases stay explicit at the validator/import/domain-service boundary.

## Resource Creation Handlers [2026-05-11]

- `CreateRedisCacheCommandHandler` is the reference shape for a resource-creation handler that first validates parent resource-group existence and write access, then parses optional enum-backed inputs, then creates and persists the aggregate.
- For handlers of this kind, keep `Handle` linear: authorize the parent scope, parse optional settings (`TlsVersion`, per-environment `RedisCacheSku`, `MaxMemoryPolicy`) in private helpers, create the aggregate, persist once, then map the result.

## Project Result Mapping [2026-04-29]

- The `Projects` slice no longer relies on injected `MapsterMapper.IMapper` inside `CreateProjectCommandHandler`, `CreateProjectWithSetupCommandHandler`, `GetProjectQueryHandler`, and `ListMyProjectsQueryHandler`.
- Canonical mapping from `Domain.ProjectAggregate.Project` to `Application.Projects.Common.ProjectResult` now lives in `Application/Projects/Common/ProjectResultMapper.cs`.
- `Api/Common/Mapping/ProjectMappingConfig.cs` delegates its `Project -> ProjectResult` Mapster rule to the same helper so API and MCP return the same shape without forcing the MCP host to load API-host DI registrations.
- Use this pattern when an Application handler needs to return an Application result model that is also consumed outside the API host: keep the domain-to-application mapping in Application, not in the API executable composition root.

## User Provisioning [2026-04-22]

- User auto-provisioning is handled by `UserProvisioningMiddleware` (ASP.NET Core middleware, `Api/Common/`).
- Runs after `UseAuthorization()`, before endpoint execution.
- On authenticated request: checks if user exists by EntraId, creates + saves immediately if not.
- Stores `UserId` in `HttpContext.Items["ProvisionedUserId"]`.
- `ICurrentUser.GetUserIdAsync()` reads from `HttpContext.Items` (synchronous, no DB call).
- `IUserRepository` is read-only: `GetByEntraIdAsync` (no create method). No `SaveChangesAsync` in repos.
- **Key design:** middleware owns its own persistence (outside MediatR UoW scope), repos stay pure reads.

## Shared Authorization Service

- `IInfraConfigAccessService` (injectable): `VerifyReadAccessAsync`, `VerifyWriteAccessAsync`
- `InfraConfigAccessService.VerifyReadAccessAsync(...)` is now the DB-003 reference split for infra-config authorization: use `IInfrastructureConfigRepository.GetByIdReadOnlyAsync(...)` for read flows, but keep `VerifyWriteAccessAsync(...)` on tracked `GetByIdAsync(...)` when later mutations can still occur.
- `AddCrossConfigReferenceCommandHandler` is the reference caveat for tracked infra-config writes that need a child collection invariant: `VerifyWriteAccessAsync(...)` returns a tracked `InfrastructureConfig`, but it does not load `CrossConfigReferences`. Before calling `InfrastructureConfig.AddCrossConfigReference(...)`, the handler must reload the aggregate through `GetByIdWithMembersAsync(...)` so the domain duplicate check runs against the persisted references instead of letting PostgreSQL reject the duplicate on `IX_CrossConfigResourceReferences_InfraConfigId_TargetResourceId` [2026-05-28].
- The APP-008 closure rule is now explicit: keep the security split as-is. Read-detail flows may mask denied access to resource/config not-found, but write flows must preserve `Forbidden` for insufficient roles instead of remasking it away. `ProjectAccessService` is the reference on project membership (`NotFound` for non-member, `Forbidden` for reader/non-owner on write/owner checks), and `InfraConfigAccessService` is the config-level reference (`VerifyReadAccessAsync(...)` remasks project-access failures to config not-found, `VerifyWriteAccessAsync(...)` preserves `Forbidden`).
- `MemberCommandHelper` for owner-only member management
- Access check: ResourceGroup has `InfraConfigId` directly; KeyVault/RedisCache have `ResourceGroupId` → load ResourceGroup → use `InfraConfigId`
- For identity-scoped read queries such as `ListRoleAssignmentsByIdentityQueryHandler`, collapse identity-not-found, parent-resource-group-not-found, and denied-read-access outcomes to the same not-found result to avoid leaking authorization boundaries; keep the handler split into focused helpers for access validation, referenced-resource loading, and projection.

## Batch Summary Loading [2026-05-12]

- `ListCrossConfigReferencesQueryHandler` is the reference fix for residual query-side N+1 when only target config names are needed.
- Prefer a batch summary repository method such as `IInfrastructureConfigRepository.GetConfigSummariesByIdsAsync(...)` over looping on `GetByIdAsync(...)` when a query only needs lightweight metadata (`Id`, `Name`) for multiple configs.
- Keep the handler orchestration simple: distinct target config IDs -> one batch summary query -> one batch resource metadata query -> in-memory join for the final result.
- Do not load full aggregates just to resolve names in read-only queries.

## Read Repositories For Hot Queries [2026-05-22]

- `IContainerAppReadRepository` and `IProjectResourceReadRepository` are the current performance reference for hot read paths that only need response DTO fields. They return Application-layer read results/projections and never domain aggregates.
- Detail read repositories that need authorization context should return a small typed result carrying both the DTO and the owning authorization scope. `ContainerAppDetailReadResult` carries `ContainerAppResult` plus `InfrastructureConfigId`, so `GetContainerAppQueryHandler` can verify access before returning the DTO.
- Keep handlers thin: authorize the scope, call one read repository projection, return the projected result. Do not inject Mapster into query handlers just to map a hot detail DTO when the read repository already shapes the result.
- If more than three resource detail endpoints need the same projection pattern, consider extracting a narrow shared read-projection helper. Until then, prefer explicit per-resource read repositories over a generic layer that hides SQL shape.

## Domain Services [2026-04-16]

- `IRoleAssignmentDomainService` / `RoleAssignmentDomainService`: extracted cross-cutting role assignment logic shared by Add/Remove/Assign/Unassign/Update identity handlers.
- Pattern: when 3+ handlers share identical domain logic (load resource, check access, validate, mutate), extract into a domain service interface + implementation registered in `Application/DependencyInjection.cs`.
- APP-011 is now the closure proof for this pattern: do not create extra domain-service layers when the existing shared service already centralizes the duplicated cross-cutting behavior cited by the audit.
- Domain services live under `Application/{Feature}/Common/`.

## Shared Handler Extraction With Leverage [2026-05-12]

- Do not introduce an `*Orchestrator` just because a handler is large. Extract only when at least one non-trivial responsibility is genuinely duplicated across handlers.
- `IAppPipelineRequestFactory` is the current reference slice for APP-005: it centralizes the repeated `AzureResourceType -> load typed compute resource -> optionally resolve Container Registry -> build AppPipelineGenerationRequest` flow that was duplicated in both `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler`.
- `BlobDownloadHelper` is the APP-005 reference for duplicated latest-artifact retrieval; extend it with narrow operations and option records instead of embedding timestamp-bucket selection or prefix rewriting directly inside large push/download handlers.
- `GenerationRequestBuilder.BuildForPipeline(...)` is the current reference extraction for pipeline-generation handlers: keep the shared `GenerationRequest` mapping for `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler` in this builder, including pipeline variable-group mappings, secure-parameter overrides, agent pool, and `BicepBasePath`, instead of rebuilding the same resource/environment/naming blocks inline in each handler.
- `IConfigPipelineGenerationService`, `IProjectPipelineAggregator`, and `IMonoRepoBlobUploadOrchestrator` are the focused application-service seams for shared project/config generation orchestration and mono-repo artifact uploads; keep them narrow instead of rebuilding large orchestration flows in handlers.
- `IApplicationFolderNameResolver` is the bootstrap-generation seam when multiple compute-resource repositories are only used to resolve the generated application folder name.
- `MultiScopeGitPushRequestBuilder` owns shared Git push composition, while `IMultiScopeGitPushExecutor` owns the provider-resolution and `PushScopedFilesAsync(...)` execution seam.
- `ProjectMultiRepoArtifactsPushService` is the project-level bulk-push seam for configuration-owned repositories: it validates and prepares all configuration plans before invoking independent Git pushes. Do not call config-level MediatR handlers from this service; keep orchestration and per-target results explicit.
- When a handler already authorizes a project, reuse the returned aggregate for layout-only guards and prefer enriched repository lookups such as `GetByIdWithAllAndPipelineVariableGroupsAsync(...)` over reloading the same aggregate through chained calls.
- This kind of extraction belongs in `Application/Common/Interfaces/Services` + `Application/Common/Services` when the shared logic spans multiple feature folders and depends on application repositories, but does not justify a larger orchestration abstraction.
