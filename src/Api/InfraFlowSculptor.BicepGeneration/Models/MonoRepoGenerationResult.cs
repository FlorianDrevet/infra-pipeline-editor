namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Result of a mono-repo Bicep generation. Contains shared Common files
/// and per-configuration output files organized by configuration name.
/// </summary>
public sealed class MonoRepoGenerationResult
{
    /// <summary>
    /// Shared files under the <c>Common/</c> directory.
    /// Keys are relative paths: <c>types.bicep</c>, <c>functions.bicep</c>, <c>constants.bicep</c>,
    /// <c>modules/{Folder}/{file}.bicep</c>, <c>modules/{Folder}/types.bicep</c>.
    /// </summary>
    public required IReadOnlyDictionary<string, string> CommonFiles { get; init; }

    /// <summary>
    /// Per-configuration files keyed by config name.
    /// Inner keys are relative paths: <c>main.bicep</c>, <c>parameters/{env}.bicepparam</c>.
    /// </summary>
    public required IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigFiles { get; init; }
}
