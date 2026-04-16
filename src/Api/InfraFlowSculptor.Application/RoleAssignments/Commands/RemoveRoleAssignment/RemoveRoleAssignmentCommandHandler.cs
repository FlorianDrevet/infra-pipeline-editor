using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;

/// <summary>
/// Handles the <see cref="RemoveRoleAssignmentCommand"/> request
/// and removes a role assignment from a source resource.
/// </summary>
public sealed class RemoveRoleAssignmentCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IRoleAssignmentDomainService roleAssignmentDomainService)
    : ICommandHandler<RemoveRoleAssignmentCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveRoleAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResourceResult = await roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
            request.SourceResourceId, includeRoleAssignments: true, cancellationToken);

        if (sourceResourceResult.IsError)
            return sourceResourceResult.Errors;

        var sourceResource = sourceResourceResult.Value;

        var assignment = sourceResource.RoleAssignments.FirstOrDefault(r => r.Id == request.RoleAssignmentId);
        if (assignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        sourceResource.RemoveRoleAssignment(request.RoleAssignmentId);

        await azureResourceRepository.UpdateAsync(sourceResource, cancellationToken);

        return Result.Deleted;
    }
}
