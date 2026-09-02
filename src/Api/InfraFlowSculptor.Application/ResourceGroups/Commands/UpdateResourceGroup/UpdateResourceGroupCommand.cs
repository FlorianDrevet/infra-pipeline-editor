using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.UpdateResourceGroup;

/// <summary>Command to update an existing resource group's name and location.</summary>
public record UpdateResourceGroupCommand(
    ResourceGroupId Id,
    Name Name,
    Location Location
) : ICommand<ResourceGroupResult>;
