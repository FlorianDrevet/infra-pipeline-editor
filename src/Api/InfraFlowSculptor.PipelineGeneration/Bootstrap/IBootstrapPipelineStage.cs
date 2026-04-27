namespace InfraFlowSculptor.PipelineGeneration.Bootstrap;

/// <summary>
/// Single transformation step in the Bootstrap pipeline generation.
/// Each stage appends its YAML contribution to the shared context.
/// </summary>
/// <remarks>
/// Stages must be stateless, idempotent on irrelevant context state,
/// and safe for singleton DI registration.
/// </remarks>
public interface IBootstrapPipelineStage
{
    /// <summary>Execution order. Lower values run first.</summary>
    int Order { get; }

    /// <summary>Executes the stage against the shared context.</summary>
    void Execute(BootstrapPipelineContext context);
}
