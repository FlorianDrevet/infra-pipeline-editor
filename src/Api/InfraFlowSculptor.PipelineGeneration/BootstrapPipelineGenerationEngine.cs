using InfraFlowSculptor.PipelineGeneration.Bootstrap;
using InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Generates a self-contained <c>bootstrap.pipeline.yml</c> for Azure DevOps.
/// Delegates to <see cref="BootstrapPipeline"/> for staged YAML generation.
/// </summary>
public sealed class BootstrapPipelineGenerationEngine
{
    private const string BootstrapFileName = "bootstrap.pipeline.yml";

    private readonly BootstrapPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance using the provided pipeline orchestrator (DI path).
    /// </summary>
    /// <param name="pipeline">The pipeline that orchestrates the bootstrap stages.</param>
    public BootstrapPipelineGenerationEngine(BootstrapPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Parameterless constructor for backward compatibility and direct test instantiation.
    /// Creates an internal pipeline with all stages in default order.
    /// </summary>
    public BootstrapPipelineGenerationEngine()
        : this(CreateDefaultPipeline())
    {
    }

    /// <summary>
    /// Generates the bootstrap pipeline YAML file from the provided request.
    /// </summary>
    /// <param name="request">The bootstrap generation request containing pipelines, variable groups, and project metadata.</param>
    /// <returns>
    /// A <see cref="PipelineGenerationResult"/> containing a single file: <c>bootstrap.pipeline.yml</c>.
    /// </returns>
    public PipelineGenerationResult Generate(BootstrapGenerationRequest request)
    {
        var context = new BootstrapPipelineContext { Request = request };
        _pipeline.Execute(context);

        return new PipelineGenerationResult
        {
            TemplateFiles = new Dictionary<string, string>
            {
                [BootstrapFileName] = context.Builder.ToString(),
            },
        };
    }

    private static BootstrapPipeline CreateDefaultPipeline() => new(new IBootstrapPipelineStage[]
    {
        new HeaderEmissionStage(),
        new ValidateSharedResourcesJobStage(),
        new PipelineProvisionJobStage(),
        new EnvironmentProvisionJobStage(),
        new VariableGroupProvisionJobStage(),
        new NoOpFallbackStage(),
    });
}
