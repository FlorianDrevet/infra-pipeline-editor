namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Result of a mono-repo pipeline generation. Contains shared template files
/// under <c>.azuredevops/</c> and per-configuration pipeline files.
/// </summary>
public sealed class MonoRepoPipelineResult
{
    /// <summary>
    /// Shared template files under the <c>.azuredevops/</c> directory.
    /// Keys are relative paths: <c>pipelines/ci.pipeline.yml</c>, <c>jobs/deploy.job.yml</c>,
    /// <c>steps/deploy-template.step.yml</c>, <c>variables/{env}.variables.yml</c>.
    /// </summary>
    public required IReadOnlyDictionary<string, string> CommonFiles { get; init; }

    /// <summary>
    /// Per-configuration files keyed by config name.
    /// Inner keys are relative paths: <c>ci.pipeline.yml</c>, <c>release.pipeline.yml</c>.
    /// </summary>
    public required IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFiles { get; init; }
}
