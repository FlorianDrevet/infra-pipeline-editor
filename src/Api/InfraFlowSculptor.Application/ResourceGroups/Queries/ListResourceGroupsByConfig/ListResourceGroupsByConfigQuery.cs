using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;

public record ListResourceGroupsByConfigQuery(
    InfrastructureConfigId InfraConfigId
) : IQuery<List<ResourceGroupResult>>;
