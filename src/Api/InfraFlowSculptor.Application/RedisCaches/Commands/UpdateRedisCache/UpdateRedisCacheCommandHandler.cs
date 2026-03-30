using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;

public class UpdateRedisCacheCommandHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateRedisCacheCommand, RedisCacheResult>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(UpdateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var redisCache = await redisCacheRepository.GetByIdAsync(request.Id, cancellationToken);
        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(redisCache.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        redisCache.Update(
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion is not null ? new TlsVersion(Enum.Parse<TlsVersion.Version>(request.MinimumTlsVersion)) : null,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth);

        if (request.EnvironmentSettings is not null)
            redisCache.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (
                        ec.EnvironmentName,
                        ec.Sku is not null ? new RedisCacheSku(Enum.Parse<RedisCacheSku.Sku>(ec.Sku)) : (RedisCacheSku?)null,
                        ec.Capacity,
                        ec.MaxMemoryPolicy is not null ? new MaxMemoryPolicy(Enum.Parse<MaxMemoryPolicy.Policy>(ec.MaxMemoryPolicy)) : (MaxMemoryPolicy?)null))
                    .ToList());

        var updatedRedisCache = await redisCacheRepository.UpdateAsync(redisCache);

        return mapper.Map<RedisCacheResult>(updatedRedisCache);
    }
}
