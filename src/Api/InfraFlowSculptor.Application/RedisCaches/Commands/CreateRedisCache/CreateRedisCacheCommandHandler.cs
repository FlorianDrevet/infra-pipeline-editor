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
    : IRequestHandler<CreateRedisCacheCommand, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(CreateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var redisCache = RedisCache.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion is not null ? new TlsVersion(Enum.Parse<TlsVersion.Version>(request.MinimumTlsVersion)) : null,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth,
            request.EnvironmentSettings?
                .Select(ec => (
                    ec.EnvironmentName,
                    ec.Sku is not null ? new RedisCacheSku(Enum.Parse<RedisCacheSku.Sku>(ec.Sku)) : (RedisCacheSku?)null,
                    ec.Capacity,
                    ec.MaxMemoryPolicy is not null ? new MaxMemoryPolicy(Enum.Parse<MaxMemoryPolicy.Policy>(ec.MaxMemoryPolicy)) : (MaxMemoryPolicy?)null))
                .ToList());

        var savedRedisCache = await redisCacheRepository.AddAsync(redisCache);

        return mapper.Map<RedisCacheResult>(savedRedisCache);
    }
}
