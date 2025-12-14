using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace Shared.Infrastructure.Persistence.Configurations;

public class SingleValueConverter<TValue> : ValueConverter<SingleValueObject<TValue>, TValue>
{
    public SingleValueConverter()
        : base(
            id => id.Value,
            value => new SingleValueObject<TValue>(value))
    {
    }
}