namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListPipelineVariableGroups;

/// <summary>Result representing a pipeline variable group with its mappings.</summary>
/// <param name="GroupId">Identifier of the variable group.</param>
/// <param name="GroupName">Name of the Azure DevOps Variable Group.</param>
/// <param name="Mappings">Variable-to-Bicep-parameter mappings within this group.</param>
public record PipelineVariableGroupResult(
    Guid GroupId,
    string GroupName,
    IReadOnlyList<PipelineVariableMappingResult> Mappings);

/// <summary>Result representing a single variable-to-Bicep-parameter mapping.</summary>
/// <param name="MappingId">Identifier of the mapping.</param>
/// <param name="PipelineVariableName">Variable name in the Azure DevOps Library.</param>
/// <param name="BicepParameterName">Target Bicep parameter name.</param>
public record PipelineVariableMappingResult(
    Guid MappingId,
    string PipelineVariableName,
    string BicepParameterName);
