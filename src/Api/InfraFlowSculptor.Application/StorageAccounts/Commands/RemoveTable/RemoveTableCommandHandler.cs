using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;

public class RemoveTableCommandHandler(IStorageAccountRepository storageAccountRepository)
    : IRequestHandler<RemoveTableCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(RemoveTableCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        var removed = await storageAccountRepository.RemoveTableAsync(request.TableId);

        if (!removed)
            return Errors.StorageAccount.TableNotFoundError(request.TableId);

        return Result.Deleted;
    }
}
