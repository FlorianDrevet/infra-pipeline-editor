namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents a cross-configuration resource reference that will be generated
/// as an existing resource declaration in deployment artifacts.
/// </summary>
public sealed class ExistingResourceReference
{
    /// <summary>Logical name of the referenced resource.</summary>
    public string ResourceName { get; init; } = string.Empty;

    /// <summary>Simple type name (e.g. "KeyVault", "LogAnalyticsWorkspace").</summary>
    public string ResourceTypeName { get; init; } = string.Empty;

    /// <summary>Azure ARM resource type string (e.g. "Microsoft.KeyVault/vaults").</summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>Name of the resource group in the target configuration.</summary>
    public string ResourceGroupName { get; init; } = string.Empty;

    /// <summary>Resource abbreviation for naming expression (e.g. "kv", "law").</summary>
    public string ResourceAbbreviation { get; init; } = string.Empty;

    /// <summary>Name of the target configuration owning this resource.</summary>
    public string SourceConfigName { get; init; } = string.Empty;
}
