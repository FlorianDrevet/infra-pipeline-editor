using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.RemoveSubnet;

/// <summary>Command to remove a subnet from a Virtual Network.</summary>
public record RemoveSubnetCommand(
    AzureResourceId VirtualNetworkId,
    Guid SubnetId
) : ICommand<VirtualNetworkResult>;
