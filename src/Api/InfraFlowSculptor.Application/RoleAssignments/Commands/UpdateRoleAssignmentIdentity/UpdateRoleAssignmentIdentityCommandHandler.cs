using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;

/// <summary>Handles the <see cref="UpdateRoleAssignmentIdentityCommand"/> request.</summary>
public sealed class UpdateRoleAssignmentIdentityCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<UpdateRoleAssignmentIdentityCommand, RoleAssignmentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<RoleAssignmentResult>> Handle(
        UpdateRoleAssignmentIdentityCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.SourceResourceId, cancellationToken);

        if (sourceResource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.SourceResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            sourceResource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(sourceResource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var managedIdentityType = new ManagedIdentityType(
            Enum.Parse<ManagedIdentityType.IdentityTypeEnum>(request.ManagedIdentityType, ignoreCase: true));

        var existingAssignment = sourceResource.RoleAssignments.FirstOrDefault(r => r.Id == request.RoleAssignmentId);
        if (existingAssignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        if (existingAssignment.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull
            && managedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.SystemAssigned)
            return Errors.RoleAssignment.AcrPullRequiresUserAssigned();

        if (managedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned
            && request.UserAssignedIdentityId is null)
            return Errors.RoleAssignment.UserAssignedIdentityRequired();

        if (request.UserAssignedIdentityId is not null)
        {
            var identityResource = await azureResourceRepository.GetByIdAsync(
                request.UserAssignedIdentityId, cancellationToken);

            if (identityResource is null)
                return Errors.RoleAssignment.UserAssignedIdentityNotFound(request.UserAssignedIdentityId);
        }

        var assignment = sourceResource.UpdateRoleAssignmentIdentity(
            request.RoleAssignmentId,
            managedIdentityType,
            managedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.UserAssigned
                ? request.UserAssignedIdentityId
                : null);

        if (assignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        await azureResourceRepository.UpdateAsync(sourceResource, cancellationToken);

        return new RoleAssignmentResult(
            assignment.Id,
            assignment.SourceResourceId,
            assignment.TargetResourceId,
            assignment.ManagedIdentityType,
            assignment.RoleDefinitionId,
            assignment.UserAssignedIdentityId);
    }
}
