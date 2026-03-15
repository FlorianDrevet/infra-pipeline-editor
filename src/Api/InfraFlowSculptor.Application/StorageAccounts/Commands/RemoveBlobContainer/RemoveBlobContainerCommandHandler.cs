using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;

public class RemoveBlobContainerCommandHandler(IStorageAccountRepository storageAccountRepository)
    : IRequestHandler<RemoveBlobContainerCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(RemoveBlobContainerCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        var removed = await storageAccountRepository.RemoveBlobContainerAsync(request.ContainerId);

        if (!removed)
            return Errors.StorageAccount.BlobContainerNotFoundError(request.ContainerId);

        return Result.Deleted;
    }
}
