using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveEnvironment;

public record RemoveEnvironmentCommand(
    InfrastructureConfigId InfraConfigId,
    EnvironmentDefinitionId EnvironmentId
) : IRequest<ErrorOr<Deleted>>;
