using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.VirtualNetworks.Requests;

/// <summary>Common properties shared by create and update Virtual Network requests.</summary>
public abstract class VirtualNetworkRequestBase
{
    /// <summary>Display name for the Virtual Network resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Virtual Network will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Whether Azure DDoS Protection Standard is enabled.</summary>
    public bool EnableDdosProtection { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<VirtualNetworkEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Virtual Network.</summary>
public class VirtualNetworkEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Address spaces in CIDR notation for this environment.</summary>
    [Required]
    public required List<string> AddressSpaces { get; init; }

    /// <summary>Optional custom DNS servers for this environment.</summary>
    public List<string>? DnsServers { get; init; }
}
