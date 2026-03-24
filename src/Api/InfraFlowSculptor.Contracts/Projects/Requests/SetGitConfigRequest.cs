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

    /// <summary>The Azure Key Vault URL that stores the authentication token.</summary>
    [Required]
    [Url]
    public required string KeyVaultUrl { get; init; }

    /// <summary>The secret name inside the Key Vault that holds the PAT / token.</summary>
    [Required]
    public required string SecretName { get; init; }
}
