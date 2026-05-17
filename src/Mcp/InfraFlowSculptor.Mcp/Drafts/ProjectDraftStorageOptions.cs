using InfraFlowSculptor.Mcp.Common;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>
/// Configures the in-memory storage behavior for MCP project drafts.
/// </summary>
public sealed class ProjectDraftStorageOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = McpOptions.SectionName + ":Drafts";

    /// <summary>The default maximum number of drafts kept in memory.</summary>
    public const int DefaultMaxDraftCount = 100;

    /// <summary>
    /// Gets or sets the maximum number of drafts that can be stored in memory at the same time.
    /// </summary>
    public int MaxDraftCount { get; set; } = DefaultMaxDraftCount;
}
