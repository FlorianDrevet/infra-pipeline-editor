namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Common;

/// <summary>
/// Carries typed per-environment Service Bus Namespace data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
/// <param name="EnvironmentName">Name of the environment (e.g., "dev", "staging", "prod").</param>
/// <param name="Sku">Optional pricing tier (Basic, Standard, Premium).</param>
/// <param name="Capacity">Optional messaging units capacity (Premium tier only, 1-16).</param>
/// <param name="ZoneRedundant">Optional flag for zone redundancy (Premium tier only).</param>
/// <param name="DisableLocalAuth">Optional flag to disable local SAS key authentication.</param>
/// <param name="MinimumTlsVersion">Optional minimum TLS version (1.0, 1.1, 1.2).</param>
public record ServiceBusNamespaceEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    bool? ZoneRedundant,
    bool? DisableLocalAuth,
    string? MinimumTlsVersion);
