using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;

/// <summary>Handles the <see cref="CheckAcrPullAccessQuery"/> request.</summary>
public sealed class CheckAcrPullAccessQueryHandler(
    IAzureResourceRepository azureResourceRepository)
    : IQueryHandler<CheckAcrPullAccessQuery, CheckAcrPullAccessResult>
{
    private const string AcrPullRoleName = "AcrPull";

    /// <inheritdoc />
    public async Task<ErrorOr<CheckAcrPullAccessResult>> Handle(
        CheckAcrPullAccessQuery request,
        CancellationToken cancellationToken)
    {
        if (string.Equals(request.AcrAuthMode, AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return CreateAdminCredentialsResult();
        }

        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.ContainerRegistry.NotFoundError(request.ResourceId);

        // If a specific ACR pull identity is requested, validate that specific identity
        if (request.AcrPullIdentityId is not null)
        {
            return await ValidateSpecificAcrPullIdentityAsync(
                request.AcrPullIdentityId,
                request.ContainerRegistryId,
                request.AcrAuthMode,
                resource,
                cancellationToken);
        }

        // Backward compatibility: find any UAI with AcrPull role on this registry
        return await ValidateAnyAcrPullIdentityAsync(
            request.ContainerRegistryId,
            request.AcrAuthMode,
            resource,
            cancellationToken);
    }

    private static CheckAcrPullAccessResult CreateAdminCredentialsResult()
    {
        return new CheckAcrPullAccessResult(
            HasAccess: true,
            MissingRoleDefinitionId: null,
            MissingRoleName: null,
            AssignedUserAssignedIdentityId: null,
            AssignedUserAssignedIdentityName: null,
            HasUserAssignedIdentity: false,
            AcrAuthMode: AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString());
    }

    private async Task<CheckAcrPullAccessResult> ValidateSpecificAcrPullIdentityAsync(
        AzureResourceId selectedIdentityId,
        AzureResourceId containerRegistryId,
        string? acrAuthMode,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        // First check if the role assignment exists on the current resource
        var roleAssignment = resource.RoleAssignments.FirstOrDefault(ra =>
            ra.UserAssignedIdentityId == selectedIdentityId &&
            ra.TargetResourceId == containerRegistryId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull &&
            ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned);

        // If not found locally, search globally: the UAI may have AcrPull on the ACR via another resource
        if (roleAssignment is null)
        {
            var allAssignmentsForIdentity = await azureResourceRepository
                .GetRoleAssignmentsByIdentityIdAsync(selectedIdentityId, cancellationToken);

            roleAssignment = allAssignmentsForIdentity.FirstOrDefault(ra =>
                ra.TargetResourceId == containerRegistryId &&
                ra.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull &&
                ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned);
        }

        var uaiResource = await azureResourceRepository.GetByIdReadOnlyAsync(
            selectedIdentityId, cancellationToken);

        var hasAccess = roleAssignment is not null;

        return new CheckAcrPullAccessResult(
            HasAccess: hasAccess,
            MissingRoleDefinitionId: hasAccess ? null : AzureRoleDefinitionCatalog.AcrPull,
            MissingRoleName: hasAccess ? null : AcrPullRoleName,
            AssignedUserAssignedIdentityId: selectedIdentityId.Value.ToString(),
            AssignedUserAssignedIdentityName: uaiResource?.Name.Value,
            HasUserAssignedIdentity: true,
            AcrAuthMode: acrAuthMode);
    }

    private async Task<CheckAcrPullAccessResult> ValidateAnyAcrPullIdentityAsync(
        AzureResourceId containerRegistryId,
        string? acrAuthMode,
        Domain.Common.BaseModels.AzureResource resource,
        CancellationToken cancellationToken)
    {
        // Look for a UAI-based AcrPull role assignment targeting this container registry
        var uaiAcrPull = resource.RoleAssignments.FirstOrDefault(ra =>
            ra.TargetResourceId == containerRegistryId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull &&
            ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned &&
            ra.UserAssignedIdentityId is not null);

        if (uaiAcrPull is not null)
        {
            var uaiResource = await azureResourceRepository.GetByIdReadOnlyAsync(
                uaiAcrPull.UserAssignedIdentityId!, cancellationToken);

            return new CheckAcrPullAccessResult(
                HasAccess: true,
                MissingRoleDefinitionId: null,
                MissingRoleName: null,
                AssignedUserAssignedIdentityId: uaiAcrPull.UserAssignedIdentityId!.Value.ToString(),
                AssignedUserAssignedIdentityName: uaiResource?.Name.Value,
                HasUserAssignedIdentity: true,
                AcrAuthMode: acrAuthMode);
        }

        // Look for any UAI-based role assignment targeting this container registry (but not AcrPull)
        var uaiOnAcr = resource.RoleAssignments.FirstOrDefault(ra =>
            ra.TargetResourceId == containerRegistryId &&
            ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned &&
            ra.UserAssignedIdentityId is not null);

        string? assignedUaiId = null;
        string? assignedUaiName = null;
        var hasUai = false;

        if (uaiOnAcr is not null)
        {
            assignedUaiId = uaiOnAcr.UserAssignedIdentityId!.Value.ToString();
            var uaiResource = await azureResourceRepository.GetByIdReadOnlyAsync(
                uaiOnAcr.UserAssignedIdentityId!, cancellationToken);
            assignedUaiName = uaiResource?.Name.Value;
            hasUai = true;
        }
        else
        {
            // Check if any UAI-based role assignment exists at all on this resource
            var anyUai = resource.RoleAssignments.FirstOrDefault(ra =>
                ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned &&
                ra.UserAssignedIdentityId is not null);

            if (anyUai is not null)
            {
                assignedUaiId = anyUai.UserAssignedIdentityId!.Value.ToString();
                var uaiResource = await azureResourceRepository.GetByIdReadOnlyAsync(
                    anyUai.UserAssignedIdentityId!, cancellationToken);
                assignedUaiName = uaiResource?.Name.Value;
                hasUai = true;
            }
        }

        return new CheckAcrPullAccessResult(
            HasAccess: false,
            MissingRoleDefinitionId: AzureRoleDefinitionCatalog.AcrPull,
            MissingRoleName: AcrPullRoleName,
            AssignedUserAssignedIdentityId: assignedUaiId,
            AssignedUserAssignedIdentityName: assignedUaiName,
            HasUserAssignedIdentity: hasUai,
            AcrAuthMode: acrAuthMode);
    }
}
