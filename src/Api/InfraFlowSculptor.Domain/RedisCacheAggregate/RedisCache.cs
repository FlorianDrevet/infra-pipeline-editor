using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate;

/// <summary>
/// Represents an Azure Cache for Redis resource (<c>Microsoft.Cache/Redis</c>).
/// Supports per-environment SKU overrides, TLS configuration, and AAD authentication.
/// </summary>
public sealed class RedisCache : AzureResource
{
    private readonly List<RedisCacheEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Redis Cache.</summary>
    public IReadOnlyCollection<RedisCacheEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the Redis engine version (e.g. 4 or 6).</summary>
    public int? RedisVersion { get; private set; }

    /// <summary>Gets whether the non-SSL port (6379) is enabled.</summary>
    public bool EnableNonSslPort { get; private set; }

    /// <summary>Gets the minimum TLS version for client connections.</summary>
    public TlsVersion? MinimumTlsVersion { get; private set; }

    /// <summary>Gets whether access key (shared key) authentication is disabled.</summary>
    public bool DisableAccessKeyAuthentication { get; private set; }

    /// <summary>Gets whether Microsoft Entra ID (AAD) authentication is enabled.</summary>
    public bool EnableAadAuth { get; private set; }

    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private RedisCache()
    {
    }

    /// <summary>Updates the resource-level properties of this Redis Cache.</summary>
    public void Update(
        Name name,
        Location location,
        int? redisVersion,
        bool enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        bool disableAccessKeyAuthentication,
        bool enableAadAuth)
    {
        Name = name;
        Location = location;
        RedisVersion = redisVersion;
        EnableNonSslPort = enableNonSslPort;
        MinimumTlsVersion = minimumTlsVersion;
        DisableAccessKeyAuthentication = disableAccessKeyAuthentication;
        EnableAadAuth = enableAadAuth;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        RedisCacheSku? sku,
        int? capacity,
        MaxMemoryPolicy? maxMemoryPolicy)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, capacity, maxMemoryPolicy);
        }
        else
        {
            _environmentSettings.Add(
                RedisCacheEnvironmentSettings.Create(Id, environmentName, sku, capacity, maxMemoryPolicy));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)> settings)
    {
        _environmentSettings.Clear();
        foreach (var s in settings)
        {
            _environmentSettings.Add(
                RedisCacheEnvironmentSettings.Create(Id, s.EnvironmentName, s.Sku, s.Capacity, s.MaxMemoryPolicy));
        }
    }

    /// <summary>Creates a new <see cref="RedisCache"/> with a generated identifier.</summary>
    public static RedisCache Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        int? redisVersion,
        bool enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        bool disableAccessKeyAuthentication,
        bool enableAadAuth,
        IReadOnlyList<(string EnvironmentName, RedisCacheSku? Sku, int? Capacity, MaxMemoryPolicy? MaxMemoryPolicy)>? environmentSettings = null)
    {
        var redisCache = new RedisCache
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            RedisVersion = redisVersion,
            EnableNonSslPort = enableNonSslPort,
            MinimumTlsVersion = minimumTlsVersion,
            DisableAccessKeyAuthentication = disableAccessKeyAuthentication,
            EnableAadAuth = enableAadAuth
        };

        if (environmentSettings is not null)
            redisCache.SetAllEnvironmentSettings(environmentSettings);

        return redisCache;
    }
}
