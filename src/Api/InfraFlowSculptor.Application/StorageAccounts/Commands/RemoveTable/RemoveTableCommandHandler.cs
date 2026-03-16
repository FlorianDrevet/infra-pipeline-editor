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
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveTableCommand, ErrorOr<Deleted>>
{
    public Task<ErrorOr<Deleted>> Handle(RemoveTableCommand request, CancellationToken cancellationToken) =>
        StorageAccountAccessHelper.RemoveSubResourceAsync(
            request.StorageAccountId, storageAccountRepository, resourceGroupRepository, infraConfigRepository, currentUser,
            () => storageAccountRepository.RemoveTableAsync(request.StorageAccountId, request.TableId),
            () => Errors.StorageAccount.TableNotFoundError(request.TableId),
            cancellationToken);
}
