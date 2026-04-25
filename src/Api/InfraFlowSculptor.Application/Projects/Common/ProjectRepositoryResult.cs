using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Application-layer result describing a project-level Git repository declaration.
/// </summary>
/// <param name="Id">The strongly-typed identifier of the repository.</param>
/// <param name="Alias">The slug-like alias unique inside the parent project.</param>
/// <param name="ProviderType">The Git hosting provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>), or <c>null</c> if the slot is not configured yet.</param>
/// <param name="RepositoryUrl">The full repository URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="Owner">The repository owner extracted from the URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="RepositoryName">The repository name extracted from the URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="DefaultBranch">The default branch name (e.g. <c>main</c>), or <c>null</c> if the slot is not configured yet.</param>
/// <param name="IsConfigured">Whether the repository connection details are fully provided.</param>
/// <param name="ContentKinds">The kinds of content hosted by this repository.</param>
public record ProjectRepositoryResult(
    ProjectRepositoryId Id,
    string Alias,
    string? ProviderType,
    string? RepositoryUrl,
    string? Owner,
    string? RepositoryName,
    string? DefaultBranch,
    bool IsConfigured,
    IReadOnlyList<string> ContentKinds);
