using System.Text.RegularExpressions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;

/// <summary>Represents a subnet within an Azure Virtual Network.</summary>
public sealed class Subnet : Entity<SubnetId>
{
    private static readonly Regex ServiceEndpointPattern = new(
        @"^Microsoft\.[A-Za-z][A-Za-z0-9]*$",
        RegexOptions.Compiled);

    /// <summary>Gets the parent virtual network identifier.</summary>
    public AzureResourceId VirtualNetworkId { get; private set; } = null!;

    /// <summary>Gets the subnet name.</summary>
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the subnet delegation (e.g. Microsoft.Web/serverFarms).</summary>
    public SubnetDelegation? Delegation { get; private set; }

    /// <summary>Gets the service endpoints enabled on this subnet.</summary>
    public IReadOnlyList<string> ServiceEndpoints { get; private set; } = [];

    /// <summary>Gets the private endpoint network policy setting.</summary>
    public PrivateEndpointNetworkPolicy PrivateEndpointNetworkPolicies { get; private set; } = null!;

    /// <summary>Gets an optional reference to a Network Security Group.</summary>
    public AzureResourceId? NsgId { get; private set; }

    private Subnet() { }

    /// <summary>Updates subnet properties.</summary>
    public void Update(
        Name name,
        SubnetDelegation? delegation,
        IReadOnlyList<string>? serviceEndpoints,
        PrivateEndpointNetworkPolicy privateEndpointNetworkPolicies,
        AzureResourceId? nsgId)
    {
        ValidateServiceEndpoints(serviceEndpoints);
        Name = name;
        Delegation = delegation;
        ServiceEndpoints = serviceEndpoints ?? [];
        PrivateEndpointNetworkPolicies = privateEndpointNetworkPolicies;
        NsgId = nsgId;
    }

    /// <summary>Creates a new subnet.</summary>
    internal static Subnet Create(
        AzureResourceId virtualNetworkId,
        Name name,
        SubnetDelegation? delegation,
        IReadOnlyList<string>? serviceEndpoints,
        PrivateEndpointNetworkPolicy privateEndpointNetworkPolicies,
        AzureResourceId? nsgId)
    {
        ValidateServiceEndpoints(serviceEndpoints);
        return new Subnet
        {
            Id = SubnetId.CreateUnique(),
            VirtualNetworkId = virtualNetworkId,
            Name = name,
            Delegation = delegation,
            ServiceEndpoints = serviceEndpoints ?? [],
            PrivateEndpointNetworkPolicies = privateEndpointNetworkPolicies,
            NsgId = nsgId
        };
    }

    private static void ValidateServiceEndpoints(IReadOnlyList<string>? serviceEndpoints)
    {
        if (serviceEndpoints is null)
            return;

        foreach (var endpoint in serviceEndpoints)
        {
            if (!ServiceEndpointPattern.IsMatch(endpoint))
                throw new ArgumentException($"Service endpoint '{endpoint}' must follow ARM resource provider format (e.g. Microsoft.Storage).");
        }
    }
}
