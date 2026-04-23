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
    /// <inheritdoc />
    public async Task<ErrorOr<CheckAcrPullAccessResult>> Handle(
        CheckAcrPullAccessQuery request,
        CancellationToken cancellationToken)
    {
        if (string.Equals(request.AcrAuthMode, AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString(), StringComparison.OrdinalIgnoreCase))
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

        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.ContainerRegistry.NotFoundError(request.ResourceId);

        // Look for a UAI-based AcrPull role assignment targeting this container registry
        var uaiAcrPull = resource.RoleAssignments.FirstOrDefault(ra =>
            ra.TargetResourceId == request.ContainerRegistryId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull &&
            ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned &&
            ra.UserAssignedIdentityId is not null);

        if (uaiAcrPull is not null)
        {
            var uaiResource = await azureResourceRepository.GetByIdAsync(
                uaiAcrPull.UserAssignedIdentityId!, cancellationToken);

            return new CheckAcrPullAccessResult(
                HasAccess: true,
                MissingRoleDefinitionId: null,
                MissingRoleName: null,
                AssignedUserAssignedIdentityId: uaiAcrPull.UserAssignedIdentityId!.Value.ToString(),
                AssignedUserAssignedIdentityName: uaiResource?.Name.Value,
                HasUserAssignedIdentity: true,
                AcrAuthMode: request.AcrAuthMode);
        }

        // Look for any UAI-based role assignment targeting this container registry (but not AcrPull)
        var uaiOnAcr = resource.RoleAssignments.FirstOrDefault(ra =>
            ra.TargetResourceId == request.ContainerRegistryId &&
            ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned &&
            ra.UserAssignedIdentityId is not null);

        string? assignedUaiId = null;
        string? assignedUaiName = null;
        var hasUai = false;

        if (uaiOnAcr is not null)
        {
            assignedUaiId = uaiOnAcr.UserAssignedIdentityId!.Value.ToString();
            var uaiResource = await azureResourceRepository.GetByIdAsync(
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
                var uaiResource = await azureResourceRepository.GetByIdAsync(
                    anyUai.UserAssignedIdentityId!, cancellationToken);
                assignedUaiName = uaiResource?.Name.Value;
                hasUai = true;
            }
        }

        return new CheckAcrPullAccessResult(
            HasAccess: false,
            MissingRoleDefinitionId: AzureRoleDefinitionCatalog.AcrPull,
            MissingRoleName: "AcrPull",
            AssignedUserAssignedIdentityId: assignedUaiId,
            AssignedUserAssignedIdentityName: assignedUaiName,
            HasUserAssignedIdentity: hasUai,
            AcrAuthMode: request.AcrAuthMode);
    }
}
