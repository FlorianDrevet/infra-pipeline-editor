using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Common;

/// <summary>
/// Groups the common dependencies required to perform access-checked operations on a StorageAccount.
/// Passed to <see cref="StorageAccountAccessHelper"/> methods to keep their parameter lists concise.
/// </summary>
internal record StorageAccountAccessContext(
    AzureResourceId StorageAccountId,
    IStorageAccountRepository StorageAccountRepository,
    IResourceGroupRepository ResourceGroupRepository,
    IInfraConfigAccessService AccessService);
