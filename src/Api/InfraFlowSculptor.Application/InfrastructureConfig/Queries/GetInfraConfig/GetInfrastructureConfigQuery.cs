using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public record GetInfrastructureConfigQuery(
    InfrastructureConfigId Id
) : IRequest<ErrorOr<GetInfrastructureConfigResult>>;