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
    public required IReadOnlyCollection<EnvironmentDefinition> Environments { get; init; }

    /// <summary>Gets or sets the environment names from the project.</summary>
    public required IReadOnlyCollection<string> EnvironmentNames { get; init; }
}
