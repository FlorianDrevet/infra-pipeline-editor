using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Models;

namespace Shared.Infrastructure.Persistence.Configurations.Converters;

public class EnumValueConverter<TEnumValueObject, TEnum> : ValueConverter<TEnumValueObject, string>
    where TEnumValueObject : EnumValueObject<TEnum>
    where TEnum : struct, Enum
{
    public EnumValueConverter()
        : base(
            enumValueObject => enumValueObject.Value.ToString(),
            value => (TEnumValueObject)Activator.CreateInstance(typeof(TEnumValueObject), Enum.Parse<TEnum>(value))!)
    {
    }
}