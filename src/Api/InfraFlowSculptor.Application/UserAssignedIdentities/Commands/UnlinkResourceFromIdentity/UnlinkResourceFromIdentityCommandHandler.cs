using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;

/// <summary>
/// Handles the <see cref="UnlinkResourceFromIdentityCommand"/> by moving role assignments
/// from the source resource to the UAI itself, preserving the granted rights.
/// </summary>
public sealed class UnlinkResourceFromIdentityCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<UnlinkResourceFromIdentityCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        UnlinkResourceFromIdentityCommand request,
        CancellationToken cancellationToken)
    {
        // Load the UAI with its role assignments
        var identity = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.IdentityId, cancellationToken);

        if (identity is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);

        // Verify write access
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            identity.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        // Load the source resource with its role assignments
        var sourceResource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.SourceResourceId, cancellationToken);

        if (sourceResource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.SourceResourceId);

        // Find all role assignments on the source that reference this UAI
        var assignmentsToMove = sourceResource.RoleAssignments
            .Where(ra => ra.UserAssignedIdentityId == request.IdentityId)
            .ToList();

        if (assignmentsToMove.Count == 0)
            return Result.Deleted;

        // Move each assignment: remove from source, add to UAI
        var managedIdentityType = new ManagedIdentityType(
            ManagedIdentityType.IdentityTypeEnum.UserAssigned);

        foreach (var ra in assignmentsToMove)
        {
            sourceResource.RemoveRoleAssignment(ra.Id);
            identity.AddRoleAssignment(
                ra.TargetResourceId,
                managedIdentityType,
                ra.RoleDefinitionId,
                request.IdentityId);
        }

        await azureResourceRepository.UpdateAsync(sourceResource, cancellationToken);
        await azureResourceRepository.UpdateAsync(identity, cancellationToken);

        return Result.Deleted;
    }
}
