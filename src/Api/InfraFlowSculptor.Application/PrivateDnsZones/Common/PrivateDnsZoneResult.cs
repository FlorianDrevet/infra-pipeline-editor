using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Common;

/// <summary>Application-layer result for a Private DNS Zone.</summary>
public record PrivateDnsZoneResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<VirtualNetworkLinkData> VirtualNetworkLinks,
    bool IsExisting = false
);
