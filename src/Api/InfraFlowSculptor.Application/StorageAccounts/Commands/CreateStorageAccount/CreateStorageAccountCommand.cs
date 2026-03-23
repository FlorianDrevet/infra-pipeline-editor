using ErrorOr;
using InfraFlowSculptor.Application.Common;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

public record CreateStorageAccountCommand(
    ResourceGroupId ResourceGroupId,
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
