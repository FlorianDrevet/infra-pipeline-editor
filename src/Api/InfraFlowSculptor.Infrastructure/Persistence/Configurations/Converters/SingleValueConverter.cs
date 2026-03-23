using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

public class SingleValueConverter<TValueObject, TValue>
    : ValueConverter<TValueObject, TValue>
    where TValueObject : SingleValueObject<TValue>
{
    public SingleValueConverter()
        : base(
            vo => vo.Value,
            value => (TValueObject)Activator.CreateInstance(
                typeof(TValueObject),
                value
            )!
        )
    {
    }
}
