namespace InfraFlowSculptor.Application.Imports.Common;

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
