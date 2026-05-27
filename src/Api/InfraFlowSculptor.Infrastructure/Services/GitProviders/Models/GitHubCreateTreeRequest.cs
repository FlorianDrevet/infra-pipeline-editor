using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders.Models;

/// <summary>
/// Request body sent to the GitHub Create Tree API endpoint.
/// </summary>
public sealed class GitHubCreateTreeRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubCreateTreeRequest"/> class.
    /// </summary>
    /// <param name="baseTree">The SHA of the base tree to update.</param>
    /// <param name="tree">The tree items to create, update, or delete.</param>
    public GitHubCreateTreeRequest(string baseTree, IReadOnlyList<GitHubCreateTreeItem> tree)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseTree);
        ArgumentNullException.ThrowIfNull(tree);

        BaseTree = baseTree;
        Tree = tree.ToArray();
    }

    /// <summary>
    /// Gets the SHA of the base tree to update.
    /// </summary>
    [JsonPropertyName("base_tree")]
    public string BaseTree { get; }

    /// <summary>
    /// Gets the tree items to create, update, or delete.
    /// </summary>
    [JsonPropertyName("tree")]
    public IReadOnlyList<GitHubCreateTreeItem> Tree { get; }
}