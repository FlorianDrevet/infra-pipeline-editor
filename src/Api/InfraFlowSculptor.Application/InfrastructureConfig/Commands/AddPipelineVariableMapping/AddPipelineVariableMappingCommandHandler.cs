using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableMapping;

/// <summary>
/// Handles adding a variable mapping to a pipeline variable group.
/// </summary>
public sealed class AddPipelineVariableMappingCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : ICommandHandler<AddPipelineVariableMappingCommand, AddPipelineVariableMappingResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AddPipelineVariableMappingResult>> Handle(
        AddPipelineVariableMappingCommand command,
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

        var result = group.AddMapping(command.PipelineVariableName, command.BicepParameterName);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);

        var mapping = result.Value;
        return new AddPipelineVariableMappingResult(
            mapping.Id.Value,
            mapping.PipelineVariableName,
            mapping.BicepParameterName);
    }
}
