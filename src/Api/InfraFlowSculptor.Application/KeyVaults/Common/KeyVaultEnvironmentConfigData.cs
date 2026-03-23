namespace InfraFlowSculptor.Application.KeyVaults.Common;

/// <summary>
/// Carries typed per-environment Key Vault configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Sku">Optional pricing tier override (e.g., "Standard", "Premium").</param>
public record KeyVaultEnvironmentConfigData(
    string EnvironmentName,
    string? Sku);
