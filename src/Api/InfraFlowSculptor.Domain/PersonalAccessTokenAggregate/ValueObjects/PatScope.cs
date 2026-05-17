using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

/// <summary>
/// Represents a single permission scope assigned to a personal access token.
/// </summary>
public sealed class PatScope : ValueObject
{
    /// <summary>Gets the scope type.</summary>
    public PatScopeType Value { get; }

    /// <summary>Initializes a new instance of the <see cref="PatScope"/> class.</summary>
    public PatScope(PatScopeType value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
