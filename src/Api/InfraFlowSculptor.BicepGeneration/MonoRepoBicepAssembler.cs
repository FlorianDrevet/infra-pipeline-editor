using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    private static readonly Regex ModulePathPattern = new("'\\./(?<path>modules/[^']+)'", RegexOptions.Compiled);

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
        var commonModulePathMaps = BuildCommonModulePathMaps(perConfigResults);

        // ── Collect all unique modules into Common ──────────────────────────
        foreach (var (configName, result) in perConfigResults)
        {
            foreach (var (path, content) in result.ModuleFiles)
            {
                var normalizedPath = commonModulePathMaps[configName][path];

                // Deduplicate identical module paths and merge folder-level types when configs contribute extras.
                if (!commonFiles.TryAdd(normalizedPath, content)
                    && normalizedPath.EndsWith("/types.bicep", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(commonFiles[normalizedPath], content, StringComparison.Ordinal))
                {
                    commonFiles[normalizedPath] = ModuleHeaderHelper.MergeTypesContent(commonFiles[normalizedPath], content);
                }
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
            var rewrittenMain = RewriteMainBicepForMonoRepo(result.MainBicep, commonModulePathMaps[configName]);
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
    private static string RewriteMainBicepForMonoRepo(
        string mainBicep,
        IReadOnlyDictionary<string, string> commonModulePathMap)
    {
        var sb = new StringBuilder(mainBicep.Length);

        foreach (var line in mainBicep.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');

            // Rewrite module path references: './modules/...' → '../Common/modules/...',
            // disambiguating per-config paths when Common contains hashed variants.
            var modulePathMatch = ModulePathPattern.Match(trimmed);
            if (modulePathMatch.Success)
            {
                var originalPath = modulePathMatch.Groups["path"].Value;
                var commonPath = commonModulePathMap.TryGetValue(originalPath, out var normalizedPath)
                    ? normalizedPath
                    : originalPath;

                trimmed = trimmed.Replace($"'./{originalPath}'", $"'../Common/{commonPath}'");
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

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> BuildCommonModulePathMaps(
        IReadOnlyDictionary<string, GenerationResult> perConfigResults)
    {
        var commonPathMaps = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var moduleEntries = perConfigResults
            .SelectMany(kv => kv.Value.ModuleFiles.Select(module => new
            {
                ConfigName = kv.Key,
                Path = module.Key,
                Content = module.Value,
            }))
            .ToList();

        foreach (var configName in perConfigResults.Keys)
        {
            commonPathMaps[configName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var group in moduleEntries.GroupBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase))
        {
            var groupedEntries = group.ToList();

            if (group.Key.EndsWith("/types.bicep", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var entry in groupedEntries)
                {
                    ((Dictionary<string, string>)commonPathMaps[entry.ConfigName])[entry.Path] = entry.Path;
                }

                continue;
            }

            var distinctContents = groupedEntries
                .Select(entry => entry.Content)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinctContents.Count <= 1)
            {
                foreach (var entry in groupedEntries)
                {
                    ((Dictionary<string, string>)commonPathMaps[entry.ConfigName])[entry.Path] = entry.Path;
                }

                continue;
            }

            var normalizedPathByContent = distinctContents.ToDictionary(
                content => content,
                content => AppendContentHashToPath(group.Key, content),
                StringComparer.Ordinal);

            foreach (var entry in groupedEntries)
            {
                ((Dictionary<string, string>)commonPathMaps[entry.ConfigName])[entry.Path] = normalizedPathByContent[entry.Content];
            }
        }

        return commonPathMaps;
    }

    private static string AppendContentHashToPath(string modulePath, string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var hash = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
        const string suffix = ".module.bicep";

        if (modulePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return $"{modulePath[..^suffix.Length]}.{hash}{suffix}";
        }

        return $"{modulePath}.{hash}";
    }
}
