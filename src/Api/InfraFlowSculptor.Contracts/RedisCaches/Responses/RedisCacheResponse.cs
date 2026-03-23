using InfraFlowSculptor.Contracts.Common;

namespace InfraFlowSculptor.Contracts.RedisCaches.Responses;

/// <summary>Represents an Azure Redis Cache resource.</summary>
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
    string MaxMemoryPolicy,
    IReadOnlyList<ResourceEnvironmentConfigResponse> EnvironmentConfigs
);
