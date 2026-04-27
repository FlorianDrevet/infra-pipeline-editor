namespace InfraFlowSculptor.PipelineGeneration.Bootstrap;

/// <summary>
/// Orchestrates the ordered execution of <see cref="IBootstrapPipelineStage"/> instances.
/// </summary>
public sealed class BootstrapPipeline
{
    private readonly IReadOnlyList<IBootstrapPipelineStage> _stages;

    /// <summary>
    /// Initializes a new instance of the <see cref="BootstrapPipeline"/> class.
    /// Stages are sorted by <see cref="IBootstrapPipelineStage.Order"/> at construction time.
    /// </summary>
    /// <param name="stages">The stages to orchestrate.</param>
    public BootstrapPipeline(IEnumerable<IBootstrapPipelineStage> stages)
    {
        _stages = stages.OrderBy(s => s.Order).ToArray();
    }

    /// <summary>
    /// Executes all stages in order against the provided context.
    /// </summary>
    /// <param name="context">The mutable context shared between stages.</param>
    public void Execute(BootstrapPipelineContext context)
    {
        foreach (var stage in _stages)
        {
            stage.Execute(context);
        }
    }
}
