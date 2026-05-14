namespace InfraFlowSculptor.Application.PrivateDnsZones.Common;

/// <summary>Carries virtual network link data within CQRS commands and results.</summary>
public record VirtualNetworkLinkData(
    string Id,
    string VirtualNetworkId,
    bool EnableAutoRegistration);
