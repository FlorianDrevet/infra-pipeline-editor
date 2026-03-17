# Project Memory — InfraFlowSculptor

> This file is the shared memory for all GitHub Copilot agents working on this repository.
> **Always read this file before starting any task.** Update it whenever you learn something new about the project (structure, conventions, patterns, bugs, decisions).
> Keep entries concise, factual, and actionable. Add the date in `[YYYY-MM-DD]` when updating a section.

---

## 1. Solution Overview

**Product goal:** Two cooperating APIs for managing Azure infrastructure — one stores the configuration, the other generates Azure Bicep files and Azure DevOps pipelines from it.

**Technology stack:**
- .NET 10 (global.json SDK 10.0.100)
- ASP.NET Core Minimal APIs
- MediatR (CQRS)
- FluentValidation
- Mapster (object mapping)
- EF Core + PostgreSQL
- Azure AD / Entra ID (JWT Bearer auth)
- ErrorOr (result pattern)
- .NET Aspire (local orchestration)
- Central Package Management (`Directory.Packages.props`)

**Solution file:** `InfraFlowSculptor.slnx`

---

## 2. Project Structure

```
src/
├── Api/                                    Main infrastructure-config API
│   ├── InfraFlowSculptor.Api               Minimal API endpoints, Mapster, DI wiring
│   ├── InfraFlowSculptor.Application       CQRS commands/queries/handlers/validators
│   ├── InfraFlowSculptor.Domain            Aggregates, entities, value objects, errors
│   ├── InfraFlowSculptor.Infrastructure    EF Core, repositories, Azure services
│   └── InfraFlowSculptor.Contracts         Request/response DTOs, validation attributes
├── BicepGenerators/                        Bicep generation API
│   ├── BicepGenerator.Api                  Minimal API endpoints
│   ├── BicepGenerator.Application          CQRS for generation
│   ├── BicepGenerator.Domain               Generation engines (strategy pattern)
│   ├── BicepGenerator.Infrastructure       Read repositories, blob services
│   └── BicepGenerator.Contracts            DTOs for generation
├── Shared/                                 Cross-cutting code
│   ├── Shared.Api                          Error handling, rate limiting, OpenAPI, auth
│   ├── Shared.Application                  IRepository<T> interface
│   ├── Shared.Domain                       DDD base classes (AggregateRoot, Entity, ValueObject, Id…)
│   └── Shared.Infrastructure               BaseRepository<T,TContext>, EF Core converters
└── Aspire/
    ├── InfraFlowSculptor.AppHost           Service orchestration (PostgreSQL, DbGate, both APIs)
    └── InfraFlowSculptor.ServiceDefaults   Shared Aspire defaults
```

---

## 3. Domain Model

### 3.1 Aggregates

| Aggregate | Root | Key Entities | Notes |
|-----------|------|-------------|-------|
| `InfrastructureConfig` | `InfrastructureConfig` | `Member`, `EnvironmentDefinition`, `ParameterDefinition`, `ResourceParameterUsage`, `EnvironmentParameterValue` | Owns resource groups indirectly |
| `ResourceGroup` | `ResourceGroup` | `AzureResource` (base), `InputOutputLink` | Hosts Azure resources |
| `KeyVault` | `KeyVault` extends `AzureResource` | — | TPT in EF Core |
| `RedisCache` | `RedisCache` extends `AzureResource` | — | TPT in EF Core |
| `User` | `User` | — | Azure AD user info |

### 3.2 Value Objects

All value objects inherit from `Shared.Domain` base classes:
- `Id<T>` — base for ID types; supports `new TId(Guid)` and `TId.Create(Guid)`
- `SingleValueObject<T>` — wraps a single primitive
- `EnumValueObject<TEnum>` — wraps an enum
- `ValueObject` — base with structural equality

Key value objects per aggregate:
- **InfrastructureConfig:** `InfrastructureConfigId`, `MemberId`, `Role` (Owner/Contributor/Reader), `EnvironmentDefinitionId`, `ParameterDefinitionId`, `ParameterType`, `Prefix`, `Suffix`, `Order`, `IsSecret`, `TenantId`, `SubscriptionId`, `RequiresApproval`
- **ResourceGroup:** `ResourceGroupId`, `Name`, `Location`
- **AzureResource:** `AzureResourceId`, `Name`
- **KeyVault:** `Sku` (enum: Premium/Standard)
- **RedisCache:** `RedisCacheSku`, `TlsVersion`, `MaxMemoryPolicy`, `RedisCacheSettings`
- **User:** `UserId`, `EntraId`, `Name`

