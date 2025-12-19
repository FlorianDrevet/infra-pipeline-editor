using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class Tag(string name, string value) : ValueObject
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Value;
    }
}