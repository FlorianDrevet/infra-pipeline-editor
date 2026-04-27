using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;

/// <summary>
/// Handles the <see cref="CreateUserAssignedIdentityCommand"/> request
/// and creates a new <see cref="UserAssignedIdentity"/> resource.
/// </summary>
public sealed class CreateUserAssignedIdentityCommandHandler(
    IUserAssignedIdentityRepository userAssignedIdentityRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateUserAssignedIdentityCommand, UserAssignedIdentityResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<UserAssignedIdentityResult>> Handle(
        CreateUserAssignedIdentityCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var identity = UserAssignedIdentity.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.IsExisting);

        var saved = await userAssignedIdentityRepository.AddAsync(identity);

        return mapper.Map<UserAssignedIdentityResult>(saved);
    }
}
