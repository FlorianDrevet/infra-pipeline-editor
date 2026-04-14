using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Azure subscription identifier.</summary>
public sealed class SubscriptionId : SingleValueObject<Guid>
{
    private SubscriptionId() { }

    public SubscriptionId(Guid value) : base(value)
    {
    }
}
