namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Common interface for all generation results (Bicep, Pipeline, etc.).
/// Provides a unified view of generated files as a path → content dictionary.
/// </summary>
public interface IGenerationResult
{
    /// <summary>
    /// All generated files. Key = relative file path, Value = file content.
    /// </summary>
    IReadOnlyDictionary<string, string> Files { get; }
}
