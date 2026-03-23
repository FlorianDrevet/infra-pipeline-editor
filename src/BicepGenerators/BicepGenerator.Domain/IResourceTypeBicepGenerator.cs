namespace BicepGenerator.Domain;

public interface IResourceTypeBicepGenerator
{
    string ResourceType { get; }

    /// <summary>
    /// The simple resource type name used for naming template lookups (e.g. "KeyVault").
    /// </summary>
    string ResourceTypeName { get; }

    GeneratedTypeModule Generate(ResourceDefinition resource);
}