using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableMapping;

/// <summary>
/// Command to add a variable-to-Bicep-parameter mapping to a pipeline variable group.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier.</param>
/// <param name="PipelineVariableName">The variable name in the Azure DevOps Library.</param>
/// <param name="BicepParameterName">The target Bicep parameter name in main.bicep.</param>
public record AddPipelineVariableMappingCommand(
    Guid InfraConfigId,
    Guid GroupId,
    string PipelineVariableName,
    string BicepParameterName)
    : ICommand<AddPipelineVariableMappingResult>;

/// <summary>Result returned after adding a pipeline variable mapping.</summary>
/// <param name="MappingId">The identifier of the created mapping.</param>
/// <param name="PipelineVariableName">The pipeline variable name.</param>
/// <param name="BicepParameterName">The Bicep parameter name.</param>
public record AddPipelineVariableMappingResult(
    Guid MappingId,
    string PipelineVariableName,
    string BicepParameterName);
