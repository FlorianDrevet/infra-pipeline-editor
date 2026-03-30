namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Result representing a project-level pipeline variable group.</summary>
/// <param name="GroupId">Identifier of the variable group.</param>
/// <param name="GroupName">Name of the Azure DevOps Variable Group.</param>
public record ProjectPipelineVariableGroupResult(
    Guid GroupId,
    string GroupName);
