using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders.Models;

/// <summary>
/// Response returned by the GitHub Create Tree API endpoint.
/// </summary>
public sealed class GitHubCreateTreeResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubCreateTreeResponse"/> class.
    /// </summary>
    /// <param name="sha">The SHA of the created tree.</param>
    [JsonConstructor]
    public GitHubCreateTreeResponse(string sha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha);

        Sha = sha;
    }

    /// <summary>
    /// Gets the SHA of the created tree.
    /// </summary>
    [JsonPropertyName("sha")]
    public string Sha { get; }
}