### 3.3 Domain Invariants

- `InfrastructureConfig.Members` is `IReadOnlyCollection<Member>` — mutated via `AddMember()`, `ChangeRole()`, `RemoveMember()` methods on the aggregate root.
- Access checks (ownership, membership) must be performed **before** calling aggregate methods (not enforced in the domain itself).
- `AzureResource` inheritance uses EF Core **TPT**: derived entities call `HasBaseType<AzureResource>().ToTable("...")` in their configurations.

### 3.4 Error Definitions

Errors live in `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.*.cs` as partial static classes:

```csharp
// Errors.Member.cs
public static partial class Errors
{
    public static class Member
    {
        public static Error ForbiddenError() => Error.Forbidden(...);
        public static Error NotFoundError(UserId userId) => Error.NotFound(...);
        public static Error AlreadyMemberError(UserId userId) => Error.Conflict(...);
        public static Error CannotRemoveOwnerError() => Error.Conflict(...);
    }
}
```

Existing error files:
- `Errors.InfrastructureConfig.cs`
- `Errors.ResourceGroup.cs` (has nested `AddResource` / `RemoveResource` sub-classes)
- `Errors.KeyVault.cs`
- `Errors.RedisCache.cs`
- `Errors.Member.cs`

**Important:** When adding a new aggregate, add a new `Errors.AggregateName.cs` file following the same partial class pattern.

---

## 4. CQRS Pattern

### 4.1 Folder Structure

```
src/Api/InfraFlowSculptor.Application/
└── FeatureName/
    ├── Commands/
    │   └── DoSomethingCommand/
    │       ├── DoSomethingCommand.cs          record : IRequest<ErrorOr<T>>
    │       ├── DoSomethingCommandHandler.cs   IRequestHandler<Cmd, ErrorOr<T>>
    │       └── DoSomethingCommandValidator.cs AbstractValidator<DoSomethingCommand>
    ├── Queries/
    │   └── GetSomethingQuery/
    │       ├── GetSomethingQuery.cs           record : IRequest<ErrorOr<T>>
    │       └── GetSomethingQueryHandler.cs    IRequestHandler<Query, ErrorOr<T>>
    └── Common/
        └── SomethingResult.cs                 Application-layer result DTO
```

### 4.2 Registration

`DependencyInjection.cs` in the Application project registers everything by assembly scan:
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

### 4.3 Shared Authorization Service

**Member management (Owner-only):**
```csharp
// MemberCommandHelper.AuthorizeOwnerAndFindTargetAsync(...)
// Location: src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Common/MemberCommandHelper.cs
```

**Resource CRUD authorization — `IInfraConfigAccessService` (injectable service):**

The access control logic is exposed via an injectable service, not a static helper.
Register once in the Application DI; inject into handlers via the constructor.

```csharp
// Interface: src/Api/InfraFlowSculptor.Application/Common/Interfaces/IInfraConfigAccessService.cs
// Implementation: src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Common/InfraConfigAccessService.cs
// DI registration: services.AddScoped<IInfraConfigAccessService, InfraConfigAccessService>();  (Application/DependencyInjection.cs)

// In a handler constructor:
public class MyCommandHandler(IInfraConfigAccessService accessService, ...) { }

// Read (any member):
var authResult = await accessService.VerifyReadAccessAsync(infraConfigId, cancellationToken);

// Write (Owner or Contributor only):
var authResult = await accessService.VerifyWriteAccessAsync(infraConfigId, cancellationToken);
```

Access check pattern for resource handlers:
- **ResourceGroup**: has `InfraConfigId` directly → call `accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, ct)`
- **KeyVault / RedisCache**: have `ResourceGroupId` → load ResourceGroup → use `resourceGroup.InfraConfigId`
- Read operations: `VerifyReadAccessAsync` — returns `NotFound` (no info leak) if not a member
- Write operations: `VerifyWriteAccessAsync` — returns `NotFound` for non-members, `Forbidden` for Readers

---

## 5. API Layer

### 5.1 Endpoint Registration Pattern

Controllers are **static extension methods** in `src/Api/InfraFlowSculptor.Api/Controllers/`:
```csharp
public static IApplicationBuilder UseXyzController(this IApplicationBuilder builder)
{
    return builder.UseEndpoints(endpoints =>
    {
        var group = endpoints.MapGroup("/route").WithTags("Tag");
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) => ...);
        group.MapPost("", async ([FromBody] CreateRequest request, ISender sender) => ...);
    });
}
```

