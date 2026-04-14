using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public record GetInfrastructureConfigQuery(
    InfrastructureConfigId Id
) : IQuery<GetInfrastructureConfigResult>;