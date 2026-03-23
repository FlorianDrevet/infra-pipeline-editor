using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Common;

public record RedisCacheResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    RedisCacheSku Sku,
    int Capacity,
    int RedisVersion,
    bool EnableNonSslPort,
    TlsVersion MinimumTlsVersion,
    MaxMemoryPolicy MaxMemoryPolicy,
    IReadOnlyList<RedisCacheEnvironmentConfigData> EnvironmentSettings
);
