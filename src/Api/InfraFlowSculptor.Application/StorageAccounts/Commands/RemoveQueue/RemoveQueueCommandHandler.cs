using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;

public class RemoveQueueCommandHandler(IStorageAccountRepository storageAccountRepository)
    : IRequestHandler<RemoveQueueCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(RemoveQueueCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        var removed = await storageAccountRepository.RemoveQueueAsync(request.QueueId);

        if (!removed)
            return Errors.StorageAccount.QueueNotFoundError(request.QueueId);

        return Result.Deleted;
    }
}
