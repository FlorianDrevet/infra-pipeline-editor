using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

public class CreateStorageAccountCommandHandler(IStorageAccountRepository storageAccountRepository, IMapper mapper)
    : IRequestHandler<CreateStorageAccountCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(CreateStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var settings = new StorageAccountSettings(
            request.Sku,
            request.Kind,
            request.AccessTier,
            request.AllowBlobPublicAccess,
            request.EnableHttpsTrafficOnly,
            request.MinimumTlsVersion);

        var storageAccount = StorageAccount.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            settings);

        var saved = await storageAccountRepository.AddAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(saved);
    }
}
