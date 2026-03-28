using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators;

/// <summary>
/// Generates pipeline deployment steps for a specific Azure resource type.
/// Strategy pattern — one implementation per resource type.
/// </summary>
public interface IResourceTypePipelineGenerator
{
    /// <summary>The Azure resource type string (e.g. "Microsoft.KeyVault/vaults").</summary>
    string ResourceType { get; }

    /// <summary>Simple type name (e.g. "KeyVault").</summary>
    string ResourceTypeName { get; }

    /// <summary>Generates deployment steps for a resource of this type.</summary>
    PipelineResourceSteps Generate(ResourceDefinition resource);
}

/// <summary>
/// Deployment steps generated for a single resource.
/// </summary>
public sealed class PipelineResourceSteps
{
    /// <summary>YAML step snippets for deploying this resource.</summary>
    public IReadOnlyList<string> Steps { get; init; } = [];

    /// <summary>Variables needed by the deployment steps.</summary>
    public IReadOnlyDictionary<string, string> Variables { get; init; } =
        new Dictionary<string, string>();
}
