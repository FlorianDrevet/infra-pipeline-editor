using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Name(string name) : ValueObject
{
    public string Value { get; protected set; } = name;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}