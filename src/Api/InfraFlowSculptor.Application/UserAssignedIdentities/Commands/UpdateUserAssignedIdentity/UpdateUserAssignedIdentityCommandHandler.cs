using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;

/// <summary>
/// Handles the <see cref="UpdateUserAssignedIdentityCommand"/> request
/// and updates the mutable properties of an existing user-assigned identity.
/// </summary>
public sealed class UpdateUserAssignedIdentityCommandHandler(
    IUserAssignedIdentityRepository userAssignedIdentityRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateUserAssignedIdentityCommand, UserAssignedIdentityResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<UserAssignedIdentityResult>> Handle(
        UpdateUserAssignedIdentityCommand request,
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

        identity.Update(request.Name, request.Location);

        var updated = await userAssignedIdentityRepository.UpdateAsync(identity);

        return mapper.Map<UserAssignedIdentityResult>(updated);
    }
}
