using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetInfraConfig;

public class GetInfrastructureConfigQueryHandler(
    IInfrastructureConfigRepository infrastructureConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetInfrastructureConfigQuery, ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        GetInfrastructureConfigQuery command, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await infrastructureConfigRepository.GetByIdWithMembersAsync(command.Id, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.Id);

        // Return NotFound (not Forbidden) to avoid revealing config existence to non-members
        if (!infraConfig.Members.Any(m => m.UserId == userId))
            return Errors.InfrastructureConfig.NotFoundError(command.Id);

        return mapper.Map<GetInfrastructureConfigResult>(infraConfig);
    }
}
