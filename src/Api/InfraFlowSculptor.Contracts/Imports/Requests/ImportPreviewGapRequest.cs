using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

/// <summary>
/// Represents one preview gap.
/// </summary>
public sealed record ImportPreviewGapRequest
{
    /// <summary>
    /// Gets the gap severity string.
    /// </summary>
    [Required]
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the gap category.
    /// </summary>
    [Required]
    public required string Category { get; init; }

    /// <summary>
    /// Gets the gap message.
    /// </summary>
    [Required]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the related source resource name when one is available.
    /// </summary>
    public string? SourceResourceName { get; init; }
}
