using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

public class AddTableCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<AddTableCommand, StorageAccountResult>
{
    public Task<ErrorOr<StorageAccountResult>> Handle(AddTableCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, resourceGroupRepository, accessService);
        return StorageAccountAccessHelper.AddSubResourceAndReloadAsync(
            ctx,
            sa => sa.AddTable(request.Name),
            storageAccountRepository.AddTableAsync,
            mapper, cancellationToken);
    }
}
