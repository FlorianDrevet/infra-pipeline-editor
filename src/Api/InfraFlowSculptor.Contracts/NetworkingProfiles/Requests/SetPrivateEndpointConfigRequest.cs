using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;

/// <summary>Request body for configuring a private endpoint on a resource.</summary>
public class SetPrivateEndpointConfigRequest
{
    /// <summary>The virtual network identifier to use for the private endpoint.</summary>
    [Required]
    public required string VirtualNetworkId { get; init; }

    /// <summary>The subnet name on the selected virtual network.</summary>
    [Required]
    public required string SubnetName { get; init; }

    /// <summary>The DNS management mode: AutoManaged, ExistingHub, or Disabled.</summary>
    [Required]
    public required string DnsMode { get; init; }

    /// <summary>The centralized DNS hub resource group ID (required when DnsMode is ExistingHub).</summary>
    public string? DnsHubResourceGroupId { get; init; }

    /// <summary>The centralized DNS hub subscription ID (required when DnsMode is ExistingHub).</summary>
    public string? DnsHubSubscriptionId { get; init; }
}
