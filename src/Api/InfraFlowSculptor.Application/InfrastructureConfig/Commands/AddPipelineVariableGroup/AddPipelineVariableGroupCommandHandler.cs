using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableGroup;

/// <summary>
/// Handles adding a pipeline variable group to an infrastructure configuration.
/// </summary>
public sealed class AddPipelineVariableGroupCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : ICommandHandler<AddPipelineVariableGroupCommand, AddPipelineVariableGroupResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AddPipelineVariableGroupResult>> Handle(
        AddPipelineVariableGroupCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfraConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Reload with pipeline variable groups to check for duplicates
        var config = await infraConfigRepository.GetByIdWithPipelineVariableGroupsAsync(configId, cancellationToken);
        if (config is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.NotFoundError(configId);

        var result = config.AddPipelineVariableGroup(command.GroupName);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);

        var group = result.Value;
        return new AddPipelineVariableGroupResult(group.Id.Value, group.GroupName);
    }
}
