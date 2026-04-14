using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;

namespace InfraFlowSculptor.Application.Projects.Commands.DeleteProject;

/// <summary>Handles the <see cref="DeleteProjectCommand"/>.</summary>
public sealed class DeleteProjectCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<DeleteProjectCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteProjectCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        await projectRepository.DeleteAsync(command.ProjectId);

        return Result.Deleted;
    }
}
