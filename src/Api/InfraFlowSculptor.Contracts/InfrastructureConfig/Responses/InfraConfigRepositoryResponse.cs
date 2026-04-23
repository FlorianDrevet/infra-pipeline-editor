namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Representation of a Git repository declared on an Infrastructure Configuration when the parent project layout is MultiRepo.</summary>
/// <param name="Id">Unique identifier of the configuration repository.</param>
/// <param name="Alias">Configuration-scoped alias (slug).</param>
/// <param name="ProviderType">Git hosting provider (e.g. GitHub, AzureDevOps).</param>
/// <param name="RepositoryUrl">Full repository URL.</param>
/// <param name="Owner">Repository owner extracted from the URL.</param>
/// <param name="RepositoryName">Repository name extracted from the URL.</param>
/// <param name="DefaultBranch">Default branch.</param>
/// <param name="ContentKinds">Kinds of content hosted (<c>Infrastructure</c>, <c>ApplicationCode</c>).</param>
public record InfraConfigRepositoryResponse(
    string Id,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string Owner,
    string RepositoryName,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds);
