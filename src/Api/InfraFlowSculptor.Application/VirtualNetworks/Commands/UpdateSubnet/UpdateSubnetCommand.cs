using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateSubnet;

/// <summary>Command to update an existing subnet within a Virtual Network.</summary>
public record UpdateSubnetCommand(
    AzureResourceId VirtualNetworkId,
    Guid SubnetId,
    string Name,
    string AddressPrefix,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId
) : ICommand<VirtualNetworkResult>;
