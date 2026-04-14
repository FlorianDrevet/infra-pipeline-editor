using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;

/// <summary>
/// Handles the <see cref="UpdateBlobContainerPublicAccessCommand"/> request
/// and returns the updated storage account if the caller has write access.
/// </summary>
public sealed class UpdateBlobContainerPublicAccessCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateBlobContainerPublicAccessCommand, StorageAccountResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<StorageAccountResult>> Handle(
        UpdateBlobContainerPublicAccessCommand request,
        CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(
            request.StorageAccountId,
            storageAccountRepository,
            resourceGroupRepository,
            accessService);

        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(ctx, cancellationToken);
        if (saResult.IsError)
            return saResult.Errors;

        var storageAccount = saResult.Value;

        var updateResult = storageAccount.UpdateBlobContainerPublicAccess(request.ContainerId, request.PublicAccess);
        if (updateResult.IsError)
            return updateResult.Errors;

        await storageAccountRepository.UpdateAsync(storageAccount);

        var reloaded = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);
        if (reloaded is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        return mapper.Map<StorageAccountResult>(reloaded);
    }
}
