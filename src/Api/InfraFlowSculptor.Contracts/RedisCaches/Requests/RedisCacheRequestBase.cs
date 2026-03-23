using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.RedisCaches.Requests;

/// <summary>Common properties shared by create and update Redis Cache requests.</summary>
public abstract class RedisCacheRequestBase
{
    /// <summary>Display name for the Redis Cache resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Redis Cache will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Pricing tier for the Redis Cache. Accepted values: <c>Basic</c>, <c>Standard</c>, <c>Premium</c>.</summary>
    [Required, EnumValidation(typeof(RedisCacheSku.Sku))]
    public required string Sku { get; init; }

    /// <summary>Redis engine version. Accepted values: <c>4</c>, <c>6</c>.</summary>
    [Required, RedisVersionValidation]
    public required int RedisVersion { get; init; }

    /// <summary>When <c>true</c>, the non-SSL port (6379) is enabled in addition to the SSL port (6380).</summary>
    [Required]
    public required bool EnableNonSslPort { get; init; }

    /// <summary>Minimum TLS protocol version accepted by the cache. Accepted values: <c>TLS1_0</c>, <c>TLS1_1</c>, <c>TLS1_2</c>.</summary>
    [Required, EnumValidation(typeof(TlsVersion.Version))]
    public required string MinimumTlsVersion { get; init; }

    /// <summary>
    /// Eviction policy applied when the cache reaches its memory limit.
    /// </summary>
    [Required, EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public required string MaxMemoryPolicy { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<RedisCacheEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Redis Cache.</summary>
public class RedisCacheEnvironmentConfigEntry
{
    /// <summary>Name of the target environment.</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override.</summary>
    [EnumValidation(typeof(RedisCacheSku.Sku))]
    public string? Sku { get; init; }

    /// <summary>Optional capacity override.</summary>
    public int? Capacity { get; init; }

    /// <summary>Optional Redis version override.</summary>
    [RedisVersionValidation]
    public int? RedisVersion { get; init; }

    /// <summary>Optional non-SSL port override.</summary>
    public bool? EnableNonSslPort { get; init; }

    /// <summary>Optional minimum TLS version override.</summary>
    [EnumValidation(typeof(TlsVersion.Version))]
    public string? MinimumTlsVersion { get; init; }

    /// <summary>Optional max memory policy override.</summary>
    [EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public string? MaxMemoryPolicy { get; init; }
}

/// <summary>Response DTO for a typed per-environment Redis Cache configuration.</summary>
public record RedisCacheEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    int? RedisVersion,
    bool? EnableNonSslPort,
    string? MinimumTlsVersion,
    string? MaxMemoryPolicy);
