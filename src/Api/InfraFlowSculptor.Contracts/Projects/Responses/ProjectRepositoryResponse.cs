namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Project-level Git repository declaration.</summary>
/// <param name="Id">Unique identifier of the repository declaration.</param>
/// <param name="Alias">Project-scoped slug-like alias (unique per project).</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>), or <c>null</c> if the slot is not configured yet.</param>
/// <param name="RepositoryUrl">Full repository URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="Owner">Repository owner extracted from the URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="RepositoryName">Repository name extracted from the URL, or <c>null</c> if the slot is not configured yet.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>), or <c>null</c> if the slot is not configured yet.</param>
/// <param name="IsConfigured">Whether the repository connection details (provider, URL, default branch) are fully provided.</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository
/// (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>).</param>
public record ProjectRepositoryResponse(
    string Id,
    string Alias,
    string? ProviderType,
    string? RepositoryUrl,
    string? Owner,
    string? RepositoryName,
    string? DefaultBranch,
    bool IsConfigured,
    IReadOnlyList<string> ContentKinds);
