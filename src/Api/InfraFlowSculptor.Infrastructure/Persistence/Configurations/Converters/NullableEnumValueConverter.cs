using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

/// <summary>
/// Converts optional enum-backed value objects to nullable string columns and back.
/// </summary>
/// <typeparam name="TEnumValueObject">The enum-backed value object type.</typeparam>
/// <typeparam name="TEnum">The wrapped enum type.</typeparam>
public sealed class NullableEnumValueConverter<TEnumValueObject, TEnum> : ValueConverter<TEnumValueObject?, string?>
    where TEnumValueObject : EnumValueObject<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NullableEnumValueConverter{TEnumValueObject, TEnum}"/> class.
    /// </summary>
    public NullableEnumValueConverter()
        : base(
            enumValueObject => enumValueObject == null ? null : enumValueObject.Value.ToString(),
            value => string.IsNullOrWhiteSpace(value)
                ? null
                : (TEnumValueObject)Activator.CreateInstance(typeof(TEnumValueObject), Enum.Parse<TEnum>(value))!)
    {
    }
}