using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;

/// <summary>
/// Handles the <see cref="DeleteUserAssignedIdentityCommand"/> request
/// and permanently deletes the specified user-assigned identity.
/// Before deletion, all role assignments referencing this identity are
/// reverted to system-assigned managed identity.
/// </summary>
public sealed class DeleteUserAssignedIdentityCommandHandler(
    IUserAssignedIdentityRepository userAssignedIdentityRepository,
    IResourceGroupRepository resourceGroupRepository,
    IAzureResourceRepository azureResourceRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteUserAssignedIdentityCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteUserAssignedIdentityCommand request,
        CancellationToken cancellationToken)
    {
        var identity = await userAssignedIdentityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (identity is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await azureResourceRepository.RevertRoleAssignmentsToSystemAssignedAsync(request.Id, cancellationToken);

        await userAssignedIdentityRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
