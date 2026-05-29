using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;

public class RemoveBlobContainerCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveBlobContainerCommand, Deleted>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveBlobContainerCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, accessService);
        return StorageAccountAccessHelper.RemoveSubResourceAsync(
            ctx,
            () => storageAccountRepository.RemoveBlobContainerAsync(request.StorageAccountId, request.ContainerId),
            () => Errors.StorageAccount.BlobContainerNotFoundError(request.ContainerId),
            cancellationToken);
    }
}
