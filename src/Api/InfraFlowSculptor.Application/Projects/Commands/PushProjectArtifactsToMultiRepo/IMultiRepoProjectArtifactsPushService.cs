using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>
/// Pushes generated project artifacts to the repositories selected for a split-layout project.
/// </summary>
public interface IMultiRepoProjectArtifactsPushService
{
    /// <summary>
    /// Pushes the latest generated project artifacts to the requested repositories.
    /// </summary>
    /// <param name="command">The command describing which repositories should receive artifacts.</param>
    /// <param name="project">The loaded split-layout project.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The per-repository push result.</returns>
    Task<ErrorOr<PushProjectArtifactsToMultiRepoResult>> PushAsync(
        PushProjectArtifactsToMultiRepoCommand command,
        Project project,
        CancellationToken cancellationToken);
}
