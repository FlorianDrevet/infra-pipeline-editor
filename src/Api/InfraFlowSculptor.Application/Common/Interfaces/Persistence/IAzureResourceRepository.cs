using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IAzureResourceRepository
{
    Task<AzureResource?> GetByIdAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<AzureResource?> GetByIdWithRoleAssignmentsAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<AzureResource?> GetByIdWithAppSettingsAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(AzureResourceId id, CancellationToken cancellationToken = default);
    Task<AzureResource> UpdateAsync(AzureResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all role assignments that reference the specified User-Assigned Identity.
    /// </summary>
    /// <param name="identityId">The <see cref="AzureResourceId"/> of the User-Assigned Identity.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of tracked <see cref="RoleAssignment"/> entities.</returns>
    Task<List<RoleAssignment>> GetRoleAssignmentsByIdentityIdAsync(AzureResourceId identityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverts all role assignments referencing the specified User-Assigned Identity
    /// back to system-assigned managed identity (sets <see cref="RoleAssignment.UserAssignedIdentityId"/>
    /// to <c>null</c> and <see cref="RoleAssignment.ManagedIdentityType"/> to <c>SystemAssigned</c>).
    /// </summary>
    /// <param name="identityId">The <see cref="AzureResourceId"/> of the User-Assigned Identity being removed.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The number of role assignments that were reverted.</returns>
    Task<int> RevertRoleAssignmentsToSystemAssignedAsync(AzureResourceId identityId, CancellationToken cancellationToken = default);
}
