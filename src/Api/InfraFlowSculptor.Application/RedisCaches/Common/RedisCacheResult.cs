using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Common;

public record RedisCacheResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    int? RedisVersion,
    bool EnableNonSslPort,
    string? MinimumTlsVersion,
    bool DisableAccessKeyAuthentication,
    bool EnableAadAuth,
    IReadOnlyCollection<RedisCacheEnvironmentConfigData> EnvironmentSettings
);
