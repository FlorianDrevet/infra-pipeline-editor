using System.Text;
using System.Text.RegularExpressions;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Low-level text-manipulation primitives shared by the Bicep injectors
/// (<see cref="BicepIdentityInjector"/>, <see cref="BicepOutputInjector"/>,
/// <see cref="BicepAppSettingsInjector"/>, <see cref="BicepTagsInjector"/>).
/// All methods are pure and operate on Bicep module text using whitespace-tolerant regex.
/// </summary>
internal static class BicepTextManipulationHelpers
{
    // ── Compiled regex patterns for whitespace-tolerant Bicep template matching ──

    /// <summary>Matches <c>resource &lt;symbol&gt; '...'</c> declarations.</summary>
    internal static readonly Regex ResourceSymbolPattern =
        new(@"resource\s+(\w+)\s+'", RegexOptions.Compiled);

    /// <summary>Matches a top-level (2-space indented) <c>identity:</c> block.</summary>
    internal static readonly Regex IdentityBlockPattern =
        new(@"^  identity\s*:", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches a top-level (2-space indented) <c>properties:</c> block.</summary>
    internal static readonly Regex PropertiesBlockPattern =
        new(@"^  properties\s*:", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches an <c>output principalId</c> declaration at the start of a line.</summary>
    internal static readonly Regex OutputPrincipalIdPattern =
        new(@"^output\s+principalId\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches a <c>param identityType</c> declaration at the start of a line.</summary>
    internal static readonly Regex ParamIdentityTypePattern =
        new(@"^param\s+identityType\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches a <c>param tags</c> declaration at the start of a line.</summary>
    internal static readonly Regex ParamTagsPattern =
        new(@"^param\s+tags\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches a <c>param appSettings</c> declaration at the start of a line.</summary>
    internal static readonly Regex ParamAppSettingsPattern =
        new(@"^param\s+appSettings\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches a <c>param envVars</c> declaration at the start of a line.</summary>
    internal static readonly Regex ParamEnvVarsPattern =
        new(@"^param\s+envVars\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches the Bicep property line <c>location: location</c>.</summary>
    internal static readonly Regex LocationPropertyPattern =
        new(@"^\s+location\s*:\s*location\b", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Matches an <c>appSettings: [</c> array opening.</summary>
    internal static readonly Regex AppSettingsArrayPattern =
        new(@"appSettings\s*:\s*\[", RegexOptions.Compiled);

    /// <summary>Matches a <c>siteConfig:</c> block opening.</summary>
    internal static readonly Regex SiteConfigPattern =
        new(@"\bsiteConfig\s*:", RegexOptions.Compiled);

    /// <summary>Matches an <c>env:</c> property (used in Container App container specs).</summary>
    internal static readonly Regex EnvPropertyPattern =
        new(@"\benv\s*:", RegexOptions.Compiled);

    /// <summary>Matches a Container App nested <c>resources: {</c> declaration.</summary>
    internal static readonly Regex ContainerResourcesPattern =
        new(@"^\s+resources\s*:\s*\{", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Returns <c>true</c> when a <c>param &lt;name&gt;</c> declaration exists in the template.
    /// Whitespace-tolerant.
    /// </summary>
    internal static bool HasParam(string bicep, string paramName) =>
        Regex.IsMatch(bicep, $@"^param\s+{Regex.Escape(paramName)}\b", RegexOptions.Multiline);

    /// <summary>
    /// Returns <c>true</c> when an <c>output &lt;name&gt;</c> declaration exists in the template.
    /// Whitespace-tolerant.
    /// </summary>
    internal static bool HasOutput(string bicep, string outputName) =>
        Regex.IsMatch(bicep, $@"^output\s+{Regex.Escape(outputName)}\b", RegexOptions.Multiline);

    /// <summary>
    /// Finds the byte index of the first line in the template that starts (after optional indentation)
    /// with the given prefix. Returns <c>-1</c> when not found.
    /// </summary>
    internal static int FindFirstLineIndex(string content, string linePrefix)
    {
        var match = Regex.Match(content, $@"^[ \t]*{Regex.Escape(linePrefix)}", RegexOptions.Multiline);
        return match.Success ? match.Index : -1;
    }

    /// <summary>
    /// Inserts <paramref name="content"/> into <paramref name="source"/> at <paramref name="position"/>
    /// without intermediate concatenations.
    /// </summary>
    internal static string InsertAt(string source, int position, string content)
    {
        var sb = new StringBuilder(source.Length + content.Length);
        sb.Append(source, 0, position);
        sb.Append(content);
        sb.Append(source, position, source.Length - position);
        return sb.ToString();
    }

    /// <summary>
    /// Finds the position of the closing brace that matches the opening brace context.
    /// Starts scanning from <paramref name="startIndex"/> with an initial depth of 1.
    /// Returns <c>-1</c> when no matching brace is found.
    /// </summary>
    internal static int FindClosingBrace(string source, int startIndex)
    {
        var depth = 1;
        for (var i = startIndex; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Capitalizes the first character of <paramref name="value"/> (ASCII / invariant).
    /// </summary>
    internal static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

    /// <summary>
    /// Adds <paramref name="typeName"/> to the existing <c>import { ... } from './types.bicep'</c> line.
    /// If no import line exists, prepends a new one.
    /// </summary>
    internal static string AddTypeImport(string moduleBicep, string typeName)
    {
        var importRegex = new Regex(@"import\s*\{([^}]+)\}\s*from\s*'\.\/types\.bicep'");
        var match = importRegex.Match(moduleBicep);
        if (match.Success)
        {
            var existingImports = match.Groups[1].Value;
            if (existingImports.Contains(typeName))
                return moduleBicep;

            var newImports = existingImports.TrimEnd() + ", " + typeName + " ";
            return moduleBicep.Replace(match.Value, $"import {{{newImports}}} from './types.bicep'");
        }

        return $"import {{ {typeName} }} from './types.bicep'\n\n" + moduleBicep;
    }
}
