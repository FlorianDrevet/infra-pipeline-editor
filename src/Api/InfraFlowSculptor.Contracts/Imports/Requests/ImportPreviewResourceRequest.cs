using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

/// <summary>
/// Represents one parsed preview resource.
/// </summary>
public sealed record ImportPreviewResourceRequest
{
    /// <summary>
    /// Gets the original source resource type.
    /// </summary>
    [Required]
    public required string SourceType { get; init; }

    /// <summary>
    /// Gets the original source resource name.
    /// </summary>
    [Required]
    public required string SourceName { get; init; }

    /// <summary>
    /// Gets the mapped InfraFlowSculptor resource type when one is available.
    /// </summary>
    public string? MappedResourceType { get; init; }

    /// <summary>
    /// Gets the mapped resource name when one is available.
    /// </summary>
    public string? MappedName { get; init; }

    /// <summary>
    /// Gets the mapping confidence string.
    /// </summary>
    [Required]
    public required string Confidence { get; init; }

    /// <summary>
    /// Gets the extracted source properties.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the source properties that could not be mapped.
    /// </summary>
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}
