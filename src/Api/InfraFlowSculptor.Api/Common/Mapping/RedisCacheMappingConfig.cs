using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Contracts.RedisCaches.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public sealed class RedisCacheMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateRedisCacheRequest, CreateRedisCacheCommand>()
            .Map(dest => dest.RedisVersion, src => src.RedisVersion)
            .Map(dest => dest.EnableNonSslPort, src => src.EnableNonSslPort)
            .Map(dest => dest.MinimumTlsVersion, src => src.MinimumTlsVersion)
            .Map(dest => dest.DisableAccessKeyAuthentication, src => src.DisableAccessKeyAuthentication)
            .Map(dest => dest.EnableAadAuth, src => src.EnableAadAuth)
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new RedisCacheEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Capacity, ec.MaxMemoryPolicy)).ToList());

        config.NewConfig<(Guid Id, UpdateRedisCacheRequest Request), UpdateRedisCacheCommand>()
            .MapWith(src => new UpdateRedisCacheCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.RedisVersion,
                src.Request.EnableNonSslPort,
                src.Request.MinimumTlsVersion,
                src.Request.DisableAccessKeyAuthentication,
                src.Request.EnableAadAuth,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new RedisCacheEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Capacity, ec.MaxMemoryPolicy)).ToList()));

        config.NewConfig<RedisCache, RedisCacheResult>()
            .Map(dest => dest.RedisVersion, src => src.RedisVersion)
            .Map(dest => dest.EnableNonSslPort, src => src.EnableNonSslPort)
            .Map(dest => dest.MinimumTlsVersion,
                src => src.MinimumTlsVersion != null ? src.MinimumTlsVersion.Value.ToString() : null)
            .Map(dest => dest.DisableAccessKeyAuthentication, src => src.DisableAccessKeyAuthentication)
            .Map(dest => dest.EnableAadAuth, src => src.EnableAadAuth)
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new RedisCacheEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku != null ? es.Sku.Value.ToString() : null,
                    es.Capacity,
                    es.MaxMemoryPolicy != null ? es.MaxMemoryPolicy.Value.ToString() : null)).ToList());

        config.NewConfig<RedisCacheEnvironmentConfigData, RedisCacheEnvironmentConfigResponse>()
            .MapWith(src => new RedisCacheEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku, src.Capacity, src.MaxMemoryPolicy));

        config.NewConfig<RedisCacheSku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, RedisCacheSku>()
            .MapWith(src => new RedisCacheSku(Enum.Parse<RedisCacheSku.Sku>(src)));

        config.NewConfig<TlsVersion, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, TlsVersion>()
            .MapWith(src => new TlsVersion(Enum.Parse<TlsVersion.Version>(src)));

        config.NewConfig<MaxMemoryPolicy, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, MaxMemoryPolicy>()
            .MapWith(src => new MaxMemoryPolicy(Enum.Parse<MaxMemoryPolicy.Policy>(src)));
    }
}
