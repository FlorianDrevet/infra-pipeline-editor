using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

/// <summary>Controls how network policies apply to private endpoints in a subnet.</summary>
public sealed class PrivateEndpointNetworkPolicy(PrivateEndpointNetworkPolicy.PolicyEnum value) : EnumValueObject<PrivateEndpointNetworkPolicy.PolicyEnum>(value)
{
    /// <summary>Available private endpoint network policy modes.</summary>
    public enum PolicyEnum
    {
        /// <summary>Network policies are disabled for private endpoints.</summary>
        Disabled,
        /// <summary>Network policies are fully enabled for private endpoints.</summary>
        Enabled,
        /// <summary>Only NSG policies apply to private endpoints.</summary>
        NetworkSecurityGroupEnabled,
        /// <summary>Only route table policies apply to private endpoints.</summary>
        RouteTableEnabled
    }
}
