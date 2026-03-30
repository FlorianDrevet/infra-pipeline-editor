using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;

public class RemoveRoleAssignmentCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveRoleAssignmentCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveRoleAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.SourceResourceId, cancellationToken);

        if (sourceResource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.SourceResourceId);

        var assignment = sourceResource.RoleAssignments.FirstOrDefault(r => r.Id == request.RoleAssignmentId);
        if (assignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            sourceResource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(sourceResource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        sourceResource.RemoveRoleAssignment(request.RoleAssignmentId);

        await azureResourceRepository.UpdateAsync(sourceResource, cancellationToken);

        return Result.Deleted;
    }
}
