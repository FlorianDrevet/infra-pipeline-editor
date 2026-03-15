using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;

public class DeleteStorageAccountCommandHandler(IStorageAccountRepository storageAccountRepository)
    : IRequestHandler<DeleteStorageAccountCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var deleted = await storageAccountRepository.DeleteAsync(request.Id);

        if (!deleted)
            return Errors.StorageAccount.NotFoundError(request.Id);

        return Result.Deleted;
    }
}
