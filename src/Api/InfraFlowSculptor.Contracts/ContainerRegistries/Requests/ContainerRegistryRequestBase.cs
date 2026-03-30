using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ContainerRegistries.Requests;

/// <summary>Common properties shared by create and update Container Registry requests.</summary>
public abstract class ContainerRegistryRequestBase
{
    /// <summary>Display name for the Container Registry resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Container Registry will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ContainerRegistryEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Container Registry.</summary>
public class ContainerRegistryEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override (e.g., "Basic", "Standard", "Premium").</summary>
    public string? Sku { get; init; }

    /// <summary>Optional flag to enable the admin user.</summary>
    public bool? AdminUserEnabled { get; init; }

    /// <summary>Optional public network access setting (e.g., "Enabled", "Disabled").</summary>
    public string? PublicNetworkAccess { get; init; }

    /// <summary>Optional flag to enable zone redundancy (Premium only).</summary>
    public bool? ZoneRedundancy { get; init; }
}

/// <summary>Response DTO for a typed per-environment Container Registry configuration.</summary>
public record ContainerRegistryEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    bool? AdminUserEnabled,
    string? PublicNetworkAccess,
    bool? ZoneRedundancy);
