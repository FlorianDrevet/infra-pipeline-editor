using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.DeleteVirtualNetwork;

/// <summary>Deletes a Virtual Network resource.</summary>
public record DeleteVirtualNetworkCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
