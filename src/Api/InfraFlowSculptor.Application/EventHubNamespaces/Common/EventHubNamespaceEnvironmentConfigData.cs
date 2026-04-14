namespace InfraFlowSculptor.Application.EventHubNamespaces.Common;

/// <summary>
/// Carries typed per-environment Event Hub Namespace data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record EventHubNamespaceEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    bool? ZoneRedundant,
    bool? DisableLocalAuth,
    string? MinimumTlsVersion,
    bool? AutoInflateEnabled,
    int? MaxThroughputUnits);
