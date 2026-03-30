namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Response representing a single variable-to-Bicep-parameter mapping at project level.</summary>
/// <param name="Id">Unique identifier of the mapping.</param>
/// <param name="PipelineVariableName">Variable name in the Azure DevOps Library.</param>
/// <param name="BicepParameterName">Target Bicep parameter name.</param>
public record ProjectPipelineVariableMappingResponse(
    string Id,
    string PipelineVariableName,
    string BicepParameterName);
