using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;

public class UpdateStorageAccountCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateStorageAccountCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(UpdateStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var ctx = new StorageAccountAccessContext(request.Id, storageAccountRepository, resourceGroupRepository, accessService);
        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(ctx, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var storageAccount = saResult.Value;

        var settings = new StorageAccountSettings(
            request.Sku,
            request.Kind,
            request.AccessTier,
            request.AllowBlobPublicAccess,
            request.EnableHttpsTrafficOnly,
            request.MinimumTlsVersion);

        storageAccount.Update(request.Name, request.Location, settings);

        if (request.EnvironmentSettings is not null)
            storageAccount.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (
                        ec.EnvironmentName,
                        ec.Sku is not null ? new StorageAccountSku(Enum.Parse<StorageAccountSku.Sku>(ec.Sku)) : (StorageAccountSku?)null,
                        ec.Kind is not null ? new StorageAccountKind(Enum.Parse<StorageAccountKind.Kind>(ec.Kind)) : (StorageAccountKind?)null,
                        ec.AccessTier is not null ? new StorageAccessTier(Enum.Parse<StorageAccessTier.Tier>(ec.AccessTier)) : (StorageAccessTier?)null,
                        ec.AllowBlobPublicAccess,
                        ec.EnableHttpsTrafficOnly,
                        ec.MinimumTlsVersion is not null ? new StorageAccountTlsVersion(Enum.Parse<StorageAccountTlsVersion.Version>(ec.MinimumTlsVersion)) : (StorageAccountTlsVersion?)null))
                    .ToList());

        var updated = await storageAccountRepository.UpdateAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(updated);
    }
}
