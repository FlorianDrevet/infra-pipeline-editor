using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

public class CreateStorageAccountCommandHandler(
    IStorageAccountRepository storageAccountRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<CreateStorageAccountCommand, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(CreateStorageAccountCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

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
            settings,
            request.EnvironmentSettings?
                .Select(ec => (
                    ec.EnvironmentName,
                    ec.Sku is not null ? new StorageAccountSku(Enum.Parse<StorageAccountSku.Sku>(ec.Sku)) : (StorageAccountSku?)null,
                    ec.Kind is not null ? new StorageAccountKind(Enum.Parse<StorageAccountKind.Kind>(ec.Kind)) : (StorageAccountKind?)null,
                    ec.AccessTier is not null ? new StorageAccessTier(Enum.Parse<StorageAccessTier.Tier>(ec.AccessTier)) : (StorageAccessTier?)null,
                    ec.AllowBlobPublicAccess,
                    ec.EnableHttpsTrafficOnly,
                    ec.MinimumTlsVersion is not null ? new StorageAccountTlsVersion(Enum.Parse<StorageAccountTlsVersion.Version>(ec.MinimumTlsVersion)) : (StorageAccountTlsVersion?)null))
                .ToList());

        var saved = await storageAccountRepository.AddAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(saved);
    }
}
