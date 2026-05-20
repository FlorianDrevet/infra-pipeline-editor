namespace InfraFlowSculptor.Application.Imports.Common.Arm;

/// <summary>
/// Strongly-typed representation of an ARM SKU block.
/// </summary>
internal sealed record ArmSku
{
    /// <summary>
    /// Gets the SKU name (e.g. "standard", "Standard_LRS", "B1").
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the SKU family (e.g. "A").
    /// </summary>
    public string? Family { get; init; }

    /// <summary>
    /// Gets the SKU tier (e.g. "Basic", "Standard").
    /// </summary>
    public string? Tier { get; init; }
}
