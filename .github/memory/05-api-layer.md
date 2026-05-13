# API Layer

## Endpoint Registration Pattern

Controllers are **static extension methods** in `src/Api/InfraFlowSculptor.Api/Controllers/`:
```csharp
public static IApplicationBuilder UseXyzController(this IApplicationBuilder builder)
{
    return builder.UseEndpoints(endpoints =>
    {
        var group = endpoints.MapGroup("/route").WithTags("Tag");
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) => ...);
    });
}
```
Registered in `Program.cs` via `app.UseXyzController()`.

## Error Conversion

Handlers return `ErrorOr<T>`. In controllers:
```csharp
result.Match(
    value => Results.Ok(mapper.Map<Response>(value)),
    errors => errors.ToErrorResult()
);
```

## Request Body Limits [2026-05-12]

- `Program.cs` now registers `AddApiRequestLimits(builder.Configuration)`.
- `ApiRequestLimitsOptions` binds the `RequestLimits` section and defaults `MaxRequestBodySizeBytes` to `52_428_800` (50 MB).
- `AddApiRequestLimits(...)` projects that typed option into `KestrelServerOptions.Limits.MaxRequestBodySize` and validates the configuration at startup.
- Focused coverage lives in `tests/InfraFlowSculptor.Api.Tests/Security/RequestLimitsServiceCollectionExtensionsTests.cs`.

## Contracts Pattern

- Request: `[Required, GuidValidation]` on properties, prefer `string` + `GuidValidation` for body GUIDs
- Response: `record SomethingResponse(string Id, string Name, ...)`
- Validation attributes: `[GuidValidation]`, `[EnumValidation(typeof(MyEnum))]`, `[RedisVersionValidation]`
- `[EnumValidation(typeof(MyEnum))]` accepts enum names case-insensitively when the incoming value is a string; keep `[Required]` responsible for null rejection.
- JSON body GUID pitfall: prefer `string` + `[Required, GuidValidation]` over `Guid` for JSON bodies to avoid deserialization errors before validation
- `AzureRoleDefinitionResponse` now carries `RequiresUserAssignedIdentity` so Angular role-assignment screens derive AcrPull-like identity constraints from backend metadata instead of hardcoded role-definition GUID checks.

## Endpoint Conventions [2026-04-16]

- All protected endpoints must include `.ProducesProblem(401)` for accurate OpenAPI 401 documentation.
- Repo-wide verification on 2026-05-12 found `179/179` controller endpoint blocks (`MapGet`/`MapPost`/`MapPut`/`MapDelete`/`MapPatch`) in `InfraFlowSculptor.Api` already document `.ProducesProblem(StatusCodes.Status401Unauthorized)`; future API-001 follow-ups should re-check the branch state before reopening the finding.
- ErrorOr extension (`ToErrorResult()`) returns **all** errors in the non-validation branch, not just the first.
- Route names must live in per-controller constants files under `src/Api/InfraFlowSculptor.Api/Controllers/Constants/` (pattern: `<ControllerBaseName>RouteNames.cs`). Controllers must not inline literals in `.WithName(...)` or `CreatedAtRoute(routeName: ...)`; `ControllerRouteNameConstantsTests` in `tests/InfraFlowSculptor.Api.Tests` enforces this for future controllers.

## Shared Dependents Endpoint [2026-05-12]

- The generic `/{id:guid}/dependents` Minimal API block is now centralized in `Controllers/Common/DependentResourcesEndpointMapper.cs`.
- When a controller exposes the standard dependent-resources flow, prefer `group.MapDependentResourcesEndpoint(routeName, resourceDisplayName)` over duplicating the `GetDependentResourcesQuery` + `ErrorOr` mapping block inline.

## Response DTO Convention (API-002) [2026-04-16]

- All response DTO ID fields use `string` (not `Guid`). Mapster config maps `Id.Value.ToString()`.
- This applies to all 18 resource responses, project/member responses, infra-config responses, and sub-resource responses.
- `GET /resource-group/{id}/resources` may now enrich `AzureResourceResponse` with optional `StorageSubResources` (blob containers, queues, tables) so `config-detail` can render Storage Account children on the first list payload without calling `GET /storage-accounts/{id}` for each account.

## Wildcard File Paths [2026-04-23]

- Endpoints exposing `/{*filePath}` must validate and normalize the route value with `SafeRelativePath.TryNormalize(...)` before dispatching to MediatR or blob storage lookups.
- Reject `..`, leading slash, drive letters, and absolute paths at the controller boundary with `400 Bad Request`; do not leave path traversal filtering to downstream handlers.

## Tag Validation [2026-04-16]

- Azure tag limits enforced: key max 512 chars, value max 256 chars, max 15 tags per entity.
- Contract-layer limits are now centralized in `TagRequestConstraints` and enforced on request DTOs via `TagRequest` string-length attributes plus `MaxCollectionCountAttribute` on `SetProjectTagsRequest`, `SetInfraConfigTagsRequest`, `AddProjectEnvironmentRequest`, and `UpdateProjectEnvironmentRequest`.
- Application validators (`SetInfraConfigTagsCommandValidator`, `SetProjectTagsCommandValidator`) still validate the command-layer equivalents.

## Mapster Mappings

- Implement `IRegister`, live in `src/Api/InfraFlowSculptor.Api/Common/Mapping/`
- Value objects → primitives: `.MapWith(src => src.Value)`
- **Nullable null checks:** use `x != null` directly — never `(object?)x`, never `is not null` (CS8122 in expression trees)
- Lightweight resource-group mappings must explicitly carry `IsExisting` on `AzureResourceResult -> AzureResourceResponse`; if omitted, Angular list badges and generation preflight diagnostics misclassify existing resources as missing environment configuration.
- When a feature mapping config starts accumulating many `NewConfig` registrations or repeated nullable list projections, split `Register(TypeAdapterConfig)` into focused private registration methods and reusable collection-projection helpers rather than leaving one monolithic `Register` method behind a Sonar suppression.
