using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace Shared.Infrastructure.Persistence.Configurations;

public class SingleValueConverter<TSingleValueObject, TValue> : ValueConverter<TSingleValueObject, TValue>
    where TSingleValueObject : SingleValueObject<TValue>
{
    public SingleValueConverter()
        : base(
            id => id.Value,
            value => (TSingleValueObject)new SingleValueObject<TValue>(value))
    {
    }
}