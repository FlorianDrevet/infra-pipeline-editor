using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

/// <summary>A user's full name consisting of first name and last name.</summary>
public sealed class Name(string firstName, string lastName) : ValueObject
{
    /// <summary>Gets the last name.</summary>
    public string LastName { get; private set; } = lastName;

    /// <summary>Gets the first name.</summary>
    public string FirstName { get; private set; } = firstName;

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return LastName;
        yield return FirstName;
    }
}