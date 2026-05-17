using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;

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
        var accessResult = await EnsureWriteAccessAsync(request.ResourceGroupId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var tlsVersionResult = ParseMinimumTlsVersion(request.MinimumTlsVersion);
        if (tlsVersionResult.IsError)
            return tlsVersionResult.Errors;

        var environmentSettingsResult = ParseEnvironmentSettings(request.EnvironmentSettings);
        if (environmentSettingsResult.IsError)
            return environmentSettingsResult.Errors;

        var redisCache = RedisCache.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.RedisVersion,
            request.EnableNonSslPort,
            tlsVersionResult.Value,
            request.DisableAccessKeyAuthentication,
            request.EnableAadAuth,
            environmentSettingsResult.Value,
            isExisting: request.IsExisting);

        var savedRedisCache = redisCacheRepository.Add(redisCache);

        return mapper.Map<RedisCacheResult>(savedRedisCache);
    }

    private async Task<ErrorOr<Success>> EnsureWriteAccessAsync(
        Domain.ResourceGroupAggregate.ValueObjects.ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(resourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return Result.Success;
    }

    private static ErrorOr<TlsVersion?> ParseMinimumTlsVersion(string? minimumTlsVersion)
    {
        return EnumValueObjectParser.ParseOrNull<TlsVersion.Version, TlsVersion>(
            minimumTlsVersion,
            static parsed => new TlsVersion(parsed),
            Errors.RedisCache.InvalidMinimumTlsVersion);
    }

    private static ErrorOr<List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>?> ParseEnvironmentSettings(
        IReadOnlyList<RedisCacheEnvironmentConfigData>? environmentSettings)
    {
        if (environmentSettings is null)
            return (List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>?)null;

        var parsedSettings = new List<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>(environmentSettings.Count);
        foreach (var environmentSetting in environmentSettings)
        {
            var parsedSettingResult = ParseEnvironmentSetting(environmentSetting);
            if (parsedSettingResult.IsError)
                return parsedSettingResult.Errors;

            parsedSettings.Add(parsedSettingResult.Value);
        }

        return parsedSettings;
    }

    private static ErrorOr<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)> ParseEnvironmentSetting(
        RedisCacheEnvironmentConfigData environmentSetting)
    {
        var skuResult = ParseSku(environmentSetting.Sku);
        if (skuResult.IsError)
            return skuResult.Errors;

        var maxMemoryPolicyResult = ParseMaxMemoryPolicy(environmentSetting.MaxMemoryPolicy);
        if (maxMemoryPolicyResult.IsError)
            return maxMemoryPolicyResult.Errors;

        return (environmentSetting.EnvironmentName, skuResult.Value, environmentSetting.Capacity, maxMemoryPolicyResult.Value);
    }

    private static ErrorOr<RedisCacheSku?> ParseSku(string? sku)
    {
        return EnumValueObjectParser.ParseOrNull<RedisCacheSku.Sku, RedisCacheSku>(
            sku,
            static parsed => new RedisCacheSku(parsed),
            Errors.RedisCache.InvalidSku);
    }

    private static ErrorOr<MaxMemoryPolicy?> ParseMaxMemoryPolicy(string? maxMemoryPolicy)
    {
        return EnumValueObjectParser.ParseOrNull<MaxMemoryPolicy.Policy, MaxMemoryPolicy>(
            maxMemoryPolicy,
            static parsed => new MaxMemoryPolicy(parsed),
            Errors.RedisCache.InvalidMaxMemoryPolicy);
    }
}
