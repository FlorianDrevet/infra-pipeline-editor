using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;

/// <summary>
/// Handles the <see cref="UnassignIdentityFromResourceCommand"/> request.
/// Removes the assigned User-Assigned Identity from a resource.
/// </summary>
public sealed class UnassignIdentityFromResourceCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<UnassignIdentityFromResourceCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        UnassignIdentityFromResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdAsync(
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

        resource.UnassignUserAssignedIdentity();

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return new Success();
    }
}
