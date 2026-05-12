using ErrorOr;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Errors;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Engine for generating application CI/CD pipeline YAML.
/// Dispatches to the appropriate <see cref="IAppPipelineGenerator"/>
/// based on resource type and deployment mode.
/// </summary>
public sealed class AppPipelineGenerationEngine
{
    private const string PipelineVariableGroupNameMessagePrefix = "Pipeline variable group names";
    private const string UnsupportedAppPipelineMessagePrefix = "Unsupported ";

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
    public ErrorOr<AppPipelineGenerationResult> Generate(AppPipelineGenerationRequest request)
    {
        // Sanitize names that become path segments or YAML references
        request.ResourceName = PathSanitizer.Sanitize(request.ResourceName);
        request.ConfigName = PathSanitizer.Sanitize(request.ConfigName);
        if (request.ApplicationName is not null)
            request.ApplicationName = PathSanitizer.Sanitize(request.ApplicationName);

        if (!DeploymentModes.All.Contains(request.DeploymentMode))
        {
            return GenerationErrors.InvalidDeploymentMode(request.DeploymentMode, DeploymentModes.All);
        }

        var generator = _generators.FirstOrDefault(g =>
            g.ResourceType == request.ResourceType &&
            string.Equals(g.DeploymentMode, request.DeploymentMode, StringComparison.OrdinalIgnoreCase));

        if (generator is null)
        {
            return GenerationErrors.MissingAppPipelineGenerator(request.ResourceType, request.DeploymentMode);
        }

        try
        {
            return generator.Generate(request);
        }
        catch (InvalidOperationException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
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
    public ErrorOr<AppPipelineGenerationResult> GenerateAll(
        IReadOnlyList<AppPipelineGenerationRequest> requests,
        AppPipelineMode mode,
        string configName)
    {
        if (requests.Count == 0)
            return new AppPipelineGenerationResult { Files = new Dictionary<string, string>() };

        if (mode == AppPipelineMode.Isolated)
            return GenerateIsolated(requests);

        return GenerateCombined(requests, configName);
    }

    /// <summary>
    /// Generates the shared application pipeline templates under <c>.azuredevops/{pipelines,jobs,steps}/</c>.
    /// These templates are referenced by per-resource wrapper pipelines via <c>extends:</c>.
    /// </summary>
    /// <returns>Dictionary keyed by repository-relative path → YAML content.</returns>
    public static IReadOnlyDictionary<string, string> GenerateSharedTemplates()
    {
        return AppPipelineTemplatesGenerator.GenerateAll();
    }

    /// <summary>
    /// Generates isolated per-resource pipeline wrappers, each under apps/{appName}/.
    /// Shared templates are emitted separately via <see cref="GenerateSharedTemplates"/>.
    /// </summary>
    private ErrorOr<AppPipelineGenerationResult> GenerateIsolated(
        IReadOnlyList<AppPipelineGenerationRequest> requests)
    {
        var mergedFiles = new Dictionary<string, string>();

        foreach (var request in requests)
        {
            var result = Generate(request);
            if (result.IsError)
                return result.Errors;

            var appName = PathSanitizer.Sanitize(request.ApplicationName ?? request.ResourceName);

            foreach (var (path, content) in result.Value.Files)
            {
                mergedFiles[BuildIsolatedOutputPath(appName, path)] = content;
            }
        }

        return new AppPipelineGenerationResult { Files = mergedFiles };
    }

    /// <summary>
    /// Generates combined per-config pipeline wrappers under apps/{configName}/.
    /// Shared templates are emitted separately via <see cref="GenerateSharedTemplates"/>.
    /// </summary>
    private ErrorOr<AppPipelineGenerationResult> GenerateCombined(
        IReadOnlyList<AppPipelineGenerationRequest> requests,
        string configName)
    {
        configName = PathSanitizer.Sanitize(configName);

        var mergedFiles = new Dictionary<string, string>();

        foreach (var request in requests)
        {
            var result = Generate(request);
            if (result.IsError)
                return result.Errors;

            var appName = PathSanitizer.Sanitize(request.ApplicationName ?? request.ResourceName);

            foreach (var (path, content) in result.Value.Files)
            {
                mergedFiles[BuildCombinedOutputPath(configName, appName, path)] = content;
            }
        }

        return new AppPipelineGenerationResult { Files = mergedFiles };
    }

    private static string BuildIsolatedOutputPath(string appName, string generatedPath)
    {
        var appRelativePath = TrimRedundantAppSegment(appName, generatedPath);
        return $"apps/{appName}/{appRelativePath}";
    }

    private static string BuildCombinedOutputPath(string configName, string appName, string generatedPath)
    {
        var appRelativePath = TrimRedundantAppSegment(appName, generatedPath)
            .Replace('/', '-');

        return $"apps/{configName}/{appName}-{appRelativePath}";
    }

    private static string TrimRedundantAppSegment(string appName, string generatedPath)
    {
        var normalizedPath = generatedPath.TrimStart('/');
        var separatorIndex = normalizedPath.IndexOf('/');
        if (separatorIndex < 0)
            return normalizedPath;

        var firstSegment = normalizedPath[..separatorIndex];
        if (!string.Equals(firstSegment, appName, StringComparison.OrdinalIgnoreCase))
            return normalizedPath;

        return normalizedPath[(separatorIndex + 1)..];
    }

    private static bool TryMapExpectedException(InvalidOperationException exception, out Error error)
    {
        if (exception.Message.StartsWith(PipelineVariableGroupNameMessagePrefix, StringComparison.Ordinal))
        {
            error = GenerationErrors.InvalidPipelineVariableGroupName(exception.Message);
            return true;
        }

        if (exception.Message.StartsWith(UnsupportedAppPipelineMessagePrefix, StringComparison.Ordinal))
        {
            error = GenerationErrors.InvalidAppPipelineConfiguration(exception.Message);
            return true;
        }

        error = default;
        return false;
    }
}
