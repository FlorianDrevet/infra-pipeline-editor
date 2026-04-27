namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 900 — Final assembly.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> all module-mutation stages have run; <see cref="BicepGenerationContext.WorkItems"/>
/// reflects the final per-module Bicep content and metadata.</para>
/// <para><b>Post-conditions:</b> <see cref="BicepGenerationContext.Result"/> is populated by
/// delegating to <see cref="BicepAssembler.Assemble"/>. Output pruning is intentionally
/// <em>not</em> performed here — it is owned by <see cref="BicepGenerationEngine"/> so the
/// mono-repo path can apply cross-configuration pruning instead.</para>
/// </remarks>
public sealed class AssemblyStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 900;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var request = context.Request;
        var modules = context.WorkItems.Select(w => w.Module).ToList();

        context.Result = BicepAssembler.Assemble(
            modules,
            request.ResourceGroups,
            request.Environments,
            request.EnvironmentNames,
            request.Resources,
            request.NamingContext,
            request.RoleAssignments,
            request.AppSettings,
            request.ExistingResourceReferences,
            request.ProjectTags,
            request.ConfigTags);
    }
}
