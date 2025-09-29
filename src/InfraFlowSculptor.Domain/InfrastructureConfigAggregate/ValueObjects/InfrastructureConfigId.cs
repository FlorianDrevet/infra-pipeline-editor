using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class InfrastructureConfigId(Guid value) : ValueObject
{
    public Guid Value { get; protected set; } = value;

    public static InfrastructureConfigId CreateUnique()
    {
        return new InfrastructureConfigId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static InfrastructureConfigId Create(Guid value)
    {
        return new InfrastructureConfigId(value);
    }
}