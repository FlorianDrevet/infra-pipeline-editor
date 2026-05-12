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
public interface IQuery<TResult> : IRequest<ErrorOr<TResult>>;
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, ErrorOr<TResult>>
    where TCommand : ICommand<TResult>;
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, ErrorOr<TResult>>
    where TQuery : IQuery<TResult>;
```

**Convention:** never use `IRequest<ErrorOr<T>>` or `IRequestHandler<,>` directly.

## Typed Dynamic Dispatch [2026-04-29]

- For heterogeneous resource-creation flows (`Application/Imports/Common/Creation/ResourceCommandFactory`), keep the dynamic selection at the command-building boundary, but dispatch concrete creation commands through explicit `ICommand<TResult>` cases.
- Canonical shape: resource type + typed context in, typed switch on concrete `Create*Command`, generic helper constrained to `ICommand<TResult>`, and `ErrorOr<Guid>` out.
- Do not reintroduce `IMediator.Send((object)...)`, reflection over `ErrorOr<T>`, or `object`-based post-processing to recover `Id`/errors.

## Unit of Work [2026-03-30]

- `IUnitOfWork` / `UnitOfWork` wraps `ProjectDbContext.SaveChangesAsync`
- `ValidationBehavior` only applies to commands implementing `ICommandBase`; queries must bypass FluentValidation even if a validator exists for the query type.
- `UnitOfWorkBehavior` only applies to `ICommand<T>` (via `ICommandBase` constraint)
- Pipeline order: `ValidationBehavior` → `UnitOfWorkBehavior` → Handler
- **Critical:** Repositories MUST NOT call `SaveChangesAsync()`.

## Registration

- `DependencyInjection.cs` (Application) registers MediatR, ValidationBehavior, UnitOfWorkBehavior, validators by assembly scan.
- `DependencyInjection.cs` (Infrastructure) registers `IUnitOfWork`.

## Validator Cross-Rules [2026-04-30]

- `CreateProjectWithSetupCommandValidator` is the canonical example of a project-slice validator that mixes scalar rules with aggregate cross-rules: `LayoutPreset` drives the allowed project-level repository count (`AllInOne` = 1, `SplitInfraCode` = 2, `MultiRepo` = 0), and repository connection details (`ProviderType`, `RepositoryUrl`, `DefaultBranch`) must be provided all together or all omitted.
- Project-level Git push commands sharing the same input envelope (`ProjectId`, `BranchName`, `CommitMessage`) should stay aligned on the same guard rails: non-empty project id, branch name required with the existing Git-safe regex and `200`-character cap, and commit message required with a `500`-character cap. `PushProjectBootstrapPipelineToGitCommandValidator` and `PushProjectGeneratedArtifactsToGitCommandValidator` were added to align this slice with the pre-existing `PushProjectBicepToGitCommandValidator` and `PushProjectPipelineToGitCommandValidator`.
- Project-level optional settings commands should validate only the input contract they own and preserve the handler/domain clear semantics. `SetAgentPoolCommandValidator` now enforces a non-empty `ProjectId` and a `200`-character cap only when `AgentPoolName` is non-blank, while `SetProjectDefaultNamingTemplateCommandValidator` enforces a non-empty `ProjectId` and reuses `NamingTemplateValidationRules` only when `Template` is non-null, so `null` still means “clear the setting”.
- `AddInfraConfigRepositoryCommandValidator` is the current APP-001 reference for infra-config repository commands: require `ProjectId` and `ConfigId`, enforce the shared alias contract (`^[a-z0-9-]+$`, max `50`), require the full connection tuple (`ProviderType`, `RepositoryUrl`, `DefaultBranch`), and reject empty `ContentKinds`. Reuse `RepositoryConnectionValidationRules` for provider/url consistency instead of duplicating ad-hoc parsing guards in the handler.
- `UpdateAppConfigurationCommandValidator` is the current APP-001 follow-up for typed resource update commands that still lacked FluentValidation: keep the rule set narrow and aligned with the handler-owned contract (`Id` and `Name` required), while leaving optional `EnvironmentSettings` semantics untouched.
- Keep these orchestration-style checks in FluentValidation when they are pure input consistency checks and do not require repository access.

## Orchestration Handlers [2026-05-11]

- `CreateProjectWithSetupCommandHandler` is the reference shape for an application command that creates an aggregate, applies project defaults, parses enum-backed inputs, projects child collections, and persists once.
- Keep `Handle` linear and orchestration-only: create the aggregate, apply the layout preset, add environments, add repositories, then persist. Push enum/value-object parsing and per-item projection into private helpers once dedicated handler tests protect the slice.

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
- `MemberCommandHelper` for owner-only member management
- Access check: ResourceGroup has `InfraConfigId` directly; KeyVault/RedisCache have `ResourceGroupId` → load ResourceGroup → use `InfraConfigId`
- For identity-scoped read queries such as `ListRoleAssignmentsByIdentityQueryHandler`, collapse identity-not-found, parent-resource-group-not-found, and denied-read-access outcomes to the same not-found result to avoid leaking authorization boundaries; keep the handler split into focused helpers for access validation, referenced-resource loading, and projection.

## Batch Summary Loading [2026-05-12]

- `ListCrossConfigReferencesQueryHandler` is the reference fix for residual query-side N+1 when only target config names are needed.
- Prefer a batch summary repository method such as `IInfrastructureConfigRepository.GetConfigSummariesByIdsAsync(...)` over looping on `GetByIdAsync(...)` when a query only needs lightweight metadata (`Id`, `Name`) for multiple configs.
- Keep the handler orchestration simple: distinct target config IDs -> one batch summary query -> one batch resource metadata query -> in-memory join for the final result.
- Do not load full aggregates just to resolve names in read-only queries.

## Domain Services [2026-04-16]

- `IRoleAssignmentDomainService` / `RoleAssignmentDomainService`: extracted cross-cutting role assignment logic shared by Add/Remove/Assign/Unassign/Update identity handlers.
- Pattern: when 3+ handlers share identical domain logic (load resource, check access, validate, mutate), extract into a domain service interface + implementation registered in `Application/DependencyInjection.cs`.
- Domain services live under `Application/{Feature}/Common/`.

## Shared Handler Extraction With Leverage [2026-05-12]

- Do not introduce an `*Orchestrator` just because a handler is large. Extract only when at least one non-trivial responsibility is genuinely duplicated across handlers.
- `IAppPipelineRequestFactory` is the current reference slice for APP-005: it centralizes the repeated `AzureResourceType -> load typed compute resource -> optionally resolve Container Registry -> build AppPipelineGenerationRequest` flow that was duplicated in both `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler`.
- `BlobDownloadHelper` is the follow-up APP-005 slice for duplicated "latest artifact" retrieval: extend this helper with narrow operations such as `GetLatestBlobFilesAsync(...)` and `GetLatestDualBucketBlobFilesAsync(...)` instead of embedding timestamp-bucket selection and prefix rewriting directly inside large project push/download handlers.
- `PushProjectGeneratedArtifactsToGitCommandHandler` is the current mono-repo APP-005 follow-up: it must reuse `BlobDownloadHelper.GetLatestBlobFilesAsync(...)` for project-level `bicep`, `pipeline`, and `bootstrap` artifacts instead of keeping a private latest-prefix loader, and its focused handler tests now lock both pipeline path normalization and pipeline not-found propagation.
- `GenerationRequestBuilder.BuildForPipeline(...)` is the current reference extraction for pipeline-generation handlers: keep the shared `GenerationRequest` mapping for `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler` in this builder, including pipeline variable-group mappings, secure-parameter overrides, agent pool, and `BicepBasePath`, instead of rebuilding the same resource/environment/naming blocks inline in each handler.
- `IConfigPipelineGenerationService` is the next reference extraction on top of `GenerationRequestBuilder`: when `GeneratePipelineCommandHandler` and `GenerateProjectPipelineCommandHandler` still share the same orchestration for `BuildForPipeline(...)` + compute-resource request creation + `AppPipelineGenerationEngine.GenerateAll(...)`, keep that flow in this focused application service instead of letting both handlers grow back with the same lower-level dependencies.
- `IApplicationFolderNameResolver` is the reference extraction for project bootstrap generation when multiple compute-resource repositories are only used to resolve the generated application folder name. `GenerateProjectBootstrapPipelineCommandHandler` should depend on this resolver rather than directly on `IContainerAppRepository`, `IWebAppRepository`, and `IFunctionAppRepository`.
- When `BlobDownloadHelper.GetLatestDualBucketBlobFilesAsync(...)` needs multiple bucket-related knobs, pass them through `BlobDownloadHelper.DualBucketBlobFilesOptions` rather than growing the helper method signature again; this is the local reference fix for the PR #328 Sonar `S107` finding.
- `MultiScopeGitPushRequestBuilder` is the current APP-005 reference slice for pure shared Git push composition: keep the merge of scoped files, root-folder re-slicing, and collision detection in this static helper instead of duplicating the same private `BuildPushRequest(...)` / `TryAddScopedFile(...)` logic across mono-repo and multi-repo push handlers.
- `IMultiScopeGitPushExecutor` is the next APP-005 extraction after `MultiScopeGitPushRequestBuilder`: when multiple handlers already build a `MultiScopeGitPushRequest`, centralize only the provider resolution + `IGitMultiScopePushProviderService` capability guard + `PushScopedFilesAsync(...)` call in this service, while each handler keeps its own request construction and result shaping.
- When a handler already authorizes a project through `IProjectAccessService.VerifyWriteAccessAsync(...)`, reuse the returned `Project` aggregate for layout-only guards such as `CanGenerateAllFromProjectLevel()` instead of reloading the project and additional domain configs just to re-check the same aggregate state.
- When a project-level generation flow needs both repository-routing metadata and pipeline variable groups, prefer a dedicated enriched repository lookup such as `IProjectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(...)` over chaining `GetByIdWithAllAsync(...)` and `GetByIdWithPipelineVariableGroupsAsync(...)` for the same aggregate in the same handler.
- The same combined-project lookup rule now also applies to config-level pipeline generation: `GeneratePipelineCommandHandler` should load routing metadata + pipeline variable groups through `GetByIdWithAllAndPipelineVariableGroupsAsync(...)` instead of chaining `GetByIdWithPipelineVariableGroupsAsync(...)` and `GetByIdWithAllAsync(...)` after infra-config access succeeds.
- This kind of extraction belongs in `Application/Common/Interfaces/Services` + `Application/Common/Services` when the shared logic spans multiple feature folders and depends on application repositories, but does not justify a larger orchestration abstraction.
