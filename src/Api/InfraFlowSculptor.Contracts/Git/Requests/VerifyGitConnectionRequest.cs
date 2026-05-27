using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Git.Requests;

/// <summary>Request to verify a Git repository connection without an existing project context.</summary>
public sealed class VerifyGitConnectionRequest
{
    /// <summary>Git hosting provider type (e.g. <c>GitHub</c>, <c>AzureDevOps</c>, <c>GitLab</c>, <c>Bitbucket</c>).</summary>
    [Required]
    public required string ProviderType { get; init; }

    /// <summary>Full repository URL to verify.</summary>
    [Required, Url]
    public required string RepositoryUrl { get; init; }

    /// <summary>Personal Access Token used to authenticate against the provider.</summary>
    [Required]
    public required string PersonalAccessToken { get; init; }
}
