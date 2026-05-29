using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using MapsterMapper;

namespace InfraFlowSculptor.Application.StorageAccounts.Common;

internal static class StorageAccountAccessHelper
{
    public static async Task<ErrorOr<StorageAccount>> GetWithReadAccessAsync(
        StorageAccountAccessContext ctx,
        CancellationToken cancellationToken)
    {
        return await GetWithAccessAsync(ctx, isWriteOperation: false, cancellationToken);
    }

    public static async Task<ErrorOr<StorageAccount>> GetWithWriteAccessAsync(
        StorageAccountAccessContext ctx,
        CancellationToken cancellationToken)
    {
        return await GetWithAccessAsync(ctx, isWriteOperation: true, cancellationToken);
    }

    public static async Task<ErrorOr<StorageAccountResult>> AddSubResourceAndReloadAsync<TSubResource>(
        StorageAccountAccessContext ctx,
        Func<StorageAccount, ErrorOr<TSubResource>> addSubResource,
        Func<TSubResource, Task<TSubResource>> persistSubResource,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(ctx, cancellationToken);
        if (saResult.IsError)
            return saResult.Errors;

        var storageAccount = saResult.Value;
        var addResult = addSubResource(storageAccount);

        if (addResult.IsError)
            return addResult.Errors;

        await persistSubResource(addResult.Value);

        var reloaded = await ctx.StorageAccountRepository.GetByIdWithSubResourcesAsync(ctx.StorageAccountId, cancellationToken);
        if (reloaded is null)
            return Errors.StorageAccount.NotFoundError(ctx.StorageAccountId);

        return mapper.Map<StorageAccountResult>(reloaded);
    }

    public static async Task<ErrorOr<Deleted>> RemoveSubResourceAsync(
        StorageAccountAccessContext ctx,
        Func<Task<bool>> removeSubResource,
        Func<Error> notFoundError,
        CancellationToken cancellationToken)
    {
        var saResult = await GetWithWriteAccessAsync(ctx, cancellationToken);
        if (saResult.IsError)
            return saResult.Errors;

        var removed = await removeSubResource();
        if (!removed)
            return notFoundError();

        return Result.Deleted;
    }

    private static async Task<ErrorOr<StorageAccount>> GetWithAccessAsync(
        StorageAccountAccessContext ctx,
        bool isWriteOperation,
        CancellationToken cancellationToken)
    {
        var storageAccount = isWriteOperation
            ? await ctx.StorageAccountRepository.GetByIdWithSubResourcesAsync(ctx.StorageAccountId, cancellationToken)
            : await ctx.StorageAccountRepository.GetByIdWithSubResourcesReadOnlyAsync(ctx.StorageAccountId, cancellationToken);
        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(ctx.StorageAccountId);

        if (storageAccount.ResourceGroup is null)
            return Errors.ResourceGroup.NotFound(storageAccount.ResourceGroupId);

        var authResult = isWriteOperation
            ? await ctx.AccessService.VerifyWriteAccessAsync(storageAccount.ResourceGroup.InfraConfigId, cancellationToken)
            : await ctx.AccessService.VerifyReadAccessAsync(storageAccount.ResourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return storageAccount;
    }
}

