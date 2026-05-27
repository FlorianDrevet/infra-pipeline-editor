using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>
/// Handles <see cref="PushProjectArtifactsToMultiRepoCommand"/>.
/// Pushes infra artifacts (Bicep + infra pipeline + bootstrap) and/or app artifacts (app pipeline + app bootstrap) to
/// the requested repositories in independent commits, returning a per-repo success/error result.
/// </summary>
public sealed class PushProjectArtifactsToMultiRepoCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IMultiRepoProjectArtifactsPushService pushService)
    : ICommandHandler<PushProjectArtifactsToMultiRepoCommand, PushProjectArtifactsToMultiRepoResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<PushProjectArtifactsToMultiRepoResult>> Handle(
        PushProjectArtifactsToMultiRepoCommand command,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        if (project.LayoutPreset.Value != LayoutPresetEnum.SplitInfraCode)
            return Errors.GitRouting.LayoutNotSupportedForMultiRepoPush;

        return await pushService.PushAsync(command, project, cancellationToken)
            .ConfigureAwait(false);
    }

}
