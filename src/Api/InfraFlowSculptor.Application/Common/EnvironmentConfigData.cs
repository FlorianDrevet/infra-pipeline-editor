namespace InfraFlowSculptor.Application.Common;

/// <summary>
/// Carries per-environment configuration data within CQRS commands and results.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Properties">Configuration key-value pairs for this environment.</param>
public record EnvironmentConfigData(
    string EnvironmentName,
    IReadOnlyDictionary<string, string> Properties);
