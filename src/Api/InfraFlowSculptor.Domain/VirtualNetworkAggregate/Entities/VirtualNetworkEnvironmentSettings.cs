using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;

/// <summary>Per-environment configuration for a virtual network (address spaces, DNS servers).</summary>
public sealed class VirtualNetworkEnvironmentSettings : Entity<VirtualNetworkEnvironmentSettingsId>
{
    /// <summary>Gets the parent virtual network identifier.</summary>
    public AzureResourceId VirtualNetworkId { get; private set; } = null!;

    /// <summary>Gets the environment name.</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets the address spaces for this environment (CIDR notation).</summary>
    public IReadOnlyList<string> AddressSpaces { get; private set; } = [];

    /// <summary>Gets the custom DNS servers for this environment.</summary>
    public IReadOnlyList<string>? DnsServers { get; private set; }

    private VirtualNetworkEnvironmentSettings() { }

    /// <summary>Creates new environment settings.</summary>
    internal static VirtualNetworkEnvironmentSettings Create(
        AzureResourceId virtualNetworkId,
        string environmentName,
        IReadOnlyList<string> addressSpaces,
        IReadOnlyList<string>? dnsServers)
    {
        return new VirtualNetworkEnvironmentSettings
        {
            Id = VirtualNetworkEnvironmentSettingsId.CreateUnique(),
            VirtualNetworkId = virtualNetworkId,
            EnvironmentName = environmentName,
            AddressSpaces = addressSpaces,
            DnsServers = dnsServers
        };
    }

    /// <summary>Updates the environment settings.</summary>
    public void Update(IReadOnlyList<string> addressSpaces, IReadOnlyList<string>? dnsServers)
    {
        AddressSpaces = addressSpaces;
        DnsServers = dnsServers;
    }
}
