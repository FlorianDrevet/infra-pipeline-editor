namespace InfraFlowSculptor.Contracts.FrontDoors.Responses;

/// <summary>Represents an Azure Front Door resource.</summary>
public record FrontDoorResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    bool WafPolicyEnabled,
    IReadOnlyList<FrontDoorOriginResponse> Origins,
    IReadOnlyList<FrontDoorEnvironmentConfigResponse> EnvironmentSettings,
    bool IsExisting = false
);

/// <summary>Response DTO for a Front Door origin.</summary>
public record FrontDoorOriginResponse(
    string Id,
    string TargetResourceId,
    string? HostName,
    bool PrivateLinkEnabled,
    int Weight,
    int Priority
);

/// <summary>Response DTO for per-environment Front Door configuration.</summary>
public record FrontDoorEnvironmentConfigResponse(
    string EnvironmentName,
    string Sku
);
