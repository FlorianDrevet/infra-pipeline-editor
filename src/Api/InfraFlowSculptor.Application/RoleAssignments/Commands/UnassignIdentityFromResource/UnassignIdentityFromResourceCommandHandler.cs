using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;

/// <summary>
/// Handles the <see cref="UnassignIdentityFromResourceCommand"/> request.
/// Removes the assigned User-Assigned Identity from a resource.
/// </summary>
public sealed class UnassignIdentityFromResourceCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IRoleAssignmentDomainService roleAssignmentDomainService)
    : ICommandHandler<UnassignIdentityFromResourceCommand, Success>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Success>> Handle(
        UnassignIdentityFromResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resourceResult = await roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
            request.ResourceId, includeRoleAssignments: false, cancellationToken);

        if (resourceResult.IsError)
            return resourceResult.Errors;

        var resource = resourceResult.Value;

        resource.UnassignUserAssignedIdentity();

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return new Success();
    }
}
