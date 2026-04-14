using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Result of Azure DevOps pipeline YAML generation for a single configuration.
/// Contains the per-config pipeline and variable files.
/// </summary>
public sealed class PipelineGenerationResult : IGenerationResult
{
    /// <summary>
    /// Per-config files: ci.pipeline.yml, release.pipeline.yml, variables/*.yml.
    /// Key = relative file path, Value = file content.
    /// </summary>
    public IReadOnlyDictionary<string, string> TemplateFiles { get; init; } =
        new Dictionary<string, string>();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Files => TemplateFiles;
}
