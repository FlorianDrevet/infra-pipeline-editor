using System.Text;
using ErrorOr;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Errors;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Infra;
using InfraFlowSculptor.PipelineGeneration.Infra.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;
using InfraFlowSculptor.PipelineGeneration.SharedTemplates;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Main engine for Azure DevOps pipeline YAML generation.
/// Delegates per-configuration file generation to <see cref="InfraPipeline"/>.
/// Shared templates are generated via the static <see cref="GenerateSharedTemplates"/> method.
/// </summary>
public sealed class PipelineGenerationEngine
{
    private const string PipelineVariableGroupNameMessagePrefix = "Pipeline variable group names";

    private readonly InfraPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance using the provided pipeline orchestrator (DI path).
    /// </summary>
    /// <param name="pipeline">The pipeline that orchestrates the infrastructure stages.</param>
    public PipelineGenerationEngine(InfraPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Parameterless constructor for backward compatibility and direct test instantiation.
    /// Creates an internal pipeline with all stages in default order.
    /// </summary>
    public PipelineGenerationEngine()
        : this(CreateDefaultPipeline())
    {
    }

    private static InfraPipeline CreateDefaultPipeline() => new(new IInfraPipelineStage[]
    {
        new CiPipelineStage(),
        new PrPipelineStage(),
        new ReleasePipelineStage(),
        new ConfigVarsStage(),
        new EnvironmentVarsStage(),
    });

    /// <summary>
    /// Appends the pool section to the YAML builder.
    /// When <paramref name="agentPoolName"/> is set, uses <c>pool: name: 'value'</c> (self-hosted).
    /// Otherwise, uses <c>pool: vmImage: 'ubuntu-latest'</c> (Microsoft-hosted).
    /// </summary>
    internal static void AppendPool(StringBuilder sb, string? agentPoolName, string indent = "        ")
    {
        sb.AppendLine($"{indent}pool:");
        if (!string.IsNullOrEmpty(agentPoolName))
        {
            sb.AppendLine($"{indent}  name: '{agentPoolName}'");
        }
        else
        {
            sb.AppendLine($"{indent}  vmImage: 'ubuntu-latest'");
        }
    }

    /// <summary>
    /// Generates per-configuration pipeline files (ci.pipeline.yml, release.pipeline.yml, variables/).
    /// </summary>
    /// <param name="request">The shared generation request containing infrastructure configuration data.</param>
    /// <param name="configName">The name of the infrastructure configuration.</param>
    /// <param name="isMonoRepo">When <c>true</c>, skips per-config variables folder (variables are shared at root level).</param>
    /// <returns>The generated per-config pipeline files.</returns>
    public ErrorOr<PipelineGenerationResult> Generate(GenerationRequest request, string configName, bool isMonoRepo = false)
    {
        try
        {
            configName = PathSanitizer.Sanitize(configName);

            var context = new InfraPipelineContext
            {
                Request = request,
                ConfigName = configName,
                IsMonoRepo = isMonoRepo,
            };

            _pipeline.Execute(context);

            return new PipelineGenerationResult { TemplateFiles = context.Files };
        }
        catch (InvalidOperationException exception) when (TryMapExpectedException(exception, out var error))
        {
            return error;
        }
    }

    /// <summary>
    /// Generates the shared template files for the .azuredevops/ directory.
    /// These are common across all configurations.
    /// </summary>
    /// <param name="configNames">The list of configuration names in the project.</param>
    /// <param name="environments">The deduplicated environment definitions across all configurations.</param>
    /// <returns>Dictionary of relative path ? YAML content for shared templates.</returns>
    public static IReadOnlyDictionary<string, string> GenerateSharedTemplates(
        IReadOnlyList<string> configNames,
        IReadOnlyList<EnvironmentDefinition> environments,
        string? agentPoolName = null,
        string? bicepBasePath = null,
        string? pipelineBasePath = null)
    {
        configNames = configNames.Select(PathSanitizer.Sanitize).ToList();
        var environmentNames = environments.Select(environment => environment.ShortName.ToLowerInvariant()).ToList();

        var files = new Dictionary<string, string>
        {
            ["pipelines/ci.pipeline.yml"] = InfraPipelineSharedTemplateBuilder.GenerateSharedCiTemplate(configNames, agentPoolName, bicepBasePath, pipelineBasePath),
            ["pipelines/pr.pipeline.yml"] = InfraPipelineSharedTemplateBuilder.GenerateSharedPrTemplate(configNames, agentPoolName, bicepBasePath, pipelineBasePath),
            ["jobs/deploy.job.yml"] = InfraPipelineSharedTemplateBuilder.GenerateSharedDeployJob(configNames, environmentNames, agentPoolName),
            ["steps/checkout.step.yml"] = InfraPipelineSharedTemplateBuilder.GenerateSharedCheckoutStep(),
            ["steps/deploy-template.step.yml"] = InfraPipelineSharedTemplateBuilder.GenerateSharedDeployStep(),
        };

        foreach (var environment in environments)
        {
            var environmentKey = environment.ShortName.ToLowerInvariant();
            files[$"variables/{environmentKey}.variables.yml"] = GenerateRootEnvironmentVariables(environment);
        }

        return files;
    }

    private static bool TryMapExpectedException(InvalidOperationException exception, out Error error)
    {
        if (exception.Message.StartsWith(PipelineVariableGroupNameMessagePrefix, StringComparison.Ordinal))
        {
            error = GenerationErrors.InvalidPipelineVariableGroupName(exception.Message);
            return true;
        }

        error = default;
        return false;
    }

    /// <summary>
    /// Generates a root-level environment variables file for mono-repo shared usage.
    /// Placed under <c>.azuredevops/variables/{env}.variables.yml</c>.
    /// </summary>
    private static string GenerateRootEnvironmentVariables(EnvironmentDefinition env)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {env.Name} Variables — Auto-generated by InfraFlowSculptor");
        sb.AppendLine();
        sb.AppendLine("variables:");

        if (!string.IsNullOrEmpty(env.AzureResourceManagerConnection))
        {
            sb.AppendLine($"  azureResourceManagerConnection: '{env.AzureResourceManagerConnection}'");
        }

        if (!string.IsNullOrEmpty(env.SubscriptionId))
        {
            sb.AppendLine($"  subscriptionId: '{env.SubscriptionId}'");
        }

        if (!string.IsNullOrEmpty(env.Location))
        {
            sb.AppendLine($"  location: '{env.Location}'");
        }

        return sb.ToString();
    }
}
