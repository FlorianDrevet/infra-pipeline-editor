using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>
/// Centralizes resource loading, authorization, and identity-type validation
/// that was previously duplicated across role-assignment command handlers.
/// </summary>
internal sealed class RoleAssignmentDomainService(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRoleAssignmentDomainService
{
    /// <inheritdoc />
    public async Task<ErrorOr<AzureResource>> LoadResourceAndAuthorizeAsync(
        AzureResourceId resourceId,
        bool includeRoleAssignments,
        CancellationToken cancellationToken)
    {
        var resource = includeRoleAssignments
            ? await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(resourceId, cancellationToken)
            : await azureResourceRepository.GetByIdAsync(resourceId, cancellationToken);

        if (resource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(resourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        return resource;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<ManagedIdentityType>> ValidateIdentityTypeAsync(
        string managedIdentityType,
        AzureResourceId? userAssignedIdentityId,
        CancellationToken cancellationToken)
    {
        var parsed = new ManagedIdentityType(
            Enum.Parse<ManagedIdentityType.IdentityTypeEnum>(managedIdentityType, ignoreCase: true));

        if (parsed.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned
            && userAssignedIdentityId is null)
            return Errors.RoleAssignment.UserAssignedIdentityRequired();

        if (userAssignedIdentityId is not null)
        {
            var identityResource = await azureResourceRepository.GetByIdAsync(
                userAssignedIdentityId, cancellationToken);

            if (identityResource is null)
                return Errors.RoleAssignment.UserAssignedIdentityNotFound(userAssignedIdentityId);
        }

        return parsed;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> ValidateIdentityResourceExistsAsync(
        AzureResourceId identityResourceId,
        CancellationToken cancellationToken)
    {
        var identityResource = await azureResourceRepository.GetByIdAsync(
            identityResourceId, cancellationToken);

        if (identityResource is null)
            return Errors.RoleAssignment.UserAssignedIdentityNotFound(identityResourceId);

        return new Success();
    }
}
