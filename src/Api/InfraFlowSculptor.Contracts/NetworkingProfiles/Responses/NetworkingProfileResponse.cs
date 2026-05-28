namespace InfraFlowSculptor.Contracts.NetworkingProfiles.Responses;

/// <summary>Response DTO for a networking profile.</summary>
public class NetworkingProfileResponse
{
    /// <summary>Unique identifier of the networking profile.</summary>
    public required string Id { get; init; }

    /// <summary>Parent infrastructure configuration identifier.</summary>
    public required string InfraConfigId { get; init; }

    /// <summary>Networking mode (Simplified, Standard, Advanced).</summary>
    public required string Mode { get; init; }

    /// <summary>VNet sourcing strategy (CreateNew, UseExisting, UseHubSpoke).</summary>
    public required string VnetSourceType { get; init; }

    /// <summary>Existing VNet Azure resource ID (if applicable).</summary>
    public string? ExistingVnetResourceId { get; init; }

    /// <summary>Address space for new VNet (if applicable).</summary>
    public string? CreateNewAddressSpace { get; init; }

    /// <summary>Subnet address prefix for PE subnet (if applicable).</summary>
    public string? CreateNewSubnetAddressPrefix { get; init; }

    /// <summary>Name of the private endpoints subnet.</summary>
    public required string PrivateEndpointsSubnetName { get; init; }

    /// <summary>DNS management mode (AutoManaged, CentralizedHub, Custom).</summary>
    public required string DnsMode { get; init; }

    /// <summary>Hub resource group ID (if DNS mode is CentralizedHub).</summary>
    public string? DnsHubResourceGroupId { get; init; }

    /// <summary>Hub subscription ID (if DNS mode is CentralizedHub).</summary>
    public string? DnsHubSubscriptionId { get; init; }
}
