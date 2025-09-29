using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Name(string value) : ValueObject
{
    public string Value { get; protected set; } = value;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}