using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
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
    public static Task<ErrorOr<StorageAccount>> GetWithReadAccessAsync(
        StorageAccountAccessContext ctx,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithReadAccessAsync(
            ctx.StorageAccountId, ctx.StorageAccountRepository.GetByIdWithSubResourcesAsync,
            Errors.StorageAccount.NotFoundError,
            ctx.ResourceGroupRepository, ctx.InfraConfigRepository, ctx.CurrentUser, cancellationToken);

    /// <summary>
    /// Loads a StorageAccount (with sub-resources) and verifies the current user has write access
    /// (Owner or Contributor) to the parent InfrastructureConfig.
    /// Returns NotFoundError if the resource is not found or the user is not a member.
    /// Returns ForbiddenError if the user has read-only access.
    /// </summary>
    public static Task<ErrorOr<StorageAccount>> GetWithWriteAccessAsync(
        StorageAccountAccessContext ctx,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithWriteAccessAsync(
            ctx.StorageAccountId, ctx.StorageAccountRepository.GetByIdWithSubResourcesAsync,
            Errors.StorageAccount.NotFoundError,
            ctx.ResourceGroupRepository, ctx.InfraConfigRepository, ctx.CurrentUser, cancellationToken);

    /// <summary>
    /// Verifies write access, adds a sub-resource to the aggregate, persists it, reloads the
    /// StorageAccount with all sub-resources, and maps it to <see cref="StorageAccountResult"/>.
    /// Eliminates the repetitive auth + add + reload + map pattern shared by Add*CommandHandlers.
    /// </summary>
    public static async Task<ErrorOr<StorageAccountResult>> AddSubResourceAndReloadAsync<TSubResource>(
        StorageAccountAccessContext ctx,
        Func<StorageAccount, TSubResource> addToAggregate,
        Func<TSubResource, Task> persistSubResource,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(ctx, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var subResource = addToAggregate(saResult.Value);
        await persistSubResource(subResource);

        var updated = await ctx.StorageAccountRepository.GetByIdWithSubResourcesAsync(ctx.StorageAccountId, cancellationToken);
        if (updated is null)
            return Errors.StorageAccount.NotFoundError(ctx.StorageAccountId);

        return mapper.Map<StorageAccountResult>(updated);
    }

    /// <summary>
    /// Verifies write access then removes a sub-resource.
    /// Eliminates the repetitive auth + remove pattern shared by Remove*CommandHandlers.
    /// </summary>
    public static async Task<ErrorOr<Deleted>> RemoveSubResourceAsync(
        StorageAccountAccessContext ctx,
        Func<Task<bool>> removeSubResource,
        Func<Error> subResourceNotFound,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(ctx, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var removed = await removeSubResource();
        if (!removed)
            return subResourceNotFound();

        return Result.Deleted;
    }
}
