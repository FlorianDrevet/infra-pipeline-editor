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
    public IReadOnlyCollection<EnvironmentDefinition> Environments { get; set; } = [];

    public IReadOnlyCollection<ResourceGroupDefinition> ResourceGroups { get; set; } = [];

    /// <summary>
    /// All environment names defined on the infrastructure configuration.
    /// </summary>
    public IReadOnlyCollection<string> EnvironmentNames { get; set; } = [];

    /// <summary>
    /// Naming templates and resource abbreviations used to generate
    /// naming functions and resolve resource names.
    /// </summary>
    public NamingContext NamingContext { get; set; } = new();

    /// <summary>
    /// All role assignments configured between resources.
    /// </summary>
    public IReadOnlyCollection<RoleAssignmentDefinition> RoleAssignments { get; set; } = [];

    /// <summary>
    /// All app settings (environment variables) configured on compute resources.
    /// </summary>
    public IReadOnlyCollection<AppSettingDefinition> AppSettings { get; set; } = [];

    /// <summary>
    /// Cross-configuration resource references.
    /// </summary>
    public IReadOnlyCollection<ExistingResourceReference> ExistingResourceReferences { get; set; } = [];

    /// <summary>
    /// Azure DevOps Pipeline Variable Groups with their variable-to-Bicep-parameter mappings.
    /// Used by the pipeline generation engine to emit <c>- group:</c> references and <c>overrideParameters</c>.
    /// </summary>
    public IReadOnlyCollection<PipelineVariableGroupDefinition> PipelineVariableGroups { get; set; } = [];

    /// <summary>
    /// Project-level default tags applied to all resources.
    /// </summary>
    public IReadOnlyDictionary<string, string> ProjectTags { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Configuration-level tags that extend or override project-level tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> ConfigTags { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Self-hosted agent pool name. When set, pipelines use <c>pool: name: 'value'</c>.
    /// When <c>null</c>, pipelines use the Microsoft-hosted pool (<c>vmImage: ubuntu-latest</c>).
    /// </summary>
    public string? AgentPoolName { get; set; }

    /// <summary>
    /// Optional sub-path inside the target repository where generated Bicep files are pushed.
    /// Used by generated release pipelines to resolve exact source paths.
    /// </summary>
    public string? BicepBasePath { get; set; }
}
