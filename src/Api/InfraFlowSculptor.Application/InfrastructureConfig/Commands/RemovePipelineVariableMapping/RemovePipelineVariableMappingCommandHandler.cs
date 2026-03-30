using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableMapping;

/// <summary>
/// Handles removing a variable mapping from a pipeline variable group.
/// </summary>
public sealed class RemovePipelineVariableMappingCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : ICommandHandler<RemovePipelineVariableMappingCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemovePipelineVariableMappingCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfraConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = await infraConfigRepository.GetByIdWithPipelineVariableGroupsAsync(configId, cancellationToken);
        if (config is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.NotFoundError(configId);

        var groupId = new PipelineVariableGroupId(command.GroupId);
        var group = config.PipelineVariableGroups.FirstOrDefault(g => g.Id == groupId);
        if (group is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.VariableGroupNotFound(groupId);

        var mappingId = new PipelineVariableMappingId(command.MappingId);
        var result = group.RemoveMapping(mappingId);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);
        return Result.Deleted;
    }
}
