using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Assembles pipeline output for mono-repo mode. Produces a <c>.azuredevops/</c> folder with
/// shared templates and per-configuration folders each containing CI/release pipelines and variables.
/// </summary>
public static class MonoRepoPipelineAssembler
{
    /// <summary>
    /// Assembles the complete mono-repo pipeline output from per-config generation results.
    /// </summary>
    /// <param name="perConfigResults">Per-config results keyed by config name.</param>
    /// <returns>A <see cref="MonoRepoPipelineResult"/> with common and per-config files.</returns>
    public static MonoRepoPipelineResult Assemble(
        IReadOnlyDictionary<string, PipelineGenerationResult> perConfigResults)
    {
        // ── Shared templates (same for all configs) ─────────────────────────
        var configNames = perConfigResults.Keys.ToList();
        var commonFiles = PipelineGenerationEngine.GenerateSharedTemplates(configNames);

        // ── Per-config folders ──────────────────────────────────────────────
        var configFiles = new Dictionary<string, IReadOnlyDictionary<string, string>>();

        foreach (var (configName, result) in perConfigResults)
        {
            configFiles[configName] = result.Files;
        }

        return new MonoRepoPipelineResult
        {
            CommonFiles = commonFiles,
            ConfigFiles = configFiles,
        };
    }
}
