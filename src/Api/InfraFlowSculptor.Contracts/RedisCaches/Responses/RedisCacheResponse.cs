using InfraFlowSculptor.Contracts.RedisCaches.Requests;

namespace InfraFlowSculptor.Contracts.RedisCaches.Responses;

/// <summary>Represents an Azure Redis Cache resource.</summary>
public record RedisCacheResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<RedisCacheEnvironmentConfigResponse> EnvironmentSettings
);
