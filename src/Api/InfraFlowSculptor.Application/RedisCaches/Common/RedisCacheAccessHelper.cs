using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.RedisCacheAggregate;

namespace InfraFlowSculptor.Application.RedisCaches.Common;

internal static class RedisCacheAccessHelper
{
    /// <summary>
    /// Loads a RedisCache and verifies the current user has read access to the parent
    /// InfrastructureConfig. Returns NotFoundError on any failure to avoid leaking
    /// the existence of the resource to non-members.
    /// </summary>
    public static async Task<ErrorOr<RedisCache>> GetWithReadAccessAsync(
        AzureResourceId redisCacheId,
        IRedisCacheRepository redisCacheRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var redisCache = await redisCacheRepository.GetByIdAsync(redisCacheId, cancellationToken);
        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(redisCacheId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(redisCache.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.RedisCache.NotFoundError(redisCacheId);

        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.RedisCache.NotFoundError(redisCacheId);

        return redisCache;
    }

    /// <summary>
    /// Loads a RedisCache and verifies the current user has write access (Owner or Contributor)
    /// to the parent InfrastructureConfig.
    /// Returns NotFoundError if the resource is not found or the user is not a member.
    /// Returns ForbiddenError if the user has read-only access.
    /// </summary>
    public static async Task<ErrorOr<RedisCache>> GetWithWriteAccessAsync(
        AzureResourceId redisCacheId,
        IRedisCacheRepository redisCacheRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var redisCache = await redisCacheRepository.GetByIdAsync(redisCacheId, cancellationToken);
        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(redisCacheId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(redisCache.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.RedisCache.NotFoundError(redisCacheId);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return redisCache;
    }
}
