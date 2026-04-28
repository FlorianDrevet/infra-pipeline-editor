using System.Text.RegularExpressions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Common;

/// <summary>
/// Helper for parsing Git repository URLs (GitHub and Azure DevOps) into their
/// owner / repository components, and for normalizing optional sub-paths.
/// Shared by <see cref="Entities.ProjectRepository"/>.
/// </summary>
internal static class RepositoryUrlHelper
{
    private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromMilliseconds(250);

    private static readonly Regex GitHubUrlRegex =
        new(@"github\.com/(?<owner>[^/]+)/(?<repo>[^/]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexMatchTimeout);

    private static readonly Regex AzureDevOpsUrlRegex =
        new(@"dev\.azure\.com/(?<org>[^/]+)/(?<project>[^/]+)/_git/(?<repo>[^/]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexMatchTimeout);

    /// <summary>
    /// Extracts the owner (org/user or "org/project" for Azure DevOps) and repository name
    /// from a Git repository URL.
    /// Falls back to the last two URL segments when no provider-specific pattern matches.
    /// </summary>
    /// <param name="providerType">The Git provider hosting the repository.</param>
    /// <param name="repositoryUrl">The full repository URL.</param>
    /// <returns>A tuple containing the owner and the repository name.</returns>
    public static (string Owner, string RepositoryName) ExtractOwnerAndRepo(
        GitProviderType providerType, string repositoryUrl)
    {
        var url = repositoryUrl.TrimEnd('/');
        if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            url = url[..^4];

        if (providerType.Value == GitProviderTypeEnum.GitHub)
        {
            var match = GitHubUrlRegex.Match(url);
            if (match.Success)
                return (match.Groups["owner"].Value, match.Groups["repo"].Value);
        }
        else
        {
            var match = AzureDevOpsUrlRegex.Match(url);
            if (match.Success)
                return ($"{match.Groups["org"].Value}/{match.Groups["project"].Value}", match.Groups["repo"].Value);
        }

        var segments = new Uri(url).AbsolutePath.Trim('/').Split('/');
        return segments.Length >= 2
            ? (segments[^2], segments[^1])
            : (string.Empty, segments.Length > 0 ? segments[^1] : string.Empty);
    }

    /// <summary>Trims surrounding slashes from an optional sub-path. Returns <c>null</c> for empty/whitespace input.</summary>
    /// <param name="basePath">The path to normalize.</param>
    /// <returns>The normalized sub-path or <c>null</c>.</returns>
    public static string? NormalizeBasePath(string? basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            return null;

        return basePath.Trim('/');
    }
}
