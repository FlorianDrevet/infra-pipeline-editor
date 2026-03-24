using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;

/// <summary>Common properties shared by create and update Container App Environment requests.</summary>
public abstract class ContainerAppEnvironmentRequestBase
{
    /// <summary>Display name for the Container App Environment resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Container App Environment will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ContainerAppEnvironmentEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Container App Environment.</summary>
public class ContainerAppEnvironmentEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override (e.g., "Consumption", "Premium").</summary>
    public string? Sku { get; init; }

    /// <summary>Optional workload profile type (e.g., "Consumption", "D4", "D8", "D16", "D32", "E4", "E8", "E16", "E32").</summary>
    public string? WorkloadProfileType { get; init; }

    /// <summary>Optional flag to enable internal load balancer.</summary>
    public bool? InternalLoadBalancerEnabled { get; init; }

    /// <summary>Optional flag to enable zone redundancy.</summary>
    public bool? ZoneRedundancyEnabled { get; init; }

    /// <summary>Optional Log Analytics workspace ID for diagnostics.</summary>
    public string? LogAnalyticsWorkspaceId { get; init; }
}

/// <summary>Response DTO for a typed per-environment Container App Environment configuration.</summary>
public record ContainerAppEnvironmentEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    string? WorkloadProfileType,
    bool? InternalLoadBalancerEnabled,
    bool? ZoneRedundancyEnabled,
    string? LogAnalyticsWorkspaceId);
