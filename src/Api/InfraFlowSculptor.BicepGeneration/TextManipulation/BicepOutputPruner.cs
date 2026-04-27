using System.Text.RegularExpressions;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Removes <c>output</c> declarations from generated module Bicep files when the corresponding
/// outputs are not referenced by the configuration's <c>main.bicep</c>.
/// </summary>
/// <remarks>
/// In mono-repo mode, the pruner combines used-output references from every per-configuration
/// <c>main.bicep</c> so that an output kept by at least one configuration is preserved in the
/// shared module.
/// </remarks>
public static class BicepOutputPruner
{
    /// <summary>
    /// Prunes unused output declarations from a single-configuration result's module files.
    /// No-op when <see cref="GenerationResult.ModuleFiles"/> is not a mutable dictionary.
    /// </summary>
    public static void PruneSingleConfig(GenerationResult result)
    {
        if (result.ModuleFiles is not Dictionary<string, string> mutableModuleFiles)
            return;

        var usedOutputsByPath = CollectUsedOutputsByPath(result.MainBicep, mutableModuleFiles);

        foreach (var path in mutableModuleFiles.Keys.ToList())
        {
            var usedOutputs = usedOutputsByPath.GetValueOrDefault(path)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mutableModuleFiles[path] = BicepAssembler.PruneUnusedOutputs(mutableModuleFiles[path], usedOutputs);
        }
    }

    /// <summary>
    /// Prunes unused output declarations from the shared <c>CommonFiles</c> of a mono-repo result,
    /// using the union of output references from every per-configuration <c>main.bicep</c>.
    /// </summary>
    public static void PruneMonoRepo(
        MonoRepoGenerationResult monoResult,
        IReadOnlyDictionary<string, GenerationResult> perConfigResults)
    {
        var combinedUsedOutputs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, configResult) in perConfigResults)
        {
            var configUsed = CollectUsedOutputsByPath(configResult.MainBicep, configResult.ModuleFiles);

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

        var mutableCommon = monoResult.CommonFiles as Dictionary<string, string>
            ?? new Dictionary<string, string>(monoResult.CommonFiles);

        foreach (var path in mutableCommon.Keys.ToList())
        {
            if (!path.StartsWith("modules/", StringComparison.OrdinalIgnoreCase))
                continue;

            var usedOutputs = combinedUsedOutputs.GetValueOrDefault(path)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mutableCommon[path] = BicepAssembler.PruneUnusedOutputs(mutableCommon[path], usedOutputs);
        }
    }

    /// <summary>
    /// Scans <c>main.bicep</c> for <c>{symbol}Module.outputs.{outputName}</c> references and groups
    /// them by the corresponding module file path (resolved from the <c>module</c> declaration in
    /// <paramref name="mainBicep"/>).
    /// </summary>
    public static Dictionary<string, HashSet<string>> CollectUsedOutputsByPath(
        string mainBicep,
        IReadOnlyDictionary<string, string> moduleFiles)
    {
        _ = moduleFiles; // retained for API parity; symbol→path resolution uses the main.bicep declaration only

        var outputRefRegex = new Regex(@"(\w+)Module\.outputs\.(\w+)");
        var matches = outputRefRegex.Matches(mainBicep);

        var symbolToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var match in matches.Cast<Match>())
        {
            var symbolName = match.Groups[1].Value;
            if (symbolToPath.ContainsKey(symbolName))
                continue;

            var declRegex = new Regex(@"module\s+" + Regex.Escape(symbolName) + @"Module\s+'\.\/([^']+)'");
            var declMatch = declRegex.Match(mainBicep);
            if (declMatch.Success)
            {
                symbolToPath[symbolName] = declMatch.Groups[1].Value;
            }
        }

        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            var symbolName = match.Groups[1].Value;
            var outputName = match.Groups[2].Value;

            if (!symbolToPath.TryGetValue(symbolName, out var filePath))
                continue;

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
