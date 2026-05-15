namespace InfraFlowSculptor.Application.FrontDoors.Common;

/// <summary>Carries Front Door origin data within CQRS commands and results.</summary>
public record FrontDoorOriginData(
    string Id,
    string TargetResourceId,
    string? HostName,
    bool PrivateLinkEnabled,
    int Weight,
    int Priority);
