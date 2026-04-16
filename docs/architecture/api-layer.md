# Couche API — Endpoints, Mapping et Erreurs

## Vue d'ensemble

La couche API (`InfraFlowSculptor.Api`) est le point d'entrée HTTP de l'application. Elle a trois responsabilités :

1. **Définir les endpoints HTTP** (routes, méthodes, paramètres)
2. **Mapper les DTOs** (Request → Command/Query, Result → Response)
3. **Convertir les erreurs** (`ErrorOr` → HTTP status codes)

Le projet utilise les **Minimal APIs** d'ASP.NET Core (pas de controllers MVC).

---

## Endpoints — Pattern de définition

Les endpoints sont définis comme des **méthodes d'extension statiques** dans `src/Api/InfraFlowSculptor.Api/Controllers/` :

```
Fichier : src/Api/InfraFlowSculptor.Api/Controllers/KeyVaultController.cs
```

```csharp
public static class KeyVaultController
{
    public static IApplicationBuilder UseKeyVaultControllerController(
        this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/keyvault")
                .WithTags("KeyVaults");

            // GET /keyvault/{id}
            group.MapGet("/{id:guid}", async (
                Guid id,
                ISender sender,
                IMapper mapper) =>
            {
                var query = new GetKeyVaultQuery(AzureResourceId.Create(id));
                var result = await sender.Send(query);
                return result.Match(
                    value => Results.Ok(mapper.Map<KeyVaultResponse>(value)),
                    errors => errors.ToErrorResult());
            });

            // POST /keyvault
            group.MapPost("", async (
                [FromBody] CreateKeyVaultRequest request,
                ISender sender,
                IMapper mapper) =>
            {
                var command = mapper.Map<CreateKeyVaultCommand>(request);
                var result = await sender.Send(command);
                return result.Match(
                    value => Results.Ok(mapper.Map<KeyVaultResponse>(value)),
                    errors => errors.ToErrorResult());
            });

            // PUT /keyvault/{id}
            group.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateKeyVaultRequest request,
                ISender sender,
                IMapper mapper) =>
            {
                var command = mapper.Map<UpdateKeyVaultCommand>((id, request));
                var result = await sender.Send(command);
                return result.Match(
                    value => Results.Ok(mapper.Map<KeyVaultResponse>(value)),
                    errors => errors.ToErrorResult());
            });
        });
    }
}
```

Les endpoints sont ensuite enregistrés dans `Program.cs` :
```csharp
app.UseKeyVaultControllerController();
app.UseResourceGroupController();
app.UseRedisCacheController();
// ... un appel par contrôleur
```

### Conventions

- **Un fichier par agrégat** : `KeyVaultController.cs`, `StorageAccountController.cs`, etc.
- **Méthode statique** : pas de classe instanciée, pas d'injection dans un constructeur
- **Injection par paramètre** : `ISender sender`, `IMapper mapper` sont injectés directement dans le lambda
- **`MapGroup()`** : regroupe les routes sous un même préfixe (ex: `/keyvault`)
- **`WithTags()`** : organise les endpoints dans Swagger/OpenAPI

---

## Mapping avec Mapster

Le projet utilise **Mapster** pour convertir entre les différentes couches :

```
Request DTO (Contracts) ───▶ Command/Query (Application)
                                    │
                               Handler traitement
                                    │
                             Result DTO (Application)
                                    │
Domain Entity ◀─────────────────────┘
      │
      ▼
Response DTO (Contracts) ───▶ HTTP Response
```

### Configuration des mappings

Les mappings sont configurés dans des classes `IRegister` dans `src/Api/InfraFlowSculptor.Api/Common/Mapping/` :

```
Fichier : src/Api/InfraFlowSculptor.Api/Common/Mapping/KeyVaultMappingConfig.cs
```

```csharp
public class KeyVaultMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Request → Command
        config.NewConfig<CreateKeyVaultRequest, CreateKeyVaultCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec =>
                        new KeyVaultEnvironmentConfigData(ec.EnvironmentName, ec.Sku))
                      .ToList());

        // Update avec ID de la route + body
        config.NewConfig<(Guid Id, UpdateKeyVaultRequest Request), UpdateKeyVaultCommand>()
            .MapWith(src => new UpdateKeyVaultCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                // ... map environment settings
            ));

        // Domain → Application Result
        config.NewConfig<KeyVault, KeyVaultResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es =>
                    new KeyVaultEnvironmentConfigData(
                        es.EnvironmentName,
                        (object?)es.Sku != null ? es.Sku.Value.ToString() : null))
                    .ToList());

        // Value Object mappings
        config.NewConfig<Sku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, Sku>()
            .MapWith(src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}
```

### Enregistrement automatique

