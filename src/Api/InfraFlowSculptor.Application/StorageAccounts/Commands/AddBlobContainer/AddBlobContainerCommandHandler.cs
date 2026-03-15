using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;

public class AddBlobContainerCommandHandler(IStorageAccountRepository storageAccountRepository, IMapper mapper)
    : IRequestHandler<AddBlobContainerCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(AddBlobContainerCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        var container = storageAccount.AddBlobContainer(request.Name, request.PublicAccess);

        await storageAccountRepository.AddBlobContainerAsync(container);

        var updated = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.StorageAccountId, cancellationToken);
        if (updated is null)
            return Errors.StorageAccount.NotFoundError(request.StorageAccountId);

        return mapper.Map<StorageAccountResult>(updated);
    }
}
