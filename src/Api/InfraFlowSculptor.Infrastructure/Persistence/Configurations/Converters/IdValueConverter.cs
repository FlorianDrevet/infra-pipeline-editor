using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

public sealed class IdValueConverter<TId> : ValueConverter<TId, Guid>
    where TId : Id<TId>
{
    public IdValueConverter()
        : base(
            id => id.Value,
            value => Id<TId>.Create(value))
    {
    }
}
