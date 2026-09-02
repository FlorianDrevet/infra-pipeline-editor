using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate;

/// <summary>
/// Represents an Azure Virtual Network (<c>Microsoft.Network/virtualNetworks</c>).
/// Manages address spaces, subnets, and DDoS protection configuration.
/// </summary>
public sealed class VirtualNetwork : AzureResource
{
    private readonly List<Subnet> _subnets = [];
    /// <summary>Gets the subnets configured in this virtual network.</summary>
    public IReadOnlyCollection<Subnet> Subnets => _subnets.AsReadOnly();

    private readonly List<VirtualNetworkEnvironmentSettings> _environmentSettings = [];
    /// <summary>Gets the per-environment configuration overrides.</summary>
    public IReadOnlyCollection<VirtualNetworkEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    private VirtualNetwork() { }

    /// <summary>Updates the resource-level properties.</summary>
    public void Update(Name name, Location location)
    {
        SetNameAndLocation(name, location);
    }

    /// <summary>Adds a subnet to this virtual network.</summary>
    public Subnet AddSubnet(
        Name name,
        string addressPrefix,
        SubnetDelegation? delegation,
        IReadOnlyList<string>? serviceEndpoints,
        PrivateEndpointNetworkPolicy privateEndpointNetworkPolicies,
        AzureResourceId? nsgId)
    {
        if (_subnets.Any(s => s.Name.Value == name.Value))
            throw new InvalidOperationException($"A subnet named '{name.Value}' already exists in this virtual network.");

        var subnet = Subnet.Create(Id, name, addressPrefix, delegation, serviceEndpoints, privateEndpointNetworkPolicies, nsgId);
        _subnets.Add(subnet);
        return subnet;
    }

    /// <summary>Removes a subnet by its identifier.</summary>
    public void RemoveSubnet(SubnetId subnetId)
    {
        var subnet = _subnets.FirstOrDefault(s => s.Id == subnetId)
            ?? throw new InvalidOperationException($"Subnet '{subnetId.Value}' not found.");
        _subnets.Remove(subnet);
    }

    /// <summary>Updates an existing subnet.</summary>
    public void UpdateSubnet(
        SubnetId subnetId,
        Name name,
        string addressPrefix,
        SubnetDelegation? delegation,
        IReadOnlyList<string>? serviceEndpoints,
        PrivateEndpointNetworkPolicy privateEndpointNetworkPolicies,
        AzureResourceId? nsgId)
    {
        var subnet = _subnets.FirstOrDefault(s => s.Id == subnetId)
            ?? throw new InvalidOperationException($"Subnet '{subnetId.Value}' not found.");
        subnet.Update(name, addressPrefix, delegation, serviceEndpoints, privateEndpointNetworkPolicies, nsgId);
    }

    /// <summary>Sets per-environment settings.</summary>
    public void SetEnvironmentSettings(string environmentName, IReadOnlyList<string> addressSpaces, IReadOnlyList<string>? dnsServers, bool enableDdosProtection)
    {
        if (IsExisting) return;
        var existing = _environmentSettings.FirstOrDefault(es => es.EnvironmentName == environmentName);
        if (existing is not null)
            existing.Update(addressSpaces, dnsServers, enableDdosProtection);
        else
            _environmentSettings.Add(VirtualNetworkEnvironmentSettings.Create(Id, environmentName, addressSpaces, dnsServers, enableDdosProtection));
    }

    /// <summary>Sets all per-environment settings at once.</summary>
    public void SetAllEnvironmentSettings(IReadOnlyList<(string EnvironmentName, IReadOnlyList<string> AddressSpaces, IReadOnlyList<string>? DnsServers, bool EnableDdosProtection)> settings)
    {
        if (IsExisting) return;
        _environmentSettings.Clear();
        foreach (var (envName, addressSpaces, dnsServers, enableDdosProtection) in settings)
            _environmentSettings.Add(VirtualNetworkEnvironmentSettings.Create(Id, envName, addressSpaces, dnsServers, enableDdosProtection));
    }

    /// <summary>Creates a new VirtualNetwork.</summary>
    public static VirtualNetwork Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, IReadOnlyList<string> AddressSpaces, IReadOnlyList<string>? DnsServers, bool EnableDdosProtection)>? environmentSettings = null,
        bool isExisting = false)
    {
        var vnet = new VirtualNetwork
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting
        };
        if (!isExisting && environmentSettings is not null)
            vnet.SetAllEnvironmentSettings(environmentSettings);
        return vnet;
    }
}
