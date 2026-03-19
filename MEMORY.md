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
├── Front/                                  Angular frontend (UI + API consumption)
│   ├── src/app/core                        Layout components (navigation/footer)
│   ├── src/app/shared                      Cross-cutting frontend services/guards/facades
│   ├── src/environments                    API base URL and runtime environment config
│   └── src/scss                            Global theming variables and style modules
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

## 14. BicepGenerator Service

- Separate ASP.NET Core API at `src/BicepGenerators/`
- Called from main API via Refit client (`IBicepGeneratorClient`)
- Reads infrastructure config from **shared PostgreSQL DB** (read-only)
- Generates Bicep files per environment, uploads to Azure Blob Storage
- Uses **strategy pattern**: `IResourceTypeBicepGenerator` per Azure resource type
- New resource types require: a new `IResourceTypeBicepGenerator` implementation + registration in `BicepGenerator.Application/DependencyInjection.cs`

---

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

#### Feature structure
- Feature pages live under `src/Front/src/app/features/<feature-name>/`
- The login component is the first entry in `features/`

#### App Registration (Azure Portal) required settings

For clientId `24c34231-a984-43b3-8ac3-9278ebd067ef`:
1. **Authentication → Platform → Single-page application (SPA)**
2. **Redirect URIs:** `http://localhost:4200` (dev) + production URL
3. **Implicit grant → unchecked** (PKCE is used automatically for SPA)
4. **API permissions:** Microsoft Graph → `openid`, `profile`, `email` (delegated)
5. **Supported account types:** single-tenant (or multitenant as needed)

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
| 2026-03-19 | copilot | **Frontend infrastructure configs UI**: created feature `src/Front/src/app/features/infrastructure-configs/` with list page (cards grid), details page (3-tabs view: Resources, Environments, Members). Enriched `InfraConfigService` and `ResourceGroupService` with Angular signals for reactive state management (`configurations`, `currentConfig`, `isLoading`, `isLoadingDetails`). Added methods `loadConfigurations()` and `loadConfigDetails(id)`. Updated routing: added `/configs` (list) and `/configs/:id` (details) as authenticated children routes. Created reusable `ConfigCardComponent` displaying resource count, environment count, member count with avatars and 3-column stat layout. Built `ConfigDetailsComponent` with `mat-tabs` showing: Resources tab (resource groups table), Environments tab (environment cards with details), Members tab (team members with role chips and actions). No dashboard changes yet; feature is routing-isolated. TypeScript typecheck passes. |
| 2026-03-19 | copilot | **UI Component Library** (`src/Front/src/app/shared/ui-library/`): created reusable component library with Atomic Design pattern. **Atoms** (simple, autonomous): `AvatarComponent` (initials circle with size/variant), `StatItemComponent` (icon+value+label), `EmptyStateComponent` (icon+title+description+action), `LoadingSpinnerComponent` (spinner+message). **Molecules** (composite, reusable): `CardComponent` (flexible card with clickable mode and variants: default/outlined/elevated), `DataTableComponent` (dynamic table with column definitions and nested property support), `TabbedViewComponent` (Material tabs wrapper). All components: standalone, OnPush, files separated (*.ts, *.html, *.scss). Barrel export in `index.ts`. Infrastructure-configs feature now uses this library instead of inline components. READMEs created for both library and feature. TypeScript typecheck passes. |
