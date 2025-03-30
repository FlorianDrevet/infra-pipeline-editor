using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class UserId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static UserId CreateUnique()
    {
        return new UserId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static UserId Create(Guid value)
    {
        return new UserId(value);
    }
}