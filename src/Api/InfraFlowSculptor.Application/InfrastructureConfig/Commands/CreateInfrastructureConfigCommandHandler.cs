using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands;

public class CreateInfrastructureConfigCommandHandler(
    IInfrastructureConfigRepository repository)
    : IRequestHandler<CreateInfrastructureConfigCommand,
        ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>>
{
    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> Handle(
        CreateInfrastructureConfigCommand command, CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var infra = Domain.InfrastructureConfigAggregate.InfrastructureConfig.Create(nameVo);

        var saved = await repository.AddAsync(infra);

        return saved;
    }
}