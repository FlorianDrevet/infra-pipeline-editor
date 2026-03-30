using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;

public record DeleteResourceGroupCommand(
    ResourceGroupId Id
) : ICommand<Deleted>;
