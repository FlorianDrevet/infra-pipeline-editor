using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;

public class ListMyInfrastructureConfigsQueryHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<ListMyInfrastructureConfigsQuery, ErrorOr<List<GetInfrastructureConfigResult>>>
{
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListMyInfrastructureConfigsQuery query, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var configs = await repository.GetAllForUserAsync(userId, cancellationToken);
        return configs.Select(c => mapper.Map<GetInfrastructureConfigResult>(c)).ToList();
    }
}
