using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public class GetRedisCacheQueryHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetRedisCacheQuery, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(GetRedisCacheQuery query, CancellationToken cancellationToken)
    {
        var result = await RedisCacheAccessHelper.GetWithReadAccessAsync(
            query.Id, redisCacheRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (result.IsError)
            return result.Errors;

        return mapper.Map<RedisCacheResult>(result.Value);
    }
}
