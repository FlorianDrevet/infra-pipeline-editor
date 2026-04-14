using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ServiceBusNamespaces.Requests;

/// <summary>Common properties shared by create and update Service Bus Namespace requests.</summary>
public abstract class ServiceBusNamespaceRequestBase
{
    /// <summary>Display name for the Service Bus Namespace resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Service Bus Namespace will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ServiceBusNamespaceEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Service Bus Namespace.</summary>
public class ServiceBusNamespaceEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier (Basic, Standard, Premium).</summary>
    public string? Sku { get; init; }

    /// <summary>Optional messaging units capacity (Premium tier only, 1-16).</summary>
    public int? Capacity { get; init; }

    /// <summary>Optional flag for zone redundancy (Premium tier only).</summary>
    public bool? ZoneRedundant { get; init; }

    /// <summary>Optional flag to disable local SAS key authentication.</summary>
    public bool? DisableLocalAuth { get; init; }

    /// <summary>Optional minimum TLS version (1.0, 1.1, 1.2).</summary>
    public string? MinimumTlsVersion { get; init; }
}
