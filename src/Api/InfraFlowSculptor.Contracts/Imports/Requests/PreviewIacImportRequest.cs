using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

/// <summary>
/// Request body for previewing an IaC import source.
/// </summary>
public sealed class PreviewIacImportRequest
{
    /// <summary>
    /// Gets the source format identifier.
    /// </summary>
    [Required]
    public required string SourceFormat { get; init; }

    /// <summary>
    /// Gets the raw source content.
    /// </summary>
    [Required]
    public required string SourceContent { get; init; }
}