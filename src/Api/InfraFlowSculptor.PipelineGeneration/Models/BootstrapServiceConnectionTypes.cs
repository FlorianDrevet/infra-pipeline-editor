namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Well-known service connection type identifiers used in bootstrap validation.
/// </summary>
public static class BootstrapServiceConnectionTypes
{
    /// <summary>Azure Resource Manager (ARM) service connection.</summary>
    public const string AzureRM = "AzureRM";

    /// <summary>Docker Registry (ACR) service connection.</summary>
    public const string DockerRegistry = "DockerRegistry";
}
