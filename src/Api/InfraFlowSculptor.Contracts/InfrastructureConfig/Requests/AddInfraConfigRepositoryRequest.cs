using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Adds a Git repository to an InfrastructureConfig (project must be in MultiRepo layout).</summary>
public sealed class AddInfraConfigRepositoryRequest
{
    /// <summary>Git provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>).</summary>
    public string ProviderType { get; init; } = string.Empty;

    /// <summary>Full repository URL.</summary>
    public string RepositoryUrl { get; init; } = string.Empty;

    /// <summary>Default branch (e.g. <c>main</c>).</summary>
    public string DefaultBranch { get; init; } = "main";

    /// <summary>Transient personal access token used to verify and persist the repository.</summary>
    [StringLength(2048)]
    public string? PersonalAccessToken { get; init; }

    /// <summary>Content kinds (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    public IReadOnlyList<string> ContentKinds { get; init; } = [];
}
