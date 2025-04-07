using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ManagedIdentityConfiguration.ValueObjects;

public sealed class ManagedIdentityId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static ManagedIdentityId CreateUnique()
    {
        return new ManagedIdentityId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static ManagedIdentityId Create(Guid value)
    {
        return new ManagedIdentityId(value);
    }
}