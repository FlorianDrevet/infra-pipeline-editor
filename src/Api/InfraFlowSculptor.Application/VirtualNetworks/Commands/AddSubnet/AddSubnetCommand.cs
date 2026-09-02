using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.AddSubnet;

/// <summary>Command to add a subnet to a Virtual Network.</summary>
public record AddSubnetCommand(
    AzureResourceId VirtualNetworkId,
    string Name,
    string AddressPrefix,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId
) : ICommand<VirtualNetworkResult>;
