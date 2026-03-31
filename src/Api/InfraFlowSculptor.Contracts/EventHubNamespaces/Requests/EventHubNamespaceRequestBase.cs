using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;

/// <summary>Common properties shared by create and update Event Hub Namespace requests.</summary>
public abstract class EventHubNamespaceRequestBase
{
    /// <summary>Display name for the Event Hub Namespace resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Event Hub Namespace will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<EventHubNamespaceEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for an Event Hub Namespace.</summary>
public class EventHubNamespaceEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier (Basic, Standard, Premium).</summary>
    public string? Sku { get; init; }

    /// <summary>Optional throughput or processing units capacity.</summary>
    public int? Capacity { get; init; }

    /// <summary>Optional flag for zone redundancy.</summary>
    public bool? ZoneRedundant { get; init; }

    /// <summary>Optional flag to disable local SAS key authentication.</summary>
    public bool? DisableLocalAuth { get; init; }

    /// <summary>Optional minimum TLS version (1.0, 1.1, 1.2).</summary>
    public string? MinimumTlsVersion { get; init; }

    /// <summary>Optional flag to enable auto-inflate (automatic scaling).</summary>
    public bool? AutoInflateEnabled { get; init; }

    /// <summary>Optional maximum throughput units when auto-inflate is enabled (0-40).</summary>
    public int? MaxThroughputUnits { get; init; }
}

/// <summary>Response DTO for a typed per-environment Event Hub Namespace configuration.</summary>
public record EventHubNamespaceEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    bool? ZoneRedundant,
    bool? DisableLocalAuth,
    string? MinimumTlsVersion,
    bool? AutoInflateEnabled,
    int? MaxThroughputUnits);