Registered in `Program.cs` via `app.UseXyzController()`.

### 5.2 Existing Endpoints

| Group | Method | Route | Command/Query |
|-------|--------|-------|--------------|
| `/infra-config` | GET | `` | `ListMyInfrastructureConfigsQuery` |
| `/infra-config` | GET | `/{id:guid}` | `GetInfrastructureConfigQuery` |
| `/infra-config` | POST | `` | `CreateInfrastructureConfigCommand` |
| `/infra-config` | POST | `/generate-bicep` | `GenerateBicepCommand` |
| `/infra-config` | POST | `/{id:guid}/members` | `AddMemberCommand` |
| `/infra-config` | PUT | `/{id:guid}/members/{userId:guid}` | `UpdateMemberRoleCommand` |
| `/infra-config` | DELETE | `/{id:guid}/members/{userId:guid}` | `RemoveMemberCommand` |
| `/keyvault` | GET/POST/PUT/DELETE | `/{id:guid}` | Key Vault CRUD |
| `/resource-group` | GET/POST | `/{id:guid}` | Resource Group CRUD |
| `/redis-cache` | GET/POST/PUT/DELETE | `/{id:guid}` | Redis Cache CRUD |
| `/generate-bicep` | POST | `` | `GenerateBicepCommand` |

### 5.3 Error Conversion

Handlers return `ErrorOr<T>`. In controllers, convert to HTTP:
```csharp
result.Match(
    value => Results.Ok(mapper.Map<Response>(value)),
    errors => errors.ToErrorResult()  // from Shared.Api.Errors
);
```

---

## 6. Contracts (DTOs)

### 6.1 Request Pattern
```csharp
// src/Api/InfraFlowSculptor.Contracts/FeatureName/Requests/DoSomethingRequest.cs
public class DoSomethingRequest
{
    [Required, GuidValidation]
    public required Guid SomeId { get; init; }

    [Required, EnumValidation(typeof(MyEnum))]
    public required string EnumValue { get; init; }
}
```

### 6.2 Response Pattern
```csharp
// src/Api/InfraFlowSculptor.Contracts/FeatureName/Responses/SomethingResponse.cs
public record SomethingResponse(string Id, string Name, /* ... */);
```

### 6.3 Validation Attributes
- `[GuidValidation]` — validates GUID format
- `[EnumValidation(typeof(MyEnum))]` — validates enum string value
- `[RedisVersionValidation]` — validates Redis major version (0–7)

### 6.4 JSON body GUID pitfall
- `[2026-03-17]` In Minimal APIs with `services.AddValidation()`, a JSON body property typed as `Guid` still fails during `System.Text.Json` deserialization **before** validation runs when the client sends an invalid GUID string.
- To return a clean HTTP 400 validation response instead of `BadHttpRequestException`, prefer `string` DTO properties plus `[Required, GuidValidation]` for body payloads that accept GUIDs from JSON, then parse to `Guid` after validation in the endpoint/mapper.
- `GuidValidation` now supports both `Guid` and `string` values and rejects `Guid.Empty` in both cases.

---

## 7. Mapster Mappings

Mapping configs implement `IRegister` and live in `src/Api/InfraFlowSculptor.Api/Common/Mapping/`:

```csharp
public class FeatureMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SourceType, DestType>()
            .Map(dest => dest.Prop, src => src.SomeProp.Value); // unwrap value objects
    }
}
```

Auto-discovered by `TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly())`.

**Key mapping conventions:**
- Value objects → primitives: `.MapWith(src => src.Value)`
- ID types → string (for HTTP responses): `.Map(dest => dest.Id, src => src.Id.Value.ToString())`
- Enum value objects → string: `.Map(dest => dest.Role, src => src.Role.Value.ToString())`

---

## 8. Persistence (EF Core)

### 8.1 DbContext
- `ProjectDbContext` at `src/Api/InfraFlowSculptor.Infrastructure/Persistence/ProjectDbContext.cs`
- Applies all `IEntityTypeConfiguration` via `ApplyConfigurationsFromAssembly()`
- PostgreSQL target

### 8.2 Entity Configuration Pattern
```csharp
public sealed class SomethingConfiguration : IEntityTypeConfiguration<Something>
{
    public void Configure(EntityTypeBuilder<Something> builder)
    {
        builder.ToTable("Somethings");
        builder.HasKey(x => x.Id);
        builder.ConfigureAggregateRootId<Something, SomethingId>(); // extension method
        builder.Property(x => x.Name).HasConversion(new SingleValueConverter<Name, string>());
        builder.Property(x => x.Status).HasConversion(new EnumValueConverter<Status, StatusEnum>());
    }
}
```

