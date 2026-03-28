namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Holds naming template information used to generate naming functions.
/// Contains the default template and optional per-resource-type overrides.
/// </summary>
public sealed class NamingContext
{
    /// <summary>
    /// The default naming template applied to all resource types unless overridden.
    /// Uses placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {resourceAbbr}, {location}.
    /// Example: "{name}-{resourceAbbr}{suffix}".
    /// When <c>null</c>, resource names are used as-is.
    /// </summary>
    public string? DefaultTemplate { get; init; }

    /// <summary>
    /// Per-resource-type naming template overrides.
    /// Key = resource type name (e.g. "ResourceGroup", "StorageAccount"),
    /// Value = naming template string.
    /// </summary>
    public IReadOnlyDictionary<string, string> ResourceTemplates { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Maps resource type names to their standard abbreviations
    /// (e.g. "KeyVault" → "kv", "ResourceGroup" → "rg").
    /// Used when calling naming functions.
    /// </summary>
    public IReadOnlyDictionary<string, string> ResourceAbbreviations { get; init; } =
        new Dictionary<string, string>();
}
