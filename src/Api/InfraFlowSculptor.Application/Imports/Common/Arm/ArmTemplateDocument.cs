using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Application.Imports.Common.Arm;

/// <summary>
/// Strongly-typed representation of an ARM JSON deployment template.
/// </summary>
internal sealed record ArmTemplateDocument
{
    /// <summary>
    /// Gets the ARM template schema URI.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>
    /// Gets the ARM template content version.
    /// </summary>
    public string? ContentVersion { get; init; }

    /// <summary>
    /// Gets the resources defined in the template.
    /// </summary>
    public IReadOnlyList<ArmResource>? Resources { get; init; }
}
