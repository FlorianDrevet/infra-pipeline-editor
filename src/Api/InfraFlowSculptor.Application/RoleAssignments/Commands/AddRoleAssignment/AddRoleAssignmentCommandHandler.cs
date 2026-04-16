using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

/// <summary>
/// Handles the <see cref="AddRoleAssignmentCommand"/> request
/// and creates a role assignment between two Azure resources.
/// </summary>
public sealed class AddRoleAssignmentCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IRoleAssignmentDomainService roleAssignmentDomainService)
    : ICommandHandler<AddRoleAssignmentCommand, RoleAssignmentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<RoleAssignmentResult>> Handle(
        AddRoleAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResourceResult = await roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
            request.SourceResourceId, includeRoleAssignments: true, cancellationToken);

        if (sourceResourceResult.IsError)
            return sourceResourceResult.Errors;

        var sourceResource = sourceResourceResult.Value;

        var targetResource = await azureResourceRepository.GetByIdAsync(
            request.TargetResourceId, cancellationToken);

        if (targetResource is null)
            return Errors.RoleAssignment.TargetResourceNotFound(request.TargetResourceId);

        var targetResourceType = targetResource.GetType().Name;
        if (!AzureRoleDefinitionCatalog.IsValidForResourceType(targetResourceType, request.RoleDefinitionId))
            return Errors.RoleAssignment.InvalidRoleDefinitionForResourceType(request.RoleDefinitionId, targetResourceType);

        var identityTypeResult = await roleAssignmentDomainService.ValidateIdentityTypeAsync(
            request.ManagedIdentityType, request.UserAssignedIdentityId, cancellationToken);

        if (identityTypeResult.IsError)
            return identityTypeResult.Errors;

        var managedIdentityType = identityTypeResult.Value;

        sourceResource.AddRoleAssignment(
            request.TargetResourceId,
            managedIdentityType,
            request.RoleDefinitionId,
            request.UserAssignedIdentityId);

        var updated = await azureResourceRepository.UpdateAsync(sourceResource, cancellationToken);

        var assignment = updated.RoleAssignments
            .First(r => r.TargetResourceId == request.TargetResourceId
                        && r.RoleDefinitionId == request.RoleDefinitionId
                        && r.ManagedIdentityType.Value == managedIdentityType.Value);

        return new RoleAssignmentResult(
            assignment.Id,
            assignment.SourceResourceId,
            assignment.TargetResourceId,
            assignment.ManagedIdentityType,
            assignment.RoleDefinitionId,
            assignment.UserAssignedIdentityId);
    }
}
