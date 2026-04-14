namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Result of application pipeline generation, containing generated YAML file contents.
/// </summary>
public class AppPipelineGenerationResult
{
    /// <summary>Map of relative file path to YAML content.</summary>
    public IReadOnlyDictionary<string, string> Files { get; init; } = new Dictionary<string, string>();
}
