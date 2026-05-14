using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Access decision for a Network Security Group rule.</summary>
public sealed class NsgAccess(NsgAccess.AccessEnum value) : EnumValueObject<NsgAccess.AccessEnum>(value)
{
    /// <summary>Available access decisions.</summary>
    public enum AccessEnum
    {
        Allow,
        Deny
    }
}
