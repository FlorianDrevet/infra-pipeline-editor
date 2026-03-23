using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
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

    private readonly List<RedisCacheEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this Redis Cache.</summary>
    public IReadOnlyCollection<RedisCacheEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

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

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        RedisCacheSku? sku,
        int? capacity,
        int? redisVersion,
        bool? enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        MaxMemoryPolicy? maxMemoryPolicy)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, capacity, redisVersion, enableNonSslPort, minimumTlsVersion, maxMemoryPolicy);
        }
        else
        {
            _environmentSettings.Add(
                RedisCacheEnvironmentSettings.Create(Id, environmentName, sku, capacity, redisVersion, enableNonSslPort, minimumTlsVersion, maxMemoryPolicy));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, int? RedisVersion, bool? EnableNonSslPort, TlsVersion? MinimumTlsVersion, MaxMemoryPolicy? MaxMemoryPolicy)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                RedisCacheEnvironmentSettings.Create(Id, s.EnvironmentName, s.Sku, s.Capacity, s.RedisVersion, s.EnableNonSslPort, s.MinimumTlsVersion, s.MaxMemoryPolicy));
        }
    }

    public static RedisCache Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        RedisCacheSku sku,
        RedisCacheSettings settings,
        IReadOnlyList<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, int? RedisVersion, bool? EnableNonSslPort, TlsVersion? MinimumTlsVersion, MaxMemoryPolicy? MaxMemoryPolicy)>? environmentSettings = null)
    {
        var redisCache = new RedisCache
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

        if (environmentSettings is not null)
            redisCache.SetAllEnvironmentSettings(environmentSettings);

        return redisCache;
    }
}
