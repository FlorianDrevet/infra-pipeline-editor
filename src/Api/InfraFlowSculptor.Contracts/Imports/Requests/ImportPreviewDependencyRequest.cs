using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Imports.Requests;

/// <summary>
/// Represents one parsed preview dependency.
/// </summary>
public sealed record ImportPreviewDependencyRequest
{
    /// <summary>
    /// Gets the source resource name.
    /// </summary>
    [Required]
    public required string FromResourceName { get; init; }

    /// <summary>
    /// Gets the target resource name.
    /// </summary>
    [Required]
    public required string ToResourceName { get; init; }

    /// <summary>
    /// Gets the dependency type.
    /// </summary>
    [Required]
    public required string DependencyType { get; init; }
}
