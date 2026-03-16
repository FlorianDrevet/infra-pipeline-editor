using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveBlobContainer;

public class RemoveBlobContainerCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveBlobContainerCommand, ErrorOr<Deleted>>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveBlobContainerCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser);
        return StorageAccountAccessHelper.RemoveSubResourceAsync(
            ctx,
            () => storageAccountRepository.RemoveBlobContainerAsync(request.StorageAccountId, request.ContainerId),
            () => Errors.StorageAccount.BlobContainerNotFoundError(request.ContainerId),
            cancellationToken);
    }
}
