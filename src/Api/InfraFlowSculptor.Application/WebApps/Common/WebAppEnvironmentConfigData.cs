namespace InfraFlowSculptor.Application.WebApps.Common;

/// <summary>
/// Carries typed per-environment Web App configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="AlwaysOn">Optional always-on override.</param>
/// <param name="HttpsOnly">Optional HTTPS-only override.</param>
/// <param name="RuntimeStack">Optional runtime stack override (e.g., "DotNet", "Node").</param>
/// <param name="RuntimeVersion">Optional runtime version override (e.g., "8.0", "20").</param>
/// <param name="DockerImageTag">Optional Docker image tag override for this environment (e.g., "latest", "v1.2.3").</param>
public record WebAppEnvironmentConfigData(
    string EnvironmentName,
    bool? AlwaysOn,
    bool? HttpsOnly,
    string? RuntimeStack,
    string? RuntimeVersion,
    string? DockerImageTag);
