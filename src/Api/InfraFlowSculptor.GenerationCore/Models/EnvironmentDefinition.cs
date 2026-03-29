namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents an environment with all its deployment-specific variables
/// used for resource naming and location resolution.
/// </summary>
public class EnvironmentDefinition
{
    /// <summary>Gets or sets the environment name (e.g. "dev", "qa", "prod").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the short environment identifier without separators (e.g. "dev", "qa").</summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure location for this environment (e.g. "westeurope").</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw prefix value (e.g. "dev").</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw suffix value (e.g. "dev").</summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure DevOps service connection name used for ARM deployments in this environment.</summary>
    public string? AzureResourceManagerConnection { get; set; }

    /// <summary>Gets or sets the Azure subscription ID for this environment.</summary>
    public string? SubscriptionId { get; set; }
}
