# Copilot Instructions

## Build, run, and template commands

- Use `.NET SDK 10.0.100` from `global.json`.
- Build the full solution with `dotnet build .\InfraFlowSculptor.slnx`.
- Run the full local stack with Aspire via `dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj`.
- Run the infrastructure configuration API only with `dotnet run --project .\src\Api\InfraFlowSculptor.Api\InfraFlowSculptor.Api.csproj`.
- Run the Bicep generator API only with `dotnet run --project .\src\BicepGenerators\BicepGenerator.Api\BicepGenerator.Api.csproj`.
- The repository is also used as a `dotnet new` template source: `dotnet new install .` then `dotnet new templatewebcqrs -o ProjectName`.
- No test projects are currently present in the repository, so there is no supported full-suite or single-test command yet.
- No repository-specific lint or formatting command is defined in the checked-in files.

## High-level architecture

- `src\Aspire` contains the distributed application host and shared service defaults. `InfraFlowSculptor.AppHost` wires up PostgreSQL, DbGate, the main API, and the Bicep generator API for local orchestration.
- `src\Api` is the main infrastructure configuration stack. It follows a layered CQRS structure:
  - `InfraFlowSculptor.Api`: Minimal API endpoint definitions, OpenAPI setup, auth policy setup, and Mapster registration.
  - `InfraFlowSculptor.Application`: MediatR commands/queries, handlers, validators, pipeline behaviors, and application-layer interfaces.
  - `InfraFlowSculptor.Domain`: aggregates, entities, value objects, and domain behavior.
  - `InfraFlowSculptor.Infrastructure`: EF Core persistence, repository implementations, auth services, Azure integrations, and the Refit client used to call the Bicep generator API.
  - `InfraFlowSculptor.Contracts`: request/response DTOs grouped by feature.
- `src\BicepGenerators` mirrors the same layered split for the API that generates Bicep output and stores artifacts.
- `src\Shared` holds reusable cross-cutting pieces used by both APIs: base DDD model types, shared application abstractions, shared API middleware/options, and persistence converters/repository helpers.
- The main request flow is: Minimal API endpoint in `Api\Controllers` -> Mapster/request mapping -> MediatR command/query in `Application` -> handler/repository/service calls -> domain model changes or reads -> EF Core persistence -> Mapster/typed response DTO back to HTTP.

## Repository-specific conventions

- Treat the domain as strict DDD. When adding domain code, decide whether it belongs as an aggregate root, entity, value object, repository, or domain service before creating files. Shared base classes live in `src\Shared\Shared.Domain\Models`.
- In the main API, domain types are organized by aggregate folders such as `src\Api\InfraFlowSculptor.Domain\InfrastructureConfigAggregate`, with nested `Entities` and `ValueObjects` folders.
- Keep CQRS artifacts together by feature in the application layer. Commands, queries, handlers, validators, and result models live under feature folders like `src\Api\InfraFlowSculptor.Application\InfrastructureConfig\...` and `src\Api\InfraFlowSculptor.Application\ResourceGroups\...`.
- API endpoints are defined as static endpoint-mapping extensions in `src\Api\InfraFlowSculptor.Api\Controllers`, not MVC controller classes. Follow the existing `MapGroup(...)`, `MapGet(...)`, and `MapPost(...)` style.
- Contracts belong in `src\Api\InfraFlowSculptor.Contracts`, typically grouped by feature with `Requests` and `Responses` subfolders. The existing GitHub agent guidance assumes request/response models stay in the contracts project.
- Use Mapster for request/command/response mapping. Mapping configuration lives in `src\Api\InfraFlowSculptor.Api\Common\Mapping` and is registered by scanning the API assembly in `AddPresentation()`.
- Register application services, repositories, auth, and external clients through the layer-specific `DependencyInjection.cs` files instead of wiring dependencies ad hoc in `Program.cs`.
- EF Core configuration belongs in `src\Api\InfraFlowSculptor.Infrastructure\Persistence\Configurations`. Existing mappings rely heavily on owned types and shared converters from `src\Shared\Shared.Infrastructure\Persistence\Configurations\Converters`.
- Repositories are defined by interfaces in the application layer and implemented in infrastructure persistence/repository folders. Reuse that split instead of referencing EF Core directly from handlers.
- Error handling uses `ErrorOr<T>` plus the shared API error extensions in `Shared.Api.Errors`; handlers return `ErrorOr` results rather than throwing for expected validation/domain failures.
- Validation is implemented with FluentValidation and enforced through the MediatR `ValidationBehavior`.
- Authentication uses Microsoft Entra ID / JWT bearer auth. The API projects set a fallback authenticated policy, expose an `IsAdmin` policy, and use `ICurrentUser`/`CurrentUser` for user context access.
- The GitHub prompt and agent files describe the product goal as storing infrastructure configuration in one API and generating Azure Bicep and Azure DevOps pipeline output in the second API. Keep that split in mind when deciding which project should own new behavior.
