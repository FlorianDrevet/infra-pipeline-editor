namespace InfraFlowSculptor.Application.ContainerRegistries.Common;

/// <summary>
/// Carries typed per-environment Container Registry data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record ContainerRegistryEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    bool? AdminUserEnabled,
    string? PublicNetworkAccess,
    bool? ZoneRedundancy);
