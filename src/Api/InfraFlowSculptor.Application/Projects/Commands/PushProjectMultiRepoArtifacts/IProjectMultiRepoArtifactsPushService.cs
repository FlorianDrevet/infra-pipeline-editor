using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Pushes generated artifacts for all requested configuration-level repositories.</summary>
public interface IProjectMultiRepoArtifactsPushService
{
    /// <summary>Pushes the artifacts described by a project-level MultiRepo command.</summary>
    /// <param name="command">The configuration and repository push targets.</param>
    /// <param name="project">The loaded MultiRepo project.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The per-repository push outcomes, or an error.</returns>
    Task<ErrorOr<PushProjectMultiRepoArtifactsResult>> PushAsync(
        PushProjectMultiRepoArtifactsCommand command,
        Project project,
        CancellationToken cancellationToken);
}