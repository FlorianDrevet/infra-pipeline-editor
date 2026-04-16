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
    IRoleAssignmentDomainService roleAssignmentDomainService)
    : ICommandHandler<UpdateRoleAssignmentIdentityCommand, RoleAssignmentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<RoleAssignmentResult>> Handle(
        UpdateRoleAssignmentIdentityCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResourceResult = await roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
            request.SourceResourceId, includeRoleAssignments: true, cancellationToken);

        if (sourceResourceResult.IsError)
            return sourceResourceResult.Errors;

        var sourceResource = sourceResourceResult.Value;

        var identityTypeResult = await roleAssignmentDomainService.ValidateIdentityTypeAsync(
            request.ManagedIdentityType, request.UserAssignedIdentityId, cancellationToken);

        if (identityTypeResult.IsError)
            return identityTypeResult.Errors;

        var managedIdentityType = identityTypeResult.Value;

        var existingAssignment = sourceResource.RoleAssignments.FirstOrDefault(r => r.Id == request.RoleAssignmentId);
        if (existingAssignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        if (existingAssignment.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull
            && managedIdentityType.Value == ManagedIdentityType.IdentityTypeEnum.SystemAssigned)
            return Errors.RoleAssignment.AcrPullRequiresUserAssigned();

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
