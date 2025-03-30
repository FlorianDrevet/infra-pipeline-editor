using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

public sealed class ResourceGroupId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static ResourceGroupId CreateUnique()
    {
        return new ResourceGroupId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static ResourceGroupId Create(Guid value)
    {
        return new ResourceGroupId(value);
    }
}