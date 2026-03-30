using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;

public class RemoveQueueCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveQueueCommand, Deleted>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveQueueCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, resourceGroupRepository, accessService);
        return StorageAccountAccessHelper.RemoveSubResourceAsync(
            ctx,
            () => storageAccountRepository.RemoveQueueAsync(request.StorageAccountId, request.QueueId),
            () => Errors.StorageAccount.QueueNotFoundError(request.QueueId),
            cancellationToken);
    }
}
