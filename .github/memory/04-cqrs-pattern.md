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
- `UnitOfWorkBehavior` only applies to `ICommand<T>` (via `ICommandBase` constraint)
- Pipeline order: `ValidationBehavior` → `UnitOfWorkBehavior` → Handler
- **Critical:** Repositories MUST NOT call `SaveChangesAsync()`.

## Registration

- `DependencyInjection.cs` (Application) registers MediatR, ValidationBehavior, UnitOfWorkBehavior, validators by assembly scan.
- `DependencyInjection.cs` (Infrastructure) registers `IUnitOfWork`.

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
- `MemberCommandHelper` for owner-only member management
- Access check: ResourceGroup has `InfraConfigId` directly; KeyVault/RedisCache have `ResourceGroupId` → load ResourceGroup → use `InfraConfigId`

## Domain Services [2026-04-16]

- `IRoleAssignmentDomainService` / `RoleAssignmentDomainService`: extracted cross-cutting role assignment logic shared by Add/Remove/Assign/Unassign/Update identity handlers.
- Pattern: when 3+ handlers share identical domain logic (load resource, check access, validate, mutate), extract into a domain service interface + implementation registered in `Application/DependencyInjection.cs`.
- Domain services live under `Application/{Feature}/Common/`.
