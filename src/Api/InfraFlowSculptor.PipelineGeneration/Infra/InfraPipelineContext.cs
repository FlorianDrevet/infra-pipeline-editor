using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Infra;

/// <summary>
/// Mutable in-flight state passed between infrastructure pipeline generation stages.
/// Created once per <see cref="PipelineGenerationEngine.Generate"/> call.
/// </summary>
public sealed class InfraPipelineContext
{
    /// <summary>The generation request.</summary>
    public required GenerationRequest Request { get; init; }

    /// <summary>Sanitized configuration name.</summary>
    public required string ConfigName { get; init; }

    /// <summary>Whether the generation targets a mono-repo layout.</summary>
    public required bool IsMonoRepo { get; init; }

    /// <summary>Generated files accumulated by stages. Key = relative path, Value = YAML content.</summary>
    public Dictionary<string, string> Files { get; } = new();
}
