using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to update an existing project-level Git repository declaration. Alias is immutable.</summary>
public sealed class UpdateProjectRepositoryRequest
{
    /// <summary>Git provider type: <c>GitHub</c> or <c>AzureDevOps</c>.</summary>
    [Required]
    public required string ProviderType { get; init; }

    /// <summary>Full repository URL (e.g. https://github.com/org/repo).</summary>
    [Required, Url]
    public required string RepositoryUrl { get; init; }

    /// <summary>Default branch name (e.g. <c>main</c>).</summary>
    [Required, StringLength(200)]
    public required string DefaultBranch { get; init; }

    /// <summary>List of content kinds hosted by the repository
    /// (<c>Infrastructure</c>, <c>ApplicationCode</c>, <c>Pipelines</c>).</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<string> ContentKinds { get; init; }
}
