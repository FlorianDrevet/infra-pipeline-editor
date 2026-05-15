using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;

/// <summary>Updates an existing Virtual Network resource.</summary>
public record UpdateVirtualNetworkCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    bool EnableDdosProtection = false,
    IReadOnlyList<VirtualNetworkEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<VirtualNetworkResult>, IHasEnvironmentSettings;
