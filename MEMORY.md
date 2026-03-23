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
| `Project` | `Project` | `ProjectMember`, `ProjectEnvironmentDefinition`, `ProjectResourceNamingTemplate` | Groups InfrastructureConfigs; owns membership/RBAC, default environments, naming conventions |
| `InfrastructureConfig` | `InfrastructureConfig` | `EnvironmentDefinition`, `ParameterDefinition`, `ResourceParameterUsage`, `EnvironmentParameterValue` | Has `ProjectId` FK to Project |
| `ResourceGroup` | `ResourceGroup` | `AzureResource` (base), `InputOutputLink`, `ResourceEnvironmentConfig` | Hosts Azure resources |
| `KeyVault` | `KeyVault` extends `AzureResource` | — | TPT in EF Core |
| `RedisCache` | `RedisCache` extends `AzureResource` | — | TPT in EF Core |
| `StorageAccount` | `StorageAccount` extends `AzureResource` | — | TPT in EF Core |
| `User` | `User` | — | Azure AD user info |

### 3.2 Value Objects

All value objects inherit from `Shared.Domain` base classes:
- `Id<T>` — base for ID types; supports `new TId(Guid)` and `TId.Create(Guid)`
- `SingleValueObject<T>` — wraps a single primitive
- `EnumValueObject<TEnum>` — wraps an enum
- `ValueObject` — base with structural equality

Key value objects per aggregate:
- **Project:** `ProjectId`, `ProjectMemberId`, `ProjectEnvironmentDefinitionId`, `ProjectResourceNamingTemplateId`
- **InfrastructureConfig:** `InfrastructureConfigId`, `Role` (Owner/Contributor/Reader), `EnvironmentDefinitionId`, `ParameterDefinitionId`, `ParameterType`, `Prefix`, `Suffix`, `Order`, `IsSecret`, `TenantId`, `SubscriptionId`, `RequiresApproval`
- **ResourceGroup:** `ResourceGroupId`, `Name`, `Location`
- **AzureResource:** `AzureResourceId`, `Name`
- **ResourceEnvironmentConfig:** `ResourceEnvironmentConfigId` — per-environment property overrides on an `AzureResource` (stored as JSON `Properties` column)
- **KeyVault:** `Sku` (enum: Premium/Standard)
- **RedisCache:** `RedisCacheSku`, `TlsVersion`, `MaxMemoryPolicy`, `RedisCacheSettings`
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
| `/keyvault` | GET/POST/PUT/DELETE | `/{id:guid}` | Key Vault CRUD |
| `/resource-group` | GET/POST | `/{id:guid}` | Resource Group CRUD |
| `/redis-cache` | GET/POST/PUT/DELETE | `/{id:guid}` | Redis Cache CRUD |
| `/generate-bicep` | POST | `` | `GenerateBicepCommand` |
| `/generate-bicep` | GET | `/{configId:guid}/download` | `DownloadBicepCommand` (returns zip) |
| `/generate-bicep` | GET | `/{configId:guid}/files/{*filePath}` | `GetBicepFileContentQuery` (returns JSON `{ content }`) |

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
