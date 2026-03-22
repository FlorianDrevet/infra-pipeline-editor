using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public class GetInfrastructureConfigQueryHandler(
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetInfrastructureConfigQuery, ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        GetInfrastructureConfigQuery command, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.Id, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        return mapper.Map<GetInfrastructureConfigResult>(accessResult.Value);
    }
}
