using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="RedisCache"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class RedisCacheEnvironmentSettings : Entity<RedisCacheEnvironmentSettingsId>
{
    /// <summary>Gets the parent Redis Cache identifier.</summary>
    public AzureResourceId RedisCacheId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override.</summary>
    public RedisCacheSku? Sku { get; private set; }

    /// <summary>Gets or sets the cache capacity override.</summary>
    public int? Capacity { get; private set; }

    /// <summary>Gets or sets the Redis engine version override.</summary>
    public int? RedisVersion { get; private set; }

    /// <summary>Gets or sets whether the non-SSL port is enabled for this environment.</summary>
    public bool? EnableNonSslPort { get; private set; }

    /// <summary>Gets or sets the minimum TLS version override.</summary>
    public TlsVersion? MinimumTlsVersion { get; private set; }

    /// <summary>Gets or sets the max memory eviction policy override.</summary>
    public MaxMemoryPolicy? MaxMemoryPolicy { get; private set; }

    private RedisCacheEnvironmentSettings() { }

    internal RedisCacheEnvironmentSettings(
        AzureResourceId redisCacheId,
        string environmentName,
        RedisCacheSku? sku,
        int? capacity,
        int? redisVersion,
        bool? enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        MaxMemoryPolicy? maxMemoryPolicy)
        : base(RedisCacheEnvironmentSettingsId.CreateUnique())
    {
        RedisCacheId = redisCacheId;
        EnvironmentName = environmentName;
        Sku = sku;
        Capacity = capacity;
        RedisVersion = redisVersion;
        EnableNonSslPort = enableNonSslPort;
        MinimumTlsVersion = minimumTlsVersion;
        MaxMemoryPolicy = maxMemoryPolicy;
    }

    /// <summary>
    /// Creates a new <see cref="RedisCacheEnvironmentSettings"/> for the specified Redis Cache and environment.
    /// </summary>
    public static RedisCacheEnvironmentSettings Create(
        AzureResourceId redisCacheId,
        string environmentName,
        RedisCacheSku? sku,
        int? capacity,
        int? redisVersion,
        bool? enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        MaxMemoryPolicy? maxMemoryPolicy)
        => new(redisCacheId, environmentName, sku, capacity, redisVersion, enableNonSslPort, minimumTlsVersion, maxMemoryPolicy);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        RedisCacheSku? sku,
        int? capacity,
        int? redisVersion,
        bool? enableNonSslPort,
        TlsVersion? minimumTlsVersion,
        MaxMemoryPolicy? maxMemoryPolicy)
    {
        Sku = sku;
        Capacity = capacity;
        RedisVersion = redisVersion;
        EnableNonSslPort = enableNonSslPort;
        MinimumTlsVersion = minimumTlsVersion;
        MaxMemoryPolicy = maxMemoryPolicy;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["skuName"] = Sku.Value.ToString();
        if (Sku is not null) dict["skuFamily"] = Sku.Value == RedisCacheSku.Sku.Premium ? "P" : "C";
        if (Capacity is not null) dict["capacity"] = Capacity.Value.ToString();
        if (RedisVersion is not null) dict["redisVersion"] = RedisVersion.Value.ToString();
        if (EnableNonSslPort is not null) dict["enableNonSslPort"] = EnableNonSslPort.Value.ToString().ToLower();
        if (MinimumTlsVersion is not null)
        {
            dict["minimumTlsVersion"] = MinimumTlsVersion.Value switch
            {
                ValueObjects.TlsVersion.Version.Tls10 => "1.0",
                ValueObjects.TlsVersion.Version.Tls11 => "1.1",
                ValueObjects.TlsVersion.Version.Tls12 => "1.2",
                _ => "1.2"
            };
        }
        return dict;
    }
}
