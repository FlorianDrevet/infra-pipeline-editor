namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Describes a single Azure DevOps pipeline definition to be created by the bootstrap pipeline.
/// </summary>
/// <param name="Name">The display name of the pipeline in Azure DevOps.</param>
/// <param name="YamlPath">The repository-relative path to the YAML file (e.g. <c>infra/ci.pipeline.yml</c>).</param>
/// <param name="Folder">The Azure DevOps folder path where the pipeline should be created (e.g. <c>\\infra</c>). Use <c>\\</c> for the root folder.</param>
public sealed record BootstrapPipelineDefinition(
    string Name,
    string YamlPath,
    string Folder = "\\");
