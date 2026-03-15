using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;

public class UpdateRedisCacheCommandHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateRedisCacheCommand, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(UpdateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var rcResult = await RedisCacheAccessHelper.GetWithWriteAccessAsync(
            request.Id, redisCacheRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (rcResult.IsError)
            return rcResult.Errors;

        var settings = new RedisCacheSettings(
            request.Capacity,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion,
            request.MaxMemoryPolicy);

        rcResult.Value.Update(request.Name, request.Location, request.Sku, settings);

        var updatedRedisCache = await redisCacheRepository.UpdateAsync(rcResult.Value);

        return mapper.Map<RedisCacheResult>(updatedRedisCache);
    }
}
