namespace InfraFlowSculptor.Contracts.PrivateEndpoints.Responses;

/// <summary>Response containing available PE group IDs for a resource type.</summary>
public record AvailableGroupIdsResponse(
    string ResourceType,
    IReadOnlyList<string> GroupIds);
