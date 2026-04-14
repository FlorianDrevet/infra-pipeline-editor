using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;

/// <summary>
/// Handles the <see cref="AssignIdentityToResourceCommand"/> request.
/// Assigns a User-Assigned Identity to a resource and removes duplicate
/// role assignments that overlap between SystemAssigned and UserAssigned.
/// </summary>
public sealed class AssignIdentityToResourceCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<AssignIdentityToResourceCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        AssignIdentityToResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.ResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var identityResource = await azureResourceRepository.GetByIdAsync(
            request.UserAssignedIdentityId, cancellationToken);

        if (identityResource is null)
            return Errors.RoleAssignment.UserAssignedIdentityNotFound(request.UserAssignedIdentityId);

        resource.AssignUserAssignedIdentity(request.UserAssignedIdentityId);

        // Deduplication: remove UserAssigned role assignments that duplicate SystemAssigned ones on this resource
        var saKeys = resource.RoleAssignments
            .Where(ra => ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.SystemAssigned)
            .Select(ra => (ra.TargetResourceId, ra.RoleDefinitionId))
            .ToHashSet();

        var duplicateUaRas = resource.RoleAssignments
            .Where(ra => ra.ManagedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned
                         && saKeys.Contains((ra.TargetResourceId, ra.RoleDefinitionId)))
            .ToList();

        foreach (var dup in duplicateUaRas)
        {
            resource.RemoveRoleAssignment(dup.Id);
        }

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return new Success();
    }
}
