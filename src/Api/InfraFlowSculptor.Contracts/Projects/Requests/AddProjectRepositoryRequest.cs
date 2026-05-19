using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to add a new project-level Git repository declaration.</summary>
/// <remarks>
/// Connection details (provider, URL, default branch) are optional: pass them all together to
/// create a fully configured repository, or omit them all to create an unconfigured slot to be
/// completed later via <c>PUT /projects/{id}/repositories/{repoId}</c>.
/// </remarks>
public sealed class AddProjectRepositoryRequest
{
    /// <summary>Git provider type: <c>GitHub</c> or <c>AzureDevOps</c>. Optional.</summary>
    public string? ProviderType { get; init; }

    /// <summary>Full repository URL (e.g. https://github.com/org/repo). Optional.</summary>
    [Url]
    public string? RepositoryUrl { get; init; }

    /// <summary>Default branch name (e.g. <c>main</c>). Optional.</summary>
    [StringLength(200)]
    public string? DefaultBranch { get; init; }

    /// <summary>Transient personal access token used to verify and persist configured repositories.</summary>
    [StringLength(2048)]
    public string? PersonalAccessToken { get; init; }

    /// <summary>List of content kinds hosted by the repository
    /// (<c>Infrastructure</c>, <c>ApplicationCode</c>).</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<string> ContentKinds { get; init; }
}
