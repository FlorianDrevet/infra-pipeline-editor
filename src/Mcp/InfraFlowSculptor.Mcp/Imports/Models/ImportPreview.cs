using InfraFlowSculptor.Application.Imports.Common;

namespace InfraFlowSculptor.Mcp.Imports.Models;

/// <summary>A stored import preview that can be applied later.</summary>
public sealed class ImportPreview
{
    /// <summary>Unique preview identifier.</summary>
    public required string PreviewId { get; init; }

    /// <summary>The shared analysis payload for the preview.</summary>
    public required ImportPreviewAnalysisResult Analysis { get; init; }
}
