using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;

/// <summary>Handles the <see cref="RemoveProjectEnvironmentCommand"/>.</summary>
public sealed class RemoveProjectEnvironmentCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : IRequestHandler<RemoveProjectEnvironmentCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectEnvironmentCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var removed = project.RemoveEnvironment(command.EnvironmentId);
        if (!removed)
            return Errors.Project.NotFoundError(command.ProjectId);

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
