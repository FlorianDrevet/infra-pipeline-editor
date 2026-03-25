using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

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
    IReadOnlyList<RedisCacheEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<RedisCacheResult>>;
