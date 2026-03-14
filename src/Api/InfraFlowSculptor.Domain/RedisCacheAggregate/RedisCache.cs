using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate;

public class RedisCache : AzureResource
{
    public required RedisCacheSku Sku { get; set; }

    /// <summary>
    /// Capacity of the Redis cache. For Basic/Standard: 0-6 (C0-C6). For Premium: 1-4 (P1-P4).
    /// </summary>
    public required int Capacity { get; set; }

    /// <summary>
    /// Redis server version (e.g., 6).
    /// </summary>
    public required int RedisVersion { get; set; }

    /// <summary>
    /// Whether to allow connections on the non-SSL port (6379).
    /// </summary>
    public required bool EnableNonSslPort { get; set; }

    public required TlsVersion MinimumTlsVersion { get; set; }

    public required MaxMemoryPolicy MaxMemoryPolicy { get; set; }

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private RedisCache()
    {
    }

    public void Update(
        Name name,
        Location location,
        RedisCacheSku sku,
        RedisCacheSettings settings)
    {
        Name = name;
        Location = location;
        Sku = sku;
        Capacity = settings.Capacity;
        RedisVersion = settings.RedisVersion;
        EnableNonSslPort = settings.EnableNonSslPort;
        MinimumTlsVersion = settings.MinimumTlsVersion;
        MaxMemoryPolicy = settings.MaxMemoryPolicy;
    }

    public static RedisCache Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        RedisCacheSku sku,
        RedisCacheSettings settings)
    {
        return new RedisCache
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            Sku = sku,
            Capacity = settings.Capacity,
            RedisVersion = settings.RedisVersion,
            EnableNonSslPort = settings.EnableNonSslPort,
            MinimumTlsVersion = settings.MinimumTlsVersion,
            MaxMemoryPolicy = settings.MaxMemoryPolicy
        };
    }
}
