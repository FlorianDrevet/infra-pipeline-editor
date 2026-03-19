using ErrorOr;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;

public record DeleteResourceGroupCommand(
    ResourceGroupId Id
) : IRequest<ErrorOr<Deleted>>;
