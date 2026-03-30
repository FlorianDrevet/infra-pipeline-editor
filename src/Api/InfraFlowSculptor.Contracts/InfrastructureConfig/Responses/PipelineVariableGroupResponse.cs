namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Response representing a pipeline variable group.</summary>
/// <param name="Id">Unique identifier of the variable group.</param>
/// <param name="GroupName">Name of the Azure DevOps Variable Group.</param>
/// <param name="Mappings">Variable-to-Bicep-parameter mappings within this group.</param>
public record PipelineVariableGroupResponse(
    string Id,
    string GroupName,
    IReadOnlyList<PipelineVariableMappingResponse> Mappings);
