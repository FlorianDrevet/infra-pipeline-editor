using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Ensures authenticated users are provisioned in persistence before application code resolves them.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>
    /// Ensures a user identified by Microsoft Entra exists and returns the persisted identifier.
    /// </summary>
    /// <param name="entraId">The authenticated Microsoft Entra object identifier.</param>
    /// <param name="name">The authenticated user's display name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted user identifier.</returns>
    Task<UserId> EnsureProvisionedAsync(
        EntraId entraId,
        Name name,
        CancellationToken cancellationToken = default);
}