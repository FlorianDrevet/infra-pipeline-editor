using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;

/// <summary>Handles the <see cref="RemoveProjectResourceAbbreviationCommand"/>.</summary>
public sealed class RemoveProjectResourceAbbreviationCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<RemoveProjectResourceAbbreviationCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectResourceAbbreviationCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (!project.RemoveResourceAbbreviation(command.ResourceType))
            return Errors.Project.ResourceAbbreviationNotFoundError(command.ResourceType);

        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
