namespace InfraFlowSculptor.Application.SqlServers.Common;

/// <summary>
/// Carries typed per-environment SQL Server configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="MinimalTlsVersion">Optional minimal TLS version override (e.g., "1.0", "1.1", "1.2").</param>
public record SqlServerEnvironmentConfigData(
    string EnvironmentName,
    string? MinimalTlsVersion);
