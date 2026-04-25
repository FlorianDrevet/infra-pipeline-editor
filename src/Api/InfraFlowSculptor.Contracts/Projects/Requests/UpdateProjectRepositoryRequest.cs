using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to update an existing project-level Git repository declaration. Alias is immutable.</summary>
/// <remarks>
/// Pass connection details (provider, URL, default branch) all together to fully configure the slot,
/// or pass them all empty/null to keep the slot in an unconfigured state.
/// </remarks>
public sealed class UpdateProjectRepositoryRequest
{
    /// <summary>Git provider type: <c>GitHub</c> or <c>AzureDevOps</c>. Optional.</summary>
    public string? ProviderType { get; init; }

    /// <summary>Full repository URL (e.g. https://github.com/org/repo). Optional.</summary>
    [Url]
    public string? RepositoryUrl { get; init; }

    /// <summary>Default branch name (e.g. <c>main</c>). Optional.</summary>
    [StringLength(200)]
    public string? DefaultBranch { get; init; }

    /// <summary>List of content kinds hosted by the repository
    /// (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<string> ContentKinds { get; init; }
}
