using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.StorageAccount;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates per-environment <c>.bicepparam</c> files for the main deployment.
/// </summary>
internal static class ParameterFileAssembler
{
    /// <summary>
    /// Generates one <c>.bicepparam</c> file per environment.
    /// Each file sets <c>environmentName</c> and the resource-specific parameter overrides.
    /// </summary>
    internal static Dictionary<string, string> GenerateEnvironmentParameterFiles(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyCollection<string> environmentNames,
        IEnumerable<ResourceDefinition> resources,
        IReadOnlyCollection<AppSettingDefinition> appSettings)
    {
        var resourceList = resources.ToList();
        var result = new Dictionary<string, string>();

        foreach (var envName in environmentNames)
        {
            var envModules = ApplyEnvironmentOverrides(modules, envName, resourceList);
            var paramContent = GenerateMainParameters(envModules, envName, appSettings);
            var fileName = $"main.{envName.ToLowerInvariant()}.bicepparam";
            result[fileName] = paramContent;
        }

        return result;
    }

    /// <summary>
    /// Clones modules and replaces matching parameter values with environment-specific overrides.
    /// </summary>
    private static IReadOnlyCollection<GeneratedTypeModule> ApplyEnvironmentOverrides(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName,
        IReadOnlyCollection<ResourceDefinition> resources)
    {
        return modules.Select(module =>
        {
            var matchingResource = resources.FirstOrDefault(r =>
            {
                var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(r.Name);
                var expectedModuleName = ResourceTypeMetadata.GetBaseModuleName(r.Type) +
                    (resourceIdentifier.Length == 0
                        ? resourceIdentifier
                        : char.ToUpperInvariant(resourceIdentifier[0]) + resourceIdentifier[1..]);
                return module.ModuleName == expectedModuleName;
            });

            if (matchingResource is null ||
                !matchingResource.EnvironmentConfigs.TryGetValue(environmentName, out var envOverrides) ||
                envOverrides.Count == 0)
            {
                return module;
            }

            var mergedParams = new Dictionary<string, object>(module.Parameters);
            foreach (var (key, value) in envOverrides)
            {
                if (mergedParams.ContainsKey(key))
                {
                    mergedParams[key] = value;
                }
            }

            return module with { Parameters = mergedParams };
        }).ToList();
    }

    /// <summary>
    /// Generates a single <c>.bicepparam</c> file setting <c>environmentName</c>
    /// and all resource-specific parameters.
    /// </summary>
    private static string GenerateMainParameters(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName,
        IReadOnlyCollection<AppSettingDefinition> appSettings)
    {
        var sb = new StringBuilder();
        var normalizedEnvironmentName = BicepFormattingHelper.SanitizeBicepKey(environmentName);

        sb.AppendLine("using '../main.bicep'");
        sb.AppendLine();
        sb.AppendLine($"param environmentName = '{normalizedEnvironmentName}'");
        sb.AppendLine();

        foreach (var module in modules)
        {
            foreach (var (key, value) in module.Parameters)
            {
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(key)} = {BicepFormattingHelper.SerializeToBicep(value)}");
            }

            foreach (var (name, _, value) in StorageAccountCompanionHelper.GetStorageAccountCorsParameters(module))
            {
                StorageAccountCompanionHelper.AppendCorsParameterAssignment(sb, name, value);
            }

            foreach (var (name, _, value) in StorageAccountCompanionHelper.GetStorageAccountLifecycleParameters(module))
            {
                StorageAccountCompanionHelper.AppendLifecycleParameterAssignment(sb, name, value);
            }

            sb.AppendLine();
        }

        // ── Static app setting params (per-environment values) ──────────
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = BicepNamingHelper.GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            var value = setting.EnvironmentValues!.TryGetValue(environmentName, out var envValue)
                ? envValue
                : string.Empty;
            sb.AppendLine($"param {paramName} = '{BicepFormattingHelper.EscapeBicepString(value)}'");
        }

        // ── ViaBicepparam secure params (placeholder — to be filled at deployment time) ──
        foreach (var setting in appSettings.Where(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = BicepNamingHelper.GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine($"param {paramName} = ''");
        }

        return sb.ToString();
    }
}
