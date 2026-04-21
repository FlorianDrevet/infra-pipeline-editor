using System.Text;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration;

/// <summary>
/// Assembles Bicep output for mono-repo mode. Produces a <c>Common/</c> folder with shared modules
/// and per-configuration folders each containing a <c>main.bicep</c> with relative references
/// to <c>../../Common/modules/...</c>.
/// </summary>
public static class MonoRepoBicepAssembler
{
    /// <summary>
    /// Assembles the complete mono-repo Bicep output from per-config generation results.
    /// </summary>
    public static MonoRepoGenerationResult Assemble(
        IReadOnlyDictionary<string, GenerationResult> perConfigResults,
        NamingContext namingContext,
        IReadOnlyList<EnvironmentDefinition> environments,
        bool hasAnyRoleAssignments)
    {
        var commonFiles = new Dictionary<string, string>();
        var configFiles = new Dictionary<string, IReadOnlyDictionary<string, string>>();

        // ── Collect all unique modules into Common ──────────────────────────
        foreach (var (_, result) in perConfigResults)
        {
            foreach (var (path, content) in result.ModuleFiles)
            {
                // Deduplicate: same module path = same content across configs
                commonFiles.TryAdd(path, content);
            }
        }

        // ── Generate shared types.bicep ─────────────────────────────────────
        var typesBicep = TypesBicepAssembler.Generate(environments, hasAnyRoleAssignments);
        commonFiles["types.bicep"] = typesBicep;

        // ── Generate shared functions.bicep ─────────────────────────────────
        var functionsBicep = FunctionsBicepAssembler.Generate(namingContext);
        commonFiles["functions.bicep"] = functionsBicep;

        // ── Merge constants.bicep (union of all role assignments) ───────────
        if (hasAnyRoleAssignments)
        {
            // Pick the biggest constants.bicep (all configs share the same roles catalog)
            var allConstants = perConfigResults.Values
                .Where(r => !string.IsNullOrEmpty(r.ConstantsBicep))
                .Select(r => r.ConstantsBicep)
                .OrderByDescending(c => c.Length)
                .FirstOrDefault();

            if (allConstants is not null)
            {
                commonFiles["constants.bicep"] = allConstants;
            }
        }

        // ── Per-config folders ──────────────────────────────────────────────
        foreach (var (configName, result) in perConfigResults)
        {
            var sanitizedName = PathSanitizer.Sanitize(configName);
            var files = new Dictionary<string, string>();

            // Rewrite main.bicep to reference Common modules via relative path
            var rewrittenMain = RewriteMainBicepForMonoRepo(result.MainBicep);
            files["main.bicep"] = rewrittenMain;

            // Parameter files go under parameters/
            foreach (var (paramFileName, paramContent) in result.EnvironmentParameterFiles)
            {
                files[$"parameters/{paramFileName}"] = paramContent;
            }

            configFiles[sanitizedName] = files;
        }

        return new MonoRepoGenerationResult
        {
            CommonFiles = commonFiles,
            ConfigFiles = configFiles,
        };
    }

    /// <summary>
    /// Rewrites <c>main.bicep</c> module references from <c>./modules/...</c> to <c>../Common/modules/...</c>
    /// and shared file imports from <c>types.bicep</c> etc. to <c>../Common/types.bicep</c> etc.
    /// </summary>
    private static string RewriteMainBicepForMonoRepo(string mainBicep)
    {
        var sb = new StringBuilder(mainBicep.Length);

        foreach (var line in mainBicep.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');

            // Rewrite module path references:  './modules/...' → '../Common/modules/...'
            if (trimmed.Contains("'./modules/"))
            {
                trimmed = trimmed.Replace("'./modules/", "'../Common/modules/");
            }

            // Rewrite import references:
            // from 'types.bicep' → from '../Common/types.bicep'
            // from 'functions.bicep' → from '../Common/functions.bicep'
            // from 'constants.bicep' → from '../Common/constants.bicep'
            if (trimmed.Contains("from 'types.bicep'"))
            {
                trimmed = trimmed.Replace("from 'types.bicep'", "from '../Common/types.bicep'");
            }
            else if (trimmed.Contains("from 'functions.bicep'"))
            {
                trimmed = trimmed.Replace("from 'functions.bicep'", "from '../Common/functions.bicep'");
            }
            else if (trimmed.Contains("from 'constants.bicep'"))
            {
                trimmed = trimmed.Replace("from 'constants.bicep'", "from '../Common/constants.bicep'");
            }

            sb.AppendLine(trimmed);
        }

        return sb.ToString();
    }
}
