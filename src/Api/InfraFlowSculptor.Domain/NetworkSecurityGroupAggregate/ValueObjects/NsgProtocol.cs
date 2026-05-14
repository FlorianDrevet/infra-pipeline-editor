using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Network protocol for a Network Security Group rule.</summary>
public sealed class NsgProtocol(NsgProtocol.ProtocolEnum value) : EnumValueObject<NsgProtocol.ProtocolEnum>(value)
{
    /// <summary>Available network protocols.</summary>
    public enum ProtocolEnum
    {
        Tcp,
        Udp,
        Icmp,
        Any
    }
}
