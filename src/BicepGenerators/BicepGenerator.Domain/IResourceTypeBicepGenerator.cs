namespace BicepGenerator.Domain;

public interface IResourceTypeBicepGenerator
{
    string ResourceType { get; }

    GeneratedTypeModule Generate(
        IReadOnlyCollection<ResourceDefinition> resources,
        EnvironmentDefinition environment);
}