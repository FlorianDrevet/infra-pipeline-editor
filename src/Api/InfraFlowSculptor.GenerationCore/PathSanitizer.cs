using System.Text.RegularExpressions;

namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Sanitizes user-provided names (configuration names, application names, etc.) for safe use
/// in file paths, folder names, pipeline YAML references, and artifact paths.
/// Spaces are replaced with dashes; consecutive dashes are collapsed; leading/trailing dashes are trimmed.
/// </summary>
public static partial class PathSanitizer
{
    /// <summary>
    /// Sanitizes the given name so it is safe for use as a file-system path segment,
    /// Azure DevOps pipeline reference, or artifact folder name.
    /// Replaces spaces and underscores with dashes, removes characters that are invalid
    /// in file paths or YAML values, collapses consecutive dashes, and trims leading/trailing dashes.
    /// </summary>
    /// <param name="name">The raw name to sanitize (e.g. an infrastructure configuration name).</param>
    /// <returns>A sanitized, dash-separated, lowercase-safe string suitable for paths and YAML.</returns>
    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        // Replace spaces and underscores with dashes
        var result = SpacesOrUnderscores().Replace(name, "-");

        // Remove characters that are invalid in file paths or YAML unquoted values
        result = InvalidPathChars().Replace(result, string.Empty);

        // Collapse consecutive dashes
        result = ConsecutiveDashes().Replace(result, "-");

        // Trim leading and trailing dashes
        return result.Trim('-');
    }

    [GeneratedRegex(@"[\s_]+", RegexOptions.Compiled)]
    private static partial Regex SpacesOrUnderscores();

    [GeneratedRegex(@"[^\w\-.]", RegexOptions.Compiled)]
    private static partial Regex InvalidPathChars();

    [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
    private static partial Regex ConsecutiveDashes();
}
