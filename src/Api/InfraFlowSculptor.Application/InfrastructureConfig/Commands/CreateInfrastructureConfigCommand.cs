using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands;

public record CreateInfrastructureConfigCommand(string Name) : IRequest<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>>;