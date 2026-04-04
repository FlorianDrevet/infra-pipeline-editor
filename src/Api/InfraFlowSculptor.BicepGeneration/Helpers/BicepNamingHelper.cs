using System.Text;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Helpers;

/// <summary>
/// Naming expression builders and identifier transforms for Bicep generation.
/// </summary>
internal static class BicepNamingHelper
{
    internal static string BuildNamingExpression(
        string logicalName,
        string resourceAbbreviation,
        string resourceTypeName,
        NamingContext namingContext)
    {
        if (namingContext.ResourceTemplates.ContainsKey(resourceTypeName))
        {
            var funcName = NamingTemplateTranslator.GetFunctionName(resourceTypeName);
            return $"{funcName}('{logicalName}', '{resourceAbbreviation}', env)";
        }

        if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
        {
            return $"BuildResourceName('{logicalName}', '{resourceAbbreviation}', env)";
        }

        return $"'{logicalName}'";
    }

    /// <summary>
    /// Collects the set of naming functions actually referenced in <c>main.bicep</c>
    /// to build the import statement.
    /// </summary>
    internal static HashSet<string> BuildFunctionImportList(
        NamingContext namingContext,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups)
    {
        var imports = new HashSet<string>();

        if (string.IsNullOrEmpty(namingContext.DefaultTemplate) &&
            namingContext.ResourceTemplates.Count == 0)
        {
            return imports;
        }

        var usedResourceTypes = modules
            .Select(m => m.ResourceTypeName)
            .Concat(resourceGroups.Select(_ => "ResourceGroup"))
            .Distinct();

        var hasDefault = false;

        foreach (var typeName in usedResourceTypes)
        {
            if (namingContext.ResourceTemplates.ContainsKey(typeName))
            {
                imports.Add(NamingTemplateTranslator.GetFunctionName(typeName));
            }
            else if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
            {
                hasDefault = true;
            }
        }

        if (hasDefault)
        {
            imports.Add("BuildResourceName");
        }

        return imports;
    }

    /// <summary>
    /// Builds a deterministic Bicep param name for a static app setting.
    /// Convention: <c>{moduleName}{PascalCase(settingName)}</c>.
    /// </summary>
    internal static string GetStaticAppSettingParamName(string targetResourceName, string settingName)
    {
        var moduleName = BicepIdentifierHelper.ToBicepIdentifier(targetResourceName);
        var pascalName = ToPascalCaseFromEnvVar(settingName);
        return $"{moduleName}{pascalName}";
    }

    /// <summary>
    /// Builds a deterministic Bicep param name for a secure Key Vault secret value.
    /// Convention: <c>{moduleName}{PascalCase(secretName)}SecretValue</c>.
    /// </summary>
    internal static string GetSecureAppSettingParamName(string targetResourceName, string secretName)
    {
        var moduleName = BicepIdentifierHelper.ToBicepIdentifier(targetResourceName);
        var pascalName = ToPascalCaseFromEnvVar(secretName);
        return $"{moduleName}{pascalName}SecretValue";
    }

    /// <summary>
    /// Converts an environment variable name (SNAKE_CASE) to PascalCase.
    /// E.g. "MY_API_URL" → "MyApiUrl", "APP__HOST" → "AppHost".
    /// </summary>
    internal static string ToPascalCaseFromEnvVar(string envVarName)
    {
        var sb = new StringBuilder();
        var capitalizeNext = true;
        foreach (var c in envVarName)
        {
            if (c is '_' or '-' or '.')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Strips the leading resource symbolic name from a catalog BicepExpression.
    /// For example, <c>kv.properties.vaultUri</c> becomes <c>properties.vaultUri</c>.
    /// Returns <c>null</c> if the expression is null or cannot be stripped (e.g. string interpolation).
    /// </summary>
    internal static string? StripResourceSymbolPrefix(string? bicepExpression)
    {
        if (bicepExpression is null)
            return null;

        var firstDot = bicepExpression.IndexOf('.');
        if (firstDot < 0)
            return bicepExpression;

        // Only strip if the prefix is a simple identifier (no quotes, parens, or interpolation)
        var prefix = bicepExpression[..firstDot];
        if (prefix.All(c => char.IsLetterOrDigit(c) || c == '_'))
            return bicepExpression[(firstDot + 1)..];

        return bicepExpression;
    }
}
