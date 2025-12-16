using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public record GenerateBicepCommand(
    Guid InfraInfrastructureConfigId
) : IRequest<ErrorOr<Uri>>;