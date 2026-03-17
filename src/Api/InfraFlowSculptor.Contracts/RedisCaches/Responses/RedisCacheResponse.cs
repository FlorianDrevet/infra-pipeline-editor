namespace InfraFlowSculptor.Contracts.RedisCaches.Responses;

/// <summary>Represents an Azure Redis Cache resource.</summary>
/// <param name="Id">Unique identifier of the Redis Cache.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Redis Cache.</param>
/// <param name="Location">Azure region where the Redis Cache is deployed.</param>
/// <param name="Sku">Pricing tier of the Redis Cache (e.g. "Basic", "Standard", "Premium").</param>
/// <param name="Capacity">Cache size (capacity tier).</param>
/// <param name="RedisVersion">Redis engine version (e.g. 4 or 6).</param>
/// <param name="EnableNonSslPort">Whether the non-SSL port (6379) is enabled.</param>
/// <param name="MinimumTlsVersion">Minimum accepted TLS version (e.g. "TLS1_2").</param>
/// <param name="MaxMemoryPolicy">Memory eviction policy (e.g. "allkeys-lru").</param>
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
