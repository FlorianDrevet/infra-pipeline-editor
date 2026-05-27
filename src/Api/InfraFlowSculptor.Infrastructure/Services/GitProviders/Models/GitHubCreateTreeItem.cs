using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Infrastructure.Services.GitProviders.Models;

/// <summary>
/// Represents a single item in a GitHub Create Tree request.
/// </summary>
[JsonConverter(typeof(GitHubCreateTreeItemJsonConverter))]
public sealed class GitHubCreateTreeItem
{
    private const string BlobMode = "100644";
    private const string BlobType = "blob";
    private const string ContentJsonPropertyName = "content";
    private const string ModeJsonPropertyName = "mode";
    private const string PathJsonPropertyName = "path";
    private const string ShaJsonPropertyName = "sha";
    private const string TypeJsonPropertyName = "type";

    private readonly bool _serializeSha;

    private GitHubCreateTreeItem(
        string path,
        string mode,
        string type,
        string? content,
        string? sha,
        bool serializeSha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(mode);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        Path = path;
        Mode = mode;
        Type = type;
        Content = content;
        Sha = sha;
        _serializeSha = serializeSha;
    }

    /// <summary>
    /// Gets the repository-relative path of the tree item.
    /// </summary>
    [JsonPropertyName(PathJsonPropertyName)]
    public string Path { get; }

    /// <summary>
    /// Gets the Git file mode for the item.
    /// </summary>
    [JsonPropertyName(ModeJsonPropertyName)]
    public string Mode { get; }

    /// <summary>
    /// Gets the Git object type for the item.
    /// </summary>
    [JsonPropertyName(TypeJsonPropertyName)]
    public string Type { get; }

    /// <summary>
    /// Gets the inline content to write for create or update operations.
    /// </summary>
    [JsonPropertyName(ContentJsonPropertyName)]
    public string? Content { get; }

    /// <summary>
    /// Gets the SHA value used by GitHub for delete operations.
    /// </summary>
    [JsonPropertyName(ShaJsonPropertyName)]
    public string? Sha { get; }

    /// <summary>
    /// Creates a tree item that writes blob content.
    /// </summary>
    /// <param name="path">The repository-relative file path.</param>
    /// <param name="content">The file content to write.</param>
    /// <returns>A tree item for a create or update operation.</returns>
    public static GitHubCreateTreeItem CreateBlob(string path, string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new GitHubCreateTreeItem(path, BlobMode, BlobType, content, sha: null, serializeSha: false);
    }

    /// <summary>
    /// Creates a tree item that deletes an existing blob.
    /// </summary>
    /// <param name="path">The repository-relative file path to delete.</param>
    /// <returns>A tree item for a delete operation.</returns>
    public static GitHubCreateTreeItem DeleteBlob(string path) =>
        new(path, BlobMode, BlobType, content: null, sha: null, serializeSha: true);

    private sealed class GitHubCreateTreeItemJsonConverter : JsonConverter<GitHubCreateTreeItem>
    {
        public override GitHubCreateTreeItem Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("GitHub create-tree items are only serialized by this client.");

        public override void Write(
            Utf8JsonWriter writer,
            GitHubCreateTreeItem value,
            JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(value);

            writer.WriteStartObject();
            writer.WriteString(PathJsonPropertyName, value.Path);
            writer.WriteString(ModeJsonPropertyName, value.Mode);
            writer.WriteString(TypeJsonPropertyName, value.Type);

            if (value.Content is not null)
            {
                writer.WriteString(ContentJsonPropertyName, value.Content);
            }

            if (value._serializeSha)
            {
                if (value.Sha is null)
                {
                    writer.WriteNull(ShaJsonPropertyName);
                }
                else
                {
                    writer.WriteString(ShaJsonPropertyName, value.Sha);
                }
            }

            writer.WriteEndObject();
        }
    }
}