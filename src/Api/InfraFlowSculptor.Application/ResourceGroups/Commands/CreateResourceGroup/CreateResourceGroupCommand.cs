using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;

namespace InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;

public record CreateResourceGroupCommand(
    InfrastructureConfigId InfraConfigId,
    Name Name,
    Location Location
) : IRequest<ErrorOr<ResourceGroupResult>>;
