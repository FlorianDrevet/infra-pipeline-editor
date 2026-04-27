using System.Text.RegularExpressions;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Emit;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 950 — IR-based output pruning.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="AssemblyStage"/> (Order 900) has populated
/// <see cref="BicepGenerationContext.Result"/> with the assembled <c>main.bicep</c> and
/// per-module Bicep file contents. Each <see cref="ModuleWorkItem"/> exposes a
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
    private static readonly Regex OutputReferencePattern = new(@"(\w+)Module\.outputs\.(\w+)", RegexOptions.Compiled);

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

        var usedOutputsByPath = CollectUsedOutputsByPath(result.MainBicep);

        foreach (var item in context.WorkItems)
        {
            var modulePath = $"modules/{item.Module.ModuleFolderName}/{item.Module.ModuleFileName}";
            if (!moduleFiles.ContainsKey(modulePath))
            {
                continue;
            }

            var usedOutputs = usedOutputsByPath.GetValueOrDefault(modulePath)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

    /// <summary>
    /// Scans <paramref name="mainBicep"/> for <c>{symbol}Module.outputs.{outputName}</c> references
    /// and groups the discovered output names by the corresponding module file path
    /// (resolved from the <c>module</c> declaration).
    /// </summary>
    /// <remarks>
    /// Module-path resolution and case-insensitive comparison match the legacy text-based pruner
    /// for byte-for-byte parity.
    /// </remarks>
    internal static Dictionary<string, HashSet<string>> CollectUsedOutputsByPath(string mainBicep)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(mainBicep))
        {
            return result;
        }

        var matches = OutputReferencePattern.Matches(mainBicep);
        if (matches.Count == 0)
        {
            return result;
        }

        var symbolToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            var symbolName = match.Groups[1].Value;
            if (symbolToPath.ContainsKey(symbolName))
            {
                continue;
            }

            var declRegex = new Regex(@"module\s+" + Regex.Escape(symbolName) + @"Module\s+'\.\/([^']+)'");
            var declMatch = declRegex.Match(mainBicep);
            if (declMatch.Success)
            {
                symbolToPath[symbolName] = declMatch.Groups[1].Value;
            }
        }

        foreach (Match match in matches)
        {
            var symbolName = match.Groups[1].Value;
            var outputName = match.Groups[2].Value;

            if (!symbolToPath.TryGetValue(symbolName, out var filePath))
            {
                continue;
            }

            if (!result.TryGetValue(filePath, out var outputs))
            {
                outputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result[filePath] = outputs;
            }
            outputs.Add(outputName);
        }

        return result;
    }
}
