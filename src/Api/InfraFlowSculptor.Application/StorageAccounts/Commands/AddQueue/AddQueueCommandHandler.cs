using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;

public class AddQueueCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<AddQueueCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(AddQueueCommand request, CancellationToken cancellationToken)
    {
        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var queue = saResult.Value.AddQueue(request.Name);

        await storageAccountRepository.AddQueueAsync(queue);

        var updated = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);
        if (updated is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        return mapper.Map<StorageAccountResult>(updated);
    }
}
