using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;

public record ListResourceGroupResourcesQuery(ResourceGroupId Id)
    : IRequest<ErrorOr<List<AzureResourceResult>>>;
