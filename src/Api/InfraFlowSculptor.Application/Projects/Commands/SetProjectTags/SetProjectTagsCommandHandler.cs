using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;

/// <summary>Handles <see cref="SetProjectTagsCommand"/> by replacing all project-level tags.</summary>
public sealed class SetProjectTagsCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService projectAccessService)
    : ICommandHandler<SetProjectTagsCommand, Updated>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(SetProjectTagsCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);

        var accessResult = await projectAccessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;

        var tags = command.Tags.Select(t => new Tag(t.Name, t.Value)).ToList();
        project.SetTags(tags);
        await projectRepository.UpdateAsync(project);

        return Result.Updated;
    }
}
