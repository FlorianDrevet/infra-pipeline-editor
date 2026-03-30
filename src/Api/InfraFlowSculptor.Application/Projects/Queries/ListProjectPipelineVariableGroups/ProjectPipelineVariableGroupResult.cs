namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;

/// <summary>Result representing a project-level pipeline variable group with its mappings.</summary>
/// <param name="GroupId">Identifier of the variable group.</param>
/// <param name="GroupName">Name of the Azure DevOps Variable Group.</param>
/// <param name="Mappings">Variable-to-Bicep-parameter mappings within this group.</param>
public record ProjectPipelineVariableGroupResult(
    Guid GroupId,
    string GroupName,
    IReadOnlyList<ProjectPipelineVariableMappingResult> Mappings);

/// <summary>Result representing a single variable-to-Bicep-parameter mapping.</summary>
/// <param name="MappingId">Identifier of the mapping.</param>
/// <param name="PipelineVariableName">Variable name in the Azure DevOps Library.</param>
/// <param name="BicepParameterName">Target Bicep parameter name.</param>
public record ProjectPipelineVariableMappingResult(
    Guid MappingId,
    string PipelineVariableName,
    string BicepParameterName);
