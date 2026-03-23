namespace InfraFlowSculptor.Domain.Common.Models;

public class EnumValueObject<TEnum>(TEnum value) : ValueObject where TEnum : struct, Enum
{
    public TEnum Value { get; protected set; } = value;

    protected EnumValueObject() : this(default)
    {
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
