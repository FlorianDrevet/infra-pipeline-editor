using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators;

/// <summary>
/// Generates application CI/CD pipeline YAML for a specific combination of
/// Azure resource type and deployment mode (Container or Code).
/// </summary>
public interface IAppPipelineGenerator
{
    /// <summary>Azure resource type this generator handles (from <see cref="GenerationCore.AzureResourceTypes"/>).</summary>
    string ResourceType { get; }

    /// <summary>Deployment mode: "Code" or "Container".</summary>
    string DeploymentMode { get; }

    /// <summary>Generates the application pipeline YAML files.</summary>
    /// <param name="request">The generation request containing resource and environment data.</param>
    /// <returns>Generated YAML files keyed by relative path.</returns>
    AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request);
}
