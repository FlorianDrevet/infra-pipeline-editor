using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableMapping;

/// <summary>
/// Command to remove a variable mapping from a pipeline variable group.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier.</param>
/// <param name="MappingId">The mapping identifier to remove.</param>
public record RemovePipelineVariableMappingCommand(Guid InfraConfigId, Guid GroupId, Guid MappingId)
    : ICommand<ErrorOr.Deleted>;
