using InfraFlowSculptor.Application.Imports.Common.Constants;

namespace InfraFlowSculptor.Application.Imports.Common.Analysis;

/// <summary>
/// Represents a gap identified during import preview analysis.
/// </summary>
public sealed record ImportPreviewGapResult
{
    /// <summary>
    /// Gets the severity of the gap.
    /// </summary>
    public required ImportPreviewGapSeverity Severity { get; init; }

    /// <summary>
    /// Gets the gap category.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the human-readable gap message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the related source resource name when available.
    /// </summary>
    public string? SourceResourceName { get; init; }
}
