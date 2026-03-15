using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;

public class AddTableCommandHandler(IStorageAccountRepository storageAccountRepository, IMapper mapper)
    : IRequestHandler<AddTableCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(AddTableCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        var table = storageAccount.AddTable(request.Name);

        await storageAccountRepository.AddTableAsync(table);

        var updated = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        return mapper.Map<StorageAccountResult>(updated!);
    }
}
