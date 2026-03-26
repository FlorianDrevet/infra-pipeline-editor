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

        var storageAccount = StorageAccount.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            new StorageAccountKind(Enum.Parse<StorageAccountKind.Kind>(request.Kind)),
            new StorageAccessTier(Enum.Parse<StorageAccessTier.Tier>(request.AccessTier)),
            request.AllowBlobPublicAccess,
            request.EnableHttpsTrafficOnly,
            new StorageAccountTlsVersion(Enum.Parse<StorageAccountTlsVersion.Version>(request.MinimumTlsVersion)),
            request.EnvironmentSettings?
                .Select(ec => (
                    ec.EnvironmentName,
                    ec.Sku is not null ? new StorageAccountSku(Enum.Parse<StorageAccountSku.Sku>(ec.Sku)) : (StorageAccountSku?)null))
                .ToList(),
            request.CorsRules?
                .Select(rule => (
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds))
                .ToList());

        var saved = await storageAccountRepository.AddAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(saved);
    }
}
