using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Configuration for a Private Endpoint attached to an Azure resource.
/// Defines the subnet, group ID, DNS zone, and approval settings.
/// </summary>
public sealed class PrivateEndpointConfig : Entity<PrivateEndpointConfigId>
{
    /// <summary>Gets the parent resource identifier.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>Gets the target subnet for the private endpoint.</summary>
    public AzureResourceId SubnetId { get; private set; } = null!;

    /// <summary>Gets the sub-resource group ID (e.g., "vault", "blob", "sqlServer").</summary>
    public string GroupId { get; private set; } = string.Empty;

    /// <summary>Gets whether the PE connection is auto-approved.</summary>
    public bool AutoApproval { get; private set; }

    /// <summary>Gets the optional Private DNS Zone for record registration.</summary>
    public AzureResourceId? PrivateDnsZoneId { get; private set; }

    /// <summary>Gets the optional custom network interface name.</summary>
    public string? CustomNetworkInterfaceName { get; private set; }

    private PrivateEndpointConfig() { }

    /// <summary>Updates the private endpoint configuration.</summary>
    public void Update(AzureResourceId subnetId, string groupId, bool autoApproval, AzureResourceId? privateDnsZoneId, string? customNetworkInterfaceName)
    {
        SubnetId = subnetId;
        GroupId = groupId;
        AutoApproval = autoApproval;
        PrivateDnsZoneId = privateDnsZoneId;
        CustomNetworkInterfaceName = customNetworkInterfaceName;
    }

    /// <summary>Creates a new PrivateEndpointConfig.</summary>
    public static PrivateEndpointConfig Create(
        AzureResourceId resourceId,
        AzureResourceId subnetId,
        string groupId,
        bool autoApproval,
        AzureResourceId? privateDnsZoneId,
        string? customNetworkInterfaceName)
    {
        return new PrivateEndpointConfig
        {
            Id = PrivateEndpointConfigId.CreateUnique(),
            ResourceId = resourceId,
            SubnetId = subnetId,
            GroupId = groupId,
            AutoApproval = autoApproval,
            PrivateDnsZoneId = privateDnsZoneId,
            CustomNetworkInterfaceName = customNetworkInterfaceName
        };
    }
}
