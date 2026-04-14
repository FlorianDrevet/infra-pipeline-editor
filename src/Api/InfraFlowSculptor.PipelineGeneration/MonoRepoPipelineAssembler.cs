using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Assembles pipeline output for mono-repo mode. Produces a <c>.azuredevops/</c> folder with
/// shared templates (including root-level variables) and per-configuration folders each containing CI/release pipelines.
/// </summary>
public static class MonoRepoPipelineAssembler
{
    /// <summary>
    /// Assembles the complete mono-repo pipeline output from per-config generation results.
    /// </summary>
    /// <param name="perConfigResults">Per-config results keyed by config name.</param>
    /// <param name="environments">The deduplicated environment definitions across all configurations.</param>
    /// <param name="agentPoolName">Optional self-hosted agent pool name for shared templates.</param>
    /// <returns>A <see cref="MonoRepoPipelineResult"/> with common and per-config files.</returns>
    public static MonoRepoPipelineResult Assemble(
        IReadOnlyDictionary<string, PipelineGenerationResult> perConfigResults,
        IReadOnlyList<EnvironmentDefinition> environments,
        string? agentPoolName = null,
        string? bicepBasePath = null,
        string? pipelineBasePath = null)
    {
        // ── Shared templates (same for all configs) + root variables ────────
        var configNames = perConfigResults.Keys.ToList();
        var commonFiles = PipelineGenerationEngine.GenerateSharedTemplates(
            configNames,
            environments,
            agentPoolName,
            bicepBasePath,
            pipelineBasePath);

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
