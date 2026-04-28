namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Represents the normalized result of previewing an IaC import source.
/// </summary>
public sealed record ImportPreviewAnalysisResult
{
    /// <summary>
    /// Gets the source format identifier.
    /// </summary>
    public required string SourceFormat { get; init; }

    /// <summary>
    /// Gets the resources parsed from the source.
    /// </summary>
    public IReadOnlyList<ImportedResourceAnalysisResult> Resources { get; init; } = [];

    /// <summary>
    /// Gets the dependency relationships parsed from the source.
    /// </summary>
    public IReadOnlyList<ImportedDependencyAnalysisResult> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets the metadata extracted from the source.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the gaps identified during the analysis.
    /// </summary>
    public IReadOnlyList<ImportPreviewGapResult> Gaps { get; init; } = [];

    /// <summary>
    /// Gets the unsupported resources found in the source.
    /// </summary>
    public IReadOnlyList<string> UnsupportedResources { get; init; } = [];

    /// <summary>
    /// Gets the human-readable analysis summary.
    /// </summary>
    public required string Summary { get; init; }
}