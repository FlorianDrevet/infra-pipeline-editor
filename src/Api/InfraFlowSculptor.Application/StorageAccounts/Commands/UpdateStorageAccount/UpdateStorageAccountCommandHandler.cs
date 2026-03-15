using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;

public class UpdateStorageAccountCommandHandler(IStorageAccountRepository storageAccountRepository, IMapper mapper)
    : IRequestHandler<UpdateStorageAccountCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(UpdateStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(request.Id, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(request.Id);

        storageAccount.Update(
            request.Name,
            request.Location,
            request.Sku,
            request.Kind,
            request.AccessTier,
            request.AllowBlobPublicAccess,
            request.EnableHttpsTrafficOnly,
            request.MinimumTlsVersion);

        var updated = await storageAccountRepository.UpdateAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(updated);
    }
}
