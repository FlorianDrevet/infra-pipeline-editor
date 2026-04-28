namespace InfraFlowSculptor.Mcp.Imports.Models;

/// <summary>A stored import preview that can be applied later.</summary>
public sealed class ImportPreview
{
    /// <summary>Unique preview identifier.</summary>
    public required string PreviewId { get; init; }

    /// <summary>The canonical project definition.</summary>
    public required ImportedProjectDefinition ProjectDefinition { get; init; }

    /// <summary>Gaps identified during analysis.</summary>
    public required IReadOnlyList<ImportGap> Gaps { get; init; }

    /// <summary>Resources from the source that have no IFS equivalent.</summary>
    public required IReadOnlyList<string> UnsupportedResources { get; init; }
}
