namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Networking profile configuration passed to the Bicep generation pipeline.
/// Used by networking-related stages to generate VNet resolution, private endpoint companions,
/// and public network access disablement.
/// </summary>
public class NetworkingProfileDefinition
{
    /// <summary>Networking mode (Simplified, Standard, Advanced).</summary>
    public required string Mode { get; init; }

    /// <summary>VNet sourcing strategy (CreateNew, UseExisting, UseHubSpoke).</summary>
    public required string VnetSourceType { get; init; }

    /// <summary>Existing VNet resource ID (when using UseExisting or UseHubSpoke).</summary>
    public string? ExistingVnetResourceId { get; init; }

    /// <summary>CIDR address space for new VNet (when CreateNew).</summary>
    public string? CreateNewAddressSpace { get; init; }

    /// <summary>Name of the subnet dedicated to private endpoints.</summary>
    public required string PrivateEndpointsSubnetName { get; init; }

    /// <summary>CIDR subnet address prefix for PE subnet.</summary>
    public string? PrivateEndpointsSubnetAddressPrefix { get; init; }

    /// <summary>DNS management mode (AutoManaged, CentralizedHub, Custom).</summary>
    public required string DnsMode { get; init; }

    /// <summary>Hub resource group ID (when DNS mode is CentralizedHub).</summary>
    public string? DnsHubResourceGroupId { get; init; }

    /// <summary>Hub subscription ID (when DNS mode is CentralizedHub).</summary>
    public string? DnsHubSubscriptionId { get; init; }
}
