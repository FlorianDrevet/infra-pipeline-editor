using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

public class AddRoleAssignmentCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<AddRoleAssignmentCommand, ErrorOr<RoleAssignmentResult>>
{
    public async Task<ErrorOr<RoleAssignmentResult>> Handle(
        AddRoleAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.SourceResourceId, cancellationToken);

        if (sourceResource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.SourceResourceId);

        var resourceType = sourceResource.GetType().Name;
        if (!AzureRoleDefinitionCatalog.IsValidForResourceType(resourceType, request.RoleDefinitionId))
            return Errors.RoleAssignment.InvalidRoleDefinitionForResourceType(request.RoleDefinitionId, resourceType);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            sourceResource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(sourceResource.ResourceGroupId);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var targetExists = await azureResourceRepository.ExistsAsync(
            request.TargetResourceId, cancellationToken);

        if (!targetExists)
            return Errors.RoleAssignment.TargetResourceNotFound(request.TargetResourceId);

        var managedIdentityType = new ManagedIdentityType(
            Enum.Parse<ManagedIdentityType.IdentityTypeEnum>(request.ManagedIdentityType, ignoreCase: true));

        sourceResource.AddRoleAssignment(request.TargetResourceId, managedIdentityType, request.RoleDefinitionId);

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
            assignment.RoleDefinitionId);
    }
}
