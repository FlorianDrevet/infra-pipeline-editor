using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

/// <summary>
/// EF Core value converter that serializes <see cref="RepositoryContentKinds"/> as a
/// CSV string of flag names (e.g. <c>"Infrastructure,Pipelines"</c>) and parses it back
/// via <see cref="RepositoryContentKinds.Parse(string)"/>.
/// </summary>
public sealed class RepositoryContentKindsConverter : ValueConverter<RepositoryContentKinds, string>
{
    /// <summary>Initializes a new instance of <see cref="RepositoryContentKindsConverter"/>.</summary>
    public RepositoryContentKindsConverter()
        : base(
            kinds => kinds.ToString(),
            value => RepositoryContentKinds.Parse(value).Value)
    {
    }
}
