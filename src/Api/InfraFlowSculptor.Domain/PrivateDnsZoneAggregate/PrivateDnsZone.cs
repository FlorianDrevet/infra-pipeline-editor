using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.Entities;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;

/// <summary>
/// Represents an Azure Private DNS Zone (<c>Microsoft.Network/privateDnsZones</c>).
/// Provides name resolution for private endpoints via virtual network links.
/// </summary>
public sealed class PrivateDnsZone : AzureResource
{
    private readonly List<VirtualNetworkLink> _virtualNetworkLinks = [];
    /// <summary>Gets the virtual network links for DNS resolution.</summary>
    public IReadOnlyCollection<VirtualNetworkLink> VirtualNetworkLinks => _virtualNetworkLinks.AsReadOnly();

    private PrivateDnsZone() { }

    /// <summary>Updates the resource-level properties.</summary>
    public void Update(Name name, Location location)
    {
        SetNameAndLocation(name, location);
    }

    /// <summary>Adds a virtual network link for DNS resolution.</summary>
    public VirtualNetworkLink AddVirtualNetworkLink(AzureResourceId virtualNetworkId, bool enableAutoRegistration)
    {
        if (_virtualNetworkLinks.Any(l => l.VirtualNetworkId == virtualNetworkId))
            throw new InvalidOperationException("This virtual network is already linked.");

        var link = VirtualNetworkLink.Create(Id, virtualNetworkId, enableAutoRegistration);
        _virtualNetworkLinks.Add(link);
        return link;
    }

    /// <summary>Removes a virtual network link.</summary>
    public void RemoveVirtualNetworkLink(VirtualNetworkLinkId linkId)
    {
        var link = _virtualNetworkLinks.FirstOrDefault(l => l.Id == linkId)
            ?? throw new InvalidOperationException($"Link '{linkId.Value}' not found.");
        _virtualNetworkLinks.Remove(link);
    }

    /// <summary>Creates a new PrivateDnsZone.</summary>
    public static PrivateDnsZone Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        bool isExisting = false)
    {
        return new PrivateDnsZone
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting
        };
    }
}
