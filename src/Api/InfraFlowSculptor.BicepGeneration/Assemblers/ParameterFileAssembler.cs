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
    /// File names use <see cref="EnvironmentDefinition.ShortName"/> to match the pipeline convention.
    /// </summary>
    internal static Dictionary<string, string> GenerateEnvironmentParameterFiles(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<EnvironmentDefinition> environments,
        IEnumerable<ResourceDefinition> resources,
        IReadOnlyList<AppSettingDefinition> appSettings)
    {
        var resourceList = resources.ToList();
        var result = new Dictionary<string, string>();

        foreach (var env in environments)
        {
            var envModules = ApplyEnvironmentOverrides(modules, env.Name, resourceList);
            var paramContent = GenerateMainParameters(envModules, env.Name, appSettings);
            var fileName = $"main.{env.ShortName.ToLowerInvariant()}.bicepparam";
            result[fileName] = paramContent;
        }

        return result;
    }

    /// <summary>
    /// Clones modules and replaces matching parameter values with environment-specific overrides.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    private static IReadOnlyCollection<GeneratedTypeModule> ApplyEnvironmentOverrides(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName,
        IReadOnlyList<ResourceDefinition> resources)
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

            if (matchingResource is null)
                return module;

            var hasEnvOverrides = matchingResource.EnvironmentConfigs.TryGetValue(environmentName, out var envOverrides)
                && envOverrides.Count > 0;

            var envCustomDomains = matchingResource.CustomDomains
                .Where(cd => cd.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!hasEnvOverrides && envCustomDomains.Count == 0)
                return module;

            var mergedParams = new Dictionary<string, object>(module.Parameters);

            if (hasEnvOverrides)
            {
                foreach (var (key, value) in envOverrides!)
                {
                    // Check if this flat key maps to a property inside a structured parameter group
                    if (module.ParameterGroupMappings.TryGetValue(key, out var mapping))
                    {
                        if (mergedParams.TryGetValue(mapping.GroupKey, out var groupObj))
                        {
                            mergedParams[mapping.GroupKey] = MergePropertyIntoObject(
                                groupObj, mapping.PropertyName, value);
                        }
                    }
                    else if (mergedParams.TryGetValue(key, out var existingValue))
                    {
                        mergedParams[key] = CoerceToOriginalType(value, existingValue);
                    }
                }
            }

            if (envCustomDomains.Count > 0 && mergedParams.ContainsKey("customDomains"))
            {
                mergedParams["customDomains"] = envCustomDomains
                    .Select(cd => (object)new Dictionary<string, object>
                    {
                        ["domainName"] = cd.DomainName,
                        ["bindingType"] = cd.BindingType
                    })
                    .ToList<object>();
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
        IReadOnlyList<AppSettingDefinition> appSettings)
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

            // Secure parameters â€” placeholder values to be replaced at deployment time
            foreach (var secureParam in module.SecureParameters)
            {
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(secureParam)} = ''");
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

        // â”€â”€ Static app setting params (per-environment values) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = BicepNamingHelper.GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            var value = setting.EnvironmentValues!.TryGetValue(environmentName, out var envValue)
                ? envValue
                : string.Empty;
            sb.AppendLine($"param {paramName} = '{BicepFormattingHelper.EscapeBicepString(value)}'");
        }

        // â”€â”€ ViaBicepparam secure params (placeholder â€” to be filled at deployment time) â”€â”€
        foreach (var setting in appSettings.Where(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = BicepNamingHelper.GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine($"param {paramName} = ''");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string override value to the C# type of the existing parameter default,
    /// so that <see cref="BicepFormattingHelper.SerializeToBicep"/> emits the correct Bicep literal
    /// (e.g. <c>false</c> instead of <c>'false'</c>).
    /// </summary>
    private static object CoerceToOriginalType(string value, object existingValue)
    {
        return existingValue switch
        {
            bool when bool.TryParse(value, out var b) => b,
            int when int.TryParse(value, out var i) => i,
            long when long.TryParse(value, out var l) => l,
            double when double.TryParse(value, out var d) => d,
            _ => value
        };
    }

    /// <summary>
    /// Creates a deep copy of an anonymous/POCO object with one property value replaced,
    /// preserving all other properties. Supports dot-separated <paramref name="propertyPath"/>
    /// for nested objects (e.g. <c>"readiness.path"</c> navigates into the <c>readiness</c>
    /// sub-object and replaces its <c>path</c> property).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    private static object MergePropertyIntoObject(object source, string propertyPath, string newValue)
    {
        var dotIndex = propertyPath.IndexOf('.');
        var head = dotIndex >= 0 ? propertyPath[..dotIndex] : propertyPath;
        var tail = dotIndex >= 0 ? propertyPath[(dotIndex + 1)..] : null;

        var dict = new Dictionary<string, object>();

        if (source is IDictionary<string, object> existingDict)
        {
            foreach (var (k, v) in existingDict)
            {
                if (k.Equals(head, StringComparison.OrdinalIgnoreCase))
                {
                    dict[k] = tail is not null
                        ? MergePropertyIntoObject(v, tail, newValue)
                        : (v is not null ? CoerceToOriginalType(newValue, v) : newValue);
                }
                else
                {
                    dict[k] = v;
                }
            }
        }
        else
        {
            var props = source.GetType().GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
            {
                var val = prop.GetValue(source);
                if (prop.Name.Equals(head, StringComparison.OrdinalIgnoreCase))
                {
                    dict[prop.Name] = tail is not null
                        ? MergePropertyIntoObject(val!, tail, newValue)
                        : (val is not null ? CoerceToOriginalType(newValue, val) : newValue);
                }
                else
                {
                    dict[prop.Name] = val!;
                }
            }
        }

        return dict;
    }
}
