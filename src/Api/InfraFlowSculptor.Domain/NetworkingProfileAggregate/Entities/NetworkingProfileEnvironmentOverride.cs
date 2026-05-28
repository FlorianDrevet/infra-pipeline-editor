using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.Entities;

/// <summary>
/// Per-environment override for networking configuration.
/// Allows different VNet references or address spaces per environment.
/// </summary>
public sealed class NetworkingProfileEnvironmentOverride : Entity<NetworkingProfileEnvironmentOverrideId>
{
    /// <summary>Gets the parent networking profile identifier.</summary>
    public NetworkingProfileId NetworkingProfileId { get; private set; } = null!;

    /// <summary>Gets the target environment name (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = null!;

    /// <summary>Gets the VNet reference override for this environment.</summary>
    public VnetReference? VnetReferenceOverride { get; private set; }

    /// <summary>Gets the DNS configuration override for this environment.</summary>
    public DnsConfig? DnsConfigOverride { get; private set; }

    private NetworkingProfileEnvironmentOverride() { }

    internal static NetworkingProfileEnvironmentOverride Create(
        NetworkingProfileId profileId,
        string environmentName,
        VnetReference? vnetReferenceOverride = null,
        DnsConfig? dnsConfigOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return new NetworkingProfileEnvironmentOverride
        {
            Id = NetworkingProfileEnvironmentOverrideId.CreateUnique(),
            NetworkingProfileId = profileId,
            EnvironmentName = environmentName,
            VnetReferenceOverride = vnetReferenceOverride,
            DnsConfigOverride = dnsConfigOverride
        };
    }

    /// <summary>Updates the VNet reference override for this environment.</summary>
    internal void UpdateVnetReference(VnetReference? vnetReference)
    {
        VnetReferenceOverride = vnetReference;
    }

    /// <summary>Updates the DNS configuration override for this environment.</summary>
    internal void UpdateDnsConfig(DnsConfig? dnsConfig)
    {
        DnsConfigOverride = dnsConfig;
    }
}
