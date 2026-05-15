using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Traffic direction for a Network Security Group rule.</summary>
public sealed class NsgDirection(NsgDirection.Direction value) : EnumValueObject<NsgDirection.Direction>(value)
{
    /// <summary>Available traffic directions.</summary>
    public enum Direction
    {
        Inbound,
        Outbound
    }
}
