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
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveQueueCommand, ErrorOr<Deleted>>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveQueueCommand request, CancellationToken cancellationToken) =>
        StorageAccountAccessHelper.RemoveSubResourceAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser,
            () => storageAccountRepository.RemoveQueueAsync(request.StorageAccountId, request.QueueId),
            () => Errors.StorageAccount.QueueNotFoundError(request.QueueId),
            cancellationToken);
}
