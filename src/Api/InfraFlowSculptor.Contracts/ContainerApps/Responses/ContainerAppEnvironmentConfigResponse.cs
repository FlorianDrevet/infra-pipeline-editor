namespace InfraFlowSculptor.Contracts.ContainerApps.Responses;

/// <summary>Response DTO for a typed per-environment Container App configuration.</summary>
public sealed record ContainerAppEnvironmentConfigResponse(
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
    int? StartupProbePort = null,
    string? ContainerRegistryServiceConnection = null);