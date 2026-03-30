using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public class ListRedisCachesQueryHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<ListRedisCachesQuery, List<RedisCacheResult>>
{
    public async Task<ErrorOr<List<RedisCacheResult>>> Handle(ListRedisCachesQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(query.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ResourceGroup.NotFound(query.ResourceGroupId);

        var redisCaches = await redisCacheRepository.GetByResourceGroupIdAsync(query.ResourceGroupId, cancellationToken);

        return redisCaches.Select(rc => mapper.Map<RedisCacheResult>(rc)).ToList();
    }
}
