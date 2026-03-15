using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
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
        var redisCache = await redisCacheRepository.GetByIdAsync(request.Id, cancellationToken);
        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(redisCache.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var settings = new RedisCacheSettings(
            request.Capacity,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion,
            request.MaxMemoryPolicy);

        redisCache.Update(
            request.Name,
            request.Location,
            request.Sku,
            settings);

        var updatedRedisCache = await redisCacheRepository.UpdateAsync(redisCache);

        return mapper.Map<RedisCacheResult>(updatedRedisCache);
    }
}
