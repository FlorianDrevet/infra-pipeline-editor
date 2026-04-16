using System.Text.RegularExpressions;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Validates naming template strings used for Azure resource name generation.
/// </summary>
public static partial class NamingTemplateValidator
{
    /// <summary>
    /// Placeholders that are recognised and substituted at generation time.
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedPlaceholders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "name", "prefix", "suffix", "env", "envShort", "resourceType", "resourceAbbr", "location"
    };

    private static readonly Regex PlaceholderRegex = new(@"\{([^}]+)\}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    // After removing all valid placeholders the remaining static text must only contain
    // letters, digits, hyphens, and underscores (broadest Azure naming allow-list).
    private static readonly Regex InvalidStaticCharRegex = new(@"[^a-zA-Z0-9\-_]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Returns the names of any unknown placeholders found in <paramref name="template"/>.
    /// Empty if all placeholders are valid.
    /// </summary>
    public static IReadOnlyCollection<string> GetUnknownPlaceholders(string template)
    {
        var unknown = new List<string>();
        foreach (Match match in PlaceholderRegex.Matches(template))
        {
            var name = match.Groups[1].Value;
            if (!AllowedPlaceholders.Contains(name))
                unknown.Add(name);
        }
        return unknown;
    }

    /// <summary>
    /// Returns <c>true</c> when the static characters of <paramref name="template"/>
    /// (everything outside <c>{placeholder}</c> tokens) are valid Azure resource name chars.
    /// </summary>
    public static bool HasValidStaticChars(string template)
    {
        var staticText = PlaceholderRegex.Replace(template, string.Empty);
        return !InvalidStaticCharRegex.IsMatch(staticText);
    }
}
