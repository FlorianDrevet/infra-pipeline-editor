using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

namespace InfraFlowSculptor.BicepGeneration.Pipeline;

/// <summary>
/// IR-based output pruning helper for mono-repo assemblies.
/// </summary>
/// <remarks>
/// Single-configuration pruning is owned by <see cref="IrOutputPruningStage"/>. The mono-repo
/// flow defers per-config pruning (<see cref="BicepGenerationContext.SkipOutputPruning"/>)
/// because shared modules in <see cref="MonoRepoGenerationResult.CommonFiles"/> must keep any
/// output referenced by at least one configuration's <c>main.bicep</c>. This helper computes
/// that union, filters each shared module's <see cref="BicepModuleSpec"/>, and re-emits the
/// pruned module text into <see cref="MonoRepoGenerationResult.CommonFiles"/>.
/// </remarks>
internal static class IrMonoRepoOutputPruner
{
    private static readonly BicepEmitter Emitter = new();

    /// <summary>
    /// Prunes unused outputs from the shared modules of a mono-repo result, using the union
    /// of output references from every per-configuration <c>main.bicep</c>. Modules that
    /// belong to a configuration whose work items the helper cannot match (e.g. modules
    /// produced outside of an <see cref="IResourceTypeBicepSpecGenerator"/>, such as
    /// <c>kvSecrets.module.bicep</c> or role-assignment templates) are left untouched.
    /// </summary>
    /// <param name="monoResult">The assembled mono-repo result whose shared modules are to be pruned.</param>
    /// <param name="perConfigResults">Per-configuration generation results. Their <c>main.bicep</c>
    /// is scanned for output references.</param>
    /// <param name="perConfigContexts">Per-configuration pipeline contexts. Their
    /// <see cref="BicepGenerationContext.WorkItems"/> are inspected to locate the
    /// <see cref="BicepModuleSpec"/> matching each shared module path.</param>
    internal static void Prune(
        MonoRepoGenerationResult monoResult,
        IReadOnlyDictionary<string, GenerationResult> perConfigResults,
        IReadOnlyDictionary<string, BicepGenerationContext> perConfigContexts)
    {
        if (monoResult.CommonFiles is not Dictionary<string, string> commonFiles)
        {
            return;
        }

        var combinedUsedOutputs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, configResult) in perConfigResults)
        {
            var configUsed = IrOutputPruningStage.CollectUsedOutputsByPath(configResult.MainBicep);
            foreach (var (path, outputs) in configUsed)
            {
                if (!combinedUsedOutputs.TryGetValue(path, out var existing))
                {
                    existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    combinedUsedOutputs[path] = existing;
                }
                existing.UnionWith(outputs);
            }
        }

        var workItemByPath = BuildWorkItemIndex(perConfigContexts);

        foreach (var path in commonFiles.Keys.ToList())
        {
            if (!path.StartsWith("modules/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!workItemByPath.TryGetValue(path, out var workItem))
            {
                // No spec available (e.g. kvSecrets module, role-assignment template) —
                // keep the existing content untouched. These templates don't declare prunable outputs.
                continue;
            }

            var usedOutputs = combinedUsedOutputs.GetValueOrDefault(path)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var prunedOutputs = workItem.Spec.Outputs
                .Where(o => usedOutputs.Contains(o.Name))
                .ToList();

            if (prunedOutputs.Count == workItem.Spec.Outputs.Count)
            {
                continue;
            }

            var prunedSpec = workItem.Spec with { Outputs = prunedOutputs };
            var newBody = Emitter.EmitModule(prunedSpec);
            var newContent = ModuleHeaderHelper.AddModuleHeader(
                workItem.Module.ResourceTypeName,
                workItem.Module.ModuleFileName,
                newBody);

            commonFiles[path] = newContent;
        }
    }

    private static Dictionary<string, ModuleWorkItem> BuildWorkItemIndex(
        IReadOnlyDictionary<string, BicepGenerationContext> perConfigContexts)
    {
        var index = new Dictionary<string, ModuleWorkItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, configContext) in perConfigContexts)
        {
            foreach (var item in configContext.WorkItems)
            {
                var path = $"modules/{item.Module.ModuleFolderName}/{item.Module.ModuleFileName}";
                index.TryAdd(path, item);
            }
        }

        return index;
    }
}
