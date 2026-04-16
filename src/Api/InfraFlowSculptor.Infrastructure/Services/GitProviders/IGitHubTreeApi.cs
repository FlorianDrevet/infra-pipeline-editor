using Refit;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders;

/// <summary>
/// Refit interface for the GitHub Git Trees API.
/// Backed by <see cref="IHttpClientFactory"/> to avoid socket exhaustion.
/// </summary>
public interface IGitHubTreeApi
{
    /// <summary>
    /// Creates a new tree object in the repository.
    /// </summary>
    [Post("/repos/{owner}/{repo}/git/trees")]
    [Headers("Accept: application/vnd.github+json", "X-GitHub-Api-Version: 2022-11-28")]
    Task<GitHubCreateTreeResponse> CreateTreeAsync(
        string owner,
        string repo,
        [Body] object body,
        [Authorize("Bearer")] string token,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from the GitHub Create Tree API endpoint.
/// </summary>
public sealed record GitHubCreateTreeResponse(string Sha);
