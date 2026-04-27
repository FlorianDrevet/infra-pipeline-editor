namespace InfraFlowSculptor.Application.ContainerApps.Common;

/// <summary>
/// Carries typed per-environment Container App configuration data within CQRS commands and results.
/// Only non-null values represent overrides for the target environment.
/// </summary>
public record ContainerAppEnvironmentConfigData(
    string EnvironmentName,
    string? CpuCores,
    string? MemoryGi,
    int? MinReplicas,
    int? MaxReplicas,
    bool? IngressEnabled,
    int? IngressTargetPort,
    bool? IngressExternal,
    string? TransportMethod,
    string? ReadinessProbePath = null,
    int? ReadinessProbePort = null,
    string? LivenessProbePath = null,
    int? LivenessProbePort = null,
    string? StartupProbePath = null,
    int? StartupProbePort = null);
