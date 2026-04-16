using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>
/// Domain service that centralizes cross-cutting validation and authorization logic
/// shared by role-assignment command handlers.
/// </summary>
public interface IRoleAssignmentDomainService
{
    /// <summary>
    /// Loads the specified Azure resource and verifies the current user has write access
    /// through the resource's parent resource group.
    /// </summary>
    /// <param name="resourceId">Identifier of the resource to load.</param>
    /// <param name="includeRoleAssignments">When <c>true</c>, eagerly loads the resource's role assignments.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The loaded resource on success, or a domain error.</returns>
    Task<ErrorOr<AzureResource>> LoadResourceAndAuthorizeAsync(
        AzureResourceId resourceId,
        bool includeRoleAssignments,
        CancellationToken cancellationToken);

    /// <summary>
    /// Parses a managed identity type string, validates that UserAssigned constraints are met
    /// (requires a <paramref name="userAssignedIdentityId"/>), and verifies the identity resource exists.
    /// </summary>
    /// <param name="managedIdentityType">The managed identity type string to parse.</param>
    /// <param name="userAssignedIdentityId">Optional User-Assigned Identity resource ID.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The parsed <see cref="ManagedIdentityType"/> on success, or a domain error.</returns>
    Task<ErrorOr<ManagedIdentityType>> ValidateIdentityTypeAsync(
        string managedIdentityType,
        AzureResourceId? userAssignedIdentityId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the specified User-Assigned Identity resource exists.
    /// </summary>
    /// <param name="identityResourceId">Identifier of the identity resource to check.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><see cref="Success"/> if the resource exists, or a domain error.</returns>
    Task<ErrorOr<Success>> ValidateIdentityResourceExistsAsync(
        AzureResourceId identityResourceId,
        CancellationToken cancellationToken);
}
