using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Handles project-level artifact pushes for the MultiRepo layout.</summary>
public sealed class PushProjectMultiRepoArtifactsCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IProjectMultiRepoArtifactsPushService pushService)
    : ICommandHandler<PushProjectMultiRepoArtifactsCommand, PushProjectMultiRepoArtifactsResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushProjectMultiRepoArtifactsResult>> Handle(
        PushProjectMultiRepoArtifactsCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService
            .VerifyWriteAccessAsync(command.ProjectId, cancellationToken)
            .ConfigureAwait(false);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = await projectRepository
            .GetByIdWithAllAsync(command.ProjectId, cancellationToken)
            .ConfigureAwait(false);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.LayoutPreset.Value != LayoutPresetEnum.MultiRepo)
            return Errors.GitRouting.LayoutNotSupportedForProjectMultiRepoPush;

        return await pushService
            .PushAsync(command, project, cancellationToken)
            .ConfigureAwait(false);
    }
}