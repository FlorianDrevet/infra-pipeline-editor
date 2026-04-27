using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 200 — App-settings analysis.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.Request"/> populated.</para>
/// <para><b>Post-conditions:</b> <see cref="BicepGenerationContext.AppSettings"/> populated with
/// per-source-resource output declarations to inject and the set of compute ARM types whose
/// instances require an <c>appSettings</c>/<c>envVars</c> parameter.</para>
/// </remarks>
public sealed class AppSettingsAnalysisStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var request = context.Request;

        // Outputs to inject on each source resource that is referenced by app settings
        // (regular output references AND sensitive outputs exported to Key Vault).
        var outputsBySourceResource = request.AppSettings
            .Where(s => (s.IsOutputReference || s.IsSensitiveOutputExportedToKeyVault)
                && s.SourceResourceName is not null
                && s.SourceOutputName is not null && s.SourceOutputBicepExpression is not null)
            .GroupBy(s => s.SourceResourceName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => (
                        OutputName: s.SourceOutputName!,
                        BicepExpression: s.SourceOutputBicepExpression!,
                        IsSecure: s.IsSensitiveOutputExportedToKeyVault))
                    .DistinctBy(x => x.OutputName, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var targetResourcesWithAppSettings = request.AppSettings
            .Select(s => s.TargetResourceName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var computeArmTypesWithAppSettings = request.Resources
            .Where(r => AzureResourceTypes.ComputeArmTypes.Contains(r.Type)
                && targetResourcesWithAppSettings.Contains(r.Name))
            .Select(r => r.Type)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        context.AppSettings = new AppSettingsAnalysisResult(
            outputsBySourceResource,
            computeArmTypesWithAppSettings);
    }
}
