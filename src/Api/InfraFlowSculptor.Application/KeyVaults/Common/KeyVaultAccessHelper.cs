using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.KeyVaultAggregate;

namespace InfraFlowSculptor.Application.KeyVaults.Common;

internal static class KeyVaultAccessHelper
{
    public static Task<ErrorOr<KeyVault>> GetWithReadAccessAsync(
        AzureResourceId keyVaultId,
        IKeyVaultRepository keyVaultRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithReadAccessAsync(
            keyVaultId, (id, ct) => keyVaultRepository.GetByIdAsync(id, ct),
            Errors.KeyVault.NotFoundError,
            resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

    public static Task<ErrorOr<KeyVault>> GetWithWriteAccessAsync(
        AzureResourceId keyVaultId,
        IKeyVaultRepository keyVaultRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken) =>
        AzureResourceAccessHelper.GetWithWriteAccessAsync(
            keyVaultId, (id, ct) => keyVaultRepository.GetByIdAsync(id, ct),
            Errors.KeyVault.NotFoundError,
            resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);
}
