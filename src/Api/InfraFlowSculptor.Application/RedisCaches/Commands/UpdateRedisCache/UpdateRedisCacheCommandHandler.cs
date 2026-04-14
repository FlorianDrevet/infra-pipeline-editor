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

        TlsVersion? tlsVersion = null;
        if (request.MinimumTlsVersion is not null)
        {
            if (!Enum.TryParse<TlsVersion.Version>(request.MinimumTlsVersion, ignoreCase: true, out var parsedTls))
                return Error.Validation(code: "RedisCache.InvalidMinimumTlsVersion", description: $"The minimum TLS version '{request.MinimumTlsVersion}' is not valid.");
            tlsVersion = new TlsVersion(parsedTls);
        }

        redisCache.Update(
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            tlsVersion,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth);

        if (request.EnvironmentSettings is not null)
        {
            var parsedSettings = new List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>();
            foreach (var ec in request.EnvironmentSettings)
            {
                RedisCacheSku? sku = null;
                if (ec.Sku is not null)
                {
                    if (!Enum.TryParse<RedisCacheSku.Sku>(ec.Sku, ignoreCase: true, out var parsedSku))
                        return Error.Validation(code: "RedisCache.InvalidSku", description: $"The SKU '{ec.Sku}' is not valid.");
                    sku = new RedisCacheSku(parsedSku);
                }

                MaxMemoryPolicy? maxMemoryPolicy = null;
                if (ec.MaxMemoryPolicy is not null)
                {
                    if (!Enum.TryParse<MaxMemoryPolicy.Policy>(ec.MaxMemoryPolicy, ignoreCase: true, out var parsedPolicy))
                        return Error.Validation(code: "RedisCache.InvalidMaxMemoryPolicy", description: $"The max memory policy '{ec.MaxMemoryPolicy}' is not valid.");
                    maxMemoryPolicy = new MaxMemoryPolicy(parsedPolicy);
                }

                parsedSettings.Add((ec.EnvironmentName, sku, ec.Capacity, maxMemoryPolicy));
            }

            redisCache.SetAllEnvironmentSettings(parsedSettings);
        }

        var updatedRedisCache = await redisCacheRepository.UpdateAsync(redisCache);

        return mapper.Map<RedisCacheResult>(updatedRedisCache);
    }
}
