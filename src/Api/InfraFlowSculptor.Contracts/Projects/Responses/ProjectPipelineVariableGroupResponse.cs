namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing a project-level pipeline variable group.</summary>
/// <param name="Id">Unique identifier of the variable group.</param>
/// <param name="GroupName">Name of the Azure DevOps Variable Group.</param>
public record ProjectPipelineVariableGroupResponse(
    string Id,
    string GroupName);
