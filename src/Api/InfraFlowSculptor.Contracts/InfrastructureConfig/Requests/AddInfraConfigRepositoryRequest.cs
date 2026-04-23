namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Adds a Git repository to an InfrastructureConfig (project must be in MultiRepo layout).</summary>
public sealed class AddInfraConfigRepositoryRequest
{
    /// <summary>Configuration-scoped alias (lowercase slug).</summary>
    public string Alias { get; init; } = string.Empty;

    /// <summary>Git provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>).</summary>
    public string ProviderType { get; init; } = string.Empty;

    /// <summary>Full repository URL.</summary>
    public string RepositoryUrl { get; init; } = string.Empty;

    /// <summary>Default branch (e.g. <c>main</c>).</summary>
    public string DefaultBranch { get; init; } = "main";

    /// <summary>Content kinds (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    public IReadOnlyList<string> ContentKinds { get; init; } = [];
}
