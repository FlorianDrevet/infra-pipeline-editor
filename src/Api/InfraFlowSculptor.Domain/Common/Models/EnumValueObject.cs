namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Base class for value objects wrapping an <see langword="enum"/> value.
/// Provides structural equality based on the enum member.
/// </summary>
/// <typeparam name="TEnum">The enum type to wrap.</typeparam>
public class EnumValueObject<TEnum>(TEnum value) : ValueObject where TEnum : struct, Enum
{
    /// <summary>Gets the underlying enum value.</summary>
    public TEnum Value { get; protected set; } = value;

    /// <summary>EF Core / serialization constructor — defaults to <c>default(TEnum)</c>.</summary>
    protected EnumValueObject() : this(default)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
