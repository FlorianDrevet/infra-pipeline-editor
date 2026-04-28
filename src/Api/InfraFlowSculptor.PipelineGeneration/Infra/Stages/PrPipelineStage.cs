namespace InfraFlowSculptor.PipelineGeneration.Infra.Stages;

/// <summary>
/// Emits the <c>pr.pipeline.yml</c> per-configuration PR validation pipeline file.
/// Always executes regardless of mono-repo mode.
/// </summary>
public sealed class PrPipelineStage : IInfraPipelineStage
{
    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public void Execute(InfraPipelineContext context)
    {
        context.Files["pr.pipeline.yml"] = ConfigPipelineBuilder.Generate(new ConfigPipelineOptions
        {
            ConfigName = context.ConfigName,
            AgentPoolName = context.Request.AgentPoolName,
            IsMonoRepo = context.IsMonoRepo,
            TemplateFileName = "pr.pipeline.yml",
            CommentHeader = "PR Validation Pipeline",
            TriggerKeyword = "pr",
            TriggerBranches = ["main", "develop", "release/*"],
        });
    }
}
