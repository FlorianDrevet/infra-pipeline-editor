using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Queries.GetUserAssignedIdentity;

/// <summary>
/// Handles the <see cref="GetUserAssignedIdentityQuery"/> request
/// and returns the matching user-assigned identity if the caller has read access.
/// </summary>
public sealed class GetUserAssignedIdentityQueryHandler(
    IUserAssignedIdentityRepository userAssignedIdentityRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetUserAssignedIdentityQuery, UserAssignedIdentityResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<UserAssignedIdentityResult>> Handle(
        GetUserAssignedIdentityQuery query,
        CancellationToken cancellationToken)
    {
        var identity = await userAssignedIdentityRepository.GetByIdAsync(query.Id, cancellationToken);
        if (identity is null)
            return Errors.UserAssignedIdentity.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.UserAssignedIdentity.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return mapper.Map<UserAssignedIdentityResult>(identity);
    }
}
