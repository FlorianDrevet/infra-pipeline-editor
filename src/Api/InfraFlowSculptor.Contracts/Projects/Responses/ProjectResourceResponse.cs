namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing an Azure resource within a project configuration.</summary>
public record ProjectResourceResponse(
    string ResourceId,
    string ResourceName,
    string ResourceType,
    string ResourceGroupName,
    string ConfigId,
    string ConfigName);
