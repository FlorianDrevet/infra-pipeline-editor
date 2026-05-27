using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to verify a Git repository connection and list branches without saving changes.</summary>
public sealed class VerifyProjectRepositoryConnectionRequest
{
    /// <summary>Git provider type: <c>GitHub</c> or <c>AzureDevOps</c>.</summary>
    [Required]
    public required string ProviderType { get; init; }

    /// <summary>Full repository URL.</summary>
    [Required, Url]
    public required string RepositoryUrl { get; init; }

    /// <summary>Optional transient PAT. Required for create verification; optional for edit verification.</summary>
    [StringLength(2048)]
    public string? PersonalAccessToken { get; init; }
}