using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 850 — Emit IR specs to text.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> all transformation stages (400–700) have been applied. Work items
/// migrated to Builder + IR have a non-null <see cref="ModuleWorkItem.Spec"/>.</para>
/// <para><b>Post-conditions:</b> for each IR-based work item, <see cref="Models.GeneratedTypeModule.ModuleBicepContent"/>
/// and <see cref="Models.GeneratedTypeModule.ModuleTypesBicepContent"/> are populated with emitted text.
/// Legacy work items (Spec is null) are left untouched.</para>
/// </remarks>
public sealed class SpecEmissionStage : IBicepGenerationStage
{
    private readonly BicepEmitter _emitter = new();

    /// <inheritdoc />
    public int Order => 850;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        foreach (var item in context.WorkItems)
        {
            if (item.Spec is null)
                continue;

            item.Module = LegacyTextModuleAdapter.EmitContent(item.Module, item.Spec, _emitter);
        }
    }
}
