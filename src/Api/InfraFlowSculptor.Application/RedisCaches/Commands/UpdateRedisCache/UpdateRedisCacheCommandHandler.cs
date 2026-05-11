using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
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

        var tlsParse = ParseTlsVersion(request.MinimumTlsVersion);
        if (tlsParse.IsError) return tlsParse.Errors;

        redisCache.Update(
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            tlsParse.Value,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth);

        if (request.EnvironmentSettings is not null)
        {
            var parsed = ParseEnvironmentSettings(request.EnvironmentSettings);
            if (parsed.IsError) return parsed.Errors;
            redisCache.SetAllEnvironmentSettings(parsed.Value);
        }

        var updatedRedisCache = await redisCacheRepository.UpdateAsync(redisCache);
        return mapper.Map<RedisCacheResult>(updatedRedisCache);
    }

    private static ErrorOr<TlsVersion?> ParseTlsVersion(string? raw)
    {
        return EnumValueObjectParser.ParseOrNull<TlsVersion.Version, TlsVersion>(
            raw,
            static parsed => new TlsVersion(parsed),
            Errors.RedisCache.InvalidMinimumTlsVersion);
    }

    private static ErrorOr<List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>> ParseEnvironmentSettings(
        IEnumerable<RedisCacheEnvironmentConfigData> environmentSettings)
    {
        var parsedSettings = new List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>();
        foreach (var ec in environmentSettings)
        {
            var skuParse = ParseSku(ec.Sku);
            if (skuParse.IsError) return skuParse.Errors;

            var policyParse = ParseMaxMemoryPolicy(ec.MaxMemoryPolicy);
            if (policyParse.IsError) return policyParse.Errors;

            parsedSettings.Add((ec.EnvironmentName, skuParse.Value, ec.Capacity, policyParse.Value));
        }

        return parsedSettings;
    }

    private static ErrorOr<RedisCacheSku?> ParseSku(string? raw)
    {
        return EnumValueObjectParser.ParseOrNull<RedisCacheSku.Sku, RedisCacheSku>(
            raw,
            static parsed => new RedisCacheSku(parsed),
            Errors.RedisCache.InvalidSku);
    }

    private static ErrorOr<MaxMemoryPolicy?> ParseMaxMemoryPolicy(string? raw)
    {
        return EnumValueObjectParser.ParseOrNull<MaxMemoryPolicy.Policy, MaxMemoryPolicy>(
            raw,
            static parsed => new MaxMemoryPolicy(parsed),
            Errors.RedisCache.InvalidMaxMemoryPolicy);
    }
}
