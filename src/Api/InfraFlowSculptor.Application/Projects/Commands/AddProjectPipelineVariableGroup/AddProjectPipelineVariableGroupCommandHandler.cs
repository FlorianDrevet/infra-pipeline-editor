using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;

/// <summary>Handles adding a pipeline variable group to a project.</summary>
public sealed class AddProjectPipelineVariableGroupCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<AddProjectPipelineVariableGroupCommand, AddProjectPipelineVariableGroupResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AddProjectPipelineVariableGroupResult>> Handle(
        AddProjectPipelineVariableGroupCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var result = project.AddPipelineVariableGroup(command.GroupName);
        if (result.IsError)
            return result.Errors;

        await projectRepository.UpdateAsync(project);

        var group = result.Value;
        return new AddProjectPipelineVariableGroupResult(group.Id.Value, group.GroupName);
    }
}