### 8.3 EF Core Converters
- `IdValueConverter<TId, TKey>` — converts ID value objects ↔ underlying type
- `SingleValueConverter<TValueObject, TPrimitive>` — wraps/unwraps single-value objects
- `EnumValueConverter<TEnumValueObject, TEnum>` — stores enum value objects as strings
- `ParameterUsageConverter` — custom converter for `ParameterUsage`

### 8.4 Repository Pattern

```csharp
// Interface (Application layer)
public interface IXyzRepository : IRepository<Xyz>
{
    Task<Xyz?> GetByIdWithRelatedAsync(XyzId id, CancellationToken ct = default);
}

// Implementation (Infrastructure layer)
public class XyzRepository(ProjectDbContext context)
    : BaseRepository<Xyz, ProjectDbContext>(context), IXyzRepository
{
    public async Task<Xyz?> GetByIdWithRelatedAsync(XyzId id, CancellationToken ct = default)
        => await Context.Xyzs.Include(x => x.Related)
            .FirstOrDefaultAsync(x => x.Id == id, ct);  // ✅ correct: compare value objects, NOT .Value
}
```

**⚠️ IMPORTANT EF Core pitfall:** Never use `x.Id.Value == id.Value` in LINQ-to-EF queries. EF Core cannot translate navigation into `.Value` on a value object. Always compare the whole value object: `x.Id == id`. EF Core uses the registered `IdValueConverter<T>` to translate this to a SQL `=` comparison on the underlying `Guid` column. Using `.Value` causes `InvalidOperationException: The LINQ expression could not be translated`.
```

`IRepository<T>` methods: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync` (returns T), `DeleteAsync`.

**Important namespace note:** In `IInfrastructureConfigRepository`, use fully-qualified type name `Domain.InfrastructureConfigAggregate.InfrastructureConfig` to avoid CS0118 ambiguity.

### 8.5 Migrations
14 migration files in `src/Api/InfraFlowSculptor.Infrastructure/Migrations/`. Always add a new migration when changing the domain model (`dotnet ef migrations add <Name>`).

---

## 9. Authentication & Authorization

- **Provider:** Azure AD (Entra ID) JWT Bearer
- **Config section:** `"AzureAd"` in appsettings
- **Fallback policy:** Authenticated users only (`RequireAuthenticatedUser`)
- **Admin policy:** `"IsAdmin"` policy
- **Current user:** `ICurrentUser` → `CurrentUser` service extracts user from `IHttpContextAccessor`

---

## 10. Build & Run Commands

```bash
# Restore (always required before build)
dotnet restore src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj

# Build main API
dotnet build src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj --no-restore

# Run full stack (Aspire)
dotnet run --project src/Aspire/InfraFlowSculptor.AppHost/InfraFlowSculptor.AppHost.csproj

# Run main API only
dotnet run --project src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj

# Run Bicep generator API only
dotnet run --project src/BicepGenerators/BicepGenerator.Api/BicepGenerator.Api.csproj
```

No test projects currently exist.

---

## 11. Sonar Quality Rules (Known Issues)

- **S1192** — Duplicate string literals in migration files (pre-existing, accepted)
- **S125** — Commented-out code in domain files (pre-existing)
- **S3218** — Nested class shadows outer member in `Errors.ResourceGroup.cs` (pre-existing)
- **new_duplicated_lines_density** — Quality gate threshold is **3%**. Any new code duplication > 3% will fail CI. Extract shared logic into helpers (see `MemberCommandHelper`).

---

## 12. Aspire Integration

- AppHost wires: PostgreSQL, DbGate (DB admin UI), main API, Bicep generator API
- Azure Blob Storage emulator: connection string exposed as `ConnectionStrings:AzureBlobStorageConnectionString`
- Main API registered as `infra-api`, Bicep generator as `bicep-api`

---

## 13. BicepGenerator Service

- Separate ASP.NET Core API at `src/BicepGenerators/`
- Called from main API via Refit client (`IBicepGeneratorClient`)
- Reads infrastructure config from **shared PostgreSQL DB** (read-only)
- Generates Bicep files per environment, uploads to Azure Blob Storage
- Uses **strategy pattern**: `IResourceTypeBicepGenerator` per Azure resource type
- New resource types require: a new `IResourceTypeBicepGenerator` implementation + registration in `BicepGenerator.Application/DependencyInjection.cs`

