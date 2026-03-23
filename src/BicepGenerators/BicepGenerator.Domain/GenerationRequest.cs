namespace BicepGenerator.Domain;

public class GenerationRequest
{
    public IEnumerable<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();
    public EnvironmentDefinition Environment { get; set; }
    public IReadOnlyList<ResourceGroupDefinition> ResourceGroups { get; set; } = [];

    /// <summary>
    /// All environment names defined on the infrastructure configuration.
    /// Used to generate one <c>.bicepparam</c> per environment.
    /// </summary>
    public IReadOnlyList<string> EnvironmentNames { get; set; } = [];
}