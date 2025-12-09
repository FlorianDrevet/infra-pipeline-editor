using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

public class CreateInfrastructureConfigCommandHandler(
    IInfrastructureConfigRepository repository, IMapper mapper)
    : IRequestHandler<CreateInfrastructureConfigCommand,
        ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        CreateInfrastructureConfigCommand command, CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var infra = Domain.InfrastructureConfigAggregate.InfrastructureConfig.Create(nameVo);

        var saved = await repository.AddAsync(infra);

        return mapper.Map<GetInfrastructureConfigResult>(saved);
    }
}