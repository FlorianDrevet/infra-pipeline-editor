using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

/// <summary>A key-value tag (name/value pair) used to annotate environment definitions.</summary>
public sealed class Tag(string name, string value) : ValueObject
{
    /// <summary>Gets the tag name.</summary>
    public string Name { get; private set; } = name;

    /// <summary>Gets the tag value.</summary>
    public string Value { get; private set; } = value;

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Value;
    }
}
