using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Resolves the repository path that should be watched by CI and PR triggers for an application pipeline.
/// </summary>
internal static class AppTriggerPathHelper
{
    private const string DefaultSourcePath = ".";

    /// <summary>
    /// Resolves the relative path to watch for source changes.
    /// Falls back to the legacy config/resource folder when no explicit source path is defined.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns>The repo-relative path to include in YAML trigger filters.</returns>
    internal static string ResolveTriggerPath(AppPipelineGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceCodePath))
        {
            return BuildLegacyTriggerPath(request);
        }

        var normalizedSourcePath = NormalizeRelativePath(request.SourceCodePath);
        return string.Equals(normalizedSourcePath, DefaultSourcePath, StringComparison.Ordinal)
            ? BuildLegacyTriggerPath(request)
            : normalizedSourcePath;
    }

    private static string BuildLegacyTriggerPath(AppPipelineGenerationRequest request)
    {
        return $"{PathSanitizer.Sanitize(request.ConfigName)}/{PathSanitizer.Sanitize(request.ResourceName)}";
    }

    private static string NormalizeRelativePath(string sourceCodePath)
    {
        var normalizedPath = sourceCodePath.Trim().Replace('\\', '/');

        while (normalizedPath.StartsWith("./", StringComparison.Ordinal))
        {
            normalizedPath = normalizedPath[2..];
        }

        normalizedPath = normalizedPath.Trim('/');

        return string.IsNullOrWhiteSpace(normalizedPath)
            ? DefaultSourcePath
            : normalizedPath;
    }
}