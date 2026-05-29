namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Per-resource private endpoint configuration carried to the Bicep generation pipeline.
/// Represents the V3 resource-level PE settings (VNet, subnet, DNS mode).
/// </summary>
public sealed class PrivateEndpointDefinition
{
    /// <summary>The domain identifier of the VNet containing the PE subnet.</summary>
    public required Guid VirtualNetworkId { get; init; }

    /// <summary>Name of the subnet where the private endpoint will be created.</summary>
    public required string SubnetName { get; init; }

    /// <summary>DNS management mode: AutoManaged, ExistingHub, or Disabled.</summary>
    public required string DnsMode { get; init; }

    /// <summary>Hub resource group ID (when DNS mode is ExistingHub).</summary>
    public string? DnsHubResourceGroupId { get; init; }

    /// <summary>Hub subscription ID (when DNS mode is ExistingHub).</summary>
    public string? DnsHubSubscriptionId { get; init; }
}
