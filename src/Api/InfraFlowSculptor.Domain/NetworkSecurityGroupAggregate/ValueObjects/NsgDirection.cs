using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Traffic direction for a Network Security Group rule.</summary>
public sealed class NsgDirection(NsgDirection.DirectionEnum value) : EnumValueObject<NsgDirection.DirectionEnum>(value)
{
    /// <summary>Available traffic directions.</summary>
    public enum DirectionEnum
    {
        Inbound,
        Outbound
    }
}
