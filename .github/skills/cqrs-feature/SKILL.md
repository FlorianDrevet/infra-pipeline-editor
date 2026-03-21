# Skill : cqrs-feature — Génération d'une feature CQRS complète

> **Charger ce skill avec `read_file` AVANT de générer tout artefact CQRS.**
> Il contient le workflow complet + les patterns validés pour InfraFlowSculptor.

---

## Pré-requis avant de commencer

1. `MEMORY.md` lu en entier (section 3 Domain Model, section 4 CQRS Pattern, section 8 Persistence).
2. Identifier :
   - Nom de la **feature** (ex: `AzureFunction`, `AppServicePlan`)
   - **Opérations** requises : Create / Read / Update / Delete / List / custom
   - **Projet cible** : API principale (`src/Api`) ou BicepGenerators (`src/BicepGenerators`)
   - Agrégat **existant à étendre** ou **nouveau agrégat** ?

Appliquer toutes les règles de l'agent `dotnet-dev` pour la qualité du code C# généré (nommage, XML docs, no magic strings, sealed, guard clauses, etc.).

---

## Étape 1 — Domaine

### Structure des dossiers

```
src/Api/InfraFlowSculptor.Domain/{AggregateName}Aggregate/
├── {AggregateName}.cs                      Aggregate root
├── ValueObjects/
│   ├── {AggregateName}Id.cs                Id value object
│   └── {OtherValueObject}.cs              Autres value objects
└── Entities/
    └── {ChildEntity}.cs                   Entités enfants (si nécessaire)
```

### Aggregate root

```csharp
/// <summary>
/// Represents the {AggregateName} aggregate root.
/// </summary>
public sealed class {AggregateName} : AggregateRoot<{AggregateName}Id>
{
    /// <summary>Gets the name of the {AggregateName}.</summary>
    public Name Name { get; private set; }

    private {AggregateName}(
        {AggregateName}Id id,
        Name name) : base(id)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new <see cref="{AggregateName}"/> instance with a generated identifier.
    /// </summary>
    public static {AggregateName} Create(Name name)
        => new({AggregateName}Id.CreateUnique(), name);

    // EF Core constructor
    private {AggregateName}() : base() { }
}
```

### Value object Id

```csharp
/// <summary>Strongly-typed identifier for <see cref="{AggregateName}"/>.</summary>
public sealed class {AggregateName}Id : Id<{AggregateName}Id>
{
    public {AggregateName}Id(Guid value) : base(value) { }
}
```

### Fichier d'erreurs

Créer `src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.{AggregateName}.cs` :

```csharp
using ErrorOr;
using InfraFlowSculptor.Domain.{AggregateName}Aggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="{AggregateName}"/> aggregate.</summary>
    public static class {AggregateName}
    {
        private const string NotFoundCode = "{AggregateName}.NotFound";
        private const string ForbiddenCode = "{AggregateName}.Forbidden";

        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError({AggregateName}Id id) =>
            Error.NotFound(code: NotFoundCode, description: $"No {AggregateName} with id {id} was found.");

        /// <summary>Returns a forbidden error when the caller lacks write access.</summary>
        public static Error ForbiddenError() =>
            Error.Forbidden(code: ForbiddenCode, description: "You do not have permission to modify this {AggregateName}.");
    }
}
```

---

## Étape 2 — Application (CQRS)

### Structure des dossiers

```
src/Api/InfraFlowSculptor.Application/{AggregateName}s/
├── Commands/
│   └── Create{AggregateName}/
│       ├── Create{AggregateName}Command.cs
│       ├── Create{AggregateName}CommandHandler.cs
│       └── Create{AggregateName}CommandValidator.cs
├── Queries/
│   └── Get{AggregateName}/
│       ├── Get{AggregateName}Query.cs
│       └── Get{AggregateName}QueryHandler.cs
└── Common/
    └── {AggregateName}Result.cs
```

### Commande

```csharp
/// <summary>Command to create a new <see cref="{AggregateName}"/>.</summary>
/// <param name="Name">The name of the {AggregateName} to create.</param>
/// <param name="ResourceGroupId">The resource group that will own this {AggregateName}.</param>
public record Create{AggregateName}Command(string Name, Guid ResourceGroupId)
    : IRequest<ErrorOr<{AggregateName}Result>>;
```

