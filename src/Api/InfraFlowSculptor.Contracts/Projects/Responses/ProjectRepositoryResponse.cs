namespace InfraFlowSculptor.Contracts.Projects.Responses;

/// <summary>Project-level Git repository declaration.</summary>
/// <param name="Id">Unique identifier of the repository declaration.</param>
/// <param name="Alias">Project-scoped slug-like alias (unique per project).</param>
/// <param name="ProviderType">Git hosting provider type (<c>GitHub</c> or <c>AzureDevOps</c>).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="Owner">Repository owner (org or user) extracted from the URL.</param>
/// <param name="RepositoryName">Repository name extracted from the URL.</param>
/// <param name="DefaultBranch">Default branch name (e.g. <c>main</c>).</param>
/// <param name="ContentKinds">List of content kinds hosted by the repository
/// (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>, <c>Pipelines</c>).</param>
public record ProjectRepositoryResponse(
    string Id,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string Owner,
    string RepositoryName,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds);
