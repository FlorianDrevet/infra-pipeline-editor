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

    /// <summary>Redis engine version (e.g. 4 or 6).</summary>
    [RedisVersionValidation]
    public int? RedisVersion { get; init; }

    /// <summary>Whether the non-SSL port (6379) is enabled.</summary>
    public bool EnableNonSslPort { get; init; }

    /// <summary>Minimum TLS version for client connections.</summary>
    [EnumValidation(typeof(TlsVersion.Version))]
    public string? MinimumTlsVersion { get; init; }

    /// <summary>Whether access key (shared key) authentication is disabled.</summary>
    public bool DisableAccessKeyAuthentication { get; init; }

    /// <summary>Whether Microsoft Entra ID (AAD) authentication is enabled.</summary>
    public bool EnableAadAuth { get; init; }

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

    /// <summary>Optional max memory policy override.</summary>
    [EnumValidation(typeof(MaxMemoryPolicy.Policy))]
    public string? MaxMemoryPolicy { get; init; }
}

/// <summary>Response DTO for a typed per-environment Redis Cache configuration.</summary>
public record RedisCacheEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    string? MaxMemoryPolicy);
