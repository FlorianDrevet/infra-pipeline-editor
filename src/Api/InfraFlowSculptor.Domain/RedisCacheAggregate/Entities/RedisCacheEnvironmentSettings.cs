using InfraFlowSculptor.Domain.Common.Models;
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

    /// <summary>Gets or sets the max memory eviction policy override.</summary>
    public MaxMemoryPolicy? MaxMemoryPolicy { get; private set; }

    private RedisCacheEnvironmentSettings() { }

    internal RedisCacheEnvironmentSettings(
        AzureResourceId redisCacheId,
        string environmentName,
        RedisCacheSku? sku,
        int? capacity,
        MaxMemoryPolicy? maxMemoryPolicy)
        : base(RedisCacheEnvironmentSettingsId.CreateUnique())
    {
        RedisCacheId = redisCacheId;
        EnvironmentName = environmentName;
        Sku = sku;
        Capacity = capacity;
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
        MaxMemoryPolicy? maxMemoryPolicy)
        => new(redisCacheId, environmentName, sku, capacity, maxMemoryPolicy);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        RedisCacheSku? sku,
        int? capacity,
        MaxMemoryPolicy? maxMemoryPolicy)
    {
        Sku = sku;
        Capacity = capacity;
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
        return dict;
    }
}
