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

## Mapster Mappings

- Implement `IRegister`, live in `src/Api/InfraFlowSculptor.Api/Common/Mapping/`
- Value objects → primitives: `.MapWith(src => src.Value)`
- **Nullable null checks:** use `x != null` directly — never `(object?)x`, never `is not null` (CS8122 in expression trees)
