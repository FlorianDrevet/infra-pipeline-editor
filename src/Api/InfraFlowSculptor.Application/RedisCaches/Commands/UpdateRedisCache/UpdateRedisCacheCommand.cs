using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;

public record UpdateRedisCacheCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    RedisCacheSku Sku,
    int Capacity,
    int RedisVersion,
    bool EnableNonSslPort,
    TlsVersion MinimumTlsVersion,
    MaxMemoryPolicy MaxMemoryPolicy,
    IReadOnlyList<RedisCacheEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<RedisCacheResult>>;
