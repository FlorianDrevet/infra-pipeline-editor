using InfraFlowSculptor.BicepGeneration.TextManipulation;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 600 — App-settings parameter injection on compute resources.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.AppSettings"/> and
/// <see cref="BicepGenerationContext.WorkItems"/> populated (stages 200 + 300).</para>
/// <para><b>Post-conditions:</b> every compute resource (Web App, Function App, Container App)
/// of an ARM type that hosts at least one resource with app settings receives an
/// <c>appSettings</c> array (Web/Function) or <c>envVars</c> array (Container App) parameter,
/// wired into the appropriate property. Resources without explicit settings still receive the
/// param (default <c>[]</c>) so all instances of an ARM type share an identical module template.</para>
/// </remarks>
public sealed class AppSettingsInjectionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 600;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var computeArmTypesWithAppSettings = context.AppSettings.ComputeArmTypesWithAppSettings;

        foreach (var item in context.WorkItems)
        {
            var resource = item.Resource;
            if (!AzureResourceTypes.ComputeArmTypes.Contains(resource.Type))
                continue;
            if (!computeArmTypesWithAppSettings.Contains(resource.Type))
                continue;

            var newContent = BicepAppSettingsInjector.Inject(item.Module.ModuleBicepContent, resource.Type);
            item.Module = item.Module with { ModuleBicepContent = newContent };
        }
    }
}
