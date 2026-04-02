using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignments;

public class ListRoleAssignmentsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<ListRoleAssignmentsQuery, RoleAssignmentsWithIdentityResult>
{
    public async Task<ErrorOr<RoleAssignmentsWithIdentityResult>> Handle(
        ListRoleAssignmentsQuery request,
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

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var roleAssignments = resource.RoleAssignments
            .Select(r => new RoleAssignmentResult(
                r.Id,
                r.SourceResourceId,
                r.TargetResourceId,
                r.ManagedIdentityType,
                r.RoleDefinitionId,
                r.UserAssignedIdentityId))
            .ToList();

        string? assignedUaiId = null;
        string? assignedUaiName = null;

        if (resource.AssignedUserAssignedIdentityId is not null)
        {
            assignedUaiId = resource.AssignedUserAssignedIdentityId.Value.ToString();
            var uaiResource = await azureResourceRepository.GetByIdAsync(
                resource.AssignedUserAssignedIdentityId, cancellationToken);
            assignedUaiName = uaiResource?.Name.Value;
        }

        return new RoleAssignmentsWithIdentityResult(
            assignedUaiId,
            assignedUaiName,
            roleAssignments);
    }
}
