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

## Unit of Work [2026-03-30]

- `IUnitOfWork` / `UnitOfWork` wraps `ProjectDbContext.SaveChangesAsync`
- `UnitOfWorkBehavior` only applies to `ICommand<T>` (via `ICommandBase` constraint)
- Pipeline order: `ValidationBehavior` → `UnitOfWorkBehavior` → Handler
- **Critical:** Repositories MUST NOT call `SaveChangesAsync()`.

## Registration

- `DependencyInjection.cs` (Application) registers MediatR, ValidationBehavior, UnitOfWorkBehavior, validators by assembly scan.
- `DependencyInjection.cs` (Infrastructure) registers `IUnitOfWork`.

## Shared Authorization Service

- `IInfraConfigAccessService` (injectable): `VerifyReadAccessAsync`, `VerifyWriteAccessAsync`
- `MemberCommandHelper` for owner-only member management
- Access check: ResourceGroup has `InfraConfigId` directly; KeyVault/RedisCache have `ResourceGroupId` → load ResourceGroup → use `InfraConfigId`
