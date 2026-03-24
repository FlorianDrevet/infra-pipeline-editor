# Project Memory — InfraFlowSculptor

> This file is the shared memory for all GitHub Copilot agents working on this repository.
> **Always read this file before starting any task.** Update it whenever you learn something new about the project (structure, conventions, patterns, bugs, decisions).
> Keep entries concise, factual, and actionable. Add the date in `[YYYY-MM-DD]` when updating a section.

---

## 1. Solution Overview

**Product goal:** A single unified API for managing Azure infrastructure configuration and generating Azure Bicep files and Azure DevOps pipelines from it. All code lives in the standard layered projects (Domain, Application, Infrastructure, Contracts, Api) plus a dedicated `InfraFlowSculptor.BicepGeneration` project for the pure Bicep generation engine.

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
├── Api/                                    Single unified API
│   ├── InfraFlowSculptor.Api               Minimal API endpoints, Mapster, DI wiring, error handling, rate limiting, OpenAPI config
│   ├── InfraFlowSculptor.Application       CQRS commands/queries/handlers/validators, IRepository<T>, service interfaces
│   ├── InfraFlowSculptor.BicepGeneration   Pure Bicep generation engine (self-contained, no domain dependency)
│   │   ├── Models/                        DTOs: GenerationRequest, GenerationResult, EnvironmentDefinition, ResourceDefinition, etc.
│   │   ├── Generators/                    IResourceTypeBicepGenerator + per-resource-type implementations + factory
│   │   └── Helpers/                       BicepIdentifierHelper, NamingTemplateTranslator
│   ├── InfraFlowSculptor.Domain            Aggregates, entities, value objects, DDD base classes, errors
│   ├── InfraFlowSculptor.Infrastructure    EF Core, repositories, base repository, converters, Azure services, blob storage
│   └── InfraFlowSculptor.Contracts         Request/response DTOs, validation attributes
├── Front/                                  Angular frontend (UI + API consumption)
│   ├── src/app/core                        Layout components (navigation/footer)
│   ├── src/app/shared                      Cross-cutting frontend services/guards/facades
│   ├── src/environments                    API base URL and runtime environment config
│   └── src/scss                            Global theming variables and style modules
└── Aspire/
    ├── InfraFlowSculptor.AppHost           Service orchestration (PostgreSQL, DbGate, single API)
    └── InfraFlowSculptor.ServiceDefaults   Shared Aspire defaults
```

---

## 3. Domain Model

### 3.1 Aggregates

| Aggregate | Root | Key Entities | Notes |
|-----------|------|-------------|-------|
| `Project` | `Project` | `ProjectMember`, `ProjectEnvironmentDefinition`, `ProjectResourceNamingTemplate` | Groups InfrastructureConfigs; owns membership/RBAC, default environments, naming conventions |
| `InfrastructureConfig` | `InfrastructureConfig` | `EnvironmentDefinition`, `ParameterDefinition`, `ResourceParameterUsage`, `EnvironmentParameterValue` | Has `ProjectId` FK to Project |
| `ResourceGroup` | `ResourceGroup` | `AzureResource` (base), `InputOutputLink`, `ResourceEnvironmentConfig` | Hosts Azure resources |
| `KeyVault` | `KeyVault` extends `AzureResource` | `KeyVaultEnvironmentSettings` | TPT in EF Core; typed per-env settings |
| `RedisCache` | `RedisCache` extends `AzureResource` | `RedisCacheEnvironmentSettings` | TPT in EF Core; typed per-env settings |
| `StorageAccount` | `StorageAccount` extends `AzureResource` | `StorageAccountEnvironmentSettings` | TPT in EF Core; typed per-env settings |
| `AppServicePlan` | `AppServicePlan` extends `AzureResource` | `AppServicePlanEnvironmentSettings` | TPT in EF Core; typed per-env settings (Sku, Capacity). OsType at resource level. |
| `WebApp` | `WebApp` extends `AzureResource` | `WebAppEnvironmentSettings` | TPT in EF Core; typed per-env settings (AlwaysOn, HttpsOnly, RuntimeStack, RuntimeVersion). FK to AppServicePlan via `AppServicePlanId`. |
| `FunctionApp` | `FunctionApp` extends `AzureResource` | `FunctionAppEnvironmentSettings` | TPT in EF Core; typed per-env settings (HttpsOnly, RuntimeStack, RuntimeVersion, MaxInstanceCount, FunctionsWorkerRuntime). FK to AppServicePlan via `AppServicePlanId`. |
| `UserAssignedIdentity` | `UserAssignedIdentity` extends `AzureResource` | — | TPT in EF Core; simplest resource type, no per-environment settings. `Microsoft.ManagedIdentity/userAssignedIdentities`. |
| `AppConfiguration` | `AppConfiguration` extends `AzureResource` | `AppConfigurationEnvironmentSettings` | TPT in EF Core; typed per-env settings (Sku, SoftDeleteRetentionInDays, PurgeProtectionEnabled, DisableLocalAuth, PublicNetworkAccess). `Microsoft.AppConfiguration/configurationStores`. |
| `ContainerAppEnvironment` | `ContainerAppEnvironment` extends `AzureResource` | `ContainerAppEnvironmentEnvironmentSettings` | TPT in EF Core; typed per-env settings (Sku, WorkloadProfileType, InternalLoadBalancerEnabled, ZoneRedundancyEnabled, LogAnalyticsWorkspaceId). `Microsoft.App/managedEnvironments@2024-03-01`. Abbreviation `cae`. |
| `ContainerApp` | `ContainerApp` extends `AzureResource` | `ContainerAppEnvironmentSettings` | TPT in EF Core; FK to ContainerAppEnvironment via `ContainerAppEnvironmentId`. Typed per-env settings (ContainerImage, CpuCores, MemoryGi, MinReplicas, MaxReplicas, IngressEnabled, IngressTargetPort, IngressExternal, TransportMethod). `Microsoft.App/containerApps@2024-03-01`. Abbreviation `ca`. |
| `LogAnalyticsWorkspace` | `LogAnalyticsWorkspace` extends `AzureResource` | `LogAnalyticsWorkspaceEnvironmentSettings` | TPT in EF Core; typed per-env settings (Sku, RetentionInDays, DailyQuotaGb). `Microsoft.OperationalInsights/workspaces@2023-09-01`. Abbreviation `law`. |
| `ApplicationInsights` | `ApplicationInsights` extends `AzureResource` | `ApplicationInsightsEnvironmentSettings` | TPT in EF Core; FK to LogAnalyticsWorkspace via `LogAnalyticsWorkspaceId`. Typed per-env settings (SamplingPercentage, RetentionInDays, DisableIpMasking, DisableLocalAuth, IngestionMode). `Microsoft.Insights/components@2020-02-02`. Abbreviation `appi`. |
| `CosmosDb` | `CosmosDb` extends `AzureResource` | `CosmosDbEnvironmentSettings` | TPT in EF Core; typed per-env settings (DatabaseApiType, ConsistencyLevel, MaxStalenessPrefix, MaxIntervalInSeconds, EnableAutomaticFailover, EnableMultipleWriteLocations, BackupPolicyType, EnableFreeTier). `Microsoft.DocumentDB/databaseAccounts@2024-05-15`. Abbreviation `cosmos`. |
| `SqlServer` | `SqlServer` extends `AzureResource` | `SqlServerEnvironmentSettings` | TPT in EF Core; resource-level Version (V12) + AdministratorLogin. Per-env settings (MinimalTlsVersion). `Microsoft.Sql/servers@2023-08-01-preview`. Abbreviation `sql`. |
| `SqlDatabase` | `SqlDatabase` extends `AzureResource` | `SqlDatabaseEnvironmentSettings` | TPT in EF Core; FK to SqlServer via `SqlServerId`. Per-env settings (Sku: Basic/Standard/Premium/GeneralPurpose/BusinessCritical/Hyperscale, MaxSizeGb, ZoneRedundant). Resource-level Collation. `Microsoft.Sql/servers/databases@2023-08-01-preview`. Abbreviation `sqldb`. |
| `User` | `User` | — | Azure AD user info |

### 3.2 Value Objects

All value objects inherit from `InfraFlowSculptor.Domain.Common.Models` base classes:
- `Id<T>` — base for ID types; supports `new TId(Guid)` and `TId.Create(Guid)`
- `SingleValueObject<T>` — wraps a single primitive
- `EnumValueObject<TEnum>` — wraps an enum
- `ValueObject` — base with structural equality

Key value objects per aggregate:
- **Project:** `ProjectId`, `ProjectMemberId`, `ProjectEnvironmentDefinitionId`, `ProjectResourceNamingTemplateId`
- **InfrastructureConfig:** `InfrastructureConfigId`, `Role` (Owner/Contributor/Reader), `EnvironmentDefinitionId`, `ParameterDefinitionId`, `ParameterType`, `Prefix`, `Suffix`, `Order`, `IsSecret`, `TenantId`, `SubscriptionId`, `RequiresApproval`
- **ResourceGroup:** `ResourceGroupId`, `Name`, `Location`
- **AzureResource:** `AzureResourceId`, `Name`
- **ResourceEnvironmentConfig:** `ResourceEnvironmentConfigId` — **DEPRECATED** (replaced by typed per-resource settings)
- **KeyVaultEnvironmentSettings:** `KeyVaultEnvironmentSettingsId`, `EnvironmentName`, nullable `Sku?`
- **RedisCacheEnvironmentSettings:** `RedisCacheEnvironmentSettingsId`, `EnvironmentName`, nullable `Sku?`, `Capacity?`, `RedisVersion?`, `EnableNonSslPort?`, `MinimumTlsVersion?`, `MaxMemoryPolicy?`
- **StorageAccountEnvironmentSettings:** `StorageAccountEnvironmentSettingsId`, `EnvironmentName`, nullable `Sku?`, `Kind?`, `AccessTier?`, `AllowBlobPublicAccess?`, `EnableHttpsTrafficOnly?`, `MinimumTlsVersion?`
- **KeyVault:** (no general-level config VOs — all config is per-environment)
- **RedisCache:** (no general-level config VOs — all config is per-environment; `RedisCacheSettings` record is unused/dead code)
- **StorageAccount:** (no general-level config VOs — all config is per-environment; `StorageAccountSettings` record is unused/dead code)
- **AppServicePlanEnvironmentSettings:** `AppServicePlanEnvironmentSettingsId`, `EnvironmentName`, nullable `Sku?` (AppServicePlanSku), `Capacity?`
- **AppServicePlan:** `AppServicePlanOsType` (Windows/Linux) — resource-level, not per-environment
- **WebAppEnvironmentSettings:** `WebAppEnvironmentSettingsId`, `EnvironmentName`, nullable `AlwaysOn?`, `HttpsOnly?`, `RuntimeStack?` (WebAppRuntimeStack), `RuntimeVersion?`
- **WebApp:** `WebAppRuntimeStack` (DotNet/Node/Python/Java/Php), FK `AppServicePlanId` (AzureResourceId)
- **FunctionAppEnvironmentSettings:** `FunctionAppEnvironmentSettingsId`, `EnvironmentName`, nullable `HttpsOnly?`, `RuntimeStack?` (FunctionAppRuntimeStack), `RuntimeVersion?`, `MaxInstanceCount?`, `FunctionsWorkerRuntime?`
- **FunctionApp:** `FunctionAppRuntimeStack` (DotNet/Node/Python/Java/PowerShell), FK `AppServicePlanId` (AzureResourceId)
- **CosmosDbEnvironmentSettings:** `CosmosDbEnvironmentSettingsId`, `EnvironmentName`, nullable `DatabaseApiType?`, `ConsistencyLevel?`, `MaxStalenessPrefix?`, `MaxIntervalInSeconds?`, `EnableAutomaticFailover?`, `EnableMultipleWriteLocations?`, `BackupPolicyType?`, `EnableFreeTier?`
- **SqlServer:** `SqlServerVersion` (V12) — resource-level. FK: none (parent resource).
- **SqlServerEnvironmentSettings:** `SqlServerEnvironmentSettingsId`, `EnvironmentName`, nullable `MinimalTlsVersion?`
- **SqlDatabase:** FK `SqlServerId` (AzureResourceId), `Collation` (string) — resource-level.
- **SqlDatabaseEnvironmentSettings:** `SqlDatabaseEnvironmentSettingsId`, `EnvironmentName`, nullable `SqlDatabaseSku?` (Basic/Standard/Premium/GeneralPurpose/BusinessCritical/Hyperscale), `MaxSizeGb?`, `ZoneRedundant?`
- **User:** `UserId`, `EntraId`, `Name`

### 3.3 Domain Invariants

- `Project.Members` is `IReadOnlyCollection<ProjectMember>` — mutated via `AddMember()`, `ChangeRole()`, `RemoveMember()` methods on the Project aggregate root.
- `InfrastructureConfig` has a `ProjectId` FK. Access checks (read/write/owner) are resolved via **project membership** — `IInfraConfigAccessService` loads the config, then delegates to `IProjectAccessService.VerifyReadAccessAsync(config.ProjectId)`.
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
- `Errors.AppServicePlan.cs`
- `Errors.WebApp.cs`
- `Errors.FunctionApp.cs`
- `Errors.Project.cs` (NotFound, Forbidden, MemberAlreadyExists, CannotRemoveOwner, MemberNotFound)

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
| `/infra-config` | DELETE | `/{id:guid}` | `DeleteInfrastructureConfigCommand` |
| `/infra-config` | POST | `/generate-bicep` | `GenerateBicepCommand` |
| `/infra-config` | POST | `/{id:guid}/members` | `AddMemberCommand` |
| `/infra-config` | PUT | `/{id:guid}/members/{userId:guid}` | `UpdateMemberRoleCommand` |
| `/infra-config` | DELETE | `/{id:guid}/members/{userId:guid}` | `RemoveMemberCommand` |
| `/projects` | DELETE | `/{id:guid}` | `DeleteProjectCommand` |
| `/projects` | POST | `/validate-recent` | `ValidateRecentItemsQuery` |
| `/keyvault` | GET/POST/PUT/DELETE | `/{id:guid}` | Key Vault CRUD |
| `/resource-group` | GET/POST | `/{id:guid}` | Resource Group CRUD |
| `/redis-cache` | GET/POST/PUT/DELETE | `/{id:guid}` | Redis Cache CRUD |
| `/app-service-plan` | GET/POST/PUT/DELETE | `/{id:guid}` | App Service Plan CRUD |
| `/web-app` | GET/POST/PUT/DELETE | `/{id:guid}` | Web App CRUD |
| `/function-app` | GET/POST/PUT/DELETE | `/{id:guid}` | Function App CRUD |
| `/generate-bicep` | POST | `` | `GenerateBicepCommand` |
| `/generate-bicep` | GET | `/{configId:guid}/download` | `DownloadBicepCommand` (returns zip) |
| `/generate-bicep` | GET | `/{configId:guid}/files/{*filePath}` | `GetBicepFileContentQuery` (returns JSON `{ content }`) |
| `/sql-server` | GET/POST/PUT/DELETE | `/{id:guid}` | SQL Server CRUD |
| `/sql-database` | GET/POST/PUT/DELETE | `/{id:guid}` | SQL Database CRUD |

### 5.3 Error Conversion

Handlers return `ErrorOr<T>`. In controllers, convert to HTTP:
```csharp
result.Match(
    value => Results.Ok(mapper.Map<Response>(value)),
    errors => errors.ToErrorResult()  // from InfraFlowSculptor.Api.Errors
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
        builder.ConfigureAggregateRootId<Something, SomethingId>(); // extension in InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions
        builder.Property(x => x.Name).HasConversion(new SingleValueConverter<Name, string>());
        builder.Property(x => x.Status).HasConversion(new EnumValueConverter<Status, StatusEnum>());
    }
}
```

### 8.3 EF Core Converters
- `IdValueConverter<TId>` — converts ID value objects ↔ underlying Guid (`InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters`)
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
`IRepository<T>` lives in `InfraFlowSculptor.Application.Common.Interfaces`, `BaseRepository<T,TContext>` is in `InfraFlowSculptor.Infrastructure.Persistence.Repositories`.

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

# Frontend commands (run from src/Front)
npm install
npm run start
npm run build
npm run typecheck
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

- AppHost wires: PostgreSQL, DbGate (DB admin UI), main API, Bicep generator API, Angular frontend
- Azure Blob Storage emulator: connection string exposed as `ConnectionStrings:AzureBlobStorageConnectionString`
- Main API registered as `infraflowsculptor-api`, Bicep generator as `bicep-generator-api`, frontend as `angular-frontend`

### 12.1 Frontend Angular dans Aspire ([2026-03-18])

**Méthode Aspire 13.x :** `AddJavaScriptApp` + `.WithNpm()` (remplace l'ancienne `AddNpmApp` qui n'existe plus).

```csharp
builder.AddJavaScriptApp("angular-frontend", "../../Front", "start:aspire")
    .WithNpm()
    .WithReference(infraApi)
    .WaitFor(infraApi)
    .WithHttpEndpoint(targetPort: 4200, env: "NG_PORT")
    .WithExternalHttpEndpoints();
