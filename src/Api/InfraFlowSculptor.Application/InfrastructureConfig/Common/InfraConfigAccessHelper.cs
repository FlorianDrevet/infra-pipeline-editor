using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

internal static class InfraConfigAccessHelper
{
    /// <summary>
    /// Verifies the current user is a member of the given InfrastructureConfig (any role).
    /// Returns NotFoundError if the config does not exist or the user is not a member,
    /// to avoid leaking the existence of the configuration to non-members.
    /// </summary>
    public static async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyReadAccessAsync(
        IInfrastructureConfigRepository repository,
        ICurrentUser currentUser,
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(infraConfigId, cancellationToken);

        if (infraConfig is null || !infraConfig.Members.Any(m => m.UserId == userId))
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        return infraConfig;
    }

    /// <summary>
    /// Verifies the current user is an Owner or Contributor in the given InfrastructureConfig.
    /// Returns NotFoundError if the config does not exist or the user is not a member.
    /// Returns ForbiddenError if the user is a Reader (insufficient role for write operations).
    /// </summary>
    public static async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyWriteAccessAsync(
        IInfrastructureConfigRepository repository,
        ICurrentUser currentUser,
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var member = infraConfig.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        if (member.Role.Value == Role.RoleEnum.Reader)
            return Errors.InfrastructureConfig.ForbiddenError();

        return infraConfig;
    }
}
