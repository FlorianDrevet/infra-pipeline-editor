using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;

/// <summary>Request body for creating or updating a networking profile.</summary>
public class SetNetworkingProfileRequest
{
    /// <summary>Networking mode: Simplified, Standard, or Advanced.</summary>
    [Required]
    public required string Mode { get; init; }

    /// <summary>VNet sourcing strategy: CreateNew, UseExisting, or UseHubSpoke.</summary>
    [Required]
    public required string VnetSourceType { get; init; }

    /// <summary>Azure resource ID of an existing VNet (required when VnetSourceType is UseExisting or UseHubSpoke).</summary>
    public string? ExistingVnetResourceId { get; init; }

    /// <summary>CIDR address space for new VNet (required when VnetSourceType is CreateNew).</summary>
    public string? CreateNewAddressSpace { get; init; }

    /// <summary>CIDR subnet address prefix for PE subnet (required when VnetSourceType is CreateNew).</summary>
    public string? CreateNewSubnetAddressPrefix { get; init; }

    /// <summary>Name of the subnet dedicated to private endpoints.</summary>
    public string? PrivateEndpointsSubnetName { get; init; }

    /// <summary>DNS mode: AutoManaged, CentralizedHub, or Custom.</summary>
    [Required]
    public required string DnsMode { get; init; }

    /// <summary>Resource group ID of DNS hub (required when DnsMode is CentralizedHub).</summary>
    public string? DnsHubResourceGroupId { get; init; }

    /// <summary>Subscription ID of DNS hub (required when DnsMode is CentralizedHub).</summary>
    public string? DnsHubSubscriptionId { get; init; }
}