```

- Port Angular fixé à 4200 via `WithHttpEndpoint(targetPort: 4200, env: "NG_PORT")`
- Le script npm `start:aspire` lance `ng serve --configuration=aspire`
- La configuration `aspire` dans `angular.json` remplace `environment.ts` par `environment.aspire.ts`

### 12.2 Proxy Angular dev-server

Fichier `src/Front/proxy.conf.js` : lit les variables d'environnement Aspire pour configurer le proxy :

| Chemin | Variable Aspire | Destination |
|--------|-----------------|-------------|
| `/api-proxy/*` | `services__infraflowsculptor-api__https__0` ou `__http__0` | API backend |
| `/bicep-api-proxy/*` | `services__bicep-generator-api__https__0` ou `__http__0` | Bicep Generator API |
| `/otlp/*` | `OTEL_EXPORTER_OTLP_ENDPOINT` | Aspire Dashboard OTLP |

- Le frontend utilise `api_url: '/api-proxy'` (environment.aspire.ts)
- Le frontend utilise `bicep_api_url: '/bicep-api-proxy'` (environment.aspire.ts)
- Le proxy supprime le préfixe `/api-proxy` avant de transmettre au backend
- Le proxy supprime aussi le préfixe `/bicep-api-proxy` avant de transmettre au Bicep Generator API
- Pas de problème CORS : toutes les requêtes browser passent par localhost:4200

### 12.3 Configurations Angular

| Config | Fichier env | Usage |
|--------|-------------|-------|
| `development` | `environment.development.ts` | `npm run start` (dev standalone) |
| `aspire` | `environment.aspire.ts` | `npm run start:aspire` (via Aspire) |
| `production` | `environment.ts` | `npm run build` |

### 12.4 Variables Aspire injectées pour le frontend

| Variable | Description |
|----------|-------------|
| `services__infraflowsculptor-api__http__0` | URL HTTP de l'API principale |
| `services__infraflowsculptor-api__https__0` | URL HTTPS de l'API principale |
| `services__bicep-generator-api__http__0` | URL HTTP de l'API Bicep Generator |
| `services__bicep-generator-api__https__0` | URL HTTPS de l'API Bicep Generator |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | URL OTLP du dashboard Aspire (si WithOtlpExporter activé) |

---

## 13. OpenTelemetry Frontend ([2026-03-18])

### Packages installés

```
@opentelemetry/api
@opentelemetry/sdk-trace-web
@opentelemetry/exporter-trace-otlp-http
@opentelemetry/instrumentation
@opentelemetry/instrumentation-fetch
@opentelemetry/resources
@opentelemetry/semantic-conventions
```

### TelemetryService

Fichier : `src/Front/src/app/shared/services/telemetry.service.ts`

- Initialisé via `APP_INITIALIZER` dans `app.config.ts` **uniquement si** `environment.otlpEnabled === true`
- `FetchInstrumentation` injecte automatiquement le header W3C `traceparent` dans chaque requête Fetch/Axios
- L'exporter OTLP pointe vers `/otlp/v1/traces` (proxy Angular → Aspire Dashboard)
- **Pas actif en dev standalone ni en prod** (seul `environment.aspire.ts` a `otlpEnabled: true`)

### API OTel v1.x (IMPORTANT)

```typescript
// ✅ Correct (v1.x)
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';

const provider = new WebTracerProvider({
  resource: resourceFromAttributes({ [ATTR_SERVICE_NAME]: 'service' }),
  spanProcessors: [new BatchSpanProcessor(exporter)],
});
provider.register();

// ❌ Obsolète (v0.x)
// new Resource({ ... })
// provider.addSpanProcessor(...)
```

### Warning CommonJS protobufjs

Ajouter dans `angular.json` → `build.options` :
```json
"allowedCommonJsDependencies": ["protobufjs/minimal"]
```

---

## 14. BicepGeneration Service

- Pure Bicep generation engine in dedicated project `InfraFlowSculptor.BicepGeneration` (namespace `InfraFlowSculptor.BicepGeneration`)
- Self-contained: no dependency on Domain, Application, or Infrastructure
- Uses **strategy pattern**: `IResourceTypeBicepGenerator` per Azure resource type
- Registered in `InfraFlowSculptor.Application/DependencyInjection.cs` as singletons
- CQRS handlers in `InfraFlowSculptor.Application/InfrastructureConfig/Commands/GenerateBicep/`
- New resource types require: a new `IResourceTypeBicepGenerator` implementation + registration in `Application/DependencyInjection.cs`
- Output files: `types.bicep`, `functions.bicep`, `main.bicep`, `main.{env}.bicepparam` (per environment), `modules/*.bicep` (per resource type)

---

## 14.1 Azure Resource Naming Conventions ([2026-03-22])

> **CRITICAL PROJECT DATA — Do NOT remove or modify without explicit user request.**

### Default naming templates (auto-set at project creation)

These templates are auto-applied by `CreateProjectCommandHandler` when a new project is created.
They were previously on `CreateInfrastructureConfigCommandHandler` but were moved to project level.

| Scope | Template | Example result |
|-------|----------|----------------|
| **Default (all resources)** | `{name}-{resourceAbbr}{suffix}` | `myapp-kv01` |
| **ResourceGroup** override | `{resourceAbbr}-{name}{suffix}` | `rg-myapp01` |
| **StorageAccount** override | `{name}{resourceAbbr}{suffix}` | `myappstg01` |

### Resource type abbreviation catalog

Defined in `ResourceAbbreviationCatalog` (`src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Common/ResourceAbbreviationCatalog.cs`):

| Resource Type | Abbreviation |
|---------------|-------------|
| KeyVault | `kv` |
| RedisCache | `redis` |
| StorageAccount | `stg` |
| ResourceGroup | `rg` |

### Available naming template placeholders

`{name}`, `{prefix}`, `{suffix}`, `{env}`, `{resourceType}`, `{resourceAbbr}`, `{location}`

Validated by `NamingTemplateValidator` — any placeholder not in this list is rejected.

### Where naming lives

- **Project level** (source of truth): `Project.DefaultNamingTemplate` + `Project.ResourceNamingTemplates` — set at project creation, editable via API
- **InfrastructureConfig level**: inherits from project by default (`UseProjectNamingConventions = true`), can be overridden per config
- **Auto-set logic**: `CreateProjectCommandHandler` in `src/Api/InfraFlowSculptor.Application/Projects/Commands/CreateProject/`

### Bicep naming integration ([2026-03-23])

The Bicep generator now produces `types.bicep` and `functions.bicep` alongside `main.bicep`:

- `types.bicep` exports `EnvironmentName` (union type), `EnvironmentVariables` (object type with `envName`/`envSuffix`/`envShortSuffix`/`envPrefix`/`envShortPrefix`/`location`), and `environments` (variable map).
- `functions.bicep` imports `EnvironmentVariables` from `types.bicep` and exports naming functions generated from the project's naming templates:
  - `BuildResourceName(name, resourceAbbr, env)` — from the default template
  - `Build{ResourceType}Name(name, resourceAbbr, env)` — per-resource-type overrides
- `NamingTemplateTranslator` converts template placeholders to Bicep string interpolation (e.g. `{suffix}` → `${env.envSuffix}`)
- `main.bicep` imports types+functions, uses `param environmentName EnvironmentName`, resolves `var env = environments[environmentName]`
- Resource group and module names are computed via naming functions: `BuildResourceName('myVault', 'kv', env)` 
- `.bicepparam` files set `param environmentName = '{envName}'` + only resource-specific params (sku, etc.)
- **Convention**: `envSuffix = '-dev'` (with leading hyphen if non-empty), `envShortSuffix = 'dev'` (raw). Prefix similarly: `envPrefix = 'dev-'`, `envShortPrefix = 'dev'`.

---

### 13.1 Runtime i18n frontend (FR/EN) ([2026-03-21])

- Runtime translation uses `@ngx-translate/core` + `@ngx-translate/http-loader` (both v17) in `src/Front`.
- Global providers: `provideTranslateService({ lang: 'fr', fallbackLang: 'fr' })` + `provideTranslateHttpLoader({ prefix: '/i18n/', suffix: '.json' })` + `provideAppInitializer(() => inject(LanguageService).initialize())` in `app.config.ts`.
- `src/Front/src/app/shared/services/language.service.ts` — central language state: `signal<AppLanguage>`, `localStorage` persistence (key: `infra-flow-sculptor.language`), fallback: persisted → `navigator.language` → `'fr'`.
- Translation JSON files: `src/Front/public/i18n/fr.json` + `en.json` — 109 keys each.
- Shell language switch: `src/Front/src/app/core/layouts/navigation/` — visible on all authenticated pages.
- i18n coverage (as of 2026-03-21): navigation, footer, login, home (including dynamic params).

#### Key usage patterns

**1. Import `TranslateModule` in every component that displays text:**
```typescript
imports: [CommonModule, TranslateModule, /* ... */]
```

**2. Pipe `| translate` in templates:**
```html
{{ 'HOME.TITLE' | translate }}
{{ 'HOME.FEEDBACK.SUCCESS' | translate: { name: itemName() } }}
```

**3. Error signal stores the i18n key, not the text:**
```typescript
protected errorKey = signal('');
this.errorKey.set('LOGIN.ERROR.MSAL_FAILED');
// Template: {{ errorKey() | translate }}
```

#### Translation key namespaces

| Namespace | Composant |
|-----------|-----------|
| `LANGUAGE` | `LanguageService`, navigation |
| `NAV` | `navigation.component` |
| `FOOTER` | `footer.component` |
| `LOGIN` | `login.component` |
| `HOME` | `home.component` |

**Rule:** every new screen adds its own root namespace. Always add keys to **both** `fr.json` and `en.json`.

### 13.2 Config Detail naming template actions ([2026-03-21])

- New dialog component: `src/Front/src/app/features/config-detail/add-naming-template-dialog/` (`.ts`, `.html`, `.scss`) used for default/resource naming template add/edit.
- `config-detail.component.ts` now supports write actions for naming templates when `canWrite()` is true:
    - set/edit default template (`setDefaultNamingTemplate`)
    - add/edit resource template (`setResourceNamingTemplate`)
    - remove resource template with confirmation (`removeResourceNamingTemplate`)
- Naming actions use dedicated UI state signals in config detail (`namingActionKey`, `namingErrorKey`) for inline loading/error feedback.
- i18n namespace extended in both `src/Front/public/i18n/fr.json` and `src/Front/public/i18n/en.json` under `CONFIG_DETAIL.NAMING_TEMPLATES.*`.

## 15. Pull Request Conventions

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
- Référence complète : `.github/agents/pr-manager.agent.md`

### Description de PR

- Utiliser le template `.github/PULL_REQUEST_TEMPLATE.md`
- Lister chaque fichier créé/modifié dans sa section de couche
- Indiquer le nom de la migration EF Core si applicable
- Valider toute la checklist avant soumission

---

## 16. Frontend (Angular)

### 16.1 Current frontend baseline

- Angular 19 standalone app in `src/Front`
- Zoneless change detection enabled in `src/Front/src/app/app.config.ts`
- Material theme and Tailwind are both active (`src/Front/src/styles.scss`)
- Shared HTTP client uses Axios (`src/Front/src/app/shared/services/axios.service.ts`)
- JWT auth and cookie persistence handled by `AuthenticationService`

### 16.2 Frontend structure conventions

- `core/` for layout/shell components (navigation/footer)
- `shared/` for reusable cross-cutting concerns (services, facades, guards, enums, interfaces)
- `environments/` is the single source of truth for backend base URLs (`api_url` + `bicep_api_url`)
- Keep DTO/interface updates in sync with backend contract changes in `InfraFlowSculptor.Contracts`

### 16.2.1 Angular build budgets ([2026-03-21])

- Production budgets in `src/Front/angular.json` → `configurations.production.budgets`
- **`anyComponentStyle`**: warning 10 kB / error 20 kB (increased from 6/10 kB to accommodate growing feature pages with tabs, member management, environment management, etc.)
- **`initial` bundle**: warning 500 kB / error 1 MB
- **Rule:** if a component SCSS exceeds the budget after adding a legitimate feature, increase the budget rather than artificially compressing styles. Keep warning ≈ 50% of error.

### 16.3 Initialization status ([2026-03-17])

- Template app name replaced with `infra-flow-sculptor-front` in `src/Front/package.json`
- Angular project key renamed from `template_angular` to `infra_flow_sculptor_front` in `src/Front/angular.json`
- Browser title initialized to `InfraFlowSculptor` in `src/Front/src/index.html`
- Root app title initialized to `InfraFlowSculptor` in `src/Front/src/app/app.component.ts`
- Front README rewritten to include stack, commands, environment config, and structure

### 16.4 Backend contracts and API clients ([2026-03-17])

All backend request/response contracts from `InfraFlowSculptor.Contracts` are mirrored as TypeScript interfaces in `src/Front/src/app/shared/interfaces/`:

| File | Contents |
|------|----------|
| `infra-config.interface.ts` | `InfrastructureConfigResponse`, `MemberResponse`, `EnvironmentDefinitionResponse`, `ResourceNamingTemplateResponse`, `TagResponse`, plus all request types |
| `resource-group.interface.ts` | `ResourceGroupResponse`, `AzureResourceResponse`, `CreateResourceGroupRequest` |
| `key-vault.interface.ts` | `KeyVaultResponse`, `CreateKeyVaultRequest`, `UpdateKeyVaultRequest` |
| `redis-cache.interface.ts` | `RedisCacheResponse`, `CreateRedisCacheRequest`, `UpdateRedisCacheRequest` |
| `storage-account.interface.ts` | `StorageAccountResponse`, `BlobContainerResponse`, `StorageQueueResponse`, `StorageTableResponse`, plus all request types |
| `role-assignment.interface.ts` | `RoleAssignmentResponse`, `AzureRoleDefinitionResponse`, `AddRoleAssignmentRequest` |
| `bicep-generator.interface.ts` | `GenerateBicepRequest`, `GenerateBicepResponse` |

Angular API services in `src/Front/src/app/shared/services/` (all `providedIn: 'root'`, use `AxiosService.request$<T>()`):

| Service | API prefix | Key methods |
|---------|-----------|-------------|
| `InfraConfigService` | `/infra-config` | `getAll`, `getById`, `getResourceGroups`, `create`, `addMember`, `updateMemberRole`, `removeMember`, `addEnvironment`, `updateEnvironment`, `removeEnvironment`, `setDefaultNamingTemplate`, `setResourceNamingTemplate`, `removeResourceNamingTemplate` |
| `ResourceGroupService` | `/resource-group` | `getById`, `getResources`, `create` |
| `KeyVaultService` | `/keyvault` | `getById`, `create`, `update`, `delete` |
| `RedisCacheService` | `/redis-cache` | `getById`, `create`, `update`, `delete` |
| `StorageAccountService` | `/storage-accounts` | `getById`, `create`, `update`, `delete`, `addBlobContainer`, `removeBlobContainer`, `addQueue`, `removeQueue`, `addTable`, `removeTable` |
| `RoleAssignmentService` | `/azure-resources/{id}/role-assignments` | `getByResourceId`, `getAvailableRoleDefinitions`, `add`, `remove` |
| `BicepGeneratorService` | `${environment.bicep_api_url}/generate-bicep` | `generate` |

Type mapping convention (C# → TypeScript): `Guid` → `string`, `IReadOnlyList<T>` → `T[]`, `string?` → `string | null`, `bool` → `boolean`, `int` → `number`.
### 16.5 Entra ID (MSAL) authentication ([2026-03-17])

#### Package
- `@azure/msal-browser@^5` installed as npm dependency (no `@azure/msal-angular` needed — HTTP layer is Axios-based)
- **⚠️ MSAL v5 breaking change:** `storeAuthStateInCookie` no longer exists in `CacheOptions` — remove it if upgrading from v2/v3

#### New / modified files

| File | Role |
|------|------|
| `src/app/shared/interfaces/environment.interface.ts` | Added `MsalConfigInterface` (`clientId`, `authority`, `redirectUri`) + `msalConfig` field on `EnvironmentInterface` |
| `src/environments/environment.ts` | MSAL config for production (`redirectUri: '/'`) |
| `src/environments/environment.development.ts` | MSAL config for dev (`redirectUri: 'http://localhost:4200'`) |
| `src/app/shared/configs/msal.config.ts` | `msalConfig: Configuration` (reads from env) + `loginRequest: PopupRequest` with scopes `openid profile email` |
| `src/app/shared/services/msal-auth.service.ts` | `MsalAuthService`: lazy-initializes `PublicClientApplication`, calls `initialize()` + `handleRedirectPromise()` once, exposes `loginPopup()`, `getActiveAccount()`, `logout()` |
| `src/app/shared/services/authentication.service.ts` | Added `_msalAccount` signal with `setMsalAccount(account)` and `getMsalAccount` accessor used to track the current MSAL account |
| `src/app/features/login/login.component.*` | **New login page**: split-panel (50/50), blue gradient left panel (logo + branding + feature list), white right panel with "Sign in with Microsoft" entry point; lazy-loaded |
| `src/app/app-routing.ts` | Added `/login` route (lazy) wired into the auth flow |
| `src/app/app.component.ts` | Uses `toSignal` + `NavigationEnd` filter to compute `isLoginPage` signal |
| `src/app/app.component.html` | Hides `<app-navigation>` and `<app-footer>` when `isLoginPage()` is true |

#### Auth UX fix ([2026-03-17])

- `LoginComponent` now uses `MsalAuthService.loginRedirect()` (instead of popup flow) so Microsoft sign-in completes in the main browser tab and lands back on `/`
- `MsalAuthService.initialize()` now prioritizes `handleRedirectPromise()` result account before falling back to cached accounts
- `MsalAuthService` exposes `loginRedirect(redirectStartPage?)` to keep redirect behavior explicit per caller

#### Auth loop fix ([2026-03-21])

- Symptom: after sign-in, the app briefly showed `/` then redirected to `/login`.
- Root cause: `MsalAuthService.getActiveAccount()` selected `getAllAccounts()[0]` when multiple cached accounts existed; this could pick the wrong account, causing API `401` and `axios` redirect to `/login`.
- Fix in `src/Front/src/app/shared/services/msal-auth.service.ts`:
    - set MSAL active account explicitly from `handleRedirectPromise()` result,
    - deterministic fallback order (MSAL active account -> authService account if present in cache -> single cached account),
    - never pick an arbitrary account when multiple cached accounts exist,
    - reuse a single `resolveActiveAccount()` path for guard and token acquisition.

#### Feature structure
- Feature pages live under `src/Front/src/app/features/<feature-name>/`
- The login component is the first entry in `features/`

#### App Registration (Azure Portal) required settings

For clientId `24c34231-a984-43b3-8ac3-9278ebd067ef`:
1. **Authentication → Platform → Single-page application (SPA)**
2. **Redirect URIs:** `http://localhost:4200` (dev) + production URL

