namespace InfraFlowSculptor.BicepGeneration.Pipeline;

/// <summary>
/// Single transformation step in the Bicep generation pipeline. Each stage reads from and
/// writes to the shared <see cref="BicepGenerationContext"/> instance.
/// </summary>
/// <remarks>
/// <para>Stages must be:</para>
/// <list type="bullet">
///   <item><description>Stateless — no instance fields beyond constructor-injected dependencies.</description></item>
///   <item><description>Idempotent on missing inputs — when the relevant context state is empty, the stage is a no-op.</description></item>
///   <item><description>Safe for singleton DI registration — multiple concurrent generations create their own context instances.</description></item>
/// </list>
/// </remarks>
public interface IBicepGenerationStage
{
    /// <summary>
    /// Execution order within the pipeline. Lower values run first. The pipeline
    /// orchestrator sorts injected stages by this value at construction time.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Runs this stage's transformation against <paramref name="context"/>.
    /// </summary>
    void Execute(BicepGenerationContext context);
}