### Handler de commande

```csharp
/// <summary>
/// Handles the <see cref="Create{AggregateName}Command"/> request.
/// </summary>
public sealed class Create{AggregateName}CommandHandler(
    I{AggregateName}Repository repository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<Create{AggregateName}Command, ErrorOr<{AggregateName}Result>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<{AggregateName}Result>> Handle(
        Create{AggregateName}Command command,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            ResourceGroupId.Create(command.ResourceGroupId), cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFoundError(ResourceGroupId.Create(command.ResourceGroupId));

        var accessResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (accessResult.IsError)
            return accessResult.Errors;

        var entity = {AggregateName}.Create(Name.Create(command.Name));
        await repository.AddAsync(entity, cancellationToken);

        return new {AggregateName}Result(entity.Id, entity.Name);
    }
}
```

### Validator

```csharp
/// <summary>
/// Validates the <see cref="Create{AggregateName}Command"/> before it is handled.
/// </summary>
public sealed class Create{AggregateName}CommandValidator
    : AbstractValidator<Create{AggregateName}Command>
{
    public Create{AggregateName}CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(80).WithMessage("Name must not exceed 80 characters.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
```

### Query + handler

```csharp
/// <summary>Query to retrieve a <see cref="{AggregateName}"/> by its identifier.</summary>
public record Get{AggregateName}Query({AggregateName}Id Id)
    : IRequest<ErrorOr<{AggregateName}Result>>;

/// <summary>Handles the <see cref="Get{AggregateName}Query"/> request.</summary>
public sealed class Get{AggregateName}QueryHandler(
    I{AggregateName}Repository repository,
    IInfraConfigAccessService accessService,
    IResourceGroupRepository resourceGroupRepository)
    : IRequestHandler<Get{AggregateName}Query, ErrorOr<{AggregateName}Result>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<{AggregateName}Result>> Handle(
        Get{AggregateName}Query query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (entity is null)
            return Errors.{AggregateName}.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            entity.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFoundError(entity.ResourceGroupId);

        var accessResult = await accessService.VerifyReadAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (accessResult.IsError)
            return accessResult.Errors;

        return new {AggregateName}Result(entity.Id, entity.Name);
    }
}
```

### Result DTO (couche Application)

```csharp
/// <summary>Application-layer result for a <see cref="{AggregateName}"/> operation.</summary>
public record {AggregateName}Result(
    {AggregateName}Id Id,
    Name Name);
```

---

## Étape 3 — Interface Repository

Dans `src/Api/InfraFlowSculptor.Application/Common/Interfaces/Persistence/` :

```csharp
/// <summary>
/// Provides persistence operations for the <see cref="{AggregateName}"/> aggregate root.
/// </summary>
public interface I{AggregateName}Repository : IRepository<{AggregateName}>
{
    /// <summary>
    /// Retrieves a <see cref="{AggregateName}"/> by identifier, including its related entities.
    /// Returns <c>null</c> if not found.
    /// </summary>
    Task<{AggregateName}?> GetByIdWithRelatedAsync(
        {AggregateName}Id id, CancellationToken cancellationToken = default);
}
```

---

## Étape 4 — Infrastructure

### Configuration EF Core

Dans `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/` :

```csharp
/// <summary>EF Core configuration for the <see cref="{AggregateName}"/> aggregate.</summary>
public sealed class {AggregateName}Configuration : IEntityTypeConfiguration<{AggregateName}>
{
    private const string TableName = "{AggregateName}s";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<{AggregateName}> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.ConfigureAggregateRootId<{AggregateName}, {AggregateName}Id>();

        builder.Property(x => x.Name)
            .HasConversion(new SingleValueConverter<Name, string>())
            .IsRequired();

        // Pour TPT (ressources Azure dérivées d'AzureResource) :
        // builder.HasBaseType<AzureResource>().ToTable(TableName);
    }
}
```

### Repository

