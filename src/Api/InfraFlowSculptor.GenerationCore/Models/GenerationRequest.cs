namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Shared generation request containing all infrastructure configuration data
/// required by both Bicep and Pipeline generation engines.
/// </summary>
public class GenerationRequest
{
    public IEnumerable<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();

    /// <summary>
    /// All environment definitions with their name, location, prefix, and suffix.
    /// </summary>
    public IReadOnlyList<EnvironmentDefinition> Environments { get; set; } = [];

    public IReadOnlyList<ResourceGroupDefinition> ResourceGroups { get; set; } = [];

    /// <summary>
    /// All environment names defined on the infrastructure configuration.
    /// </summary>
    public IReadOnlyList<string> EnvironmentNames { get; set; } = [];

    /// <summary>
    /// Naming templates and resource abbreviations used to generate
    /// naming functions and resolve resource names.
    /// </summary>
    public NamingContext NamingContext { get; set; } = new();

    /// <summary>
    /// All role assignments configured between resources.
    /// </summary>
    public IReadOnlyList<RoleAssignmentDefinition> RoleAssignments { get; set; } = [];

    /// <summary>
    /// All app settings (environment variables) configured on compute resources.
    /// </summary>
    public IReadOnlyList<AppSettingDefinition> AppSettings { get; set; } = [];

    /// <summary>
    /// Cross-configuration resource references.
    /// </summary>
    public IReadOnlyList<ExistingResourceReference> ExistingResourceReferences { get; set; } = [];
}
