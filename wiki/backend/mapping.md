# Mapping (Mapster)

## Strategy

InfraFlowSculptor uses **Mapster** for object-to-object mapping between:
- Request DTOs → Commands/Queries
- Domain entities → Response DTOs
- Domain models → Generation DTOs

---

## Registration

Mapster configurations are registered centrally in the API project:

```csharp
// Program.cs
services.AddMapster();

// Scan all assemblies for IRegister implementations
TypeAdapterConfig.GlobalSettings.Scan(
    typeof(Program).Assembly,          // Api
    typeof(ICommand<>).Assembly,       // Application
    typeof(Project).Assembly           // Domain
);
```

---

## Configuration Pattern

```csharp
public sealed class ContainerAppMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ContainerApp, ContainerAppResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceGroupId, src => src.ResourceGroupId.Value.ToString());

        config.NewConfig<CreateContainerAppRequest, CreateContainerAppCommand>();
    }
}
```

---

## Critical Conventions

### Response DTO IDs: Always `string`

```csharp
// ✅ Correct
public sealed record ContainerAppResponse(string Id, string Name, ...);

// ❌ Wrong — typed IDs cannot serialize to JSON
public sealed record ContainerAppResponse(AzureResourceId Id, ...);
```

Mapster maps `Id.Value.ToString()` automatically when configured.

### Null Checks: Use `x != null`

```csharp
// ✅ Correct in Mapster expression trees
.Map(dest => dest.Registry, src => src.Registry != null ? src.Registry.Name : null)

// ❌ CS8122 — pattern matching not supported in expression trees
.Map(dest => dest.Registry, src => src.Registry is not null ? ...)

// ❌ CS8122 — cast workaround doesn't compile
.Map(dest => dest.Registry, src => (object?)src.Registry != null ? ...)
```

### Implicit Conversions

When the mapping is trivial (same property names and types), no config is needed:

```csharp
// Auto-maps if property names match
config.NewConfig<UpdateContainerAppRequest, UpdateContainerAppCommand>();
```

---

## Request → Command Mapping (API Layer)

```csharp
// In Minimal API endpoint
app.MapPost("/container-apps", async (
    CreateContainerAppRequest request,
    ISender sender) =>
{
    var command = request.Adapt<CreateContainerAppCommand>();
    var result = await sender.Send(command);
    return result.ToApiResult();
});
```

---

## Entity → Response Mapping (Query Handler)

```csharp
// In read repository (projection at DB level)
return await _context.ContainerApps
    .AsNoTracking()
    .Where(x => x.Id == id)
    .ProjectToType<ContainerAppResponse>() // Mapster projection
    .FirstOrDefaultAsync(cancellationToken);
```

`ProjectToType<T>()` generates the SQL projection directly, avoiding full entity materialization.
