using System.Text.Json;

namespace InfraFlowSculptor.Application.Imports.Common.Arm;

/// <summary>
/// Strongly-typed representation of a single ARM resource entry.
/// </summary>
internal sealed record ArmResource
{
    /// <summary>
    /// Gets the ARM provider resource type (e.g. "Microsoft.KeyVault/vaults").
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the ARM API version for this resource.
    /// </summary>
    public string? ApiVersion { get; init; }

    /// <summary>
    /// Gets the resource name (may contain ARM expressions).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the deployment location.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Gets the resource kind (e.g. "StorageV2", "web").
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>
    /// Gets the resource-level SKU when present.
    /// </summary>
    public ArmSku? Sku { get; init; }

    /// <summary>
    /// Gets the explicit dependency references.
    /// </summary>
    public IReadOnlyList<string>? DependsOn { get; init; }

    /// <summary>
    /// Gets the resource-specific properties bag.
    /// Kept as <see cref="JsonElement"/> because the inner structure varies per resource type.
    /// </summary>
    public JsonElement? Properties { get; init; }
}
