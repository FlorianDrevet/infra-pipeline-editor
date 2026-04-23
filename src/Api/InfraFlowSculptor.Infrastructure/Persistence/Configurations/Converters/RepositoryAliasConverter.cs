using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

/// <summary>
/// EF Core value converter for <see cref="RepositoryAlias"/>. The alias is persisted as
/// its underlying slug string and rehydrated through <see cref="RepositoryAlias.Create(string)"/>.
/// A dedicated converter is required because <see cref="RepositoryAlias"/> derives directly
/// from <c>ValueObject</c> (not <c>SingleValueObject&lt;string&gt;</c>) and exposes only a private constructor.
/// </summary>
public sealed class RepositoryAliasConverter : ValueConverter<RepositoryAlias, string>
{
    /// <summary>Initializes a new instance of <see cref="RepositoryAliasConverter"/>.</summary>
    public RepositoryAliasConverter()
        : base(
            alias => alias.Value,
            value => RepositoryAlias.Create(value).Value)
    {
    }
}
