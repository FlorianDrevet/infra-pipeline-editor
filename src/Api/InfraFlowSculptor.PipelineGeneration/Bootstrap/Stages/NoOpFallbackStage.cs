using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>NothingToProvision</c> no-op job when no other stage has emitted a provisioning job.
/// Always runs last and checks <see cref="BootstrapPipelineContext.HasProvisioningJob"/> at execution time.
/// </summary>
public sealed class NoOpFallbackStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 999;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        if (context.HasProvisioningJob)
            return;

        var sb = context.Builder;
        var request = context.Request;

        AppendJobHeader(sb, "NothingToProvision", "Nothing to Provision", request.AgentPoolName, dependsOn: null);
        sb.AppendLine($"{StepIndent}- powershell: |");
        sb.AppendLine($"{StepBodyIndent}Write-Host 'No pipelines, environments, or variable groups were requested for bootstrap.'");
        sb.AppendLine($"{StepPropertyIndent}displayName: 'No-op'");
        sb.AppendLine();
    }
}
