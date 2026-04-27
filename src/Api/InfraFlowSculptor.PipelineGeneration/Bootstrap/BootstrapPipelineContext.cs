using System.Text;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap;

/// <summary>
/// Mutable in-flight state passed between Bootstrap pipeline stages.
/// Created once per <see cref="BootstrapPipelineGenerationEngine.Generate"/> call.
/// </summary>
public sealed class BootstrapPipelineContext
{
    /// <summary>The original bootstrap request.</summary>
    public required BootstrapGenerationRequest Request { get; init; }

    /// <summary>The YAML content being built by stages.</summary>
    public StringBuilder Builder { get; } = new();

    /// <summary>Tracks whether at least one provisioning job was emitted.</summary>
    public bool HasProvisioningJob { get; set; }
}
