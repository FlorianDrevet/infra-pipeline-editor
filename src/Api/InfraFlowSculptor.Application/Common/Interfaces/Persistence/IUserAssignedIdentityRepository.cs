using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository for <see cref="UserAssignedIdentity"/> aggregate persistence operations.
/// </summary>
public interface IUserAssignedIdentityRepository : IRepository<UserAssignedIdentity>
{
    /// <summary>
    /// Returns all user-assigned identities belonging to the specified resource group.
    /// </summary>
    /// <param name="resourceGroupId">The resource group identifier to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of matching <see cref="UserAssignedIdentity"/> entities.</returns>
    Task<List<UserAssignedIdentity>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default);
}
