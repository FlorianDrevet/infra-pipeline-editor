using ErrorOr;
using InfraFlowSculptor.Application.Common.Clients;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public class GenerateBicepCommandHandler(IInfrastructureConfigRepository infrastructureConfigRepository, IGenerateBicepClient generateBicepClient)
    : IRequestHandler<GenerateBicepCommand, ErrorOr<Uri>>
{
    public async Task<ErrorOr<Uri>> Handle(GenerateBicepCommand command, CancellationToken cancellationToken)
    {
        var infraId = new InfrastructureConfigId(command.InfraInfrastructureConfigId);
        var infrastructureConfig = await infrastructureConfigRepository.GetByIdAsync(infraId, cancellationToken);
        //TODO
        
        var bicepUri = await generateBicepClient.GenerateBicepAsync(cancellationToken);
        return bicepUri;
    }
}