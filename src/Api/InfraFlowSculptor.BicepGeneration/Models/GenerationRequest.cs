namespace InfraFlowSculptor.BicepGeneration.Models;

public class GenerationRequest
{
    public IEnumerable<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();

    /// <summary>
    /// All environment definitions with their name, location, prefix, and suffix.
    /// Used to generate the <c>types.bicep</c> environment variable map
    /// and one <c>.bicepparam</c> per environment.
    /// </summary>
    public IReadOnlyList<EnvironmentDefinition> Environments { get; set; } = [];

    public IReadOnlyList<ResourceGroupDefinition> ResourceGroups { get; set; } = [];

    /// <summary>
    /// All environment names defined on the infrastructure configuration.
    /// Used to generate one <c>.bicepparam</c> per environment.
    /// </summary>
    public IReadOnlyList<string> EnvironmentNames { get; set; } = [];

    /// <summary>
    /// Naming templates and resource abbreviations used to generate
    /// <c>functions.bicep</c> and resolve resource names in <c>main.bicep</c>.
    /// </summary>
    public NamingContext NamingContext { get; set; } = new();

    /// <summary>
    /// All role assignments configured between resources.
    /// Used to generate <c>constants.bicep</c>, role assignment modules, and RBAC declarations in <c>main.bicep</c>.
    /// </summary>
    public IReadOnlyList<RoleAssignmentDefinition> RoleAssignments { get; set; } = [];
}
