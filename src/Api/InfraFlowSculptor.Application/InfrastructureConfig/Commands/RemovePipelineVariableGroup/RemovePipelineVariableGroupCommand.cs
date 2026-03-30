using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableGroup;

/// <summary>
/// Command to remove a pipeline variable group from an infrastructure configuration.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier to remove.</param>
public record RemovePipelineVariableGroupCommand(Guid InfraConfigId, Guid GroupId)
    : ICommand<ErrorOr.Deleted>;
