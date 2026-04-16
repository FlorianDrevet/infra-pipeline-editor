using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;

public record UpdateRedisCacheCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    int? RedisVersion,
    bool EnableNonSslPort,
    string? MinimumTlsVersion,
    bool DisableAccessKeyAuthentication,
    bool EnableAadAuth,
    IReadOnlyCollection<RedisCacheEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<RedisCacheResult>;
