using System.Text.RegularExpressions;

namespace InfraFlowSculptor.BicepGeneration;

/// <summary>
/// Translates naming template strings (e.g. <c>{name}-{resourceAbbr}{suffix}</c>)
/// into Bicep string interpolation expressions used inside user-defined functions.
/// </summary>
internal static class NamingTemplateTranslator
{
    /// <summary>
    /// Maps naming template placeholders to their corresponding Bicep function parameter
    /// or environment variable expressions.
    /// </summary>
    private static readonly Dictionary<string, string> PlaceholderMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = "${name}",
        ["resourceAbbr"] = "${resourceAbbr}",
        ["resourceType"] = "${resourceType}",
        ["suffix"] = "${env.envSuffix}",
        ["prefix"] = "${env.envPrefix}",
        ["env"] = "${env.envName}",
        ["envShort"] = "${env.envShort}",
        ["location"] = "${env.location}",
    };

    private static readonly Regex PlaceholderRegex = new(@"\{([^}]+)\}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Converts a naming template to a Bicep string interpolation expression
    /// wrapped in single quotes (ready to use as a function return expression).
    /// </summary>
    internal static string ToBicepInterpolation(string template)
    {
        var result = PlaceholderRegex.Replace(template, match =>
        {
            var placeholder = match.Groups[1].Value;
            return PlaceholderMappings.TryGetValue(placeholder, out var bicepExpr)
                ? bicepExpr
                : $"${{{placeholder}}}";
        });

        return $"'{result}'";
    }

    /// <summary>
    /// Determines whether the given template uses the <c>{resourceType}</c> placeholder,
    /// which would require an additional function parameter.
    /// </summary>
    internal static bool UsesResourceType(string template)
    {
        return template.Contains("{resourceType}", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Generates a valid Bicep function name for a per-resource-type naming template override.
    /// </summary>
    internal static string GetFunctionName(string resourceType)
    {
        return $"Build{resourceType}Name";
    }
}
