using InfraFlowSculptor.Contracts.RedisCaches.Requests;

namespace InfraFlowSculptor.Contracts.RedisCaches.Responses;

/// <summary>Represents an Azure Redis Cache resource.</summary>
public record RedisCacheResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    int? RedisVersion,
    bool EnableNonSslPort,
    string? MinimumTlsVersion,
    bool DisableAccessKeyAuthentication,
    bool EnableAadAuth,
    IReadOnlyCollection<RedisCacheEnvironmentConfigResponse> EnvironmentSettings
);
