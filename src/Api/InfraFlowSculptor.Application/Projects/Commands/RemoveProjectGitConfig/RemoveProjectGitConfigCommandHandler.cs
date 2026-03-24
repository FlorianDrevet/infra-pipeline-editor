using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectGitConfig;

/// <summary>Handles the <see cref="RemoveProjectGitConfigCommand"/>.</summary>
public sealed class RemoveProjectGitConfigCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : IRequestHandler<RemoveProjectGitConfigCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectGitConfigCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var result = project.RemoveGitRepositoryConfiguration();
        if (result.IsError)
            return result.Errors;

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
