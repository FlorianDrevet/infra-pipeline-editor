namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Request to generate Bicep files for an entire project in mono-repo mode.
/// Contains one <see cref="GenerationRequest"/> per infrastructure configuration.
/// </summary>
public sealed class MonoRepoGenerationRequest
{
    /// <summary>Gets or sets the per-configuration generation requests keyed by configuration name.</summary>
    public required IReadOnlyDictionary<string, GenerationRequest> ConfigRequests { get; init; }

    /// <summary>Gets or sets the shared naming context from the project.</summary>
    public required NamingContext NamingContext { get; init; }

    /// <summary>Gets or sets the shared environment definitions from the project.</summary>
    public required IReadOnlyList<EnvironmentDefinition> Environments { get; init; }

    /// <summary>Gets or sets the environment names from the project.</summary>
    public required IReadOnlyList<string> EnvironmentNames { get; init; }

    /// <summary>
    /// When <c>true</c>, shared files (<c>types.bicep</c>, <c>functions.bicep</c>, <c>constants.bicep</c>,
    /// <c>modules/...</c>) are emitted at the repository root and per-config <c>main.bicep</c> references
    /// them via <c>../</c> instead of <c>../Common/</c>. When <c>false</c> (default), shared files are
    /// emitted under a <c>Common/</c> folder.
    /// </summary>
    public bool FlattenShared { get; init; }
}
