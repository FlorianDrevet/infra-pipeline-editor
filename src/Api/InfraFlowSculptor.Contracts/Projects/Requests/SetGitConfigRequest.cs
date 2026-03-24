using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set or update the Git repository configuration on a project.</summary>
public sealed class SetGitConfigRequest
{
    /// <summary>The Git provider type: "GitHub" or "AzureDevOps".</summary>
    [Required]
    public required string ProviderType { get; init; }

    /// <summary>The full repository URL (e.g. https://github.com/org/repo).</summary>
    [Required]
    [Url]
    public required string RepositoryUrl { get; init; }

    /// <summary>The default/base branch name (e.g. "main").</summary>
    [Required]
    public required string DefaultBranch { get; init; }

    /// <summary>Optional sub-path inside the repository where Bicep files should be pushed.</summary>
    public string? BasePath { get; init; }

    /// <summary>The personal access token used to authenticate with the Git provider.</summary>
    [Required]
    public required string PersonalAccessToken { get; init; }
}
