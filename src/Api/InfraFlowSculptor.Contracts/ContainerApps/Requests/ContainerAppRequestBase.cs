using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ContainerApps.Requests;

/// <summary>Common properties shared by create and update Container App requests.</summary>
public abstract class ContainerAppRequestBase
{
    /// <summary>Display name for the Container App resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Container App will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the Container App Environment that hosts this Container App.</summary>
    [Required, GuidValidation]
    public required Guid ContainerAppEnvironmentId { get; init; }

    /// <summary>Optional Container Registry identifier for authenticated image pulls.</summary>
    [GuidValidation]
    public Guid? ContainerRegistryId { get; init; }

    /// <summary>Optional base Docker image name (e.g., "myregistry.azurecr.io/myapp/api") without the tag.</summary>
    public string? DockerImageName { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ContainerAppEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Container App.</summary>
public class ContainerAppEnvironmentConfigEntry
{
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
}

/// <summary>Response DTO for a typed per-environment Container App configuration.</summary>
public record ContainerAppEnvironmentConfigResponse(
    string EnvironmentName,
    string? CpuCores,
    string? MemoryGi,
    int? MinReplicas,
    int? MaxReplicas,
    bool? IngressEnabled,
    int? IngressTargetPort,
    bool? IngressExternal,
    string? TransportMethod);