---

## 14. Pull Request Conventions

### Titre obligatoire

Format : `type(scope): description courte du but principal`

| type       | usage                                   |
|------------|-----------------------------------------|
| `feat`     | nouvelle fonctionnalité                 |
| `fix`      | correction de bug                       |
| `refactor` | refactoring sans changement fonctionnel |
| `perf`     | amélioration des performances           |
| `docs`     | documentation uniquement                |
| `test`     | tests                                   |
| `chore`    | maintenance, dépendances                |
| `ci`       | pipelines CI/CD                         |
| `style`    | formatage, lint                         |
| `revert`   | annulation                              |

- Le scope est en kebab-case (ex : `key-vault`, `storage-account`, `bicep`)
- Le titre décrit le **but global**, jamais la dernière tâche effectuée
- Référence complète : `.github/agents/pr-conventions.agent.md`

### Description de PR

- Utiliser le template `.github/PULL_REQUEST_TEMPLATE.md`
- Lister chaque fichier créé/modifié dans sa section de couche
- Indiquer le nom de la migration EF Core si applicable
- Valider toute la checklist avant soumission

---

## 15. Changelog

| Date | Author | Change |
|------|--------|--------|
| 2026-03-15 | copilot | Initial MEMORY.md created from full project exploration |
| 2026-03-15 | copilot | Fixed `InvalidOperationException` in `InfrastructureConfigRepository.GetByIdWithMembersAsync`: `c.Id.Value == id.Value` → `c.Id == id` (EF Core cannot translate `.Value` property access on value objects in LINQ queries) |
| 2026-03-15 | copilot | Added authorization checks to all resource CRUD endpoints (ResourceGroup, KeyVault, RedisCache): created `InfraConfigAccessHelper` (`VerifyReadAccessAsync`/`VerifyWriteAccessAsync`), added `Errors.InfrastructureConfig.ForbiddenError()`, overrode `ResourceGroupRepository.GetByIdAsync` with safe LINQ pattern |
| 2026-03-15 | copilot | Added RBAC role assignment feature: `RoleAssignment` entity on `AzureResource`, `ManagedIdentityType` value object, `IAzureResourceRepository`, `AzureResourceBaseRepository`, `AddRoleAssignment`/`RemoveRoleAssignment`/`ListRoleAssignments` CQRS handlers, EF Core migration `AddRoleAssignmentsTable`, REST endpoints under `/azure-resources/{id}/role-assignments` |
| 2026-03-17 | copilot | Added PR conventions: `.github/PULL_REQUEST_TEMPLATE.md`, `.github/agents/pr-conventions.agent.md`, updated `copilot-instructions.md`, `memory.agent.md`, `cqrs.agent.md` with mandatory PR title format `type(scope): description` and description template |
| 2026-03-17 | copilot | Added `GET /infra-config/{id}/resource-groups` endpoint: new `ListResourceGroupsByConfigQuery` + handler, `IResourceGroupRepository.GetByInfraConfigIdAsync` |
| 2026-03-17 | copilot | Refactored `InfraConfigAccessHelper` (static) → `IInfraConfigAccessService` injectable service. Interface in `Application/Common/Interfaces/`, implementation in `InfrastructureConfig/Common/InfraConfigAccessService.cs`. Updated all 11 handlers (ResourceGroup, KeyVault, RedisCache) to inject the service. |
| 2026-03-17 | copilot | Fixed environment body GUID validation: `AddEnvironmentRequest`/`UpdateEnvironmentRequest` now use `string` + `[GuidValidation]`, `GuidValidation` accepts strings and `Guid`, and `InfrastructureConfigController` parses IDs after validation to avoid `BadHttpRequestException` on invalid JSON GUIDs |
| 2026-03-17 | copilot | Updated InfrastructureConfig command handlers (`AddEnvironment`, `UpdateEnvironment`, `RemoveEnvironment`, `SetDefaultNamingTemplate`, `SetResourceNamingTemplate`, `RemoveResourceNamingTemplate`) to use `IInfraConfigAccessService` instead of removed `InfraConfigAccessHelper`. |
| 2026-03-17 | copilot | Updated StorageAccount commands/queries to use `IInfraConfigAccessService`; added `StorageAccountAccessHelper` and updated `StorageAccountAccessContext` to carry `IInfraConfigAccessService` instead of legacy `IInfrastructureConfigRepository` + `ICurrentUser`. |
