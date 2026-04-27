using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 700 — Tags parameter injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated.</para>
/// <para><b>Post-conditions:</b> every module declares <c>param tags object = {}</c> and applies
/// it via <c>tags: tags</c> on the resource declaration (after <c>location: location</c>).
/// No-op when the param is already present.</para>
/// </remarks>
public sealed class TagsInjectionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 700;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        foreach (var item in context.WorkItems)
        {
            item.Spec = item.Spec.WithTags();
        }
    }
}
