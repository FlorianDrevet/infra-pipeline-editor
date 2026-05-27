namespace InfraFlowSculptor.Domain.ContainerAppAggregate.Models;

/// <summary>
/// Carries typed per-environment Container App configuration overrides inside the domain model.
/// </summary>
public sealed record ContainerAppEnvironmentSettingsData(
    string EnvironmentName,
    string? CpuCores = null,
    string? MemoryGi = null,
    int? MinReplicas = null,
    int? MaxReplicas = null,
    bool? IngressEnabled = null,
    int? IngressTargetPort = null,
    bool? IngressExternal = null,
    string? TransportMethod = null,
    string? ReadinessProbePath = null,
    int? ReadinessProbePort = null,
    string? LivenessProbePath = null,
    int? LivenessProbePort = null,
    string? StartupProbePath = null,
    int? StartupProbePort = null,
    string? ContainerRegistryServiceConnection = null);