Tous les `IRegister` sont découverts par scan d'assembly dans `Common/Mapping/DependencyInjection.cs` :
```csharp
public static IServiceCollection AddMapping(this IServiceCollection services)
{
    var config = TypeAdapterConfig.GlobalSettings;
    config.Scan(Assembly.GetExecutingAssembly());

    services.AddSingleton(config);
    services.AddScoped<IMapper, ServiceMapper>();
    return services;
}
```

**Il n'y a rien d'autre à faire** pour ajouter un nouveau mapping : créer un fichier `IRegister` dans le dossier `Common/Mapping/` et il est automatiquement pris en compte.

### Conventions de mapping

| Conversion | Technique |
|-----------|-----------|
| Value Object → primitif | `.MapWith(src => src.Value)` |
| ID → string (réponse HTTP) | `.Map(dest => dest.Id, src => src.Id.Value.ToString())` |
| Enum VO → string | `.MapWith(src => src.Value.ToString())` |
| String → Value Object | `.MapWith(src => new MyVO(src))` |
| Tuple `(Guid, Request)` → Command | `.MapWith(src => new Command(src.Id.Adapt<MyId>(), ...))` |

---

## Gestion des erreurs

### Conversion ErrorOr → HTTP

Dans chaque endpoint, le résultat du handler est converti en réponse HTTP via `result.Match()` :

```csharp
result.Match(
    value => Results.Ok(mapper.Map<KeyVaultResponse>(value)),   // Succès → 200 OK
    errors => errors.ToErrorResult()                            // Erreurs → HTTP approprié
);
```

`ToErrorResult()` est une extension dans `InfraFlowSculptor.Api.Errors` qui convertit les types d'erreur `ErrorOr` en status codes HTTP :

| Type d'erreur | HTTP Status |
|---------------|-------------|
| `Error.Validation` | 400 Bad Request |
| `Error.NotFound` | 404 Not Found |
| `Error.Forbidden` | 403 Forbidden |
| `Error.Conflict` | 409 Conflict |
| `Error.Unexpected` | 500 Internal Server Error |

**Comportement multi-erreurs :**
- Si **toutes** les erreurs sont de type `Validation` → `400 ValidationProblem` avec le dictionnaire complet des erreurs par champ.
- Si **au moins une** erreur est non-Validation → `400 Bad Request` avec **toutes** les erreurs dans le corps : `{ "errors": [{ "code": "...", "description": "..." }, ...] }`.

Cela garantit qu'aucune erreur n'est silencieusement ignorée.

### Middleware d'erreur global

En complément, un middleware global (`UseErrorHandling()`) capture les exceptions non gérées et retourne un `ProblemDetails` standardisé.

---

## Contracts — Les DTOs HTTP

Les DTOs de requête et réponse vivent dans le projet **`InfraFlowSculptor.Contracts`** :

```
src/Api/InfraFlowSculptor.Contracts/
├── KeyVaults/
│   ├── Requests/
│   │   ├── CreateKeyVaultRequest.cs
│   │   └── UpdateKeyVaultRequest.cs
│   └── Responses/
│       ├── KeyVaultResponse.cs
│       └── KeyVaultEnvironmentConfigResponse.cs
├── Projects/
│   ├── Requests/
│   └── Responses/
└── ...
```

### Exemple de Request

```csharp
public class CreateKeyVaultRequest
{
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Location { get; init; }

    public List<KeyVaultEnvironmentConfigRequest>? EnvironmentSettings { get; init; }
}
```

### Exemple de Response

```csharp
public record KeyVaultResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    List<KeyVaultEnvironmentConfigResponse> EnvironmentSettings);
```

### Attributs de validation personnalisés

| Attribut | Rôle |
|----------|------|
| `[GuidValidation]` | Valide le format GUID, rejette `Guid.Empty` |
| `[EnumValidation(typeof(MyEnum))]` | Valide que la string correspond à une valeur de l'enum |

> **⚠️ Attention :** Pour les propriétés `Guid` dans un body JSON, préférer le type `string` + `[GuidValidation]` plutôt que `Guid` natif. Un Guid invalide en JSON provoque une erreur de désérialisation avant même la validation, ce qui donne un message d'erreur peu clair.

---

## Authentification et autorisation

### Configuration globale

```csharp
// Program.cs
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsAdmin", policy => policy.RequireRole("Admin"));

// DependencyInjection.cs (AddPresentation)
services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
```

- **Tous les endpoints** nécessitent un utilisateur authentifié (fallback policy)
- Le provider est **Microsoft Entra ID** (Azure AD) via JWT Bearer
- L'utilisateur courant est accessible via `ICurrentUser` (injecté dans les services)

---

## Pages connexes

- [Architecture du projet](overview.md) — Vue d'ensemble des couches
- [CQRS et MediatR](cqrs-patterns.md) — Les commandes/queries envoyées depuis les endpoints
- [Domain-Driven Design](ddd-concepts.md) — Les types de domaine utilisés dans les mappings
- [Persistance EF Core](persistence.md) — Les repositories appelés par les handlers
