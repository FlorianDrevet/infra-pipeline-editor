using InfraFlowSculptor.PipelineGeneration;

namespace InfraFlowSculptor.Application.Common.Generation;

/// <summary>
/// Classifies generated pipeline files as either application-code-bound or infrastructure-bound,
/// based on a deterministic path convention.
/// </summary>
/// <remarks>
/// Files emitted by <c>AppPipelineGenerationEngine</c> live under <c>apps/...</c> (per-resource wrappers)
/// or under the shared template prefixes returned by <see cref="AppPipelineGenerationEngine.GenerateSharedTemplates"/>.
/// Everything else (Bicep, infra pipeline templates, bootstrap) belongs to the infrastructure repository.
/// </remarks>
public static class AppPipelineFileClassifier
{
    private static readonly IReadOnlySet<string> AppSharedTemplatePaths =
        AppPipelineGenerationEngine.GenerateSharedTemplates().Keys.ToHashSet(StringComparer.Ordinal);

    /// <summary>The set of known shared application pipeline template paths classified as Application code.</summary>
    public static IReadOnlySet<string> KnownAppSharedTemplatePaths => AppSharedTemplatePaths;

    /// <summary>Returns <see langword="true"/> when the file path belongs to the application-code repository.</summary>
    /// <param name="path">The repository-relative file path.</param>
    public static bool IsApplicationCodeFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (path.StartsWith("apps/", StringComparison.Ordinal))
            return true;

        return AppSharedTemplatePaths.Contains(path);
    }

    /// <summary>Splits a flat file dictionary into (infra, app) buckets keyed by path.</summary>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="files">The flat dictionary keyed by repository-relative path.</param>
    public static (IReadOnlyDictionary<string, TValue> Infra, IReadOnlyDictionary<string, TValue> App)
        Split<TValue>(IReadOnlyDictionary<string, TValue> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var infra = new Dictionary<string, TValue>(StringComparer.Ordinal);
        var app = new Dictionary<string, TValue>(StringComparer.Ordinal);

        foreach (var (path, value) in files)
        {
            if (IsApplicationCodeFile(path))
                app[path] = value;
            else
                infra[path] = value;
        }

        return (infra, app);
    }
}
