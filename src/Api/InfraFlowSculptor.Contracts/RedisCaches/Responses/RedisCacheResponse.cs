namespace InfraFlowSculptor.Contracts.RedisCaches.Responses;

public record RedisCacheResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Sku,
    int Capacity,
    int RedisVersion,
    bool EnableNonSslPort,
    string MinimumTlsVersion,
    string MaxMemoryPolicy
);