```csharp
/// <summary>
/// EF Core implementation of <see cref="I{AggregateName}Repository"/>.
/// </summary>
public sealed class {AggregateName}Repository(ProjectDbContext context)
    : BaseRepository<{AggregateName}, ProjectDbContext>(context),
      I{AggregateName}Repository
{
    /// <inheritdoc />
    public async Task<{AggregateName}?> GetByIdWithRelatedAsync(
        {AggregateName}Id id, CancellationToken cancellationToken = default)
        => await Context.{AggregateName}s
            .Include(x => x.RoleAssignments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)  // ✅ comparer le value object entier
            .ConfigureAwait(false);
}
```

> **⚠️ Piège EF Core critique :** Toujours écrire `x.Id == id` dans les prédicats LINQ — jamais `x.Id.Value == id.Value`. EF Core ne peut pas traduire `.Value` sur un value object. Voir MEMORY.md section 8.4.

### Enregistrement DI

Dans `src/Api/InfraFlowSculptor.Infrastructure/DependencyInjection.cs` — méthode `AddRepositories()` :

```csharp
services.AddScoped<I{AggregateName}Repository, {AggregateName}Repository>();
```

---

## Étape 5 — Contrats

Dans `src/Api/InfraFlowSculptor.Contracts/{AggregateName}s/` :

```csharp
// Requests/Create{AggregateName}Request.cs
public class Create{AggregateName}Request
{
    [Required]
    public required string Name { get; init; }

    [Required, GuidValidation]
    public required string ResourceGroupId { get; init; }
}

// Responses/{AggregateName}Response.cs
public record {AggregateName}Response(string Id, string Name, string ResourceGroupId);
```

> **Rappel :** Utiliser `string` + `[GuidValidation]` pour les GUIDs en body JSON (pas `Guid` directement) — voir MEMORY.md section 6.4.

---

## Étape 6 — Mapping Mapster

Dans `src/Api/InfraFlowSculptor.Api/Common/Mapping/{AggregateName}MappingConfig.cs` :

```csharp
/// <summary>Mapster mapping configuration for the {AggregateName} feature.</summary>
public sealed class {AggregateName}MappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        // ID types
        config.NewConfig<{AggregateName}Id, Guid>().MapWith(src => src.Value);
        config.NewConfig<Guid, {AggregateName}Id>().MapWith(src => {AggregateName}Id.Create(src));

        // Request → Command
        config.NewConfig<Create{AggregateName}Request, Create{AggregateName}Command>()
            .Map(dest => dest.ResourceGroupId, src => Guid.Parse(src.ResourceGroupId));

        // Result → Response
        config.NewConfig<{AggregateName}Result, {AggregateName}Response>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value);
    }
}
```

---

## Étape 7 — Endpoint Minimal API

Dans `src/Api/InfraFlowSculptor.Api/Controllers/{AggregateName}Controller.cs` :

```csharp
/// <summary>Minimal API endpoint definitions for the {AggregateName} feature.</summary>
public static class {AggregateName}Controller
{
    /// <summary>Registers the {AggregateName} endpoints on the application builder.</summary>
    public static IApplicationBuilder Use{AggregateName}Controller(
        this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints
                .MapGroup("/{aggregate-name}")
                .WithTags("{Aggregate Name}");

            group.MapGet("/{id:guid}", Get{AggregateName});
            group.MapPost("", Create{AggregateName});
            group.MapPut("/{id:guid}", Update{AggregateName});
            group.MapDelete("/{id:guid}", Delete{AggregateName});
        });
    }

    private static async Task<IResult> Get{AggregateName}(
        Guid id,
        ISender sender,
        IMapper mapper)
    {
        var query = new Get{AggregateName}Query({AggregateName}Id.Create(id));
        var result = await sender.Send(query);
        return result.Match(
            value => Results.Ok(mapper.Map<{AggregateName}Response>(value)),
            errors => errors.ToErrorResult());
    }

    private static async Task<IResult> Create{AggregateName}(
        [FromBody] Create{AggregateName}Request request,
        ISender sender,
        IMapper mapper)
    {
        var command = mapper.Map<Create{AggregateName}Command>(request);
        var result = await sender.Send(command);
        return result.Match(
            value => Results.Created(
                $"/{aggregate-name}/{value.Id.Value}",
                mapper.Map<{AggregateName}Response>(value)),
            errors => errors.ToErrorResult());
    }
}
```

