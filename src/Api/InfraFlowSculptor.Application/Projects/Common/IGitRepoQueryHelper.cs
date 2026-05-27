using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Resolves the common git-repo query context (access, project, config, target, secret, provider)
/// shared by multiple query handlers.
/// </summary>
public interface IGitRepoQueryHelper
{
    /// <summary>
    /// Verifies read access, loads the project and optional config, resolves the repository target,
    /// fetches the PAT secret, and creates the appropriate git provider.
    /// </summary>
    Task<ErrorOr<GitRepoQueryContext>> ResolveAsync(
        ProjectId projectId,
        InfrastructureConfigId? configId,
        CancellationToken cancellationToken);
}
