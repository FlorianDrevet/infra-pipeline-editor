using InfraFlowSculptor.Application.Common;
using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;

public record UpdateStorageAccountCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    StorageAccountSku Sku,
    StorageAccountKind Kind,
    StorageAccessTier AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    StorageAccountTlsVersion MinimumTlsVersion,
    IReadOnlyList<EnvironmentConfigData>? EnvironmentConfigs = null
) : IRequest<ErrorOr<StorageAccountResult>>;
