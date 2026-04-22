namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents a custom domain binding for Bicep generation.
/// </summary>
public class CustomDomainDefinition
{
    /// <summary>Gets or sets the deployment environment name.</summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>Gets or sets the fully qualified domain name.</summary>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>Gets or sets the SSL binding type ("SniEnabled" or "Disabled").</summary>
    public string BindingType { get; set; } = "SniEnabled";
}
