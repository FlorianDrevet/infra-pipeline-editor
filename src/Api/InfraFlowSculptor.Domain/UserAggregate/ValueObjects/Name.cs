using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class Name(string lastName, string firstName) : ValueObject
{
    public string LastName { get; set; } = lastName;
    public string FirstName { get; set; } = firstName;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return LastName;
        yield return FirstName;
    }
}