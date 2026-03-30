using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableMapping;

/// <summary>Handles removing a variable mapping from a project-level pipeline variable group.</summary>
public sealed class RemoveProjectPipelineVariableMappingCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<RemoveProjectPipelineVariableMappingCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectPipelineVariableMappingCommand command,
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

        var mappingId = new ProjectPipelineVariableMappingId(command.MappingId);
        var result = group.RemoveMapping(mappingId);
        if (result.IsError)
            return result.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
