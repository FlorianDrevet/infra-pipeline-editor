using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;

public record GetResourceGroupQuery(
    ResourceGroupId Id
) : IQuery<ResourceGroupResult>;