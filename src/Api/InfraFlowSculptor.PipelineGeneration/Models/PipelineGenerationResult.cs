using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Result of Azure DevOps pipeline YAML generation.
/// Contains the main pipeline file and optional template files.
/// </summary>
public sealed class PipelineGenerationResult : IGenerationResult
{
    /// <summary>Content of the main <c>azure-pipelines.yml</c> file.</summary>
    public string MainPipelineYaml { get; init; } = string.Empty;

    /// <summary>
    /// Template files referenced by the main pipeline.
    /// Key = relative file path (e.g. <c>templates/deploy-rg.yml</c>), Value = file content.
    /// </summary>
    public IReadOnlyDictionary<string, string> TemplateFiles { get; init; } =
        new Dictionary<string, string>();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Files
    {
        get
        {
            var files = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(MainPipelineYaml))
                files["pipelines/ci.pipeline.yml"] = MainPipelineYaml;

            foreach (var (path, content) in TemplateFiles)
                files[path] = content;

            return files;
        }
    }
}
