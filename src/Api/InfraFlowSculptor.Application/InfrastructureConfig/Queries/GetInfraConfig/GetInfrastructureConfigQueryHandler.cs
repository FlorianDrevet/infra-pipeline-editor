using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public class GetInfrastructureConfigQueryHandler(IInfrastructureConfigRepository infrastructureConfigRepository, IMapper mapper)
    : IRequestHandler<GetInfrastructureConfigQuery, ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(GetInfrastructureConfigQuery command, CancellationToken cancellationToken)
    {
        var infraConfig = await infrastructureConfigRepository.GetByIdAsync(command.Id, cancellationToken);
        if (infraConfig is null)
        {
            return Errors.InfrastructureConfig.NotFoundError(command.Id);
        }

        var result = mapper.Map<GetInfrastructureConfigResult>(infraConfig);
        return result;
    }
}