using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Access decision for a Network Security Group rule.</summary>
public sealed class NsgAccess(NsgAccess.Access value) : EnumValueObject<NsgAccess.Access>(value)
{
    /// <summary>Available access decisions.</summary>
    public enum Access
    {
        Allow,
        Deny
    }
}
