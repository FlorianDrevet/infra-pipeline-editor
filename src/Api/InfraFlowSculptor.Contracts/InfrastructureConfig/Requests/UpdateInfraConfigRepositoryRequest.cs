namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Updates an existing InfraConfigRepository.</summary>
public sealed class UpdateInfraConfigRepositoryRequest
{
    /// <summary>Git provider type.</summary>
    public string ProviderType { get; init; } = string.Empty;

    /// <summary>Full repository URL.</summary>
    public string RepositoryUrl { get; init; } = string.Empty;

    /// <summary>Default branch.</summary>
    public string DefaultBranch { get; init; } = "main";

    /// <summary>Content kinds (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    public IReadOnlyList<string> ContentKinds { get; init; } = [];
}
