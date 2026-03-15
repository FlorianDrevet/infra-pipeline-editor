namespace BicepGenerator.Domain;

public interface IResourceTypeBicepGenerator
{
    string ResourceType { get; }

    GeneratedTypeModule Generate(
        ResourceDefinition resource,
        EnvironmentDefinition environment);
}