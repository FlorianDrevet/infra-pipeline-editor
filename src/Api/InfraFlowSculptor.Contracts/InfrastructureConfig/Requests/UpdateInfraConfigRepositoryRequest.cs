using System.ComponentModel.DataAnnotations;

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

    /// <summary>Optional transient PAT. When omitted, the existing repository-scoped PAT is reused.</summary>
    [StringLength(2048)]
    public string? PersonalAccessToken { get; init; }

    /// <summary>Content kinds (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    public IReadOnlyList<string> ContentKinds { get; init; } = [];
}
