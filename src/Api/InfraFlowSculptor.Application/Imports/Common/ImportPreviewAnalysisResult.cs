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

/// <summary>
/// Represents a parsed resource in an import preview analysis.
/// </summary>
public sealed record ImportedResourceAnalysisResult
{
    /// <summary>
    /// Gets the original source resource type.
    /// </summary>
    public required string SourceType { get; init; }

    /// <summary>
    /// Gets the original source resource name.
    /// </summary>
    public required string SourceName { get; init; }

    /// <summary>
    /// Gets the mapped InfraFlowSculptor resource type when supported.
    /// </summary>
    public string? MappedResourceType { get; init; }

    /// <summary>
    /// Gets the suggested mapped resource name when supported.
    /// </summary>
    public string? MappedName { get; init; }

    /// <summary>
    /// Gets the confidence level of the mapping.
    /// </summary>
    public ImportPreviewMappingConfidence Confidence { get; init; } = ImportPreviewMappingConfidence.High;

    /// <summary>
    /// Gets the properties extracted from the source resource.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the source properties that could not be mapped.
    /// </summary>
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}

/// <summary>
/// Defines mapping confidence levels for import preview resources.
/// </summary>
public enum ImportPreviewMappingConfidence
{
    /// <summary>
    /// A high-confidence mapping.
    /// </summary>
    High,

    /// <summary>
    /// A medium-confidence mapping.
    /// </summary>
    Medium,

    /// <summary>
    /// A low-confidence mapping.
    /// </summary>
    Low,
}

/// <summary>
/// Represents a dependency between two parsed resources.
/// </summary>
/// <param name="FromResourceName">The source resource that depends on another resource.</param>
/// <param name="ToResourceName">The target resource name.</param>
/// <param name="DependencyType">The dependency type.</param>
public sealed record ImportedDependencyAnalysisResult(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);

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

/// <summary>
/// Defines gap severity levels for import preview analysis.
/// </summary>
public enum ImportPreviewGapSeverity
{
    /// <summary>
    /// Informational severity.
    /// </summary>
    Info,

    /// <summary>
    /// Warning severity.
    /// </summary>
    Warning,

    /// <summary>
    /// Error severity.
    /// </summary>
    Error,
}