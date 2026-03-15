using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using MapsterMapper;

namespace InfraFlowSculptor.Application.StorageAccounts.Common;

internal static class StorageAccountAccessHelper
{
    /// <summary>
    /// Loads a StorageAccount (with sub-resources) and verifies the current user has read access
    /// to the parent InfrastructureConfig. Returns NotFoundError on any failure to avoid leaking
    /// the existence of the resource to non-members.
    /// </summary>
    public static async Task<ErrorOr<StorageAccount>> GetWithReadAccessAsync(
        AzureResourceId storageAccountId,
        IStorageAccountRepository storageAccountRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(storageAccountId, cancellationToken);
        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(storageAccount.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        return storageAccount;
    }

    /// <summary>
    /// Loads a StorageAccount (with sub-resources) and verifies the current user has write access
    /// (Owner or Contributor) to the parent InfrastructureConfig.
    /// Returns NotFoundError if the resource is not found or the user is not a member.
    /// Returns ForbiddenError if the user has read-only access.
    /// </summary>
    public static async Task<ErrorOr<StorageAccount>> GetWithWriteAccessAsync(
        AzureResourceId storageAccountId,
        IStorageAccountRepository storageAccountRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(storageAccountId, cancellationToken);
        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(storageAccount.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return storageAccount;
    }

    /// <summary>
    /// Verifies write access, adds a sub-resource to the aggregate, persists it, reloads the
    /// StorageAccount with all sub-resources, and maps it to <see cref="StorageAccountResult"/>.
    /// Eliminates the repetitive auth + add + reload + map pattern shared by Add*CommandHandlers.
    /// </summary>
    public static async Task<ErrorOr<StorageAccountResult>> AddSubResourceAndReloadAsync<TSubResource>(
        AzureResourceId storageAccountId,
        IStorageAccountRepository storageAccountRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        Func<StorageAccount, TSubResource> addToAggregate,
        Func<TSubResource, Task> persistSubResource,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(
            storageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var subResource = addToAggregate(saResult.Value);
        await persistSubResource(subResource);

        var updated = await storageAccountRepository.GetByIdWithSubResourcesAsync(storageAccountId, cancellationToken);
        if (updated is null)
            return Errors.StorageAccount.NotFoundError(storageAccountId);

        return mapper.Map<StorageAccountResult>(updated);
    }

    /// <summary>
    /// Verifies write access then removes a sub-resource.
    /// Eliminates the repetitive auth + remove pattern shared by Remove*CommandHandlers.
    /// </summary>
    public static async Task<ErrorOr<Deleted>> RemoveSubResourceAsync(
        AzureResourceId storageAccountId,
        IStorageAccountRepository storageAccountRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        Func<Task<bool>> removeSubResource,
        Func<Error> subResourceNotFound,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(
            storageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var removed = await removeSubResource();
        if (!removed)
            return subResourceNotFound();

        return Result.Deleted;
    }
}
