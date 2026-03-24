using System.Text.RegularExpressions;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Stores the configuration needed to push generated Bicep files to a remote Git repository.
/// Owned by a single <see cref="Project"/> (0..1 relationship).
/// </summary>
public sealed partial class GitRepositoryConfiguration : Entity<GitRepositoryConfigurationId>
{
    /// <summary>Gets the Git hosting provider type.</summary>
    public GitProviderType ProviderType { get; private set; } = null!;

    /// <summary>Gets the full repository URL (e.g. https://github.com/org/repo).</summary>
    public string RepositoryUrl { get; private set; } = null!;

    /// <summary>Gets the default/base branch name (e.g. "main").</summary>
    public string DefaultBranch { get; private set; } = null!;

    /// <summary>Gets the optional sub-path inside the repository where Bicep files should be pushed.</summary>
    public string? BasePath { get; private set; }

    /// <summary>Gets the repository owner (org or user, extracted from the URL).</summary>
    public string Owner { get; private set; } = null!;

    /// <summary>Gets the repository name (extracted from the URL).</summary>
    public string RepositoryName { get; private set; } = null!;

    /// <summary>Gets the parent project identifier.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private GitRepositoryConfiguration() { }

    /// <summary>Creates a new <see cref="GitRepositoryConfiguration"/> with a generated identifier.</summary>
    public static GitRepositoryConfiguration Create(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        string? basePath,
        ProjectId projectId)
    {
        var (owner, repoName) = ExtractOwnerAndRepo(providerType, repositoryUrl);

        return new GitRepositoryConfiguration
        {
            Id = GitRepositoryConfigurationId.CreateUnique(),
            ProviderType = providerType,
            RepositoryUrl = repositoryUrl.TrimEnd('/'),
            DefaultBranch = defaultBranch,
            BasePath = NormalizeBasePath(basePath),
            Owner = owner,
            RepositoryName = repoName,
            ProjectId = projectId,
        };
    }

    /// <summary>Updates this configuration in place.</summary>
    public void Update(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        string? basePath)
    {
        var (owner, repoName) = ExtractOwnerAndRepo(providerType, repositoryUrl);

        ProviderType = providerType;
        RepositoryUrl = repositoryUrl.TrimEnd('/');
        DefaultBranch = defaultBranch;
        BasePath = NormalizeBasePath(basePath);
        Owner = owner;
        RepositoryName = repoName;
    }

    private static (string Owner, string RepositoryName) ExtractOwnerAndRepo(
        GitProviderType providerType, string repositoryUrl)
    {
        var url = repositoryUrl.TrimEnd('/');
        if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            url = url[..^4];

        if (providerType.Value == GitProviderTypeEnum.GitHub)
        {
            // Expected: https://github.com/{owner}/{repo}
            var match = GitHubUrlRegex().Match(url);
            if (match.Success)
                return (match.Groups["owner"].Value, match.Groups["repo"].Value);
        }
        else
        {
            // Expected: https://dev.azure.com/{org}/{project}/_git/{repo}
            var match = AzureDevOpsUrlRegex().Match(url);
            if (match.Success)
                return ($"{match.Groups["org"].Value}/{match.Groups["project"].Value}", match.Groups["repo"].Value);
        }

        // Fallback: last two segments
        var segments = new Uri(url).AbsolutePath.Trim('/').Split('/');
        return segments.Length >= 2
            ? (segments[^2], segments[^1])
            : (string.Empty, segments.Length > 0 ? segments[^1] : string.Empty);
    }

    private static string? NormalizeBasePath(string? basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            return null;

        return basePath.Trim('/');
    }

    [GeneratedRegex(@"github\.com/(?<owner>[^/]+)/(?<repo>[^/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex GitHubUrlRegex();

    [GeneratedRegex(@"dev\.azure\.com/(?<org>[^/]+)/(?<project>[^/]+)/_git/(?<repo>[^/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AzureDevOpsUrlRegex();
}
