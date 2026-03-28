namespace InfraFlowSculptor.PipelineGeneration.Models;

/// <summary>
/// Options specific to Azure DevOps pipeline generation.
/// Stub — will be extended when pipeline content format is defined.
/// </summary>
public sealed class PipelineGenerationOptions
{
    /// <summary>Agent pool name (e.g. "ubuntu-latest").</summary>
    public string AgentPool { get; init; } = "ubuntu-latest";

    /// <summary>Branch trigger patterns (e.g. ["main", "develop"]).</summary>
    public IReadOnlyList<string> TriggerBranches { get; init; } = ["main"];
}
