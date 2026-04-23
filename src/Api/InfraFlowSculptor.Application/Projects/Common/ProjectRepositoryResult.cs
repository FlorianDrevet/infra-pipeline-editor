using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Application-layer result describing a project-level Git repository declaration.
/// </summary>
/// <param name="Id">The strongly-typed identifier of the repository.</param>
/// <param name="Alias">The slug-like alias unique inside the parent project.</param>
/// <param name="ProviderType">The Git hosting provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">The full repository URL.</param>
/// <param name="Owner">The repository owner (org or user) extracted from the URL.</param>
/// <param name="RepositoryName">The repository name extracted from the URL.</param>
/// <param name="DefaultBranch">The default branch name (e.g. <c>main</c>).</param>
/// <param name="ContentKinds">The kinds of content hosted by this repository.</param>
public record ProjectRepositoryResult(
    ProjectRepositoryId Id,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string Owner,
    string RepositoryName,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds);
