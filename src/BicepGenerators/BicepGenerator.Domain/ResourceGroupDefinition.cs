namespace BicepGenerator.Domain;

/// <summary>
/// Represents a resource group with its logical name and location.
/// </summary>
public class ResourceGroupDefinition
{
    /// <summary>Gets or sets the logical resource group name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the Azure location for this resource group.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Gets or sets the resource abbreviation (default: "rg").</summary>
    public string ResourceAbbreviation { get; init; } = "rg";
}
