using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;

public record ListResourceGroupsByConfigQuery(
    InfrastructureConfigId InfraConfigId
) : IRequest<ErrorOr<List<ResourceGroupResult>>>;