### 16.6 Shell layout and home page ([2026-03-21])

- `src/Front/src/app/core/layouts/navigation/*` is now the authenticated shell header: brand link to `/`, single `Accueil` nav item, current Entra account summary, and logout action via `MsalAuthService.logout()`. Includes FR/EN language switch (pill-shaped button group, dark nav context).
- `src/Front/src/app/core/layouts/footer/*` is now a lightweight informative footer aligned with the premium blue/cyan SaaS baseline from the login page.
- Protected root route in `src/Front/src/app/app-routing.ts` now lazy-loads `features/home/home.component` at `/`.
- `src/Front/src/app/features/home/*` provides the first authenticated landing page:
    - loads configurations with `InfraConfigService.getAll()` on init,
    - creates a configuration with `InfraConfigService.create({ name })`,
    - exposes explicit loading, empty, error, success, and retry states,
    - keeps the main CTA visible above the fold and reuses the login page blue/cyan premium visual language.
- `src/Front/src/app/app.component.*` now applies a real authenticated shell background instead of only a bare `main` min-height.

#### Visual baseline — palette and tokens (validated 2026-03-21)

```scss
// Background gradient — premium blue/cyan identity
background: linear-gradient(135deg, #1a237e 0%, #0288d1 50%, #00bcd4 100%);

// Card / form surfaces
background: rgba(255, 255, 255, 0.08);
border: 1px solid rgba(255, 255, 255, 0.15);
border-radius: 16px;
backdrop-filter: blur(10px);

// Text on dark background
color: rgba(255, 255, 255, 0.9);    // primary
color: rgba(148, 203, 255, 0.85);   // accent / secondary
color: rgba(255, 255, 255, 0.6);    // tertiary / labels

// CTA button
background: linear-gradient(135deg, #0288d1, #00bcd4);
border-radius: 12px;
```

#### Hero H1 typography — validated values

```scss
h1 {
  font-size: clamp(1.5rem, 2.2vw, 2.2rem);
  line-height: 1.2;
  max-width: 26ch;   // ≥ 20ch mandatory — narrow max-width creates heavy vertical stacking
}
```

#### Hero 2-column grid — validated ratio

```scss
.hero-panel {
  grid-template-columns: minmax(0, 1.2fr) minmax(16rem, 1fr); // ~55/45 — balanced
}
.hero-panel__content {
  justify-content: flex-start; // NOT space-between — avoids vertical stretch
}
```

### 16.7 Configuration detail page ([2026-03-22])

- New route `config/:id` inside authenticated children in `src/Front/src/app/app-routing.ts`
- `src/Front/src/app/features/config-detail/*` — standalone component (3 files)
  - Reads route param `id` via `ActivatedRoute`
  - Parallel fetch: `InfraConfigService.getById(id)` + `InfraConfigService.getResourceGroups(id)`
  - Sections: header (name + ID), members table, environment cards (with tags), resource groups, naming templates
  - Loading / error / empty states
  - Back button navigating to `/`
- Config cards on home page are now clickable `<a routerLink="/config/{{config.id}}">` elements with hover effect
- i18n namespace: `CONFIG_DETAIL.*` in both `fr.json` and `en.json`

### 16.8 Search and sort in config list ([2026-03-22])

- `HomeComponent` now has `searchQuery` and `sortBy` signals, plus `filteredConfigs` computed signal
- Search is case-insensitive on config name
- Sort options: by name (alphabetical), by environment count, by member count
- Search bar and sort dropdown rendered between list heading and config cards
- "No results" empty state when search matches nothing
- i18n keys: `HOME.SEARCH.*`, `HOME.SORT.*`

### 16.9 Member name bug fix + Owner-only actions ([2025-07-24])

- **Bug fix (name not showing after add):** `openAddMemberDialog()` and `onRoleChange()` now re-fetch config via `getById()` after mutation instead of using the direct API response. The backend `UpdateAsync` does not re-include `User` nav properties on newly added/modified members, causing empty `firstName`/`lastName`.
- **EntraId in member response chain:** Added `EntraId` (Azure AD OID) to `MemberResult`, `MemberResponse`, and their Mapster mappings so the frontend can identify the current user.
- **Owner-only UI gating:** `config-detail.component.ts` now injects `AuthenticationService`, computes `isOwner` by matching `AccountInfo.localAccountId` against `member.entraId`. The add-member button, role-change dropdown, and remove button are hidden for non-Owner users via `@if (isOwner())` in the template.

### 16.9 Create configuration dialog ([2026-03-22])

- `src/Front/src/app/features/home/create-config-dialog/*` — Material dialog component (3 files)
  - Reactive form moved from `HomeComponent` into the dialog
  - `MatDialogRef.close(createdConfig)` on success, cancel closes with no result
  - Uses `mat-dialog-title`, `mat-dialog-content`, `mat-dialog-actions` directives
- `HomeComponent` create panel simplified to a CTA card with "Add configuration" button that opens the dialog via `MatDialog.open()`
- Hero CTA also opens the dialog instead of scrolling to anchor
- Success feedback banner still shown in home page after dialog closes
- i18n keys: `HOME.DIALOG.*`

### 16.10 Member management on config detail page ([2026-03-22])

#### Backend enrichment
- `MemberResponse` now includes `FirstName` and `LastName` (resolved via `User` navigation property on `Member` entity)
- `Member` entity has `public User? User { get; private set; }` navigation property for EF Core read-side optimization
- `MemberConfiguration` adds `HasOne(pm => pm.User).WithMany().HasForeignKey(pm => pm.UserId).OnDelete(DeleteBehavior.Restrict)`
- All `InfrastructureConfigRepository` queries that `.Include(c => c.Members)` now also `.ThenInclude(m => m.User!)`
- New endpoint `GET /infra-config/users` returns all registered users (`ListUsersQuery` → `UserResponse(Id, FirstName, LastName)`)
- `IUserRepository` extended with `GetAllAsync` and `GetByIdsAsync`

#### Frontend member management
- `MemberResponse` interface updated with `firstName`, `lastName`
- New `UserResponse` interface and `getUsers()` method on `InfraConfigService`
- `addMember`/`updateMemberRole` return types changed to `Promise<InfrastructureConfigResponse>` (matching actual API response)
- Config detail page members section:
  - Shows `firstName lastName` instead of userId GUID
  - Inline `mat-select` per member for role changing (Owner/Contributor/Reader)
  - Remove button with confirmation dialog

| 2026-03-24 | copilot | Added **LogAnalyticsWorkspace** + **ApplicationInsights** aggregates — full-stack CQRS following ContainerAppEnvironment/ContainerApp parent-child pattern. **51 new files, 7 modified.** **LogAnalyticsWorkspace Domain**: extends `AzureResource` (no extra resource-level props). `LogAnalyticsWorkspaceEnvironmentSettings` entity with per-env overrides (Sku?, RetentionInDays?, DailyQuotaGb?). `Microsoft.OperationalInsights/workspaces@2023-09-01`. Abbreviation `law`. **ApplicationInsights Domain**: extends `AzureResource` with `LogAnalyticsWorkspaceId` FK (`AzureResourceId`). `ApplicationInsightsEnvironmentSettings` with per-env overrides (SamplingPercentage?, RetentionInDays?, DisableIpMasking?, DisableLocalAuth?, IngestionMode?). `Microsoft.Insights/components@2020-02-02`. Abbreviation `appi`. **Application**: Full CQRS for both (Create/Update/Delete commands + handlers + validators, Get query + handler, Result records, ConfigData records, repository interfaces). **Infrastructure**: TPT EF Core configurations, `AzureResourceRepository<T>` implementations. **Contracts**: RequestBase + Create/Update requests + Responses for both. **API**: Controllers with endpoints, Mapster mapping configs. **Bicep**: `LogAnalyticsWorkspaceTypeBicepGenerator` and `ApplicationInsightsTypeBicepGenerator`. **Modified**: Infrastructure DI, ProjectDbContext (4 new DbSets), Application DI (2 new generators), Program.cs (2 new controllers), ResourceAbbreviationCatalog, AzureRoleDefinitionCatalog (8 new role definitions), InfrastructureConfigReadRepository (settings queries + MapResource switch arms). **Namespace conflict fix**: `IApplicationInsightsRepository` uses `using ApplicationInsightsEntity = ...` alias because the `Application.ApplicationInsights` folder namespace shadows the domain type. Migration: `AddLogAnalyticsWorkspaceAndApplicationInsights`. |
  - "Add member" button opening `AddMemberDialog` (user picker + role selector)
- New components:
  - `src/Front/src/app/features/config-detail/add-member-dialog/*` (3 files) — user select filtered to exclude existing members
  - `src/Front/src/app/shared/components/confirm-dialog/*` (3 files) — reusable confirm dialog with i18n
- Roles: `Owner`, `Contributor`, `Reader` (matching `Role.RoleEnum` in domain)
- 17 new i18n keys under `CONFIG_DETAIL.MEMBERS.*` in both `fr.json` and `en.json`

---

## 18. Documentation Azure ([2026-03-19])

La documentation Azure est versionnée dans le dépôt GitHub dans le dossier `docs/azure/` :

| Fichier | Contenu |
|---------|---------|
| `docs/README.md` | Index principal de la documentation |
| `docs/azure/README.md` | Sommaire de la documentation Azure |
| `docs/azure/architecture.md` | Architecture cloud, services Azure, flux auth |
| `docs/azure/ressources.md` | Ressources provisionnées par l'app + ressources infra |
| `docs/azure/securite-iam.md` | Entra ID, scopes, rôles, bonnes pratiques |
| `docs/azure/conventions-nommage.md` | Règles de nommage ressources Azure |

### Lien Azure DevOps

Le wiki ADO `Infra-Flow-Sculptor-Wiki` contient la section `/Documentation-Azure` avec 4 sous-pages qui pointent vers les fichiers GitHub correspondants.

### Limitation : "Publish code as wiki"

La fonctionnalité Azure DevOps "Publish code as wiki" (sync auto Git → wiki) **ne fonctionne qu'avec Azure Repos**, pas GitHub. Pour l'activer sur GitHub, il faut d'abord créer un **service connection GitHub** dans Azure DevOps (Project Settings → Service connections). Les étapes manuelles sont décrites dans la page wiki `/Documentation-Azure`.

---

## 19. Agent custom merge-main ([2026-03-19])

- Fichier: `.github/agents/merge-main.agent.md`
- Responsabilite: fusionner `origin/main` sur la branche courante, resoudre les conflits en s'appuyant sur `MEMORY.md`, puis ajouter les adaptations necessaires quand des nouveautes de `main` impactent la branche courante sans conflit Git explicite.
- Protocole impose: lecture complete de `MEMORY.md` au demarrage, verification de branche/etat local, `git fetch origin main --prune`, merge, resolution semantique des conflits, verification build/typecheck selon perimetre, mise a jour de `MEMORY.md` en fin d'execution.

---

## 20. Agent angular-front ([2026-03-21])

- Fichier : `.github/agents/angular-front.agent.md`
- Responsabilité : **Tout travail frontend dans `src/Front` doit passer par cet agent.**
- Couvre : standalone components (3 fichiers obligatoires : `.ts`, `.html`, `.scss`), Signals (`signal`, `computed`, `effect`, `input`, `output`, `model`, `resource`, `toSignal`), zoneless change detection, `inject()`, nouvelle syntaxe template (`@if`/`@for`/`@switch`), Angular Material + Tailwind, Axios services, interfaces alignées sur les contrats backend, routes lazy loading, guards fonctions, environnements.
- **Protocole de délégation :** L'agent `memory` délègue toute tâche `src/Front` à `angular-front` — il ne génère jamais de code Angular directement.
- Validation post-implémentation : `npm run typecheck` + `npm run build` dans `src/Front`.

---

## 21. Agent dotnet-dev ([2026-03-21])

- Fichier : `.github/agents/dotnet-dev.agent.md`
- Responsabilité : **Toute génération ou modification de code C#/.NET doit respecter les règles de cet agent.**
- Couvre :
  - **Conventions de nommage Microsoft** : `PascalCase` classes/méthodes/props, `_camelCase` champs privés, `I` prefix interfaces, suffixe `Async` pour les Tasks.
  - **Documentation XML obligatoire** sur tous les membres `public`/`protected` : `<summary>`, `<param>`, `<returns>`, `<exception>`, `<see cref="">`.
  - **No magic strings** : constantes dédiées, `nameof()`, classes `Errors.*`, `AuthorizationPolicies`, `ConfigurationKeys`.
  - **SOLID** : SRP (une responsabilité par classe), OCP (extension par composition), LSP, ISP (interfaces étroites), DIP (dépendance vers les abstractions).
  - **Découpage** : extraire en helper statique si logique réutilisable sans état ; créer un service injectable si logique avec dépendances ; garder dans le handler si unique.
  - **Async/Await** : suffixe `Async`, propager `CancellationToken`, jamais `.Result`/`.GetAwaiter().GetResult()`, `ConfigureAwait(false)` dans les librairies.
  - **Gestion des nulls** : `<Nullable>enable</Nullable>`, guard clause `ArgumentNullException.ThrowIfNull`, pattern matching `is null`.
  - **Immutabilité** : `record` pour les DTOs, `init` pour les propriétés, `readonly` pour les champs, `IReadOnlyCollection<T>` pour les collections exposées.
  - **Pattern matching** et switch expressions pour remplacer les if/else chaînés.
  - **Guard clauses** (early return) pour éviter les pyramides d'imbrication.
  - **`sealed`** sur les classes non héritables (handlers, validators, configurations EF Core).
  - **EF Core** : `AsNoTracking()` en lecture, `FirstOrDefaultAsync` plutôt que `ToListAsync().First()`, comparaisons sur value objects entiers (jamais `.Value`).
  - **FluentValidation** : un validator par commande, `.WithMessage()` obligatoire.
  - **Logging** : `ILogger<T>` injecté, structured logging (paramètres, pas interpolation), niveaux appropriés.
  - **Code smells** : Long Method (< 30 lignes), Feature Envy, Primitive Obsession, Dead Code supprimé.
  - **Sécurité** : pas de SQL par concaténation, pas de secrets hardcodés, `NotFound` pour non-membres (no info leak).
- Validation : `dotnet build .\InfraFlowSculptor.slnx` obligatoire avant commit.

---

## 22. Refactoring agents — @memory → @dev + Skill cqrs-feature ([2026-03-21])

### Changements

| Avant | Après |
|-------|-------|
| `@memory` — agent monolithique (mémoire + CQRS + coordination) | `@dev` — orchestrateur léger (lire mémoire, router, charger skills, updater mémoire) |
| Guide CQRS inline dans `memory.agent.md` | Skill `cqrs-feature` dans `.github/skills/cqrs-feature/SKILL.md` |
| `memory.agent.md` actif | `memory.agent.md` déprécié (redirecteur vers `@dev`) |

