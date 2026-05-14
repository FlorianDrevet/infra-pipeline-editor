using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.Entities;

/// <summary>Represents a virtual network link to a Private DNS Zone.</summary>
public sealed class VirtualNetworkLink : Entity<VirtualNetworkLinkId>
{
    /// <summary>Gets the parent DNS zone identifier.</summary>
    public AzureResourceId PrivateDnsZoneId { get; private set; } = null!;

    /// <summary>Gets the linked virtual network identifier.</summary>
    public AzureResourceId VirtualNetworkId { get; private set; } = null!;

    /// <summary>Gets whether auto-registration of DNS records is enabled.</summary>
    public bool EnableAutoRegistration { get; private set; }

    private VirtualNetworkLink() { }

    /// <summary>Creates a new virtual network link.</summary>
    internal static VirtualNetworkLink Create(AzureResourceId dnsZoneId, AzureResourceId virtualNetworkId, bool enableAutoRegistration)
    {
        return new VirtualNetworkLink
        {
            Id = VirtualNetworkLinkId.CreateUnique(),
            PrivateDnsZoneId = dnsZoneId,
            VirtualNetworkId = virtualNetworkId,
            EnableAutoRegistration = enableAutoRegistration
        };
    }
}
