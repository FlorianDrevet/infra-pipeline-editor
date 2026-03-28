namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents an Azure resource definition with its type, properties, and per-environment config overrides.
/// </summary>
public class ResourceDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;

    /// <summary>
    /// The short resource type abbreviation (e.g. "kv", "redis", "stg")
    /// used in naming functions as the <c>resourceAbbr</c> parameter.
    /// </summary>
    public string ResourceAbbreviation { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, string> Properties { get; set; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Per-environment property overrides. Key = environment name, Value = property overrides for that environment.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> EnvironmentConfigs { get; set; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>();
}
