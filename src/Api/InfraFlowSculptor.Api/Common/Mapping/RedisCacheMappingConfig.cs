using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Contracts.RedisCaches.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class RedisCacheMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateRedisCacheRequest, CreateRedisCacheCommand>();

        config.NewConfig<(Guid Id, UpdateRedisCacheRequest Request), UpdateRedisCacheCommand>()
            .MapWith(src => new UpdateRedisCacheCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.Sku.Adapt<RedisCacheSku>(),
                src.Request.Capacity,
                src.Request.RedisVersion,
                src.Request.EnableNonSslPort,
                src.Request.MinimumTlsVersion.Adapt<TlsVersion>(),
                src.Request.MaxMemoryPolicy.Adapt<MaxMemoryPolicy>()));

        config.NewConfig<RedisCacheSku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, RedisCacheSku>()
            .MapWith(src => new RedisCacheSku(Enum.Parse<RedisCacheSku.RedisCacheSkuEnum>(src)));

        config.NewConfig<TlsVersion, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, TlsVersion>()
            .MapWith(src => new TlsVersion(Enum.Parse<TlsVersion.TlsVersionEnum>(src)));

        config.NewConfig<MaxMemoryPolicy, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, MaxMemoryPolicy>()
            .MapWith(src => new MaxMemoryPolicy(Enum.Parse<MaxMemoryPolicy.MaxMemoryPolicyEnum>(src)));
    }
}
