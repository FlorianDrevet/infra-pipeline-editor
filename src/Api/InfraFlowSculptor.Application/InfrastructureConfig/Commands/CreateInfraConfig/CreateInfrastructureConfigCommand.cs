using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

public record CreateInfrastructureConfigCommand(string Name) : IRequest<ErrorOr<GetInfrastructureConfigResult>>;