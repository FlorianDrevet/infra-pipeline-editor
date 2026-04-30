namespace InfraFlowSculptor.Contracts.Imports.Responses;

/// <summary>
/// Response returned after previewing an IaC import source.
/// </summary>
/// <param name="SourceFormat">The source format identifier.</param>
/// <param name="ParsedResourceCount">The number of parsed resources.</param>
/// <param name="MappedResources">The mapped resources.</param>
/// <param name="Gaps">The gaps identified during analysis.</param>
/// <param name="UnsupportedResources">The unsupported source resource names.</param>
/// <param name="Dependencies">The parsed dependencies.</param>
/// <param name="Metadata">The source-level metadata.</param>
/// <param name="Summary">The analysis summary.</param>
public sealed record PreviewIacImportResponse(
    string SourceFormat,
    int ParsedResourceCount,
    IReadOnlyList<PreviewIacImportMappedResourceResponseItem> MappedResources,
    IReadOnlyList<PreviewIacImportGapResponseItem> Gaps,
    IReadOnlyList<string> UnsupportedResources,
    IReadOnlyList<PreviewIacImportDependencyResponseItem> Dependencies,
    IReadOnlyDictionary<string, string> Metadata,
    string Summary);

/// <summary>
/// Represents a mapped resource returned by the import preview response.
/// </summary>
/// <param name="SourceType">The original source resource type.</param>
/// <param name="SourceName">The original source resource name.</param>
/// <param name="MappedResourceType">The mapped InfraFlowSculptor resource type.</param>
/// <param name="MappedName">The suggested mapped name.</param>
/// <param name="Confidence">The mapping confidence.</param>
/// <param name="ExtractedProperties">The extracted source properties.</param>
/// <param name="UnmappedProperties">The unmapped source properties.</param>
public sealed record PreviewIacImportMappedResourceResponseItem(
    string SourceType,
    string SourceName,
    string MappedResourceType,
    string MappedName,
    string Confidence,
    IReadOnlyDictionary<string, object?> ExtractedProperties,
    IReadOnlyList<string> UnmappedProperties);

/// <summary>
/// Represents a gap returned by the import preview response.
/// </summary>
/// <param name="Severity">The gap severity.</param>
/// <param name="Category">The gap category.</param>
/// <param name="Message">The gap message.</param>
/// <param name="SourceResourceName">The related source resource name.</param>
public sealed record PreviewIacImportGapResponseItem(
    string Severity,
    string Category,
    string Message,
    string? SourceResourceName);

/// <summary>
/// Represents a dependency returned by the import preview response.
/// </summary>
/// <param name="FromResourceName">The source resource name.</param>
/// <param name="ToResourceName">The target resource name.</param>
/// <param name="DependencyType">The dependency type.</param>
public sealed record PreviewIacImportDependencyResponseItem(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);