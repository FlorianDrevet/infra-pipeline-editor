using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

public class AddBlobContainerCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<AddBlobContainerCommand, StorageAccountResult>
{
    public Task<ErrorOr<StorageAccountResult>> Handle(AddBlobContainerCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, resourceGroupRepository, accessService);
        return StorageAccountAccessHelper.AddSubResourceAndReloadAsync(
            ctx,
            sa => sa.AddBlobContainer(request.Name, request.PublicAccess),
            storageAccountRepository.AddBlobContainerAsync,
            mapper, cancellationToken);
    }
}
