using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Generic helper that encapsulates the load-resource → load-ResourceGroup → verify-InfraConfig
/// access-check pattern shared by all AzureResource-derived access helpers.
/// </summary>
internal static class AzureResourceAccessHelper
{
    /// <summary>
    /// Loads a resource and verifies the current user has read access to the parent
    /// InfrastructureConfig. Returns <paramref name="notFoundError"/> on any failure
    /// to avoid leaking the existence of the resource to non-members.
    /// </summary>
    public static async Task<ErrorOr<TResource>> GetWithReadAccessAsync<TResource>(
        AzureResourceId resourceId,
        Func<AzureResourceId, CancellationToken, Task<TResource?>> loadResource,
        Func<AzureResourceId, Error> notFoundError,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
        where TResource : AzureResource
    {
        var resource = await loadResource(resourceId, cancellationToken);
        if (resource is null)
            return notFoundError(resourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return notFoundError(resourceId);

        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return notFoundError(resourceId);

        return resource;
    }

    /// <summary>
    /// Loads a resource and verifies the current user has write access (Owner or Contributor)
    /// to the parent InfrastructureConfig.
    /// Returns <paramref name="notFoundError"/> if the resource is not found or the user is not
    /// a member. Returns ForbiddenError if the user has read-only access.
    /// </summary>
    public static async Task<ErrorOr<TResource>> GetWithWriteAccessAsync<TResource>(
        AzureResourceId resourceId,
        Func<AzureResourceId, CancellationToken, Task<TResource?>> loadResource,
        Func<AzureResourceId, Error> notFoundError,
        IResourceGroupRepository resourceGroupRepository,
        IInfrastructureConfigRepository infraConfigRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
        where TResource : AzureResource
    {
        var resource = await loadResource(resourceId, cancellationToken);
        if (resource is null)
            return notFoundError(resourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return notFoundError(resourceId);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return resource;
    }
}
