using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

public record CreateRedisCacheCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    int? RedisVersion,
    bool EnableNonSslPort,
    string? MinimumTlsVersion,
    bool DisableAccessKeyAuthentication,
    bool EnableAadAuth,
    IReadOnlyList<RedisCacheEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<RedisCacheResult>;
