using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableGroup;

/// <summary>
/// Handles removing a pipeline variable group from an infrastructure configuration.
/// </summary>
public sealed class RemovePipelineVariableGroupCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : ICommandHandler<RemovePipelineVariableGroupCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemovePipelineVariableGroupCommand command,
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
        var result = config.RemovePipelineVariableGroup(groupId);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);
        return Result.Deleted;
    }
}
