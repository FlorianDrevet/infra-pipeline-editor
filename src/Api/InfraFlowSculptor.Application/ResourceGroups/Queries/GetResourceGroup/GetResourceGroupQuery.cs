using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;

public record GetResourceGroupQuery(
    ResourceGroupId Id
) : IRequest<ErrorOr<ResourceGroupResult>>;