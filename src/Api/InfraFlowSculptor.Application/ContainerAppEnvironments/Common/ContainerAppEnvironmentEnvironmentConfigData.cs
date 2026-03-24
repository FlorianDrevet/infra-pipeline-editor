namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Common;

/// <summary>
/// Carries typed per-environment Container App Environment data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record ContainerAppEnvironmentEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    string? WorkloadProfileType,
    bool? InternalLoadBalancerEnabled,
    bool? ZoneRedundancyEnabled,
    string? LogAnalyticsWorkspaceId);
