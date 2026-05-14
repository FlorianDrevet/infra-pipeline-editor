namespace InfraFlowSculptor.Contracts.PrivateDnsZones.Responses;

/// <summary>Represents an Azure Private DNS Zone resource.</summary>
public record PrivateDnsZoneResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<VirtualNetworkLinkResponse> VirtualNetworkLinks,
    bool IsExisting = false
);

/// <summary>Response DTO for a virtual network link in a Private DNS Zone.</summary>
public record VirtualNetworkLinkResponse(
    string Id,
    string VirtualNetworkId,
    bool EnableAutoRegistration
);
