using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 950 — IR-based output pruning.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="AssemblyStage"/> (Order 900) has populated
/// <see cref="BicepGenerationContext.Result"/> with the assembled <c>main.bicep</c>, the
/// per-module Bicep file contents, and <see cref="Models.GenerationResult.UsedOutputsByModulePath"/>
/// (populated during <c>main.bicep</c> emission). Each <see cref="ModuleWorkItem"/> exposes a
/// <see cref="BicepModuleSpec"/> describing its outputs.</para>
/// <para><b>Post-conditions:</b> for every work item whose generated module file is present in
/// <see cref="Models.GenerationResult.ModuleFiles"/>, <see cref="BicepModuleSpec.Outputs"/> is filtered
/// to the outputs actually referenced by <c>main.bicep</c> (case-insensitive), and the file content
/// is replaced by a freshly emitted module text (preserving the standard module header).</para>
/// <para>The stage is a no-op when <see cref="BicepGenerationContext.SkipOutputPruning"/> is set
/// — used by the mono-repo flow which prunes once across all configurations after the shared
/// assembly step.</para>
/// </remarks>
public sealed class IrOutputPruningStage : IBicepGenerationStage
{
    private readonly BicepEmitter _emitter = new();

    /// <inheritdoc />
    public int Order => 950;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        if (context.SkipOutputPruning)
        {
            return;
        }

        var result = context.Result;
        if (result is null)
        {
            return;
        }

        if (result.ModuleFiles is not Dictionary<string, string> moduleFiles)
        {
            return;
        }

        var usedOutputsByPath = result.UsedOutputsByModulePath;

        foreach (var item in context.WorkItems)
        {
            var modulePath = $"modules/{item.Module.ModuleFolderName}/{item.Module.ModuleFileName}";
            if (!moduleFiles.ContainsKey(modulePath))
            {
                continue;
            }

            var usedOutputs = usedOutputsByPath.TryGetValue(modulePath, out var outputs)
                ? new HashSet<string>(outputs, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var prunedOutputs = item.Spec.Outputs
                .Where(o => usedOutputs.Contains(o.Name))
                .ToList();

            if (prunedOutputs.Count == item.Spec.Outputs.Count)
            {
                continue;
            }

            var prunedSpec = item.Spec with { Outputs = prunedOutputs };
            item.Spec = prunedSpec;

            var newBody = _emitter.EmitModule(prunedSpec);
            var newContent = ModuleHeaderHelper.AddModuleHeader(
                item.Module.ResourceTypeName,
                item.Module.ModuleFileName,
                newBody);

            moduleFiles[modulePath] = newContent;
        }
    }
}
