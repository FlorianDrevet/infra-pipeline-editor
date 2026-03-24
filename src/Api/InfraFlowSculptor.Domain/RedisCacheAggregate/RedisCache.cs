using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate;

public class RedisCache : AzureResource
{
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
        Location location)
    {
        Name = name;
        Location = location;
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
        IReadOnlyList<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, int? RedisVersion, bool? EnableNonSslPort, TlsVersion? MinimumTlsVersion, MaxMemoryPolicy? MaxMemoryPolicy)>? environmentSettings = null)
    {
        var redisCache = new RedisCache
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            redisCache.SetAllEnvironmentSettings(environmentSettings);

        return redisCache;
    }
}
