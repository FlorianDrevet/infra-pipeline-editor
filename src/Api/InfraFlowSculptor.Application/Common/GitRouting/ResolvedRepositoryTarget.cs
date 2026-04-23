using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.GitRouting;

/// <summary>
/// Immutable description of the concrete Git repository (and path) that a generator or
/// pusher must target. Produced by <see cref="IRepositoryTargetResolver"/> after combining
/// the <see cref="Domain.InfrastructureConfigAggregate.ValueObjects.RepositoryBinding"/> overrides
/// with the project-level <see cref="Domain.ProjectAggregate.Entities.ProjectRepository"/> definition.
/// </summary>
/// <param name="Alias">
/// The alias of the resolved <c>ProjectRepository</c>.
/// </param>
/// <param name="ProviderType">The Git hosting provider (GitHub, AzureDevOps, …).</param>
/// <param name="RepositoryUrl">Full repository URL, normalized (no trailing slash).</param>
/// <param name="Owner">Repository owner (org/user) extracted from the URL.</param>
/// <param name="RepositoryName">Repository name extracted from the URL.</param>
/// <param name="Branch">Effective branch (binding override, then repository default).</param>
/// <param name="BasePath">
/// Optional sub-path inside the repository for infrastructure artifacts.
/// Populated for <see cref="ArtifactKind.Infrastructure"/> and also kept for pipeline artifacts
/// since release pipelines reference the infra path.
/// </param>
/// <param name="PipelineBasePath">
/// Optional sub-path inside the repository for pipeline artifacts
/// (only populated for <see cref="ArtifactKind.Pipeline"/> and <see cref="ArtifactKind.Bootstrap"/>).
/// </param>
/// <param name="PatSecretName">
/// Optional Key Vault secret name storing the Personal Access Token used to authenticate.
/// Reserved for a future PAT-per-repository feature; currently always <c>null</c>.
/// </param>
public sealed record ResolvedRepositoryTarget(
    string Alias,
    GitProviderType ProviderType,
    string RepositoryUrl,
    string Owner,
    string RepositoryName,
    string Branch,
    string? BasePath,
    string? PipelineBasePath,
    string? PatSecretName);
