namespace BicepGenerator.Domain;

public class GenerationRequest
{
    public IEnumerable<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();
    public EnvironmentDefinition Environment { get; set; } 
}