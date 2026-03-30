using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableMapping;

/// <summary>Handles adding a variable mapping to a project-level pipeline variable group.</summary>
public sealed class AddProjectPipelineVariableMappingCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<AddProjectPipelineVariableMappingCommand, AddProjectPipelineVariableMappingResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AddProjectPipelineVariableMappingResult>> Handle(
        AddProjectPipelineVariableMappingCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var groupId = new ProjectPipelineVariableGroupId(command.GroupId);
        var group = project.PipelineVariableGroups.FirstOrDefault(g => g.Id == groupId);
        if (group is null)
            return Errors.Project.VariableGroupNotFoundError(groupId);

        var result = group.AddMapping(command.PipelineVariableName, command.BicepParameterName);
        if (result.IsError)
            return result.Errors;

        await projectRepository.UpdateAsync(project);

        var mapping = result.Value;
        return new AddProjectPipelineVariableMappingResult(
            mapping.Id.Value,
            mapping.PipelineVariableName,
            mapping.BicepParameterName);
    }
}
