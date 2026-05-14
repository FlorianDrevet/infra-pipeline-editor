using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;

/// <summary>Creates a new Virtual Network resource.</summary>
public record CreateVirtualNetworkCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool EnableDdosProtection = false,
    IReadOnlyList<VirtualNetworkEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<VirtualNetworkResult>;
