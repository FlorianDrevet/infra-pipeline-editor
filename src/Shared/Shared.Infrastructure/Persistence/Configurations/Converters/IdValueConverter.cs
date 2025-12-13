using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Domain.Models;

namespace Shared.Infrastructure.Persistence.Configurations;

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