using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableGroup;

/// <summary>
/// Command to add a new Azure DevOps Pipeline Variable Group to an infrastructure configuration.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
/// <param name="GroupName">The name of the Azure DevOps Variable Group.</param>
public record AddPipelineVariableGroupCommand(Guid InfraConfigId, string GroupName)
    : ICommand<AddPipelineVariableGroupResult>;

/// <summary>Result returned after adding a pipeline variable group.</summary>
/// <param name="GroupId">The identifier of the created group.</param>
/// <param name="GroupName">The name of the variable group.</param>
public record AddPipelineVariableGroupResult(Guid GroupId, string GroupName);
