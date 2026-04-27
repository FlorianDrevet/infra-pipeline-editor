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

## Contracts Pattern

- Request: `[Required, GuidValidation]` on properties, prefer `string` + `GuidValidation` for body GUIDs
- Response: `record SomethingResponse(string Id, string Name, ...)`
- Validation attributes: `[GuidValidation]`, `[EnumValidation(typeof(MyEnum))]`, `[RedisVersionValidation]`
- JSON body GUID pitfall: prefer `string` + `[Required, GuidValidation]` over `Guid` for JSON bodies to avoid deserialization errors before validation

## Endpoint Conventions [2026-04-16]

- All protected endpoints must include `.ProducesProblem(401)` for accurate OpenAPI 401 documentation.
- ErrorOr extension (`ToErrorResult()`) returns **all** errors in the non-validation branch, not just the first.

## Response DTO Convention (API-002) [2026-04-16]

- All response DTO ID fields use `string` (not `Guid`). Mapster config maps `Id.Value.ToString()`.
- This applies to all 18 resource responses, project/member responses, infra-config responses, and sub-resource responses.
- `GET /resource-group/{id}/resources` may now enrich `AzureResourceResponse` with optional `StorageSubResources` (blob containers, queues, tables) so `config-detail` can render Storage Account children on the first list payload without calling `GET /storage-accounts/{id}` for each account.

## Wildcard File Paths [2026-04-23]

- Endpoints exposing `/{*filePath}` must validate and normalize the route value with `SafeRelativePath.TryNormalize(...)` before dispatching to MediatR or blob storage lookups.
- Reject `..`, leading slash, drive letters, and absolute paths at the controller boundary with `400 Bad Request`; do not leave path traversal filtering to downstream handlers.

## Tag Validation [2026-04-16]

- Azure tag limits enforced: key max 512 chars, value max 256 chars, max 15 tags per entity.
- Validated in `SetInfraConfigTagsCommandValidator` and `SetProjectTagsCommandValidator`.

## Mapster Mappings

- Implement `IRegister`, live in `src/Api/InfraFlowSculptor.Api/Common/Mapping/`
- Value objects → primitives: `.MapWith(src => src.Value)`
- **Nullable null checks:** use `x != null` directly — never `(object?)x`, never `is not null` (CS8122 in expression trees)
- Lightweight resource-group mappings must explicitly carry `IsExisting` on `AzureResourceResult -> AzureResourceResponse`; if omitted, Angular list badges and generation preflight diagnostics misclassify existing resources as missing environment configuration.