Enregistrer dans `Program.cs` : `app.Use{AggregateName}Controller();`

---

## Étape 8 — Migration EF Core

```bash
dotnet ef migrations add Add{AggregateName} `
  --project src/Api/InfraFlowSculptor.Infrastructure `
  --startup-project src/Api/InfraFlowSculptor.Api
```

> **⚠️ Vérifier le contenu de la migration générée.** Si elle contient `CREATE TABLE` pour des tables qui existent déjà, le snapshot EF Core est corrompu — voir MEMORY.md section 17 pour la procédure de correction.

---

## Étape 9 — Frontend (si les contrats changent)

Si l'étape 5 a créé ou modifié des contrats HTTP :

1. Déléguer à l'agent `angular-front` en lui donnant :
   - Le nom de la feature
   - Les interfaces TypeScript à créer (miroir des `*Request` et `*Response`)
   - La route à ajouter (`/feature-name` → `loadComponent`)
   - Le service Axios à créer dans `shared/services/`
2. Vérifier : `npm run typecheck` + `npm run build` dans `src/Front`

---

## Règles de qualité — Non-négociables

| Règle | Détail |
|-------|--------|
| **ErrorOr partout** | Handlers retournent `ErrorOr<T>`, jamais d'exception pour les erreurs métier |
| **EF Core via Repository** | Jamais d'accès `DbContext` direct depuis un handler |
| **Comparaison ID** | `x.Id == id` toujours (jamais `x.Id.Value == id.Value`) |
| **Validator par commande** | Toujours créer `AbstractValidator<TCommand>` avec `.WithMessage()` sur chaque règle |
| **Duplication < 3%** | Si logique commune à 2+ handlers → extraire helper ou service |
| **Ambiguïté namespace** | Utiliser le nom complet `Domain.InfrastructureConfigAggregate.InfrastructureConfig` si CS0118 |
| **SonarQube S2325** | Méthodes EF Core config sans `this` → les rendre `static` |

---

## Checklist de génération complète

```
Domaine
[ ] {AggregateName}.cs (aggregate root avec Create() statique + constructeur EF Core privé)
[ ] {AggregateName}Id.cs (value object Id)
[ ] Autres value objects selon les propriétés
[ ] Errors.{AggregateName}.cs (codes d'erreur en constantes privées)

Application
[ ] Create/Update/Delete{AggregateName}Command + Handler + Validator
[ ] Get{AggregateName}Query + Handler
[ ] List{AggregateName}Query + Handler (si liste nécessaire)
[ ] {AggregateName}Result.cs (DTO couche Application)
[ ] I{AggregateName}Repository interface

Infrastructure
[ ] {AggregateName}Configuration.cs (EF Core, ToTable avec constante)
[ ] {AggregateName}Repository.cs (sealed, ConfigureAwait(false), x.Id == id)
[ ] DependencyInjection.cs mis à jour (AddScoped)

Contrats
[ ] Create/Update{AggregateName}Request.cs (required, GuidValidation pour les Guids JSON)
[ ] {AggregateName}Response.cs (record, Id en string)

API
[ ] {AggregateName}MappingConfig.cs (IRegister, sealed)
[ ] {AggregateName}Controller.cs (static, Minimal API style)
[ ] Program.cs mis à jour (app.Use{AggregateName}Controller())

Migration & Build
[ ] dotnet ef migrations add Add{AggregateName}
[ ] Vérifier le contenu de la migration (pas de CREATE TABLE parasites)
[ ] dotnet build .\InfraFlowSculptor.slnx — 0 erreur, 0 warning nouveau

Frontend (si contrats changés)
[ ] Interface TypeScript dans shared/interfaces/
[ ] Service Axios dans shared/services/
[ ] Route lazy dans app-routing.ts
[ ] Délégué à angular-front
[ ] npm run typecheck + npm run build passent

Mémoire
[ ] MEMORY.md section 3 mis à jour (nouveau agrégat)
[ ] MEMORY.md Changelog mis à jour
```
