using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

public class CreateRedisCacheCommandHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateRedisCacheCommand, RedisCacheResult>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(CreateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

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

        List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>? parsedSettings = null;
        if (request.EnvironmentSettings is not null)
        {
            parsedSettings = [];
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
        }

        var redisCache = RedisCache.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            tlsVersion,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth,
            parsedSettings,
            isExisting: request.IsExisting);

        var savedRedisCache = await redisCacheRepository.AddAsync(redisCache);

        return mapper.Map<RedisCacheResult>(savedRedisCache);
    }
}
