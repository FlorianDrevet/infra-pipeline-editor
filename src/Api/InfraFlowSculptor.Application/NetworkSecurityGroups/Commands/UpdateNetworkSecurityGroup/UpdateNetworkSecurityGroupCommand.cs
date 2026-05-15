using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;

/// <summary>Updates an existing Network Security Group resource.</summary>
public record UpdateNetworkSecurityGroupCommand(
    AzureResourceId Id,
    Name Name,
    Location Location
) : ICommand<NetworkSecurityGroupResult>;
