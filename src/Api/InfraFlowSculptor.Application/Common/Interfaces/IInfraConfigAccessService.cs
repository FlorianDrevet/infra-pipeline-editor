using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Service that enforces access control for InfrastructureConfig resources.
/// Inject this service into command and query handlers instead of passing
/// repository and current-user dependencies individually.
/// </summary>
public interface IInfraConfigAccessService
{
    /// <summary>
    /// Verifies the current user is a member of the given InfrastructureConfig (any role).
    /// Returns <c>NotFoundError</c> if the configuration does not exist or the user is not a
    /// member, to avoid leaking the existence of the configuration to non-members.
    /// </summary>
    Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyReadAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the current user is an Owner or Contributor in the given InfrastructureConfig.
    /// Returns <c>NotFoundError</c> if the configuration does not exist or the user is not a member.
    /// Returns <c>ForbiddenError</c> if the user is a Reader (insufficient role for write operations).
    /// </summary>
    Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyWriteAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);
}
