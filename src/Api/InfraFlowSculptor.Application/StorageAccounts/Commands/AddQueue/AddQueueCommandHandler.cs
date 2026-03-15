using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
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
    public Task<ErrorOr<StorageAccountResult>> Handle(AddQueueCommand request, CancellationToken cancellationToken) =>
        StorageAccountAccessHelper.AddSubResourceAndReloadAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser,
            sa => sa.AddQueue(request.Name),
            storageAccountRepository.AddQueueAsync,
            mapper, cancellationToken);
}