### Nouveaux fichiers
- `.github/agents/dev.agent.md` — Point d'entrée principal, orchestrateur léger
- `.github/skills/cqrs-feature/SKILL.md` — Guide des 9 étapes CQRS avec exemples, chargé à la demande
- `.github/agents/memory.agent.md` — Réduit à un redirecteur vers `@dev`

### Concept Skill
Un **Skill** est un fichier `SKILL.md` de connaissance pure, chargé à la demande avec `read_file` quand la tâche le justifie.
Pas d'outils, lazy-loaded, composable. Idéal pour centraliser un workflow réutilisable sans alourdir les agents.
Voir la section "Skills" de `copilot-instructions.md` pour la liste des skills disponibles.

---

## 23. Skill UI/UX frontend — ui-ux-front-saas ([2026-03-21])

- Fichier : `.github/skills/ui-ux-front-saas/SKILL.md`
- Décision d'architecture : **Skill** (et non nouvel agent) pour la couche UI/UX.

### Pourquoi un Skill est plus pertinent qu'un agent ici

| Option | Limite | Choix |
|--------|--------|-------|
| Nouvel agent UI/UX | Ajouterait un sous-agent de plus à orchestrer pour chaque tâche front, avec risque de doublon avec `angular-front` | Non retenu |
| Skill UI/UX | Connaissance transversale, lazy-loaded, réutilisable par `dev` et `angular-front`, plus simple à maintenir | **Retenu** |

### Règle d'usage

- Toute tâche frontend qui touche un écran, un composant visuel, un layout, du HTML ou du SCSS doit charger ce skill en premier.
- Le skill impose l'alignement avec la baseline visuelle login :
    - `src/Front/src/app/features/login/login.component.html`
    - `src/Front/src/app/features/login/login.component.scss`
- Il encode le cadrage UX/UI SaaS B2B cloud, design system, accessibilité WCAG 2.1 AA, responsive, mode clair/sombre, et outputs handoff.

### Fichiers mis à jour

- `.github/agents/angular-front.agent.md` : skill UI/UX rendu obligatoire pour tâches UI
- `.github/agents/dev.agent.md` : routing + skills catalog enrichis avec `ui-ux-front-saas`
- `.github/copilot-instructions.md` : enregistrement global du skill + exigence frontend UI

---

## 24. Agent Aspire debug — aspire-debug ([2026-03-21])

- Fichier : `.github/agents/aspire-debug.agent.md`
- Décision d'architecture : **Agent** (et non skill) pour le debug runtime Aspire.

### Pourquoi un Agent est plus pertinent qu'un Skill ici

| Option | Limite | Choix |
|--------|--------|-------|
| Skill debug Aspire | Un skill n'apporte que de la connaissance, sans orchestration opérationnelle ni séparation de contexte dédiée | Non retenu |
| Agent debug Aspire | Workflow outillé orienté diagnostics runtime (état ressources, logs structurés, console logs, traces, recovery/restart), facilement délégable depuis `dev` | **Retenu** |

### Règle d'usage

- Toute investigation runtime/AppHost (startup failure, ressource KO, dépendance indisponible, corrélation logs/traces) doit passer par `aspire-debug`.
- Ordre de diagnostic imposé : apphost -> ressources -> structured logs -> console logs -> traces -> recovery minimal -> validation post-fix.
- Le correctif code éventuel est ensuite délégué à `dotnet-dev` (backend) ou `angular-front` (frontend).

### Fichiers mis à jour

- `.github/agents/aspire-debug.agent.md` : nouvel agent spécialisé debug Aspire + MCP
- `.github/agents/dev.agent.md` : ajout du routage "Debug runtime/AppHost Aspire"
- `.github/copilot-instructions.md` : ajout de l'agent spécialisé Aspire runtime debugging

---

## 16. Changelog
## 17. Changelog

