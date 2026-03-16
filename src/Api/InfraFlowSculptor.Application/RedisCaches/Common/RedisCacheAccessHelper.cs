using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.RedisCacheAggregate;

namespace InfraFlowSculptor.Application.RedisCaches.Common;

internal static class RedisCacheAccessHelper
{
    public static Task<ErrorOr<RedisCache>> GetWithReadAccessAsync(
        AzureResourceId redisCacheId,
        IRedisCacheRepository redisCacheRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithReadAccessAsync(
            redisCacheId, (id, ct) => redisCacheRepository.GetByIdAsync(id, ct),
            Errors.RedisCache.NotFoundError,
            resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

    public static Task<ErrorOr<RedisCache>> GetWithWriteAccessAsync(
        AzureResourceId redisCacheId,
        IRedisCacheRepository redisCacheRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithWriteAccessAsync(
            redisCacheId, (id, ct) => redisCacheRepository.GetByIdAsync(id, ct),
            Errors.RedisCache.NotFoundError,
            resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);
}
