using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Network protocol for a Network Security Group rule.</summary>
public sealed class NsgProtocol(NsgProtocol.Protocol value) : EnumValueObject<NsgProtocol.Protocol>(value)
{
    /// <summary>Available network protocols.</summary>
    public enum Protocol
    {
        Tcp,
        Udp,
        Icmp,
        Any
    }
}
