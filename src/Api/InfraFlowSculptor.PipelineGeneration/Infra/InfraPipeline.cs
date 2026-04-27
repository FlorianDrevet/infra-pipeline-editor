namespace InfraFlowSculptor.PipelineGeneration.Infra;

/// <summary>
/// Orchestrates the ordered execution of <see cref="IInfraPipelineStage"/> instances.
/// </summary>
public sealed class InfraPipeline
{
    private readonly IReadOnlyList<IInfraPipelineStage> _stages;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfraPipeline"/> class.
    /// Stages are sorted by <see cref="IInfraPipelineStage.Order"/> at construction time.
    /// </summary>
    /// <param name="stages">The stages to orchestrate.</param>
    public InfraPipeline(IEnumerable<IInfraPipelineStage> stages)
    {
        _stages = stages.OrderBy(s => s.Order).ToArray();
    }

    /// <summary>
    /// Executes all stages in order against the provided context.
    /// </summary>
    /// <param name="context">The mutable context shared between stages.</param>
    public void Execute(InfraPipelineContext context)
    {
        foreach (var stage in _stages)
        {
            stage.Execute(context);
        }
    }
}
