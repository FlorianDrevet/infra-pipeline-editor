namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Normalizes generated pipeline artifact paths before they are pushed to Git.
/// This preserves compatibility with legacy blobs that duplicated the app segment
/// as <c>apps/{appName}/{resourceName}/...</c> when both names were logically the same.
/// </summary>
public static class GeneratedPipelinePathNormalizer
{
    /// <summary>
    /// Normalizes the provided generated pipeline file paths.
    /// </summary>
    /// <param name="files">The generated files keyed by relative path.</param>
    /// <returns>The normalized file map keyed by repository-relative path.</returns>
    public static Dictionary<string, string> Normalize(IReadOnlyDictionary<string, string> files)
    {
        var normalizedFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (path, content) in files)
        {
            normalizedFiles[NormalizePath(path)] = content;
        }

        return normalizedFiles;
    }

    private static string NormalizePath(string path)
    {
        var normalizedPath = path.TrimStart('/');
        var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Strip leading routing-bucket segment introduced by the project-level pipeline upload layout
        // ({prefix}/infra/... and {prefix}/app/...). Legacy push handlers read all files merged.
        if (segments.Length >= 2
            && (string.Equals(segments[0], "infra", StringComparison.OrdinalIgnoreCase)
                || string.Equals(segments[0], "app", StringComparison.OrdinalIgnoreCase)))
        {
            segments = segments.Skip(1).ToArray();
            normalizedPath = string.Join('/', segments);
        }

        if (segments.Length >= 3
            && string.Equals(segments[0], "apps", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[1], segments[2], StringComparison.OrdinalIgnoreCase))
        {
            return string.Join('/', segments.Take(2).Concat(segments.Skip(3)));
        }

        return normalizedPath;
    }
}