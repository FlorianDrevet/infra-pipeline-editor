namespace InfraFlowSculptor.Application.FunctionApps.Common;

/// <summary>
/// Carries typed per-environment Function App configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="HttpsOnly">Optional HTTPS-only override.</param>
/// <param name="MaxInstanceCount">Optional maximum scale-out instance count override.</param>
/// <param name="DockerImageTag">Optional Docker image tag override for this environment (e.g., "latest", "v1.2.3").</param>
public record FunctionAppEnvironmentConfigData(
    string EnvironmentName,
    bool? HttpsOnly,
    int? MaxInstanceCount,
    string? DockerImageTag);
