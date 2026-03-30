using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;

public record ListResourceGroupResourcesQuery(ResourceGroupId Id)
    : IQuery<List<AzureResourceResult>>;
