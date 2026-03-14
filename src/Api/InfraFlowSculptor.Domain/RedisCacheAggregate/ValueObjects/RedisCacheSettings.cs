namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

/// <summary>
/// Groups the Redis-specific configuration settings to keep method signatures concise.
/// </summary>
public record RedisCacheSettings(
    int Capacity,
    int RedisVersion,
    bool EnableNonSslPort,
    TlsVersion MinimumTlsVersion,
    MaxMemoryPolicy MaxMemoryPolicy);
