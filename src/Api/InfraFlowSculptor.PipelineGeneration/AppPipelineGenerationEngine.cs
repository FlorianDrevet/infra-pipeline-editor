using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Engine for generating application CI/CD pipeline YAML.
/// Dispatches to the appropriate <see cref="IAppPipelineGenerator"/>
/// based on resource type and deployment mode.
/// </summary>
public sealed class AppPipelineGenerationEngine
{
    private readonly IReadOnlyList<IAppPipelineGenerator> _generators;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppPipelineGenerationEngine"/> class.
    /// </summary>
    /// <param name="generators">The registered application pipeline generators.</param>
    public AppPipelineGenerationEngine(IEnumerable<IAppPipelineGenerator> generators)
    {
        _generators = generators.ToList();
    }

    /// <summary>
    /// Generates application pipeline YAML for the given request.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>Generated YAML files keyed by relative path.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no generator is registered for the resource type and deployment mode combination.
    /// </exception>
    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        var generator = _generators.FirstOrDefault(g =>
            g.ResourceType == request.ResourceType &&
            string.Equals(g.DeploymentMode, request.DeploymentMode, StringComparison.OrdinalIgnoreCase));

        if (generator is null)
        {
            throw new InvalidOperationException(
                $"No application pipeline generator registered for resource type '{request.ResourceType}' with deployment mode '{request.DeploymentMode}'.");
        }

        return generator.Generate(request);
    }
}
