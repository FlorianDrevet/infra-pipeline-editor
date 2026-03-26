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
        var corsRules = CorsRuleSanitizer.Sanitize(request.CorsRules);
        var tableCorsRules = CorsRuleSanitizer.Sanitize(request.TableCorsRules);

        var ctx = new StorageAccountAccessContext(request.Id, storageAccountRepository, resourceGroupRepository, accessService);
        var saResult = await StorageAccountAccessHelper.GetWithWriteAccessAsync(ctx, cancellationToken);

        if (saResult.IsError)
            return saResult.Errors;

        var storageAccount = saResult.Value;

        storageAccount.Update(
            request.Name,
            request.Location,
            new StorageAccountKind(Enum.Parse<StorageAccountKind.Kind>(request.Kind)),
            new StorageAccessTier(Enum.Parse<StorageAccessTier.Tier>(request.AccessTier)),
            request.AllowBlobPublicAccess,
            request.EnableHttpsTrafficOnly,
            new StorageAccountTlsVersion(Enum.Parse<StorageAccountTlsVersion.Version>(request.MinimumTlsVersion)));

        if (request.EnvironmentSettings is not null)
            storageAccount.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (
                        ec.EnvironmentName,
                        ec.Sku is not null ? new StorageAccountSku(Enum.Parse<StorageAccountSku.Sku>(ec.Sku)) : (StorageAccountSku?)null))
                    .ToList());

        if (corsRules is not null)
            storageAccount.SetCorsRules(
                corsRules
                    .Select(rule => (
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds))
                    .ToList());

        if (tableCorsRules is not null)
            storageAccount.SetTableCorsRules(
                tableCorsRules
                    .Select(rule => (
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds))
                    .ToList());

        var updated = await storageAccountRepository.UpdateAsync(storageAccount);

        return mapper.Map<StorageAccountResult>(updated);
    }
}
