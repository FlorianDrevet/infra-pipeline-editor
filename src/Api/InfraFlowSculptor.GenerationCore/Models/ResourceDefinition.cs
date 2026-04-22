namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents an Azure resource definition with its type, properties, and per-environment config overrides.
/// </summary>
public class ResourceDefinition
{
    /// <summary>The unique identifier of this resource, used to resolve cross-resource references.</summary>
    public Guid ResourceId { get; set; }

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

    /// <summary>
    /// The Bicep identifier of the User-Assigned Identity explicitly assigned to this resource, if any.
    /// When set, the resource's identity block must include 'UserAssigned' even without UA role assignments.
    /// </summary>
    public string? AssignedUserAssignedIdentityName { get; set; }

    /// <summary>
    /// Custom domain bindings configured on this resource across all environments.
    /// Only applicable to compute resources (ContainerApp, WebApp, FunctionApp).
    /// </summary>
    public IReadOnlyList<CustomDomainDefinition> CustomDomains { get; set; } = [];
}
