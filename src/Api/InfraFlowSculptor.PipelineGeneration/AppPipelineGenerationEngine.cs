using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="request"/> contains an unrecognised deployment mode.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no generator is registered for the resource type and deployment mode combination.
    /// </exception>
    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        if (!Enum.TryParse<DeploymentMode.DeploymentModeType>(request.DeploymentMode, ignoreCase: true, out _))
        {
            throw new ArgumentException(
                $"Invalid deployment mode '{request.DeploymentMode}'. Valid values are: {string.Join(", ", Enum.GetNames<DeploymentMode.DeploymentModeType>())}.",
                nameof(request.DeploymentMode));
        }

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

    /// <summary>
    /// Generates application pipelines for multiple compute resources.
    /// In Isolated mode, produces per-resource pipeline files under apps/{applicationName}/.
    /// In Combined mode, produces a single CI + release pipeline with parallel jobs.
    /// </summary>
    /// <param name="requests">The compute resource generation requests.</param>
    /// <param name="mode">The pipeline mode (Isolated or Combined).</param>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <returns>The merged pipeline generation result.</returns>
    public AppPipelineGenerationResult GenerateAll(
        IReadOnlyList<AppPipelineGenerationRequest> requests,
        AppPipelineMode mode,
        string configName)
    {
        if (requests.Count == 0)
            return new AppPipelineGenerationResult { Files = new Dictionary<string, string>() };

        if (mode == AppPipelineMode.Isolated)
            return GenerateIsolated(requests, configName);

        return GenerateCombined(requests, configName);
    }

    /// <summary>
    /// Generates isolated per-resource pipelines, each under apps/{appName}/.
    /// </summary>
    private AppPipelineGenerationResult GenerateIsolated(
        IReadOnlyList<AppPipelineGenerationRequest> requests,
        string configName)
    {
        var mergedFiles = new Dictionary<string, string>();

        foreach (var request in requests)
        {
            var result = Generate(request);
            var appName = request.ApplicationName ?? request.ResourceName;

            foreach (var (path, content) in result.Files)
            {
                mergedFiles[$"apps/{appName}/{path}"] = content;
            }
        }

        return new AppPipelineGenerationResult { Files = mergedFiles };
    }

    /// <summary>
    /// Generates a single combined CI + release pipeline with parallel jobs per resource.
    /// </summary>
    private AppPipelineGenerationResult GenerateCombined(
        IReadOnlyList<AppPipelineGenerationRequest> requests,
        string configName)
    {
        // For combined mode, generate per-resource then merge under a single directory
        var mergedFiles = new Dictionary<string, string>();

        foreach (var request in requests)
        {
            var result = Generate(request);
            var appName = request.ApplicationName ?? request.ResourceName;

            foreach (var (path, content) in result.Files)
            {
                mergedFiles[$"apps/{configName}/{appName}-{path}"] = content;
            }
        }

        return new AppPipelineGenerationResult { Files = mergedFiles };
    }
}
