using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.ContainerApps.Requests;

/// <summary>Typed per-environment configuration entry for a Container App.</summary>
public sealed class ContainerAppEnvironmentConfigEntry
{
    private const int ContainerRegistryServiceConnectionMaxLength = 200;

    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional CPU cores allocation (e.g., "0.25", "0.5", "1.0", "2.0").</summary>
    public string? CpuCores { get; init; }

    /// <summary>Optional memory allocation (e.g., "0.5Gi", "1.0Gi", "2.0Gi").</summary>
    public string? MemoryGi { get; init; }

    /// <summary>Optional minimum number of replicas override.</summary>
    public int? MinReplicas { get; init; }

    /// <summary>Optional maximum number of replicas override.</summary>
    public int? MaxReplicas { get; init; }

    /// <summary>Optional flag to enable ingress.</summary>
    public bool? IngressEnabled { get; init; }

    /// <summary>Optional ingress target port.</summary>
    public int? IngressTargetPort { get; init; }

    /// <summary>Optional flag for external ingress.</summary>
    public bool? IngressExternal { get; init; }

    /// <summary>Optional transport method (e.g., "auto", "http", "http2", "tcp").</summary>
    public string? TransportMethod { get; init; }

    /// <summary>Optional HTTP path for the readiness probe (e.g., "/healthz/ready").</summary>
    public string? ReadinessProbePath { get; init; }

    /// <summary>Optional port for the readiness probe (1-65535).</summary>
    public int? ReadinessProbePort { get; init; }

    /// <summary>Optional HTTP path for the liveness probe (e.g., "/healthz/live").</summary>
    public string? LivenessProbePath { get; init; }

    /// <summary>Optional port for the liveness probe (1-65535).</summary>
    public int? LivenessProbePort { get; init; }

    /// <summary>Optional HTTP path for the startup probe (e.g., "/healthz/startup").</summary>
    public string? StartupProbePath { get; init; }

    /// <summary>Optional port for the startup probe (1-65535).</summary>
    public int? StartupProbePort { get; init; }

    /// <summary>Optional Azure DevOps Docker/ACR service connection name used by the application CI pipeline.</summary>
    [MaxLength(ContainerRegistryServiceConnectionMaxLength)]
    public string? ContainerRegistryServiceConnection { get; init; }
}