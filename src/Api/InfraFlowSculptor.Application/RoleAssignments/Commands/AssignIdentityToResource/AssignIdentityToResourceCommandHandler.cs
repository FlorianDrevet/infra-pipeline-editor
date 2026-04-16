using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
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
    IRoleAssignmentDomainService roleAssignmentDomainService)
    : ICommandHandler<AssignIdentityToResourceCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        AssignIdentityToResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resourceResult = await roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
            request.ResourceId, includeRoleAssignments: true, cancellationToken);

        if (resourceResult.IsError)
            return resourceResult.Errors;

        var resource = resourceResult.Value;

        var identityCheck = await roleAssignmentDomainService.ValidateIdentityResourceExistsAsync(
            request.UserAssignedIdentityId, cancellationToken);

        if (identityCheck.IsError)
            return identityCheck.Errors;

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
