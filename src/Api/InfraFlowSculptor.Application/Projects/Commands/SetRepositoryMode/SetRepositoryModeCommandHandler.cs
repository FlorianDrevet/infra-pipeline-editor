using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetRepositoryMode;

/// <summary>Handles the <see cref="SetRepositoryModeCommand"/>.</summary>
public sealed class SetRepositoryModeCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService)
    : ICommandHandler<SetRepositoryModeCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        SetRepositoryModeCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (!Enum.TryParse<RepositoryModeEnum>(command.RepositoryMode, ignoreCase: true, out var modeEnum))
            return Errors.Project.InvalidRepositoryModeError(command.RepositoryMode);

        project.SetRepositoryMode(new RepositoryMode(modeEnum));
        await projectRepository.UpdateAsync(project);

        return Result.Success;
    }
}
