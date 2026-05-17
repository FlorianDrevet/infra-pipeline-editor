using InfraFlowSculptor.Mcp.Common;

namespace InfraFlowSculptor.Mcp.Imports;

/// <summary>
/// Configures the in-memory storage behavior for MCP import previews.
/// </summary>
public sealed class ImportPreviewStorageOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = McpOptions.SectionName + ":Imports";

    /// <summary>The default maximum number of import previews kept in memory.</summary>
    public const int DefaultMaxPreviewCount = 50;

    /// <summary>
    /// Gets or sets the maximum number of import previews that can be stored in memory at the same time.
    /// </summary>
    public int MaxPreviewCount { get; set; } = DefaultMaxPreviewCount;
}
