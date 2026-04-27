using InfraFlowSculptor.BicepGeneration.TextManipulation;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 500 — Output declaration injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.AppSettings"/> and
/// <see cref="BicepGenerationContext.WorkItems"/> populated (stages 200 + 300).</para>
/// <para><b>Post-conditions:</b> each module that is the source of one or more app-setting
/// references receives <c>output</c> declarations appended at the bottom of its template.
/// Sensitive outputs (KV-exported) are decorated with <c>@secure()</c>.</para>
/// </remarks>
public sealed class OutputInjectionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 500;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var outputsBySource = context.AppSettings.OutputsBySourceResource;

        foreach (var item in context.WorkItems)
        {
            if (!outputsBySource.TryGetValue(item.Resource.Name, out var outputs))
                continue;

            var injections = outputs
                .Select(o => new ModuleOutputInjection(o.OutputName, o.BicepExpression, o.IsSecure))
                .ToArray();

            var newContent = BicepOutputInjector.Inject(item.Module.ModuleBicepContent, injections);
            item.Module = item.Module with { ModuleBicepContent = newContent };
        }
    }
}