| Date | Author | Change |
|------|--------|--------|
| 2026-03-15 | copilot | Initial MEMORY.md created from full project exploration |
| 2026-03-15 | copilot | Fixed `InvalidOperationException` in `InfrastructureConfigRepository.GetByIdWithMembersAsync`: `c.Id.Value == id.Value` → `c.Id == id` (EF Core cannot translate `.Value` property access on value objects in LINQ queries) |
| 2026-03-15 | copilot | Added authorization checks to all resource CRUD endpoints (ResourceGroup, KeyVault, RedisCache): created `InfraConfigAccessHelper` (`VerifyReadAccessAsync`/`VerifyWriteAccessAsync`), added `Errors.InfrastructureConfig.ForbiddenError()`, overrode `ResourceGroupRepository.GetByIdAsync` with safe LINQ pattern |
| 2026-03-15 | copilot | Added RBAC role assignment feature: `RoleAssignment` entity on `AzureResource`, `ManagedIdentityType` value object, `IAzureResourceRepository`, `AzureResourceBaseRepository`, `AddRoleAssignment`/`RemoveRoleAssignment`/`ListRoleAssignments` CQRS handlers, EF Core migration `AddRoleAssignmentsTable`, REST endpoints under `/azure-resources/{id}/role-assignments` |
| 2026-03-17 | copilot | Added PR conventions: `.github/PULL_REQUEST_TEMPLATE.md`, `.github/agents/pr-manager.agent.md`, updated `copilot-instructions.md`, `memory.agent.md`, `cqrs.agent.md` with mandatory PR title format `type(scope): description` and description template |
| 2026-03-17 | copilot | Added `GET /infra-config/{id}/resource-groups` endpoint: new `ListResourceGroupsByConfigQuery` + handler, `IResourceGroupRepository.GetByInfraConfigIdAsync` |
| 2026-03-17 | copilot | Refactored `InfraConfigAccessHelper` (static) → `IInfraConfigAccessService` injectable service. Interface in `Application/Common/Interfaces/`, implementation in `InfrastructureConfig/Common/InfraConfigAccessService.cs`. Updated all 11 handlers (ResourceGroup, KeyVault, RedisCache) to inject the service. |
| 2026-03-17 | copilot | Fixed environment body GUID validation: `AddEnvironmentRequest`/`UpdateEnvironmentRequest` now use `string` + `[GuidValidation]`, `GuidValidation` accepts strings and `Guid`, and `InfrastructureConfigController` parses IDs after validation to avoid `BadHttpRequestException` on invalid JSON GUIDs |
| 2026-03-17 | copilot | Updated InfrastructureConfig command handlers (`AddEnvironment`, `UpdateEnvironment`, `RemoveEnvironment`, `SetDefaultNamingTemplate`, `SetResourceNamingTemplate`, `RemoveResourceNamingTemplate`) to use `IInfraConfigAccessService` instead of removed `InfraConfigAccessHelper`. |
| 2026-03-17 | copilot | Updated StorageAccount commands/queries to use `IInfraConfigAccessService`; added `StorageAccountAccessHelper` and updated `StorageAccountAccessContext` to carry `IInfraConfigAccessService` instead of legacy `IInfrastructureConfigRepository` + `ICurrentUser`. |
| 2026-03-17 | copilot | Explored and initialized Angular frontend template (`src/Front`): renamed package/project identifiers, set application title, updated frontend README, and documented frontend architecture/conventions in memory and `.github` agent instructions. |
| 2026-03-17 | copilot | Fixed startup crash `42P07: relation "InfrastructureConfigs" already exists`: migration `20260317163342_StorageAccount` was generated from a corrupted/empty EF Core snapshot, causing it to recreate all tables from scratch. Fixed by replacing the `Up()`/`Down()` bodies with empty methods and regenerating `Designer.cs` from the correct `ProjectDbContextModelSnapshot.cs`. All tables already existed in the DB; only the snapshot was out of sync. **Pattern to watch:** if a new migration contains `CREATE TABLE` for tables that should already exist, the snapshot was corrupted — fix with empty `Up()`/`Down()` + regenerated `Designer.cs`. |
| 2026-03-17 | copilot | Added Azure Storage Account Bicep generation: `StorageAccountTypeBicepGenerator` now uses all properties (`sku`, `kind`, `accessTier`, `allowBlobPublicAccess`, `supportsHttpsTrafficOnly`, `minimumTlsVersion`) from `resource.Properties`. Added `StorageAccount` case in `InfrastructureConfigReadRepository.MapResource()` with `MapStorageTlsVersion()` helper mapping Tls10/11/12 → TLS1_0/TLS1_1/TLS1_2. Azure Bicep property name is `supportsHttpsTrafficOnly` (not `enableHttpsTrafficOnly`). |
| 2026-03-17 | copilot | Added all backend contracts as TypeScript interfaces and Angular API services in `src/Front/src/app/shared/`. Interfaces in `interfaces/` folder (infra-config, resource-group, key-vault, redis-cache, storage-account, role-assignment). Services in `services/` folder (InfraConfigService, ResourceGroupService, KeyVaultService, RedisCacheService, StorageAccountService, RoleAssignmentService) — all `providedIn: 'root'`, using `AxiosService.request$<T>()`. |
| 2026-03-17 | copilot | Added Microsoft Entra ID (MSAL) authentication to Angular frontend: installed `@azure/msal-browser@^5`, created `MsalAuthService` + `msal.config.ts`, added `MsalConfigInterface` to `EnvironmentInterface`, created split-panel login page under `src/app/features/login/`, updated `AuthenticationService` to support Azure AD `roles[]` claim, added lazy `/login` route, hid nav/footer on login page. App registration `24c34231-a984-43b3-8ac3-9278ebd067ef` requires: SPA platform, redirect URIs `http://localhost:4200` + prod URL, Graph `openid`/`profile`/`email` permissions, implicit grant unchecked. |
| 2026-03-17 | copilot | Fixed frontend MSAL sign-in UX where popup ended on app UI: switched login action to redirect flow (`loginRedirect`) from `LoginComponent`, restored authenticated account from redirect result in `MsalAuthService`, and kept landing page on `/` in the main tab. |
| 2026-03-18 | copilot | Integrated Angular frontend into Aspire: `AddJavaScriptApp` + `.WithNpm()` (Aspire 13.x API), `start:aspire` npm script, `environment.aspire.ts` with `/api-proxy` base URL, `proxy.conf.js` relaying `/api-proxy/*` → backend and `/otlp/*` → Aspire Dashboard, `TelemetryService` with OTel Web SDK v1.x (fetch instrumentation + OTLP export), `APP_INITIALIZER` in `app.config.ts` conditional on `otlpEnabled`, fixed `apiScopes` missing from `MsalConfigInterface`, added `allowedCommonJsDependencies` for protobufjs. |
| 2026-03-18 | copilot | Validated Aspire MCP runtime inspection flow: use `list apphosts` + `list resources` + `list structured logs`/`list console logs` to quickly confirm stack health and retrieve latest startup events (EF migrations, listening URLs, Angular proxy targets). |
| 2026-03-17 | copilot | Fixed frontend MSAL sign-in UX where popup ended on app UI: switched login action to redirect flow (`loginRedirect`) from `LoginComponent`, restored authenticated account from redirect result in `MsalAuthService`, and kept landing page on `/` in the main tab. |
| 2026-03-19 | copilot | Added Azure documentation versioning: created `docs/azure/` folder in GitHub repo (README, architecture.md, ressources.md, securite-iam.md, conventions-nommage.md). Created ADO wiki section `/Documentation-Azure` with 4 sub-pages (Architecture-Cloud, Ressources-Managees, Securite-IAM, Conventions-Nommage) each linking back to the GitHub source file. Updated ADO wiki Home page to include the new section. Note: "Publish code as wiki" feature (auto-sync from Git to ADO wiki) requires the repo to be in Azure Repos or a GitHub service connection — manual steps described in `/Documentation-Azure` wiki page. |
| 2026-03-19 | copilot | Cleaned post-merge `MEMORY.md` structure: harmonized section numbering (13→17 block), fixed duplicated frontend subsection index (`16.4`/`16.5`), and kept all historical entries intact. |
| 2026-03-19 | copilot | Connected frontend to Bicep Generator API: added `bicep_api_url` in Angular environments, added `/bicep-api-proxy` in `proxy.conf.js`, injected `bicep-generator-api` reference into AppHost Angular app, and created typed frontend `BicepGeneratorService.generate()` calling `POST /generate-bicep`. |
| 2026-03-19 | copilot | Added custom agent `.github/agents/merge-main.agent.md` to automate merge from `main` into current branch with conflict resolution guided by `MEMORY.md`, plus post-merge adaptations when new features from `main` require extra updates on the current branch. |
| 2026-03-19 | copilot | Added Azure DevOps section (section 5) in `.github/agents/pr-manager.agent.md`: search or create Epic/US, add Tasks per action, link PR to ADO work items via `az boards`, update item statuses. Updated `memory.agent.md` checklist to include ADO step. ADO coordinates: org `florian-drevet`, project `Infra Flow Sculptor`. |
| 2026-03-19 | copilot | Renamed `.github/agents/pr-conventions.agent.md` to `.github/agents/pr-manager.agent.md` and updated all references in `MEMORY.md`, `copilot-instructions.md`, and `memory.agent.md`. |
| 2026-03-19 | copilot | Updated `.github/agents/pr-manager.agent.md`: added step 5 (documentation verification and update) in the PR creation protocol to check and update docs (`docs/`, `docs/azure/`, README, wiki) whenever changes impact documentation or require new information. |
| 2026-03-21 | copilot | Created `.github/agents/angular-front.agent.md`: new specialized agent for all Angular 19 frontend work. Covers signals, standalone, zoneless, inject(), @if/@for/@switch, Material+Tailwind, Axios services, lazy routes. Updated `memory.agent.md` to delegate all `src/Front` tasks to this agent. Updated `copilot-instructions.md` with explicit agent reference and expanded frontend conventions. Added section 20 in MEMORY.md. |
| 2026-03-21 | copilot | Created `.github/agents/dotnet-dev.agent.md`: new specialized agent for all C#/.NET backend work. Covers Microsoft naming conventions, XML documentation on all public members, no-magic-strings (constants/nameof), SOLID principles, async/await best practices, nullable reference types, immutability (record/init/readonly), pattern matching, guard clauses, sealed classes, EF Core pitfalls (AsNoTracking, value object comparisons), FluentValidation (WithMessage), ILogger<T> structured logging, code smell prevention (Long Method, Feature Envy, Primitive Obsession). Updated `memory.agent.md` and `copilot-instructions.md` with Specialized agents section. Added section 21 in MEMORY.md. |
| 2026-03-21 | copilot | Refactored agent architecture: renamed `@memory` → `@dev` (new `dev.agent.md` orchestrator); extracted CQRS guide into lazy-loaded skill `.github/skills/cqrs-feature/SKILL.md`; deprecated `memory.agent.md` to a redirect notice; added Skills concept + `cqrs-feature` skill to `copilot-instructions.md`; added section 22 in MEMORY.md. |
| 2026-03-21 | copilot | Added skill `.github/skills/ui-ux-front-saas/SKILL.md` for frontend UI/UX governance (based on login visual baseline + SaaS B2B cloud prompt). Enforced skill loading in `angular-front.agent.md`, registered routing in `dev.agent.md`, and added mandatory usage in `.github/copilot-instructions.md`. Added section 23 in MEMORY.md. |
| 2026-03-21 | copilot | Added `.github/agents/aspire-debug.agent.md` for runtime diagnostics with Aspire MCP (resource health, structured logs, console logs, traces, restart/recovery workflow). Registered routing in `dev.agent.md` and specialized-agent policy in `.github/copilot-instructions.md`. Added section 24 in MEMORY.md with agent-vs-skill rationale. |
| 2026-03-21 | copilot | Rebalanced home hero typography/layout in `src/Front/src/app/features/home/home.component.scss` to reduce the heavy left text block: smaller H1 scale, wider line length, lighter vertical distribution, and more balanced column ratio against stats cards. |
| 2026-03-21 | copilot | Fixed frontend post-login redirect loop to `/login`: `MsalAuthService` no longer selects `getAllAccounts()[0]` arbitrarily. It now sets/uses MSAL active account deterministically (redirect account first, then safe fallbacks) and avoids wrong-account token acquisition that triggered API `401` and forced login redirect. |
| 2026-03-22 | copilot | Added 3 frontend features: (1) Configuration detail page at `/config/:id` with members, environments, resource groups, naming templates sections + back navigation. (2) Search (by name) + sort (name/environments/members) in home config list with computed signals. (3) Replaced inline create form with Material dialog (`create-config-dialog`), hero CTA now opens dialog. Added `CONFIG_DETAIL.*`, `HOME.SEARCH.*`, `HOME.SORT.*`, `HOME.DIALOG.*` i18n keys in both fr.json and en.json. New files: `features/config-detail/*` (3 files), `features/home/create-config-dialog/*` (3 files). Modified: `app-routing.ts`, `home.component.ts/html/scss`, `fr.json`, `en.json`. |
| 2026-03-22 | copilot | Added member management: **Backend**: enriched `MemberResponse` with `FirstName`/`LastName` (added `User` nav property on `Member` entity, `.ThenInclude(m => m.User!)` in all InfrastructureConfigRepository queries, updated Mapster mappings). Added `GET /infra-config/users` endpoint (`ListUsersQuery`/`UserResponse`). Added `IUserRepository.GetAllAsync`/`GetByIdsAsync`. **Frontend**: updated `MemberResponse` interface with `firstName`/`lastName`, added `UserResponse` interface, `getUsers()` method on `InfraConfigService`. Config detail page shows member names instead of IDs, inline `mat-select` for role change (Owner/Contributor/Reader), remove button with confirm dialog, "Add member" button opening a dialog (`add-member-dialog`) with user picker + role selector. Created reusable `ConfirmDialogComponent`. Added 17 i18n keys under `CONFIG_DETAIL.MEMBERS` in both FR/EN. |
| 2026-03-22 | copilot | Refactored config detail page from stacked sections to **tabbed layout** using `MatTabsModule`. Tabs in order: Resource Groups (default), Environments, Members, Naming Templates. Each tab label has an icon + count badge. Improved empty states with centered icon + message. Added 4 TABS i18n keys in both FR/EN. Updated `angular.json` component style budget to `6kB warning / 10kB error` to accommodate growth from member management + tabs. |
| 2026-03-22 | copilot | Improved Members tab UX: (1) Members now grouped by role sections (Owner > Contributor > Reader) with role icon, title, and count badge per section — uses computed signal `membersByRole`. (2) Add-member dialog: replaced `mat-select` user dropdown with `mat-autocomplete` search bar (type-ahead filtering by first/last name). Added `ADD_DIALOG_SEARCH_PLACEHOLDER` and `ADD_DIALOG_NO_RESULTS` i18n keys in both FR/EN. |
| 2026-03-21 | copilot | Added environment management UI on config detail page: add/edit/remove environments via dialog (`add-environment-dialog` component, 3 files), sorted environment cards with edit/delete action buttons gated by `canWrite` computed (Owner or Contributor). Reusable confirm dialog for delete. Increased `anyComponentStyle` budget to 10kB warning / 20kB error in `angular.json`. Added 30+ i18n keys under `CONFIG_DETAIL.ENVIRONMENTS.*` in both FR/EN. |
| 2026-03-21 | copilot | Fixed auth redirect loop root cause: **Backend port collision** — BicepGenerator.Api and InfraFlowSculptor.Api both configured for HTTPS port 7246 in `launchSettings.json`. Changed BicepGenerator to port 7247 to avoid conflict. When Aspire assigned port 7246 it served Bicep API instead of main API, causing all requests to go to wrong backend → 401 → auth loop. **Pattern to watch:** Ensure no port collisions in `launchSettings.json` profiles. |
| 2026-03-21 | copilot | **Enum file organization constraint** — Refactored `LocationEnum` from inline in `add-environment-dialog.component.ts` to dedicated file `src/Front/src/app/features/config-detail/enums/location.enum.ts`. Established rule: **backend enums require frontend dropdowns (`<mat-select>`), and enums MUST live in separate `.enum.ts` files** (never inline in components). Updated `angular-front.agent.md` section "Enums TypeScript — Règles strictes" with file placement logic (shared → `src/app/shared/enums/`, feature-only → `src/app/features/{feature}/enums/`), template examples, and Material dropdown usage pattern. Updated `frontend-enum-convention.md` memory with detailed rule + file structure + usage example. |
| 2026-03-21 | copilot | Added "Add Resource Group" dialog on config detail page: new `add-resource-group-dialog` component (3 files) with name + location (dropdown reusing `LOCATION_OPTIONS`) form, wired into Resource Groups tab with add button gated by `canWrite`. After creation, refreshes resource groups list. Added 10 i18n keys under `CONFIG_DETAIL.RESOURCE_GROUPS.*` in both FR/EN. |
| 2026-03-21 | copilot | Refactored Resource Groups tab from flat cards to **expandable accordion** with lazy-loaded resources. New `add-resource-dialog` component (3 files) with multi-step flow: type picker (KeyVault / RedisCache / StorageAccount) → resource-specific form. New `resource-type.enum.ts` (`ResourceTypeEnum`, `RESOURCE_TYPE_OPTIONS`, `RESOURCE_TYPE_ICONS`). Resources loaded via `ResourceGroupService.getResources()` with caching in `rgResources` signal. Add-resource button gated by `canWrite`. Added 30+ i18n keys under `CONFIG_DETAIL.RESOURCES.*` in both FR/EN. Fixed Angular template strict-mode warnings by using `{ [rgId: string]: AzureResourceResponse[] | undefined }` type instead of `Record<string, T[]>`. |
| 2026-03-21 | copilot | Added naming template management UI in config detail: new `add-naming-template-dialog` component (3 files), write actions for default and per-resource templates (add/edit/remove) in `config-detail`, inline loading/error states via `namingActionKey`/`namingErrorKey`, and i18n additions under `CONFIG_DETAIL.NAMING_TEMPLATES.*` in both `fr.json` and `en.json`. |
| 2026-03-22 | copilot | Enhanced naming templates: **Backend**: added `{resourceAbbr}` placeholder to `NamingTemplateValidator.AllowedPlaceholders`, created `ResourceAbbreviationCatalog` (KeyVault→kv, RedisCache→redis, StorageAccount→stg, ResourceGroup→rg), modified `CreateInfrastructureConfigCommandHandler` to auto-set default naming template `{name}-{resourceAbbr}{suffix}` + resource overrides for ResourceGroup (`{resourceAbbr}-{name}{suffix}`) and StorageAccount (`{name}{resourceAbbr}{suffix}`). **Frontend**: added clickable placeholder variable chips in naming template dialog (click to insert `{placeholder}` at cursor position), `RESOURCE_TYPE_ABBREVIATIONS` in `resource-type.enum.ts`, i18n placeholder descriptions (`PLACEHOLDERS.name/prefix/suffix/env/resourceType/resourceAbbr/location`) in both FR/EN. |
| 2026-03-22 | copilot | Added naming preview in Resource Groups tab: environment selector dropdown in toolbar to pick a preview environment, then each resource group and resource shows a green pill with the estimated deployment name resolved from naming templates (default + per-resource-type overrides) and the selected environment's `prefix`, `suffix`, `env`, `location` values. New `resolveNamingPreview()` method, `previewEnvId`/`previewEnv` signals, `FormsModule` import for `ngModel` binding. Added `PREVIEW_ENV`, `PREVIEW_NONE`, `PREVIEW_TOOLTIP` i18n keys in both FR/EN. |
| 2026-03-22 | copilot | Improved environment dialog order UX: added interactive timeline visualization showing all environments in deployment order with the current environment highlighted (blue dot + bold label). When user changes the order value, the timeline reactively updates to show the new position. Order input is now bounded with `Validators.min(0)` / `Validators.max(count)` and a hint showing the valid range. Dialog now receives `allEnvironments` in its data. Added `ORDER_HINT`, `TIMELINE_ARIA` i18n keys in both FR/EN. |
| 2026-03-22 | copilot | Replaced order input with left/right arrow buttons below the timeline in environment dialog. Removed `order` form control (now driven by `currentOrder` signal + `moveOrder(delta)`). Removed index numbers under timeline labels. |
| 2026-03-22 | copilot | Fixed environment dialog timeline upper bound: order positions are now **1-based** with `minOrder = 1` and `maxOrder = otherEnvironments.length + 1`, so the right arrow can reach the true final slot after the last environment. Default create position also now starts at the far-right slot. |
| 2026-03-22 | copilot | Fixed environment dialog order timeline navigation bugs: rewrote `timelineItems` computed to use **index-based positioning** instead of comparing raw backend order values. Old logic used `findIndex(item.order >= order)` which broke when backend orders were non-contiguous (e.g. [1,3] after excluding current env). New logic uses `insertIdx = position - 1` to splice current env at the correct array index. Added `computeInitialPosition()` to convert backend order into a 1-based position relative to other environments. **Pattern**: always treat timeline display as position-based (1..N+1), never compare against raw backend order values. |
| 2026-03-22 | copilot | Fixed environment order save mismatch: `onSubmit` was sending the visual position (1-based) directly as the backend `order` value. When backend orders were non-contiguous (gaps from previous edits) this caused: move 1 → nothing changes, move 2 → only shifts by 1. Added `computeTargetOrder()` that converts visual position back to the correct backend order value by looking up `otherEnvironments[position-1].order`. For "after last" slot: CREATE sends `lastOrder+1` (so `ShiftOrdersUp` has nothing to shift), UPDATE sends `lastOrder` (so `ReorderEnvironments` shifts the last env down correctly). "No move" detection compares `currentOrder` against `initialPosition` to return the original order unchanged. **Pattern**: visual position ≠ backend order value; always convert before sending to API. |
| 2026-03-22 | copilot | Fixed remaining edit-mode direction bug in `computeTargetOrder()`: moving **right by one slot** was overshooting by 2 positions after save. Root cause: using `others[position-1].order` for both directions. Final rule is direction-aware in edit mode: moving left → target `others[position-1].order`; moving right → jumped-over `others[position-2].order`. This aligns exactly with backend `ReorderEnvironments` range semantics and prevents over-shift. |
| 2026-03-22 | copilot | Added **Project aggregate** as new hierarchy level: `Project` (aggregate root) owns `ProjectMember` entities with Role (Owner/Contributor/Reader). `InfrastructureConfig` gains a `ProjectId` FK — membership/RBAC checks now go through project, not infra config. Removed `Member` entity, `MemberId` VO from InfrastructureConfig aggregate. Created full CQRS stack (CreateProject, AddProjectMember, RemoveProjectMember, UpdateProjectMemberRole, GetProject, ListMyProjects, ListProjectConfigs). `IInfraConfigAccessService` now delegates to `IProjectAccessService`. New `ProjectController` at `/projects`. `GetInfrastructureConfigQueryHandler` now uses access service. Old member endpoints removed from `InfrastructureConfigController`. EF migration `AddProjectAggregate`: creates `Projects`/`project_members` tables, drops `infrastructureconfig_members`, adds `ProjectId` column + FK on `InfrastructureConfigs`. |
| 2026-03-22 | copilot | Added visual pipeline connector between environment cards in Environments tab: replaced `env-grid` (CSS grid) with `env-pipeline` (flex column), added `env-pipeline__connector` with a cyan `arrow_forward` icon (rotated 90°) between each card to visually convey deployment order flow. Removed `.order-chip` from card headers and SCSS (order is now implicit in the pipeline flow). |
| 2026-03-21 | copilot | Fixed environment order collision bug: added order shifting logic in the `InfrastructureConfig` aggregate root. `AddEnvironment` shifts existing envs with order >= new order up by 1 (`ShiftOrdersUp`). `UpdateEnvironment` reorders siblings when order changes (`ReorderEnvironments` — shifts range up or down depending on direction). `RemoveEnvironment` closes the gap by shifting envs with order > removed down by 1 (`ShiftOrdersDown`). This ensures order uniqueness as a domain invariant. |
| 2026-03-21 | copilot | Revamped navigation header: replaced dull dark `rgba(5,23,44,.78)` background with vibrant gradient `linear-gradient(135deg, #0d2f66, #1565c0, #0288d1)` matching login/home pages. Replaced inline SVG logo with `mat-icon cloud_circle` (added `MatIconModule` import). Updated brand icon to frosted glass style (`rgba(255,255,255,0.15)` bg, `#b3e5fc` icon color). Refreshed border to cyan glow (`rgba(0,188,212,0.3)`). Updated avatar, logout button, and action borders to use `rgba(255,255,255,*)` tones for consistency on the brighter gradient. |
| 2026-03-22 | copilot | Added **project-level environments & naming conventions with per-config inheritance**: **Domain** — new `ProjectEnvironmentDefinition` entity (OwnsMany on Project), `ProjectResourceNamingTemplate` entity (HasMany), `Project.DefaultNamingTemplate` property, full CRUD methods on Project aggregate (AddEnvironment, UpdateEnvironment, RemoveEnvironment with order shifting, SetDefaultNamingTemplate, SetResourceNamingTemplate, RemoveResourceNamingTemplate). `InfrastructureConfig` added `UseProjectEnvironments` and `UseProjectNamingConventions` booleans (default true) with setter methods. **Application** — CQRS commands: AddProjectEnvironment, UpdateProjectEnvironment, RemoveProjectEnvironment, SetProjectDefaultNamingTemplate, SetProjectResourceNamingTemplate, RemoveProjectResourceNamingTemplate (all with FluentValidation + IProjectAccessService write auth). SetInheritance command for toggling inheritance on InfraConfig. Updated ProjectResult, GetInfrastructureConfigResult, GetProjectQueryHandler (now uses GetByIdWithAllAsync). **Infrastructure** — updated ProjectConfiguration (OwnsMany environments with tags, HasMany naming templates, DefaultNamingTemplate conversion), new ProjectResourceNamingTemplateConfiguration, InfrastructureConfigConfiguration (UseProject* bool columns with default true), ProjectRepository.GetByIdWithAllAsync, ProjectDbContext (added DbSet<ProjectResourceNamingTemplate>). Migration `AddProjectEnvironmentsAndNaming`. **Contracts** — updated ProjectResponse/InfrastructureConfigResponse, new request DTOs (AddProjectEnvironmentRequest, UpdateProjectEnvironmentRequest, SetProjectDefaultNamingTemplateRequest, SetProjectResourceNamingTemplateRequest, SetInheritanceRequest). **API** — new endpoints on ProjectController (POST/PUT/DELETE environments, PUT default naming, PUT/DELETE resource naming), new PUT /infra-config/{id}/inheritance on InfrastructureConfigController. Updated Mapster mappings for ProjectEnvironmentDefinition/ProjectResourceNamingTemplate. **Frontend** — project-detail: 2 new tabs (Environments, Naming) with add/edit/remove dialogs, shared LocationEnum. config-detail: inheritance toggle (mat-slide-toggle) per tab with "Inherited from project" / "Custom override" visual indicators; when inherited, env/naming are read-only. Updated ProjectResponse/InfrastructureConfigResponse interfaces, ProjectService (env/naming CRUD), InfraConfigService.setInheritance(). i18n additions in both FR/EN for all new features. |
| 2026-03-22 | copilot | Moved default naming template auto-configuration from `CreateInfrastructureConfigCommandHandler` to `CreateProjectCommandHandler`. Now every new project gets: default template `{name}-{resourceAbbr}{suffix}`, ResourceGroup override `{resourceAbbr}-{name}{suffix}`, StorageAccount override `{name}{resourceAbbr}{suffix}`. InfraConfigs no longer auto-set naming on creation (they inherit from project). Added section 14.1 in MEMORY.md documenting naming conventions as critical project data. |
| 2026-03-22 | copilot | Added **delete project and delete configuration** with permission checks: **Backend** — `DeleteProjectCommand` + handler (Owner access via `IProjectAccessService.VerifyOwnerAccessAsync`), `DeleteInfrastructureConfigCommand` + handler (Owner access on parent project). New `DELETE /projects/{id}` and `DELETE /infra-config/{id}` endpoints. **Frontend** — `ProjectService.deleteProject()` and `InfraConfigService.delete()` service methods. Project-detail page: delete project button in header (visible only if `isOwner()`), delete config button on each config card (visible only if `isOwner()`), both with confirm dialog and navigation on success. Config-detail page: added `isOwner` computed (from parent project members), delete button in header (visible only if `isOwner()`), navigates to parent project on success. SCSS: `delete-btn` (header) and `config-card__delete-btn` (inline card) styles. i18n: `PROJECT_DETAIL.DELETE.*`, `PROJECT_DETAIL.DELETE_CONFIG.*`, `CONFIG_DETAIL.DELETE.*` keys in both FR/EN. |
| 2026-03-22 | copilot | Redesigned **home page as dashboard** + separate **/projects page** with search/sort/filter. **New files**: `features/projects/projects.component.{ts,html,scss}` (full project catalog with search, sort by name/members/favorites, filter favorites only, star toggle), `shared/services/favorites.service.ts` (localStorage favorites with signal API), `shared/services/recently-viewed.service.ts` (localStorage recent items, max 8). **Rewritten**: `features/home/home.component.{ts,html,scss}` (dashboard with greeting bar, quick actions, favorites panel, recently viewed panel, project overview grid). **Modified**: `app-routing.ts` (added `/projects` route), `navigation.component.html` (added Projects nav link), `project-detail.component.ts` + `config-detail.component.ts` (added RecentlyViewedService tracking), `en.json` + `fr.json` (replaced HOME section, added PROJECTS + NAV.PROJECTS keys). UX inspired by GitHub/Azure DevOps dashboards. |
| 2026-03-22 | copilot | Added **Bicep generation UI** on config-detail page. **New feature**: "Generate Bicep" CTA button in config header (green accent, terminal icon), calling `BicepGeneratorService.generate()`. Results displayed in a dark terminal-style panel with: loading state (animated cursor + blinking prompt), error state (retry button), success state (file tree showing `main.bicep`, `main.bicepparam`, and `modules/*.bicep` with open-in-new-tab links). Panel uses Dracula-inspired color scheme (`#0d1117` bg, `#50fa7b` green, `#8be9fd` cyan, `#ff79c6` pink). Slide-in animation on open, closeable. i18n: `CONFIG_DETAIL.BICEP.*` keys (12 keys) in both FR/EN. Increased `anyComponentStyle` budget to 24kB warning / 40kB error in `angular.json`. |
| 2026-03-22 | copilot | Fixed **Bicep generation 401 error** with proper multi-audience auth: BicepGenerator.Api has its own Azure AD app registration (ClientId `6960eaa6-...`, scope `api://6960eaa6-.../Generate`), distinct from the main API (ClientId `4f7f2dbd-...`). Frontend now acquires a **separate token** for the Bicep API via `MsalAuthService.getAccessTokenForScopes(bicepApiScopes)`. Added `bicepApiScopes` to `MsalConfigInterface` and all 3 environment files. `BicepGeneratorService` no longer uses `AxiosService` (which has a global interceptor for main-API tokens); instead it makes a direct `axios.post()` with the Bicep-specific Bearer token. **Pattern**: when calling APIs with different Azure AD app registrations, acquire separate tokens with `acquireTokenSilent` per-audience — never reuse a token across audiences. Prerequisite: the SPA app registration must have `api://6960eaa6-.../Generate` API permission configured. |
| 2026-03-22 | copilot | Fixed **HTTP/2 protocol error** on BicepGenerator.Api (`400 — An HTTP/1.x request was sent to an HTTP/2 only endpoint`). Root cause: Kestrel defaulted to HTTP/2-only on the HTTPS endpoint; requests from the Angular dev-server proxy (Node.js) and browsers always use HTTP/1.1 for proxied connections. Fix: (1) Configured Kestrel `ConfigureEndpointDefaults` with `HttpProtocols.Http1AndHttp2` in `Program.cs` to accept both protocols. (2) Removed `app.UseHttpsRedirection()` — unnecessary and harmful when running behind Aspire DCP proxy or Angular dev-server proxy (causes redirect to HTTPS endpoint which may reject HTTP/1.1). **Pattern**: always set `Http1AndHttp2` on APIs that receive browser or proxy traffic; never use `UseHttpsRedirection()` for APIs running behind reverse proxies. |
| 2026-03-22 | copilot | Added **Bicep download button**: after successful generation, a "Download all files" button in the terminal panel fetches all blob URLs, packages them into a zip (`{configName}-bicep.zip`) using JSZip, and triggers a browser download via file-saver. Files are organized as `main.bicep`, `main.bicepparam`, and `modules/*.bicep` inside the zip. Installed `jszip` + `file-saver` + `@types/file-saver`. Added `bicepDownloading` signal, `downloadBicepFiles()` method. Added `jszip`/`file-saver` to `allowedCommonJsDependencies`. Added `DOWNLOAD`/`DOWNLOADING` i18n keys in both FR/EN. SCSS: green accent pill button matching terminal Dracula theme. |
| 2026-03-22 | copilot | Fixed **CORS error on Bicep download** by adding server-side zip endpoint. Browser `fetch()` to Azurite blob storage (`127.0.0.1:dynamic-port`) was blocked by CORS. Solution: added `GET /generate-bicep/{configId}/download` endpoint on BicepGenerator.Api that reads blobs server-side, creates a zip using `System.IO.Compression.ZipArchive`, and returns it as `application/zip`. **Backend**: `DownloadBicepCommand` + `DownloadBicepCommandHandler` (lists blobs by prefix, picks latest timestamp folder, downloads and zips all files). Extended `IBlobService` with `DownloadContentAsync` and `ListBlobsAsync`, implemented in `BlobService`. **Frontend**: replaced JSZip client-side download with `BicepGeneratorService.downloadZip(configId)` calling the new endpoint. Removed `jszip` from `allowedCommonJsDependencies` (kept `file-saver` for `saveAs`). **Pattern**: never expose blob storage directly to the browser; always proxy through the API to avoid CORS issues with storage emulators and SAS token leakage. |
| 2026-03-22 | copilot | Added **Bicep file content viewer** in terminal panel. Clicking a file in the tree opens its content inline in a second Dracula-styled terminal below the file tree. **Backend**: `GetBicepFileContentQuery` + handler (reads single blob by config ID + file path, returns latest timestamp version). New `GET /generate-bicep/{configId}/files/{*filePath}` endpoint returning `{ content }`. **Frontend**: files changed from `<div>` to `<button>` (clickable, active state with cyan highlight), file viewer panel with loading/content/error states, toggle behavior (click again to close). `BicepGeneratorService.getFileContent()` method added. i18n: `FILE_LOADING`/`FILE_ERROR` keys in FR/EN. Bumped `anyComponentStyle` budget to 28kB warning / 48kB error. |
| 2026-03-22 | copilot | Added **per-environment resource configuration** with per-env `.bicepparam` file generation. **Domain**: new `ResourceEnvironmentConfig` entity on `AzureResource` with `EnvironmentName` + `Properties` (Dictionary<string,string>), new `ResourceEnvironmentConfigId` VO. `AzureResource` gains `SetEnvironmentConfig()`/`SetAllEnvironmentConfigs()` methods. `KeyVault.Create()`/`RedisCache.Create()`/`StorageAccount.Create()` accept optional environment configs. **Application**: new `EnvironmentConfigData` record, all Create/Update commands + handlers for KeyVault/RedisCache/StorageAccount updated to pass environment configs. **Infrastructure**: `ResourceEnvironmentConfigConfiguration` (jsonb Properties column, unique index on ResourceId+EnvironmentName), `AzureResourceConfiguration.HasMany(EnvironmentConfigs)`, new `DbSet<ResourceEnvironmentConfig>`. Migration `AddResourceEnvironmentConfigs`. **Contracts**: `ResourceEnvironmentConfigEntry` (request DTO) + `ResourceEnvironmentConfigResponse` (response DTO), added to all resource request/response contracts. **API Mapping**: Mapster configs updated for all 3 resource types. **BicepGenerator**: `ResourceDefinition.EnvironmentConfigs` property, `GenerationRequest.EnvironmentNames`, `GenerationResult.EnvironmentParameterFiles` (replaces single `MainBicepParameters`), `BicepAssembler.GenerateEnvironmentParameterFiles()` generates one `main.{envName}.bicepparam` per environment with property overrides merged. `GenerateBicepCommandHandler` uploads per-env param files. `GenerateBicepResult`/`GenerateBicepResponse` return `ParameterFileUris` dict. `InfrastructureConfigReadRepository` includes `EnvironmentConfigs` in read models. **Naming convention**: `main.{envName}.bicepparam` (e.g. `main.dev.bicepparam`, `main.staging.bicepparam`, `main.prod.bicepparam`). |
| 2026-03-22 | copilot | Redesigned **add-resource dialog** to **3-step per-environment flow**: Step 1 = resource type picker (KeyVault/RedisCache/StorageAccount), Step 2 = common fields (name, location), Step 3 = `MatTabGroup` with one tab per environment showing typed resource-specific form fields (SKU, capacity, TLS version, etc.) instead of generic key-value overrides. `FormArray<FormGroup>` holds per-env configs. Auto-prefill from first env on first tab visit, "Copy from first" button for convenience. `buildEnvironmentConfigs()` maps form values to `ResourceEnvironmentConfigEntry[]` with correct backend property keys (`skuName`, `skuCapacity`, `supportsHttpsTrafficOnly`, etc.). Updated `config-detail.component.ts` to pass effective environments to dialog (respecting project/config inheritance). Bicep terminal panel now shows `@for` loop of `main.{env}.bicepparam` entries from `parameterFileUris` dict. Frontend interfaces updated: `ResourceEnvironmentConfigEntry`/`ResourceEnvironmentConfigResponse` in shared interfaces, `parameterFileUris: Record<string, string>` in `bicep-generator.interface.ts`. Added 8+ i18n keys (`ENV_STEP_TITLE`, `ENV_NO_ENVIRONMENTS`, `COMMON_FIELDS`, `NEXT`, `BACK_TO_COMMON`, `COPY_FROM_FIRST`) in both FR/EN. |
| 2026-03-23 | copilot | Fixed **missing `.bicepparam` files in Bicep generation** when `UseProjectEnvironments = true` (default). Root cause: `InfrastructureConfigReadRepository.GetByIdWithResourcesAsync()` read environments from `config.EnvironmentDefinitions`, which is empty when inheritance is enabled — environments live on the `Project` entity. Fix: added conditional branch that loads `project.EnvironmentDefinitions` from `dbContext.Projects` when `config.UseProjectEnvironments` is true. **Pattern**: when resolving environments for Bicep generation, always check the inheritance flag and fall back to the parent project's environments. |
| 2026-03-23 | copilot | **Removed general/default resource configuration** — only `Name` and `Location` remain as top-level properties on `AzureResource`. All resource-specific configuration (SKU, Capacity, Kind, AccessTier, TLS, etc.) now lives **exclusively** in per-environment typed settings entities (`KeyVaultEnvironmentSettings`, `RedisCacheEnvironmentSettings`, `StorageAccountEnvironmentSettings`). **Backend**: removed general config properties from domain aggregates (`KeyVault.Sku`, `RedisCache.Sku/Capacity/RedisVersion/EnableNonSslPort/MinimumTlsVersion/MaxMemoryPolicy`, `StorageAccount.Sku/Kind/AccessTier/AllowBlobPublicAccess/EnableHttpsTrafficOnly/MinimumTlsVersion`), simplified `Create()`/`Update()` to only take `Name`+`Location`, cleaned all CQRS commands/handlers/results, contracts, Mapster mappings, EF Core configurations. Cleaned `BicepGenerator.InfrastructureConfigReadRepository.MapResource()` (empty general `properties` dict, removed `MapTlsVersion`/`MapStorageTlsVersion` helpers). Removed `Capacity` + SKU/capacity validation from `CreateRedisCacheRequest`/`UpdateRedisCacheRequest`. **Frontend**: removed general config from interfaces, simplified resource-edit General tab (only Name+Location), removed general config extraction from add-resource-dialog `onSubmit()`. **Dead code**: `RedisCacheSettings` and `StorageAccountSettings` value object records are now unused. **Note**: EF Core migration NOT yet created — column drops need a separate migration. |
| 2026-03-23 | copilot | Refactored **environment settings from generic Dictionary to typed per-resource entities** (Option C). **Domain**: replaced `ResourceEnvironmentConfig` (generic `Dictionary<string,string> Properties`) with 3 typed entities: `KeyVaultEnvironmentSettings` (nullable `Sku?`), `RedisCacheEnvironmentSettings` (nullable `Sku?`, `Capacity?`, `RedisVersion?`, `EnableNonSslPort?`, `MinimumTlsVersion?`, `MaxMemoryPolicy?`), `StorageAccountEnvironmentSettings` (nullable `Sku?`, `Kind?`, `AccessTier?`, `AllowBlobPublicAccess?`, `EnableHttpsTrafficOnly?`, `MinimumTlsVersion?`). Each entity has a `ToDictionary()` method for BicepGenerator compatibility. `AzureResource` no longer owns environment configs; each concrete resource type owns its typed settings list. **Application**: 3 typed `*EnvironmentConfigData` records, all Create/Update commands+handlers updated. **Contracts**: typed `*EnvironmentConfigEntry` (request) and `*EnvironmentConfigResponse` (response) per resource type. **Infrastructure**: 3 new EF Core configurations with unique index on `(ResourceId, EnvironmentName)`. **API Mapping**: per-resource Mapster configs for typed entries. **BicepGenerator**: `InfrastructureConfigReadRepository` loads typed settings per resource and uses `ToDictionary()` bridge. **Frontend**: typed TypeScript interfaces + `add-resource-dialog` builds typed entries per resource type. **Migration**: `TypedEnvironmentSettings` — creates 3 typed tables, drops FK from old `ResourceEnvironmentConfigs`. **Pattern**: nullable properties = "only override if non-null", `ToDictionary()` preserves BicepAssembler compatibility. |
| 2026-03-23 | copilot | Added **resource edit page and resource delete** in frontend. **New route**: `/config/:configId/resource/:resourceType/:resourceId` (lazy-loaded `ResourceEditComponent`). **New files**: `src/Front/src/app/features/resource-edit/resource-edit.component.{ts,html,scss}` — full resource management page with 4 tabs: General (editable form: name, location, type-specific fields), Environments (pill-based environment selector with per-env override forms), Identity & Access (placeholder with coming-soon panel for managed identity, RBAC, access policies), App Configuration (placeholder with coming-soon panel for env vars, secrets, per-env overrides for ACA/App Services/AZF). **Config-detail changes**: resource items now have edit (link to resource page) and delete (confirm dialog) action buttons. Injected `KeyVaultService`, `RedisCacheService`, `StorageAccountService` into config-detail for delete operations. Added `.resource-action-btn` styles. **i18n**: new `RESOURCE_EDIT.*` namespace (60+ keys) in both `fr.json` and `en.json`. Extended `CONFIG_DETAIL.RESOURCES` with `EDIT`, `DELETE`, `DELETE_CONFIRM_*`, `DELETE_ERROR` keys. **Design**: environment selector uses pill buttons (not tabs) for clear per-env switching. Form layout uses responsive grid (`16rem` min). Disabled future tabs show orange "Soon" badge with feature preview chips. |
| 2026-03-23 | copilot | Added **environment-aware Bicep naming with `types.bicep` and `functions.bicep`** generation. **Architecture**: Bicep output now generates 5 file categories: `types.bicep` (exported `EnvironmentName` union type, `EnvironmentVariables` type, `environments` variable map with `envName`/`envSuffix`/`envShortSuffix`/`envPrefix`/`envShortPrefix`/`location` per env), `functions.bicep` (exported naming functions derived from project naming templates — one default `BuildResourceName` + per-resource-type overrides like `BuildResourceGroupName`/`BuildStorageAccountName`), `main.bicep` (imports types+functions, uses `param environmentName EnvironmentName`, resolves `var env = environments[environmentName]`, names resource groups and modules via naming functions, location from `env.location`), per-env `.bicepparam` files (`param environmentName = '{env}'` + resource-specific params), and `modules/*.bicep` (unchanged). **Domain changes**: `EnvironmentDefinition` gains `Name`/`Prefix`/`Suffix`; new `NamingContext` record (DefaultTemplate, ResourceTemplates, ResourceAbbreviations); new `NamingTemplateTranslator` (converts `{name}-{resourceAbbr}{suffix}` → Bicep `'${name}-${resourceAbbr}${env.envSuffix}'`); `ResourceDefinition`/`ResourceGroupDefinition` gain `ResourceAbbreviation`; `GeneratedTypeModule` gains `LogicalResourceName`/`ResourceAbbreviation`/`ResourceTypeName`; `GenerationResult` gains `TypesBicep`/`FunctionsBicep`; `IResourceTypeBicepGenerator` interface simplified (no `EnvironmentDefinition` param, adds `ResourceTypeName` property). **BicepAssembler** fully rewritten: `GenerateTypesBicep()`, `GenerateFunctionsBicep()`, updated `GenerateMainBicep()` with imports/env/naming, updated `GenerateMainParameters()` with `environmentName` param. **Read model**: `EnvironmentDefinitionReadModel` gains `Prefix`/`Suffix`; new `NamingContextReadModel`; `InfrastructureConfigReadModel` gains `NamingContext`. **Repository**: `InfrastructureConfigReadRepository` now loads project naming templates (`Include(p => p.ResourceNamingTemplates)`) and config-level templates, resolves inheritance. **Handler**: `GenerateBicepCommandHandler` passes `NamingContext` + `Environments` to engine, uploads `types.bicep` and `functions.bicep`. **Frontend**: file tree shows `types.bicep` (purple badge) and `functions.bicep` (orange badge) before `main.bicep`. i18n: `TYPES`/`FUNCTIONS` keys in both FR/EN. **Convention**: suffix values include leading hyphen for standard resources (`-dev`) and raw value for storage accounts (`dev`); both available as `envSuffix`/`envShortSuffix` in the Bicep EnvironmentVariables type. |
| 2026-03-23 | copilot | Fixed **environment settings lost after save** on resource-edit page. Root cause: `KeyVaultRepository`, `RedisCacheRepository`, and `StorageAccountRepository` `GetByIdAsync` methods did NOT `.Include(x => x.EnvironmentSettings)`. This caused two bugs: (1) **Read path**: GET endpoint returned empty `environmentSettings` array because EF Core didn't populate the navigation property. (2) **Write path**: `SetAllEnvironmentSettings()` called `_environmentSettings.Clear()` on an empty collection (EF didn't load existing rows), then added new entries — orphaning old DB rows. Fix: added `.Include(x => x.EnvironmentSettings)` to `GetByIdAsync` and `GetByResourceGroupIdAsync` in all 3 repositories. **Pattern**: when an aggregate owns a child collection used in both read and write paths, always `.Include()` it in repository queries — otherwise `Clear()` + re-add will not properly delete old rows. |
| 2026-03-23 | copilot | Fixed **Bicep generation error ("Impossible de generer les fichiers Bicep")** — caused by MSAL silent token acquisition failure for the Bicep API scope (`api://6960eaa6.../Generate`). **Root cause**: `MsalAuthService.getAccessTokenForScopes()` only tried `acquireTokenSilent`; when the user hadn't previously consented to the Bicep API scope (or the cached token expired), it returned `null`, the service threw, and the generic error message was shown. **Fix**: (1) Added `acquireTokenPopup` fallback in `getAccessTokenForScopes()` when silent acquisition fails. (2) Improved `generateBicep()` error handling in `config-detail.component.ts` to distinguish auth errors (401/403 or missing token) from generation errors with specific i18n keys. (3) Added `GENERATE_AUTH_ERROR` i18n key in FR/EN. **Pattern**: for multi-API MSAL setups, always fall back to interactive token acquisition when silent fails for scopes that may not have been consented to yet. |
| 2026-03-23 | copilot | **Merged Bicep Generator API into main API** (Option A architecture). Eliminated the separate `BicepGenerator.Api` and `BicepGenerator.Infrastructure` projects. Kept `BicepGenerator.Domain/Application/Contracts` as separate csproj with dual-interface pattern. |
| 2026-03-23 | copilot | **Fully merged BicepGenerator.* into main API layers** — eliminated ALL separate BicepGenerator projects (Domain, Application, Contracts). **Domain**: Bicep generation classes moved to `InfraFlowSculptor.Domain/InfrastructureConfigAggregate/BicepGeneration/` (BicepGenerationEngine, BicepAssembler, type generators, naming helpers). **Application**: Commands (GenerateBicep, DownloadBicep) and Query (GetBicepFileContent) moved to `InfraFlowSculptor.Application/InfrastructureConfig/Commands|Queries/`. ReadModels moved to `InfrastructureConfig/ReadModels/`. `IInfrastructureConfigReadRepository` moved to main interfaces. Domain services registered directly in `AddApplication()`. **Contracts**: `GenerateBicepRequest`/`GenerateBicepResponse` moved to `InfraFlowSculptor.Contracts/InfrastructureConfig/`. **Infrastructure**: eliminated dual-interface pattern — `BlobService` implements single `IBlobService` (expanded with `UploadContentAsync`/`DownloadContentAsync`/`ListBlobsAsync`), `DateTimeProvider` implements single `IDateTimeProvider`. Simple direct registrations. **Deleted**: entire `src/BicepGenerators/` directory, removed from slnx. Build 0 errors. |
| 2026-03-23 | copilot | **Eliminated Shared projects + extracted BicepGeneration csproj**. Two architectural changes: (1) **Removed all Shared projects** (`Shared.Domain`, `Shared.Application`, `Shared.Infrastructure`, `Shared.Api`) — all contents moved into main API layers. DDD base classes (`AggregateRoot`, `Entity`, `ValueObject`, `Id`, `SingleValueObject`, `EnumValueObject`) → `InfraFlowSculptor.Domain/Common/Models/` (namespace `InfraFlowSculptor.Domain.Common.Models`). `IRepository<T>` → `InfraFlowSculptor.Application/Common/Interfaces/`. `BaseRepository<T,TContext>`, EF Core converters, extensions → `InfraFlowSculptor.Infrastructure/Persistence/`. Error handling, rate limiting, OpenAPI config, options → `InfraFlowSculptor.Api/`. Fixed inconsistent namespaces (`BicepGenerator.Domain.Common.Models`, `Shared.Domain.Domain.Models`, `Shared.Domain.Models` all unified to `InfraFlowSculptor.Domain.Common.Models`). Removed all Shared project references from csproj files, deleted `src/Shared/` directory, removed from slnx. (2) **Extracted Bicep generation** into `InfraFlowSculptor.BicepGeneration` csproj (namespace `InfraFlowSculptor.BicepGeneration`) — pure generation engine with no Domain/Infrastructure dependency. Contains: BicepGenerationEngine, BicepAssembler, type generators, naming helpers, all DTOs (GenerationRequest, GenerationResult, etc.). Referenced by Application only. Build 0 errors, 62 warnings (pre-existing). |
| 2026-03-23 | copilot | Fixed package-version mismatch warnings (`NU1608`) by removing obsolete `MediatR.Extensions.Microsoft.DependencyInjection` reference (v11 constraint) while keeping `MediatR` v13. Updated `InfraFlowSculptor.Application.csproj` and `Directory.Packages.props`. Full solution build now succeeds without NU1608 warnings (`dotnet build .\InfraFlowSculptor.slnx`), remaining warnings are nullable/XML pre-existing warnings. |
| 2026-03-23 | copilot | Fixed IDE error `Partial method 'Regex PlaceholderRegex()' must have an implementation part because it has accessibility modifiers` by replacing `GeneratedRegex` partial methods with static compiled `Regex` fields in `NamingTemplateTranslator` and `NamingTemplateValidator`. This removes IDE/compiler feature-version coupling while preserving behavior. |
| 2026-03-23 | copilot | Fixed IDE duplicate-type diagnostic (`Duplicate definition 'InfraFlowSculptor.Application.InfrastructureConfig.Common.NamingTemplateValidator'`) by declaring `NamingTemplateValidator` as `partial` to be compatible with analyzer/source-generated companion declarations. |
| 2026-03-23 | copilot | **Reorganized `InfraFlowSculptor.BicepGeneration` project** from flat root (16 files) into logical subfolders: `Models/` (7 DTOs: EnvironmentDefinition, GeneratedTypeModule, GenerationRequest, GenerationResult, NamingContext, ResourceDefinition, ResourceGroupDefinition), `Generators/` (5 files: IResourceTypeBicepGenerator, KeyVault/RedisCache/StorageAccount generators, ResourceGeneratorFactory), `Helpers/` (2 files: BicepIdentifierHelper, NamingTemplateTranslator). BicepAssembler and BicepGenerationEngine stay at root as entry points. Namespaces updated to match folder paths (`.Models`, `.Generators`, `.Helpers`). Updated usings in Application DependencyInjection.cs and GenerateBicepCommandHandler.cs. Full solution build 0 errors 0 warnings. |
| 2026-03-23 | copilot | Added **App Service Plan and Web App** aggregates with full CQRS stack. **Domain**: `AppServicePlan` extends `AzureResource` with `OsType` (Windows/Linux) + `AppServicePlanEnvironmentSettings` (Sku, Capacity). `WebApp` extends `AzureResource` with `AppServicePlanId` FK, `RuntimeStack` (DotNet/Node/Python/Java/Php), `RuntimeVersion`, `AlwaysOn`, `HttpsOnly` + `WebAppEnvironmentSettings` (per-env overrides). **Application**: Create/Update/Delete commands + handlers, Get queries, typed result records, `IAppServicePlanRepository`/`IWebAppRepository`. **Infrastructure**: EF Core TPT configs (tables `AppServicePlans`, `WebApps`, `AppServicePlanEnvironmentSettings`, `WebAppEnvironmentSettings`), repositories with `.Include(EnvironmentSettings)`, DI registration, `InfrastructureConfigReadRepository` updated for Bicep generation mapping. **Contracts**: typed request/response DTOs with `EnumValidation` attributes. **API**: Mapster mapping configs + Minimal API controllers at `/app-service-plan` and `/web-app`. **Bicep**: `AppServicePlanTypeBicepGenerator` (`Microsoft.Web/serverfarms`) and `WebAppTypeBicepGenerator` (`Microsoft.Web/sites`), registered in DI. **ResourceAbbreviationCatalog**: `AppServicePlan→asp`, `WebApp→app`. **Migration**: `AddAppServicePlanAndWebApp`. **Frontend**: interfaces, services (`AppServicePlanService`, `WebAppService`), enums (`OsTypeEnum`, `RuntimeStackEnum`, `AppServicePlanSkuEnum`), updated `resource-type.enum.ts` and `add-resource-dialog` with type-specific forms for both resources. i18n keys in FR/EN. |
| 2026-03-23 | copilot | Redesigned **WebApp creation UX** in `add-resource-dialog`: replaced manual App Service Plan ID text input with a guided multi-step flow. New `'plan-selection'` step lists existing App Service Plans from the resource group as clickable cards. `'create-plan'` step opens inline ASP creation form (name, location, OS type) that creates the plan via API and auto-selects it. Common step shows a read-only `.plan-indicator` with "Change" button. Added `ResourceGroupService` injection, plan loading/selection/creation signals and methods, `createPlanForm`, `onCreatePlanAndContinue()`. New SCSS: `.plan-picker`, `.plan-card`, `.plan-indicator`. Added 11 i18n keys (`PLAN_SELECT_*`, `CREATE_PLAN_*`, `SELECTED_PLAN`, `CHANGE_PLAN`) in both FR/EN. |
| 2026-03-23 | copilot | Implemented **Role Assignment management with managed identity** on the resource-edit page. **Backend**: Extended `AzureRoleDefinitionCatalog` with roles for `StorageAccount` (8 roles: Blob Data Contributor/Reader/Owner, Table Data Contributor/Reader, Queue Data Contributor/Reader, Account Contributor), `AppServicePlan` (3 roles: Website Contributor, Contributor, Reader), `WebApp` (3 roles: Website Contributor, Contributor, Reader). **Frontend**: Enabled the disabled "Identity & Access" tab in `resource-edit.component` — replaced placeholder with functional role assignment UI. New `add-role-assignment-dialog` component (3 files) with 2-step flow: Step 1 — target resource picker (cards showing all sibling resources from config's RGs, excluding self); Step 2 — configure identity type (SystemAssigned/UserAssigned radio) + role dropdown (loaded via `getAvailableRoleDefinitions(targetId)`) with description + doc link. Resource-edit component now injects `RoleAssignmentService` + `ResourceGroupService`, loads role assignments + all config resources on init, resolves target names/types and role names from loaded data. Role assignment cards show target resource icon+name+type, role name with shield icon, identity type badge, doc link, and remove button with confirmation dialog. Added 35+ i18n keys under `RESOURCE_EDIT.ROLE_ASSIGNMENTS.*` and `RESOURCE_EDIT.ADD_ROLE_DIALOG.*` in both FR/EN. |
| 2026-03-23 | copilot | Fixed **resource-edit page crash for AppServicePlan and WebApp** ("Impossible de charger cette ressource" with no HTTP error). Root cause: `resource-edit.component.ts` `loadResource()` switch had no `case 'AppServicePlan'` or `case 'WebApp'`, falling to `default: throw` caught silently. Fix: added full AppServicePlan and WebApp support to `resource-edit.component.ts` (imports, service injections, `loadResource` cases, `buildGeneralForm` type-specific fields, `buildSingleEnvForm` environment overrides, `onSave`/`deleteResource` logic, new `buildAppServicePlanEnvSettings()`/`buildWebAppEnvSettings()` methods). Updated `resource-edit.component.html`: added General tab sections (osType for ASP; runtimeStack/runtimeVersion/alwaysOn/httpsOnly for WebApp) and Environment tab `@case` blocks. Added `RESOURCE_EDIT.FIELDS.OS_TYPE/RUNTIME_STACK/RUNTIME_VERSION/ALWAYS_ON` i18n keys in both FR/EN. |
| 2026-03-23 | copilot | Added **UserAssignedIdentity** aggregate — simplest resource type with no per-environment settings. **Domain**: `UserAssignedIdentity` extends `AzureResource` (Name, Location only) + `Errors.UserAssignedIdentity`. **Application**: Create/Update/Delete commands + handlers + validator, Get query + handler, `UserAssignedIdentityResult`, `IUserAssignedIdentityRepository`. **Infrastructure**: EF Core TPT config (table `UserAssignedIdentities`), `UserAssignedIdentityRepository`, DbSet, DI registration. **Contracts**: Create/Update requests + `UserAssignedIdentityResponse`. **API**: Mapster mapping config + Minimal API controller at `/user-assigned-identity`. **Bicep**: `UserAssignedIdentityTypeBicepGenerator` (`Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31`) with `principalId`/`clientId` outputs. **Catalogs**: `ResourceAbbreviationCatalog` → `UserAssignedIdentity→id`. `AzureRoleDefinitionCatalog` → Managed Identity Operator + Contributor roles. `InfrastructureConfigReadRepository.MapResource` updated. **Migration**: `AddUserAssignedIdentity`. **Frontend**: `UserAssignedIdentityResponse` interface, `UserAssignedIdentityService` (CRUD), updated `ResourceTypeEnum` (+icon `fingerprint`, +abbr `id`), `add-resource-dialog` (direct submit without env step), `resource-edit.component` (load/save/delete cases), `config-detail.component` (delete case). No per-environment settings step since identity has no configurable properties. |
| 2026-03-23 | copilot | Added **UserAssignedIdentityId to RoleAssignment** for User-Assigned identity selection. **Backend**: `RoleAssignment` entity gains nullable `UserAssignedIdentityId` (AzureResourceId?). `AzureResource.AddRoleAssignment()` accepts optional `userAssignedIdentityId` parameter. `AddRoleAssignmentCommand`/`AddRoleAssignmentRequest`/`RoleAssignmentResponse`/`RoleAssignmentResult` updated with `UserAssignedIdentityId` field. Handler validates: (1) UA identity required when ManagedIdentityType is UserAssigned, (2) UA identity resource must exist. New errors: `Errors.RoleAssignment.UserAssignedIdentityRequired()` + `UserAssignedIdentityNotFound()`. EF Core config with inline nullable Guid conversion. Mapping config updated. Migration `AddUserAssignedIdentityIdToRoleAssignment`. **Frontend**: `AddRoleAssignmentRequest`/`RoleAssignmentResponse` interfaces updated with `userAssignedIdentityId?`. `add-role-assignment-dialog` enhanced: when "UserAssigned" is selected, shows scrollable identity picker listing all existing UserAssignedIdentity resources from config's resource groups (filtered from `siblingResources`) + inline creation form (name input → calls `UserAssignedIdentityService.create()`). `AddRoleAssignmentDialogData` extended with `resourceGroupId`/`configLocation`. `resource-edit.component` passes new data fields to dialog. `canSubmit` now requires `selectedIdentityId` when identity type is UserAssigned. i18n: 8 new keys (`IDENTITY_PICKER_LABEL`, `NO_IDENTITIES`, `CREATE_IDENTITY`, etc.) in both FR/EN. |
| 2026-03-23 | copilot | Added **FunctionApp** aggregate — full-stack CQRS resource type mirroring WebApp pattern, deployed on App Service Plans. **Backend** (24 new files, 8 modified): **Domain**: `FunctionApp` extends `AzureResource` with `AppServicePlanId` FK, `RuntimeStack` (DotNet/Node/Python/Java/PowerShell via `FunctionAppRuntimeStack`), `RuntimeVersion`, `HttpsOnly`. `FunctionAppEnvironmentSettings` entity with per-env overrides (HttpsOnly?, RuntimeStack?, RuntimeVersion?, MaxInstanceCount?, FunctionsWorkerRuntime?). `FunctionAppEnvironmentSettingsId` value object. **Application**: Create/Update/Delete commands + handlers + validators, Get query + handler, `FunctionAppResult`, `FunctionAppEnvironmentConfigData`, `IFunctionAppRepository`. **Infrastructure**: EF Core TPT config (table `FunctionApps` + `FunctionAppEnvironmentSettings`), `FunctionAppRepository` with `.Include(EnvironmentSettings)`, DbSet registrations. **Contracts**: `FunctionAppRequestBase`/`CreateFunctionAppRequest`/`UpdateFunctionAppRequest` + `FunctionAppResponse` + env config entry/response DTOs. **API**: `FunctionAppMappingConfig` (Mapster) + `FunctionAppController` at `/function-app`. **Bicep**: `FunctionAppTypeBicepGenerator` (`Microsoft.Web/sites@2023-12-01` with `kind: 'functionapp'`), factory key `Microsoft.Web/sites/functionapp`. **Catalogs**: `ResourceAbbreviationCatalog` → `FunctionApp→func`. `AzureRoleDefinitionCatalog` → Website Contributor + Contributor + Reader roles. `InfrastructureConfigReadRepository.MapResource` updated. **Migration**: `AddFunctionApp`. **Frontend**: `FunctionAppResponse`/`CreateFunctionAppRequest`/`UpdateFunctionAppRequest` interfaces, `FunctionAppService` (CRUD at `/function-app`), `FunctionAppRuntimeStackEnum` (DotNet/Node/Python/Java/PowerShell), `ResourceTypeEnum` updated (+icon `bolt`, +abbr `func`). `add-resource-dialog`: FunctionApp uses same plan-selection flow as WebApp (plan-selection → create-plan → common → environments). `resource-edit.component`: full load/save/delete/env-form support. `config-detail.component`: delete support (also fixed missing WebApp/AppServicePlan delete cases). i18n: `ADD_DIALOG_TITLE_FunctionApp`, `FUNCTIONS_WORKER_RUNTIME`, `MAX_INSTANCE_COUNT` keys in both FR/EN. |
| 2026-03-23 | copilot | Added **AppConfiguration** aggregate — full-stack CQRS resource type for Azure App Configuration (`Microsoft.AppConfiguration/configurationStores@2023-03-01`). **Backend** (22 new files, 8 modified): **Domain**: `AppConfiguration` extends `AzureResource` (Name, Location, no extra resource-level props). `AppConfigurationEnvironmentSettings` entity with per-env overrides (Sku?, SoftDeleteRetentionInDays?, PurgeProtectionEnabled?, DisableLocalAuth?, PublicNetworkAccess?). `AppConfigurationEnvironmentSettingsId` value object. **Application**: Create/Update/Delete commands + handlers + validators, Get query + handler, `AppConfigurationResult`, `AppConfigurationEnvironmentConfigData`, `IAppConfigurationRepository`. **Infrastructure**: EF Core TPT config (table `AppConfigurations` + `AppConfigurationEnvironmentSettings`), `AppConfigurationRepository` with `.Include(EnvironmentSettings)`, DbSet registrations. **Contracts**: `AppConfigurationRequestBase`/`CreateAppConfigurationRequest`/`UpdateAppConfigurationRequest` + `AppConfigurationResponse` + env config entry/response DTOs. **API**: `AppConfigurationMappingConfig` (Mapster) + `AppConfigurationController` at `/app-configuration`. **Bicep**: `AppConfigurationTypeBicepGenerator` (`Microsoft.AppConfiguration/configurationStores@2023-03-01`). **Catalogs**: `ResourceAbbreviationCatalog` → `AppConfiguration→appcs`. `AzureRoleDefinitionCatalog` → App Configuration Data Reader + Data Owner + Contributor roles. `InfrastructureConfigReadRepository.MapResource` updated. **Migration**: `AddAppConfiguration`. **Frontend** (2 new files, 8 modified): `AppConfigurationResponse`/`CreateAppConfigurationRequest`/`UpdateAppConfigurationRequest` interfaces, `AppConfigurationService` (CRUD at `/app-configuration`), `ResourceTypeEnum` updated (+icon `tune`, +abbr `appcs`). `add-resource-dialog`: simple flow like KeyVault (type → common → environments) with SKU (Free/Standard), SoftDeleteRetention, PurgeProtection toggle, DisableLocalAuth toggle, PublicNetworkAccess (Enabled/Disabled). `resource-edit.component`: full load/save/delete/env-form support. `config-detail.component`: delete support. i18n: `ADD_DIALOG_TITLE_AppConfiguration`, `SOFT_DELETE_RETENTION`, `PURGE_PROTECTION`, `DISABLE_LOCAL_AUTH`, `PUBLIC_NETWORK_ACCESS` keys in both FR/EN. |
| 2026-03-23 | copilot | Added **ContainerAppEnvironment** + **ContainerApp** aggregates — full-stack CQRS for Azure Container Apps. **Backend** (50 new files, 7 modified): **ContainerAppEnvironment Domain**: `ContainerAppEnvironment` extends `AzureResource` (simple, no extra resource-level props). `ContainerAppEnvironmentEnvironmentSettings` entity with per-env overrides (Sku?, WorkloadProfileType?, InternalLoadBalancerEnabled?, ZoneRedundancyEnabled?, LogAnalyticsWorkspaceId?). **ContainerApp Domain**: `ContainerApp` extends `AzureResource` with `ContainerAppEnvironmentId` FK (AzureResourceId). `ContainerAppEnvironmentSettings` entity with per-env overrides (ContainerImage?, CpuCores?, MemoryGi?, MinReplicas?, MaxReplicas?, IngressEnabled?, IngressTargetPort?, IngressExternal?, TransportMethod?). **Application** (both): Create/Update/Delete commands + handlers + validators, Get query + handler, typed Result/EnvironmentConfigData records, IRepository interfaces. Create handler for ContainerApp validates ContainerAppEnvironment exists. **Infrastructure**: EF Core TPT configs, repositories, DI registrations, DbSets. **Contracts**: RequestBase/Create/Update requests + Response + env config entry/response DTOs. ContainerApp contracts include `ContainerAppEnvironmentId`. **API**: MappingConfigs + Controllers at `/container-app-environment` and `/container-app`. **Bicep**: `ContainerAppEnvironmentTypeBicepGenerator` (`Microsoft.App/managedEnvironments@2024-03-01`) + `ContainerAppTypeBicepGenerator` (`Microsoft.App/containerApps@2024-03-01`). **Catalogs**: `ResourceAbbreviationCatalog` → `ContainerAppEnvironment→cae`, `ContainerApp→ca`. `AzureRoleDefinitionCatalog` → Container App Environment Contributor + ContainerApp Contributor + shared Contributor/Reader roles. **Migration**: `AddContainerAppEnvironmentAndContainerApp`. **Frontend** (4 new files, 8 modified): Interfaces + services for both types. `ResourceTypeEnum` updated (+icon `cloud_queue`/`view_in_ar`, +abbr `cae`/`ca`). `add-resource-dialog`: ContainerAppEnvironment uses simple flow (type → common → environments) with SKU (Consumption/Premium), WorkloadProfile (Consumption/D4/D8/D16/D32/E4/E8/E16/E32), LogAnalyticsWorkspaceId, InternalLoadBalancer toggle, ZoneRedundancy toggle. ContainerApp uses plan-selection flow (same as WebApp/FunctionApp) to pick existing ContainerAppEnvironment or create inline, then environments with ContainerImage, CPU (0.25-4.0), Memory (0.5-8.0Gi), MinReplicas, MaxReplicas, Transport (auto/http/http2/tcp), IngressTargetPort, IngressEnabled/IngressExternal toggles. `resource-edit.component`: full load/save/delete/env-form support for both. `config-detail.component`: delete support for both. i18n: all new keys in FR/EN. |
| 2026-03-24 | copilot | Added **CosmosDb** aggregate — full-stack CQRS for Azure Cosmos DB (`Microsoft.DocumentDB/databaseAccounts@2024-05-15`). **Backend** (27 new files, 7 modified): **Domain**: `CosmosDb` extends `AzureResource` (simple, no extra resource-level props). `CosmosDbEnvironmentSettings` entity with per-env overrides (DatabaseApiType?, ConsistencyLevel?, MaxStalenessPrefix?, MaxIntervalInSeconds?, EnableAutomaticFailover?, EnableMultipleWriteLocations?, BackupPolicyType?, EnableFreeTier?). `CosmosDbEnvironmentSettingsId` value object. **Application**: Create/Update/Delete commands + handlers + validators, Get query + handler, `CosmosDbResult`, `CosmosDbEnvironmentConfigData`, `ICosmosDbRepository`. **Infrastructure**: EF Core TPT config (table `CosmosDbAccounts` + `CosmosDbEnvironmentSettings` with unique index on `(CosmosDbId, EnvironmentName)`), `CosmosDbRepository` with `.Include(EnvironmentSettings)`, DbSet registrations. **Contracts**: `CosmosDbRequestBase`/`CreateCosmosDbRequest`/`UpdateCosmosDbRequest` + `CosmosDbResponse` + env config entry/response DTOs with 8 nullable per-env fields. **API**: `CosmosDbMappingConfig` (Mapster) + `CosmosDbController` at `/cosmos-db`. **Bicep**: `CosmosDbTypeBicepGenerator` (`Microsoft.DocumentDB/databaseAccounts@2024-05-15`) with kind mapping (SQL→GlobalDocumentDB, MongoDB→MongoDB) and capabilities (EnableMongo, EnableCassandra, EnableTable, EnableGremlin). **Catalogs**: `ResourceAbbreviationCatalog` → `CosmosDb→cosmos`. `AzureRoleDefinitionCatalog` → Cosmos DB Account Reader + DocumentDB Account Contributor + Cosmos DB Operator + Contributor + Reader roles. `InfrastructureConfigReadRepository.MapResource` updated. **Migration**: `AddCosmosDb`. **Frontend** (2 new files, 8 modified): `CosmosDbResponse`/`CreateCosmosDbRequest`/`UpdateCosmosDbRequest` interfaces, `CosmosDbService` (CRUD at `/cosmos-db`). `ResourceTypeEnum` updated (+icon `public`, +abbr `cosmos`). `add-resource-dialog`: simple flow (type → common → environments) with DatabaseApiType (SQL/MongoDB/Cassandra/Table/Gremlin), ConsistencyLevel (Eventual/ConsistentPrefix/Session/BoundedStaleness/Strong), MaxStalenessPrefix, MaxIntervalInSeconds, BackupPolicyType (Periodic/Continuous), EnableAutomaticFailover/EnableMultipleWriteLocations/EnableFreeTier toggles. `resource-edit.component`: full load/save/delete/env-form support. `config-detail.component`: delete support. i18n: all new keys in FR/EN. |
| 2026-03-24 | copilot | Added **LogAnalyticsWorkspace** + **ApplicationInsights** aggregates — full-stack CQRS for Azure monitoring resources (parent-child pair like AppServicePlan→WebApp). **Backend** (51 new files, 7 modified): **LogAnalyticsWorkspace Domain**: `LogAnalyticsWorkspace` extends `AzureResource` (simple, no extra resource-level props). `LogAnalyticsWorkspaceEnvironmentSettings` entity with per-env overrides (Sku?, RetentionInDays?, DailyQuotaGb?). **ApplicationInsights Domain**: `ApplicationInsights` extends `AzureResource` with `LogAnalyticsWorkspaceId` FK (AzureResourceId). `ApplicationInsightsEnvironmentSettings` entity with per-env overrides (SamplingPercentage?, RetentionInDays?, DisableIpMasking?, DisableLocalAuth?, IngestionMode?). **Application** (both): Create/Update/Delete commands + handlers + validators, Get query + handler, typed Result/EnvironmentConfigData records, IRepository interfaces. Create handler for ApplicationInsights validates LogAnalyticsWorkspace exists. **Infrastructure**: EF Core TPT configs, repositories, DI registrations, DbSets. **Contracts**: RequestBase/Create/Update requests + Response + env config entry/response DTOs. ApplicationInsights contracts include `LogAnalyticsWorkspaceId`. **API**: MappingConfigs + Controllers at `/log-analytics-workspace` and `/application-insights`. **Bicep**: `LogAnalyticsWorkspaceTypeBicepGenerator` (`Microsoft.OperationalInsights/workspaces@2023-09-01`) + `ApplicationInsightsTypeBicepGenerator` (`Microsoft.Insights/components@2020-02-02`). **Catalogs**: `ResourceAbbreviationCatalog` → `LogAnalyticsWorkspace→law`, `ApplicationInsights→appi`. `AzureRoleDefinitionCatalog` → Log Analytics Contributor/Reader + Application Insights Component Contributor + Monitoring Contributor/Reader roles. **Migration**: `AddLogAnalyticsWorkspaceAndApplicationInsights`. **Frontend** (4 new files, 8 modified): Interfaces + services for both types. `ResourceTypeEnum` updated (+icon `analytics`/`monitoring`, +abbr `law`/`appi`). `add-resource-dialog`: LogAnalyticsWorkspace uses simple flow with SKU (Free/PerGB2018/PerNode/Premium/Standard/Standalone/CapacityReservation), RetentionInDays, DailyQuotaGb. ApplicationInsights uses plan-selection flow (pick existing LogAnalyticsWorkspace or create inline), then environments with SamplingPercentage, RetentionInDays (30-730 dropdown), IngestionMode (ApplicationInsights/LogAnalytics/AppInsightsWithDiagnosticSettings), DisableIpMasking toggle, DisableLocalAuth toggle. `resource-edit.component`: full load/save/delete/env-form support for both. `config-detail.component`: delete support for both. i18n: all new keys in FR/EN. |
| 2026-03-25 | copilot | Added **SqlServer** + **SqlDatabase** aggregates — full-stack backend CQRS for Azure SQL (parent-child pair like AppServicePlan→WebApp). **Backend** (~52 new files, 7 modified): **SqlServer Domain**: `SqlServer` extends `AzureResource` with resource-level `Version` (SqlServerVersion: V12) + `AdministratorLogin` (string). `SqlServerEnvironmentSettings` entity with per-env `MinimalTlsVersion?`. `SqlServerEnvironmentSettingsId` value object. **SqlDatabase Domain**: `SqlDatabase` extends `AzureResource` with `SqlServerId` FK (AzureResourceId) + `Collation` (string, default `SQL_Latin1_General_CP1_CI_AS`). `SqlDatabaseEnvironmentSettings` entity with per-env `Sku?` (SqlDatabaseSku: Basic/Standard/Premium/GeneralPurpose/BusinessCritical/Hyperscale), `MaxSizeGb?`, `ZoneRedundant?`. **Application** (both): Create/Update/Delete commands + handlers + validators, Get query + handler, typed Result/EnvironmentConfigData records, IRepository interfaces. CreateSqlDatabase handler validates SqlServer exists. **Infrastructure**: EF Core TPT configs (tables `SqlServers`/`SqlServerEnvironmentSettings`/`SqlDatabases`/`SqlDatabaseEnvironmentSettings`), repositories, DI registrations, 4 DbSets. **Contracts**: RequestBase/Create/Update requests + Response + env config entry/response DTOs. SqlDatabase contracts include `SqlServerId`. **API**: MappingConfigs + Controllers at `/sql-server` and `/sql-database`. **Bicep**: `SqlServerTypeBicepGenerator` (`Microsoft.Sql/servers@2023-08-01-preview`) with `administratorLoginPassword` @secure param + `fullyQualifiedDomainName` output. `SqlDatabaseTypeBicepGenerator` (`Microsoft.Sql/servers/databases@2023-08-01-preview`) with parent reference. **Catalogs**: `ResourceAbbreviationCatalog` → `SqlServer→sql`, `SqlDatabase→sqldb`. `AzureRoleDefinitionCatalog` → SQL Server Contributor + SQL Security Manager + SQL DB Contributor + Contributor/Reader roles. `InfrastructureConfigReadRepository.MapResource` updated with both types. **Migration**: `AddSqlServerAndSqlDatabase`. Frontend not yet implemented. |
| 2026-03-24 | copilot | Added **resource category sections** in add-resource dialog type picker. Resources are now grouped into 4 categories: **Compute** (AppServicePlan, WebApp, FunctionApp, ContainerAppEnvironment, ContainerApp), **Storage & Databases** (StorageAccount, CosmosDb, RedisCache), **Security & Identity** (KeyVault, UserAssignedIdentity), **Monitoring & Configuration** (LogAnalyticsWorkspace, ApplicationInsights, AppConfiguration). New `RESOURCE_TYPE_CATEGORIES` constant + `ResourceTypeCategory` interface in `resource-type.enum.ts`. Updated `add-resource-dialog` HTML/TS/SCSS with category headers (icon + label). i18n: `CATEGORY_COMPUTE`, `CATEGORY_STORAGE_DB`, `CATEGORY_SECURITY`, `CATEGORY_MONITORING` keys in FR/EN. |
| 2026-03-24 | copilot | Added **backend-validated recently viewed items** on home dashboard. Previously, recent items were stored entirely in localStorage and would show deleted projects or projects the user lost access to. Now: **Backend** — new `POST /projects/validate-recent` endpoint (`ValidateRecentItemsQuery` + handler) that receives a list of `{ id, type }` entries, checks which projects/configs the user still has access to (via `IProjectRepository.GetAllForUserAsync` + `IInfrastructureConfigRepository.GetAllForUserAsync`), and returns only valid items with fresh name/description. New files: `ValidateRecentItemsRequest`/`RecentItemResponse` (Contracts), `ValidateRecentItemsQuery`/`ValidateRecentItemsQueryHandler`/`RecentItemResult` (Application). **Frontend** — `RecentlyViewedService` gains `validateAndRefresh()` method calling the new endpoint, which removes stale items and refreshes names from backend data. `HomeComponent.ngOnInit()` now calls `validateAndRefresh()` in parallel with `loadProjects()`. `ProjectService` gains `validateRecentItems()` method. New `RecentItemResponse`/`ValidateRecentItemsRequest` interfaces in `project.interface.ts`. |
| 2026-03-24 | copilot | Fixed **naming preview not showing** on config-detail Resource Groups tab when environments/naming are inherited from the project (`useProjectEnvironments = true` / `useProjectNamingConventions = true`). Root cause: `sortedEnvironments`, `previewEnv`, `resolveNamingPreview`, and initial `previewEnvId` selection all read from `config.environmentDefinitions` / `config.resourceNamingTemplates`, which are empty when inheritance is active. Fix: introduced `effectiveEnvironments` computed that resolves from project or config based on inheritance flag; `previewEnv` and `sortedEnvironments` now use `effectiveEnvironments`; `resolveNamingPreview` uses effective naming templates; `loadConfig` initial env selection uses effective envs. **Pattern**: always resolve effective environments/naming through inheritance check before using them in UI. |
