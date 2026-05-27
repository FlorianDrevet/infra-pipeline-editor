namespace InfraFlowSculptor.Mcp.Drafts.Models;

/// <summary>An inferred repository from the user's prompt.</summary>
public sealed class DraftRepositoryIntent
{
    /// <summary>Content kinds hosted by this repository.</summary>
    public List<string> ContentKinds { get; set; } = [];

    /// <summary>Optional provider type (GitHub, AzureDevOps).</summary>
    public string? ProviderType { get; set; }

    /// <summary>Optional repository URL.</summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>Optional default branch name.</summary>
    public string? DefaultBranch { get; set; }

    /// <summary>Optional personal access token for repository authentication.</summary>
    public string? PersonalAccessToken { get; set; }
}
