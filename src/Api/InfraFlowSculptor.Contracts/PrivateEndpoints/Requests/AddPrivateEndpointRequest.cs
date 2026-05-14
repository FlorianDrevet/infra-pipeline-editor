using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.PrivateEndpoints.Requests;

/// <summary>Request to add a private endpoint configuration to a resource.</summary>
public class AddPrivateEndpointRequest
{
    /// <summary>Target subnet for the private endpoint.</summary>
    [Required, GuidValidation]
    public required Guid SubnetId { get; init; }

    /// <summary>Sub-resource group ID (e.g., "vault", "blob", "sqlServer").</summary>
    [Required, StringLength(100)]
    public required string GroupId { get; init; }

    /// <summary>Whether the PE connection is auto-approved.</summary>
    public bool AutoApproval { get; init; } = true;

    /// <summary>Optional Private DNS Zone for record registration.</summary>
    [GuidValidation]
    public Guid? PrivateDnsZoneId { get; init; }

    /// <summary>Optional custom network interface name.</summary>
    [StringLength(80)]
    public string? CustomNetworkInterfaceName { get; init; }
}
