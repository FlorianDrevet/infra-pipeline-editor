using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class SubscriptionId: SingleValueObject<Guid>
{
    private SubscriptionId() { }

    public SubscriptionId(Guid value) : base(value)
    {
    }
}