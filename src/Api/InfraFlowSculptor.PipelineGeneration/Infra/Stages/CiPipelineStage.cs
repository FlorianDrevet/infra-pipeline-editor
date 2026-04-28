namespace InfraFlowSculptor.PipelineGeneration.Infra.Stages;

/// <summary>
/// Emits the <c>ci.pipeline.yml</c> per-configuration CI pipeline file.
/// Always executes regardless of mono-repo mode.
/// </summary>
public sealed class CiPipelineStage : IInfraPipelineStage
{
    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public void Execute(InfraPipelineContext context)
    {
        context.Files["ci.pipeline.yml"] = ConfigPipelineBuilder.Generate(new ConfigPipelineOptions
        {
            ConfigName = context.ConfigName,
            AgentPoolName = context.Request.AgentPoolName,
            IsMonoRepo = context.IsMonoRepo,
            TemplateFileName = "ci.pipeline.yml",
            CommentHeader = "CI Pipeline",
            TriggerKeyword = "trigger",
            TriggerBranches = ["release/*"],
            ContinueOnError = true,
        });
    }
}
