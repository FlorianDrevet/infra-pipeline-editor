using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;

/// <summary>Handles removing a pipeline variable group from a project.</summary>
public sealed class RemoveProjectPipelineVariableGroupCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<RemoveProjectPipelineVariableGroupCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectPipelineVariableGroupCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var groupId = new ProjectPipelineVariableGroupId(command.GroupId);
        var result = project.RemovePipelineVariableGroup(groupId);
        if (result.IsError)
            return result.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
