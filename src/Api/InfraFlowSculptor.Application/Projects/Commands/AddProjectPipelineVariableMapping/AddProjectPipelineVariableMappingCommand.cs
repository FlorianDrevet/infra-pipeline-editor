using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableMapping;

/// <summary>
/// Command to add a variable-to-Bicep-parameter mapping to a project-level pipeline variable group.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier.</param>
/// <param name="PipelineVariableName">The variable name in the Azure DevOps Library.</param>
/// <param name="BicepParameterName">The target Bicep parameter name in main.bicep.</param>
public record AddProjectPipelineVariableMappingCommand(
    ProjectId ProjectId,
    Guid GroupId,
    string PipelineVariableName,
    string BicepParameterName)
    : ICommand<AddProjectPipelineVariableMappingResult>;

/// <summary>Result returned after adding a project pipeline variable mapping.</summary>
/// <param name="MappingId">The identifier of the created mapping.</param>
/// <param name="PipelineVariableName">The pipeline variable name.</param>
/// <param name="BicepParameterName">The Bicep parameter name.</param>
public record AddProjectPipelineVariableMappingResult(
    Guid MappingId,
    string PipelineVariableName,
    string BicepParameterName);
