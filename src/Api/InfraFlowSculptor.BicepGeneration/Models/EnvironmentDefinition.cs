namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Represents an environment with all its deployment-specific variables
/// used for Bicep resource naming and location resolution.
/// </summary>
public class EnvironmentDefinition
{
    /// <summary>Gets or sets the environment name (e.g. "dev", "qa", "prod").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure location for this environment (e.g. "westeurope").</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw prefix value (e.g. "dev").</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw suffix value (e.g. "dev").</summary>
    public string Suffix { get; set; } = string.Empty;
}
