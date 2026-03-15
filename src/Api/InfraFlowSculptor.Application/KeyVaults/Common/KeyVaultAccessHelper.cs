using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.KeyVaultAggregate;

namespace InfraFlowSculptor.Application.KeyVaults.Common;

internal static class KeyVaultAccessHelper
{
    /// <summary>
    /// Loads a KeyVault and verifies the current user has read access to the parent
    /// InfrastructureConfig. Returns NotFoundError on any failure to avoid leaking
    /// the existence of the resource to non-members.
    /// </summary>
    public static async Task<ErrorOr<KeyVault>> GetWithReadAccessAsync(
        AzureResourceId keyVaultId,
        IKeyVaultRepository keyVaultRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(keyVaultId, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(keyVaultId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(keyVault.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.KeyVault.NotFoundError(keyVaultId);

        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.KeyVault.NotFoundError(keyVaultId);

        return keyVault;
    }

    /// <summary>
    /// Loads a KeyVault and verifies the current user has write access (Owner or Contributor)
    /// to the parent InfrastructureConfig.
    /// Returns NotFoundError if the resource is not found or the user is not a member.
    /// Returns ForbiddenError if the user has read-only access.
    /// </summary>
    public static async Task<ErrorOr<KeyVault>> GetWithWriteAccessAsync(
        AzureResourceId keyVaultId,
        IKeyVaultRepository keyVaultRepository,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(keyVaultId, cancellationToken);
        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(keyVaultId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(keyVault.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.KeyVault.NotFoundError(keyVaultId);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return keyVault;
    }
}
