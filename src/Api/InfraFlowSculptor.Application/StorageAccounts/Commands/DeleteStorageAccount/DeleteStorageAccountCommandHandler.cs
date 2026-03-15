using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;

public class DeleteStorageAccountCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteStorageAccountCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(
            request.Id, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        await storageAccountRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
