using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;

/// <summary>Creates a new Network Security Group resource.</summary>
public record CreateNetworkSecurityGroupCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool IsExisting = false
) : ICommand<NetworkSecurityGroupResult>;
