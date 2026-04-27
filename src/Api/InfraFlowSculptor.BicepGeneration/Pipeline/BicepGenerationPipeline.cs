namespace InfraFlowSculptor.BicepGeneration.Pipeline;

/// <summary>
/// Orchestrates the ordered execution of <see cref="IBicepGenerationStage"/> instances against
/// a <see cref="BicepGenerationContext"/>. Stages are sorted by <see cref="IBicepGenerationStage.Order"/>
/// at construction time.
/// </summary>
public sealed class BicepGenerationPipeline
{
    private readonly IReadOnlyList<IBicepGenerationStage> _stages;

    /// <summary>
    /// Creates a pipeline that executes <paramref name="stages"/> in ascending order of
    /// <see cref="IBicepGenerationStage.Order"/>.
    /// </summary>
    public BicepGenerationPipeline(IEnumerable<IBicepGenerationStage> stages)
    {
        _stages = stages.OrderBy(s => s.Order).ToArray();
    }

    /// <summary>
    /// Runs every registered stage against <paramref name="context"/> in order.
    /// </summary>
    public void Execute(BicepGenerationContext context)
    {
        foreach (var stage in _stages)
        {
            stage.Execute(context);
        }
    }
}
