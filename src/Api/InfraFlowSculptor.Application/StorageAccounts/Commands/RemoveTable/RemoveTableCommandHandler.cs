using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;

public class RemoveTableCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveTableCommand, Deleted>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveTableCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.StorageAccountId, storageAccountRepository, resourceGroupRepository, accessService);
        return StorageAccountAccessHelper.RemoveSubResourceAsync(
            ctx,
            () => storageAccountRepository.RemoveTableAsync(request.StorageAccountId, request.TableId),
            () => Errors.StorageAccount.TableNotFoundError(request.TableId),
            cancellationToken);
    }
}
